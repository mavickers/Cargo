using System;
using System.Collections.Concurrent;
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
        private Package<TContent> _package { get; set; }
        private int _stationRepeatLimit { get; set; } = 100;
        private ConcurrentQueue<Type> _stations { get; } = new ConcurrentQueue<Type>();
        private ConcurrentDictionary<Type, object> _services { get; } = new ConcurrentDictionary<Type, object>();
        private bool _withAbortOnError { get; set; } = true;

        public Package<TContent> Package => _package;

        private Bus() { }

        public TContent Go(TContent content, Func<TContent, TContent> callback = null)
        {
            if (content == null) throw new ArgumentException("\"content\" parameter is null");

            var allStations = _stations.Append(_finalStation).Where(s => s != null);

            foreach (var stationType in allStations)
            {
                if (IsAsyncStationType(stationType))
                    throw new InvalidOperationException($"Station '{stationType.FullName}' is async. Use GoAsync() instead of Go().");
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
            var stationList = _stations.Append(_finalStation).Where(s => s != null).ToList();
            var hasFinalStation = _finalStation != null;
            var iteration = 1;

            if (syncProcessMethod == null) throw new Exception("Bus.GoAsync failed - unable to access sync Process method");
            if (asyncProcessMethod == null) throw new Exception("Bus.GoAsync failed - unable to access async ProcessAsync method");
            if (packageProperty == null) throw new Exception("Unable to access package property");

            _package = Cargo.Package.New<TContent>(content, _services);
            _package.CancellationToken = cancellationToken;
            _package.Trace($"{typeof(TContent).FullName} begin trace");

            callback = callback ?? (arg => arg);

            while (currentStationIndex < stationList.Count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stationType = stationList[currentStationIndex];
                var isLastStation = stationList[currentStationIndex] == stationList.Last();
                var isAsync = IsAsyncStationType(stationType);
                var currentStation = Activator.CreateInstance(stationList[currentStationIndex]);
                var stationPrefix = $"{stationList[currentStationIndex].FullName}";

                if (currentStation == null) throw new Exception($"Unable to instantiate {stationList[currentStationIndex].FullName}");

                packageProperty.SetValue(currentStation, _package);

                try
                {
                    if (iteration > _stationRepeatLimit) throw new OverflowException("Bus.Go failed - station execution iterations exceeded repeat limit");

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

                    var result = Station.Result.New(stationType, action, Succeeded);

                    _package.Trace($"{stationPrefix} finished - {action.ActionType} ({action.ActionMessage ?? "N/A"})");
                    _package.Results.Enqueue(result);
                }
                catch (Exception exception)
                {
                    var actualException = exception;
                    if (actualException is TargetInvocationException tie && tie.InnerException != null)
                        actualException = tie.InnerException;
                    if (actualException is AggregateException ae && ae.InnerExceptions.Count == 1)
                        actualException = ae.InnerExceptions[0];

                    var action = _withAbortOnError ? Station.Action.Abort(actualException) : Station.Action.Next(actualException);
                    var result = Station.Result.New(stationType, action, Failed, actualException);

                    _package.Trace($"{stationPrefix} finished - {action.ActionType} ({action.ActionMessage ?? "N/A"})");
                    _package.Results.Enqueue(result);
                }

                var isRepeating = _package.LastStationResult?.IsRepeating ?? false;
                var isAborting = _package.LastStationResult?.IsAborting ?? false;
                var wasFailure = _package.LastStationResult?.WasFailure ?? false;
                var gotoFinalStation = !isLastStation && (isAborting || (wasFailure && _withAbortOnError && hasFinalStation));

                iteration = isRepeating ? iteration + 1 : 1;

                // if we are repeating the station then do not change the index and continue
                // if we are going to the final station then change the index to the total count - 1
                // if we aborted then go past the total count (this will end the loop)
                // otherwise increment the index

                currentStationIndex += isRepeating ? 0 : gotoFinalStation ? stationList.Count - currentStationIndex - 1 : isAborting ? stationList.Count + 1 : 1;
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

        public Bus<TContent> WithFinalStation<TStation>()
        {
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

        public Bus<TContent> WithStationRepeatLimit(int limit)
        {
            _stationRepeatLimit = limit;
            return this;
        }

        private void WithStation(Type stationType, bool throwOnInvalid = true)
        {
            if (!IsValidStationType(stationType))
            {
                if (throwOnInvalid) throw new ArgumentException($"Type '{stationType.FullName}' does not inherit from Station<{typeof(TContent).Name}> or StationAsync<{typeof(TContent).Name}>");
                return;
            }

            _stations.Enqueue(stationType);
        }

        private static bool IsValidStationType(Type stationType)
        {
            var current = stationType;

            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    var genericDef = current.GetGenericTypeDefinition();
                    if (genericDef == typeof(Station<>) || genericDef == typeof(StationAsync<>)) return true;
                }

                current = current.BaseType;
            }

            return false;
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
