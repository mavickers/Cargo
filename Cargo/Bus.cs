using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static LightPath.Cargo.Station.Output;
using static LightPath.Cargo.Strategies;

namespace LightPath.Cargo
{
    public static class Bus
    {
        public static Bus<TContent> New<TContent>() where TContent : class
        {
            return Bus<TContent>.New();
        }
    }

    public class Bus<TContent> where TContent : class
    {
        private Type _finalStation { get; set; }
        private IStationObserver _observer { get; set; }
        private Package<TContent> _package { get; set; }
        private int _stationRepeatLimit { get; set; } = 100;
        private ConcurrentQueue<Type> _stations { get; } = new ConcurrentQueue<Type>();
        private ConcurrentDictionary<Type, object> _services { get; } = new ConcurrentDictionary<Type, object>();
        private bool _withAbortOnError { get; set; } = true;
        private bool _withAbortOnCancel { get; set; } = false;

        public Package<TContent> Package => _package;

        private Bus() { }

        public TContent Go(TContent content, Func<TContent, TContent> callback = null)
        {
            if (content == null) throw new ArgumentException("\"content\" parameter is null");

            var allStations = _stations.Append(_finalStation).Where(s => s != null);

            foreach (var stationType in allStations)
            {
                if (IsAsyncStationType(stationType)) throw new InvalidOperationException($"Station '{stationType.FullName}' is async. Use GoAsync() instead of Go().");
            }

            return GoAsync(content, CancellationToken.None, callback).GetAwaiter().GetResult();
        }

        public async Task<TContent> GoAsync
        (
            TContent content,
            CancellationToken cancellationToken = default,
            Func<TContent, TContent> callback = null
        )
        {
            if (content == null) throw new ArgumentException("\"content\" parameter is null");

            var currentStationIndex = 0;
            var syncProcessMethod = typeof(Station<TContent>).GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
            var asyncProcessMethod = typeof(StationAsync<TContent>).GetMethod("ProcessAsync", BindingFlags.Public | BindingFlags.Instance);
            var packageProperty = typeof(StationBase<TContent>).GetProperty("_package", BindingFlags.NonPublic | BindingFlags.Instance);
            var repeatLimitProperty = typeof(StationBase<TContent>).GetProperty("RepeatLimit", BindingFlags.NonPublic | BindingFlags.Instance);
            var stationList = _stations.Append(_finalStation).Where(s => s != null).ToList();
            var hasFinalStation = _finalStation != null;
            var iteration = 1;

            if (syncProcessMethod == null) throw new Exception("Bus.GoAsync failed - unable to access sync Process method");
            if (asyncProcessMethod == null) throw new Exception("Bus.GoAsync failed - unable to access async ProcessAsync method");
            if (packageProperty == null) throw new Exception("Unable to access package property");
            if (repeatLimitProperty == null) throw new Exception("Unable to access repeat limit property");

            _package = Cargo.Package.New<TContent>(content, _services);
            _package.CancellationToken = cancellationToken;
            _package.Trace($"{typeof(TContent).FullName} begin trace");

            callback = callback ?? (arg => arg);

            while (currentStationIndex < stationList.Count)
            {
                var stationType = stationList[currentStationIndex];
                var stationPrefix = stationType.FullName;
                var isLastStation = currentStationIndex == stationList.Count - 1;

                Station.Result result;

                // under abort-on-cancel the final station gets exactly one run; a Repeat it returns after cancellation would
                // otherwise rerun it with every delay skipped until the repeat limit, so iteration 2+ falls through to the abort
                var runsDespiteCancel = _withAbortOnCancel && isLastStation && hasFinalStation && iteration == 1;

                if (cancellationToken.IsCancellationRequested && !runsDespiteCancel)
                {
                    if (!_withAbortOnCancel) throw new OperationCanceledException(cancellationToken);

                    var cancelException = new OperationCanceledException(cancellationToken);
                    var action = Station.Action.Abort(cancelException);

                    result = Station.Result.New(stationType, action, Failed, cancelException);
                }
                else
                {
                    var isAsync = IsAsyncStationType(stationType);

                    _observer?.Starting(stationType, iteration);

                    var stopwatch = Stopwatch.StartNew();

                    try
                    {
                        // construction sits inside the try so a throwing constructor becomes a Failed result and the final station still runs
                        var currentStation = Activator.CreateInstance(stationType);

                        if (currentStation == null) throw new Exception($"Unable to instantiate {stationType.FullName}");

                        packageProperty.SetValue(currentStation, _package);

                        var repeatLimit = (int?)repeatLimitProperty.GetValue(currentStation) ?? _stationRepeatLimit;

                        if (iteration > repeatLimit) throw new OverflowException("Bus.Go failed - station execution iterations exceeded repeat limit");

                        _package.Trace();
                        _package.Trace($"{stationPrefix} begin");

                        Station.Action action;

                        if (isAsync)
                        {
                            var task = (Task<Station.Action>)asyncProcessMethod.Invoke(currentStation, null);
                            action = await task.ConfigureAwait(false);
                        }
                        else
                        {
                            action = (Station.Action)syncProcessMethod.Invoke(currentStation, null);
                        }

                        result = Station.Result.New(stationType, action, Succeeded);
                    }
                    catch (Exception exception)
                    {
                        var actualException = exception;
                        if (actualException is TargetInvocationException tie && tie.InnerException != null) actualException = tie.InnerException;
                        if (actualException is AggregateException ae && ae.InnerExceptions.Count == 1) actualException = ae.InnerExceptions[0];
                        if (actualException is OperationCanceledException oce && cancellationToken.IsCancellationRequested)
                        {
                            if (!_withAbortOnCancel) throw new OperationCanceledException(oce.Message, oce, cancellationToken);
                        }

                        var action = _withAbortOnError ? Station.Action.Abort(actualException) : Station.Action.Next(actualException);

                        result = Station.Result.New(stationType, action, Failed, actualException);
                    }

                    result.Elapsed = stopwatch.Elapsed;

                    _package.Trace($"{stationPrefix} finished - {result._action.ActionType} ({result.ActionMessage ?? "N/A"}) in {result.Elapsed.TotalMilliseconds:F0} ms");
                }

                result.Iteration = iteration;

                _package.Results.Enqueue(result);
                _observer?.Finished(result);

                var isRepeating = result.IsRepeating;
                var isAborting = result.IsAborting;
                var wasFailure = result.WasFailure;
                var gotoFinalStation = !isLastStation && (isAborting || (wasFailure && _withAbortOnError && hasFinalStation));

                iteration = isRepeating ? iteration + 1 : 1;

                // if we are repeating the station then do not change the index and continue
                // if we are going to the final station then change the index to the total count - 1
                // if we aborted then go past the total count (this will end the loop)
                // otherwise increment the index

                currentStationIndex += isRepeating ? 0 : gotoFinalStation ? stationList.Count - currentStationIndex - 1 : isAborting ? stationList.Count + 1 : 1;

                if (isRepeating && result.ActionDelay > TimeSpan.Zero)
                {
                    _package.Trace($"{stationPrefix} repeat delayed {result.ActionDelay.TotalMilliseconds:F0} ms");

                    // a cancellation here is handled by the IsCancellationRequested check at the top of the next loop pass
                    try { await Task.Delay(result.ActionDelay, cancellationToken).ConfigureAwait(false); }
                    catch (OperationCanceledException) { }
                }
            }

            _package.Trace();
            _package.Trace($"{typeof(TContent).FullName} end trace");

            return callback(content);
        }

        // TODO: consider splitting Bus if perf is a concern

        public static Bus<TContent> New()
        {
            return new Bus<TContent>();
        }

        public Bus<TContent> WithAbortOnError()
        {
            _withAbortOnError = true;
            return this;
        }

        public Bus<TContent> WithNoAbortOnError()
        {
            _withAbortOnError = false;
            return this;
        }

        public Bus<TContent> WithAbortOnCancel()
        {
            _withAbortOnCancel = true;
            return this;
        }

        public Bus<TContent> WithNoAbortOnCancel()
        {
            _withAbortOnCancel = false;
            return this;
        }

        public Bus<TContent> WithFinalStation<TStation>()
        {
            var invalidReason = GetInvalidStationReason(typeof(TStation));

            if (invalidReason != null) throw new ArgumentException(invalidReason);

            _finalStation = typeof(TStation);
            return this;
        }

        /// <summary>
        /// Register a service with a declared type
        /// </summary>
        public Bus<TContent> WithService<TService>(TService service)
        {
            if (service != null && !_services.TryAdd(typeof(TService), service)) throw new Exception("Unable to update services dictionary");

            return this;
        }

        /// <summary>
        /// Register a service using the actual type of the service
        /// </summary>
        public Bus<TContent> WithService(object service)
        {
            if (service != null && !_services.TryAdd(service.GetType(), service)) throw new Exception("Unable to update services dictionary");

            return this;
        }

        /// <summary>
        /// Add multiple services to the bus
        /// </summary>
        public Bus<TContent> WithServices(ServiceRegistrationStrategy strategy, params object[] services)
        {
            if (services == null) return this;

            foreach (var service in services)
            {
                var type = service.GetType();
                var firstInterface = type.GetInterfaces().FirstOrDefault();

                if (firstInterface == null || type.IsEnum || strategy == ServiceRegistrationStrategy.AsDeclaredType) WithService(service);
                else if(!_services.TryAdd(firstInterface, service)) throw new Exception("Unable to update services dictionary");
            }

            return this;
        }

        public Bus<TContent> WithStation<TStation>()
        {
            WithStation(typeof(TStation));

            return this;
        }

        public Bus<TContent> WithStations(params Type[] stationTypes)
        {
            if (stationTypes == null) return this;
            if (stationTypes.Length == 0) return this;

            foreach (var stationType in stationTypes)
            {
                WithStation(stationType, throwOnInvalid: false);
            }

            return this;
        }

        /// <summary>
        /// Receive Starting/Finished calls as each station runs. The calls happen outside the station's
        /// error handling, so an observer that throws escapes Go/GoAsync rather than failing the station.
        /// </summary>
        public Bus<TContent> WithStationObserver(IStationObserver observer)
        {
            _observer = observer;
            return this;
        }

        public Bus<TContent> WithStationRepeatLimit(int limit)
        {
            _stationRepeatLimit = limit;
            return this;
        }

        /// <summary>
        /// With throwOnInvalid false (WithStations) a type that is not a station at all is skipped;
        /// a station that is misconfigured still throws, so a setup error never disappears silently.
        /// </summary>
        private void WithStation(Type stationType, bool throwOnInvalid = true)
        {
            if (!throwOnInvalid && GetStationBaseType(stationType) == null) return;

            var invalidReason = GetInvalidStationReason(stationType);

            if (invalidReason != null) throw new ArgumentException(invalidReason);

            _stations.Enqueue(stationType);
        }

        /// <summary>
        /// Null when the type can run on this bus. The content-type and constructor checks catch at
        /// registration what would otherwise surface as a reflection exception from Go.
        /// </summary>
        private static string GetInvalidStationReason(Type stationType)
        {
            var stationBase = GetStationBaseType(stationType);
            var expected = $"Station<{typeof(TContent).Name}> or StationAsync<{typeof(TContent).Name}>";

            if (stationBase == null) return $"Type '{stationType.FullName}' does not inherit from {expected}";
            if (stationBase.GetGenericArguments()[0] != typeof(TContent)) return $"Type '{stationType.FullName}' inherits from {stationBase.Name.Split('`')[0]}<{stationBase.GetGenericArguments()[0].Name}>, not {expected}";
            if (stationType.IsAbstract || stationType.GetConstructor(Type.EmptyTypes) == null) return $"Type '{stationType.FullName}' must be concrete with a public parameterless constructor";

            return null;
        }

        /// <summary>The closed Station&lt;T&gt; or StationAsync&lt;T&gt; in the type's base chain, or null.</summary>
        private static Type GetStationBaseType(Type stationType)
        {
            var current = stationType;

            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    var genericDef = current.GetGenericTypeDefinition();
                    if (genericDef == typeof(Station<>) || genericDef == typeof(StationAsync<>)) return current;
                }

                current = current.BaseType;
            }

            return null;
        }

        private static bool IsAsyncStationType(Type stationType)
        {
            var current = stationType;

            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(StationAsync<>)) return true;

                current = current.BaseType;
            }

            return false;
        }
    }
}
