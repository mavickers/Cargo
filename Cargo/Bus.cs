﻿using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Reflection;
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

        internal static Bus<TContent> SetAndReturn<TContent>(this Bus<TContent> bus, string propertyName, object value) where TContent : class
        {
            var property = typeof(Bus<TContent>).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null) throw new Exception($"Cound not find property named '{propertyName}'");

            property.SetValue(bus, value);

            return bus;
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

            var currentStationIndex = 0;
            var processMethod = typeof(Station<TContent>).GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
            var stationList = _stations.Append(_finalStation).Where(s => s != null).ToList();
            var hasFinalStation = _finalStation != null;
            var iteration = 1;
            
            if (processMethod == null) throw new Exception("Bus.Go failed - unable to access processmethod");

            _package = Cargo.Package.New<TContent>(content, _services);
            _package.Trace($"{typeof(TContent).FullName} begin trace");

            callback = callback ?? (arg => arg);

            while (currentStationIndex < stationList.Count)
            {
                var stationType = stationList[currentStationIndex];
                var isLastStation = stationList[currentStationIndex] == stationList.Last();
                var currentStation = Activator.CreateInstance(stationList[currentStationIndex]);
                var packageProperty = typeof(Station<TContent>).GetProperty("_package", BindingFlags.NonPublic | BindingFlags.Instance);
                var stationPrefix = $"{stationList[currentStationIndex].FullName}";

                if (currentStation == null) throw new Exception($"Unable to instantiate {stationList[currentStationIndex].FullName}");
                if (packageProperty == null) throw new Exception("Unable to access package property");

                packageProperty.SetValue(currentStation, _package);

                try
                {
                    if (iteration > _stationRepeatLimit) throw new OverflowException("Bus.Go failed - station execution iterations exceeded repeat limit");

                    _package.Trace();
                    _package.Trace($"{stationPrefix} begin");

                    var action = (Station.Action)processMethod.Invoke(currentStation, null);
                    var result = Station.Result.New(stationType, action, Succeeded);

                    _package.Trace($"{stationPrefix} finished - {action.ActionType} ({action.ActionMessage ?? "N/A"})");
                    _package.Results.Enqueue(result);
                }
                catch (Exception exception)
                {
                    var actualException = exception is TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception;
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

        public static Bus<TContent> New()
        {
            return new Bus<TContent>();
        }

        public Bus<TContent> WithAbortOnError() => this.SetAndReturn(nameof(_withAbortOnError), true);
        public Bus<TContent> WithNoAbortOnError() => this.SetAndReturn(nameof(_withAbortOnError), false);
        public Bus<TContent> WithFinalStation<TStation>() => this.SetAndReturn(nameof(_finalStation), typeof(TStation));

        /// <summary>
        /// Register a service with a declared type
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="service"></param>
        /// <returns>Bus&lt;TContent&gt;</returns>
        /// <exception cref="Exception"></exception>
        public Bus<TContent> WithService<TService>(TService service)
        {
            if (service != null && !_services.TryAdd(typeof(TService), service)) throw new Exception("Unable to update services dictionary");

            return this;
        }

        /// <summary>
        /// Register a service using the actual type of the service
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Bus<TContent> WithService(object service)
        {
            if (service != null && !_services.TryAdd(service.GetType(), service)) throw new Exception("Unable to update services dictionary");

            return this;
        }

        /// <summary>
        /// Add multiple services to the bus
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>
        /// Enums will always be registered with their declared type!
        /// </remarks>
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

        /// <summary>
        /// Add multiple services to the bus
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Obsolete("Use WithServices(ServiceRegistrationStrategy, params object[])")]
        public Bus<TContent> WithServices(params object[] services)
        {
            foreach (var service in services)
            {
                var interfaces = service.GetType().GetInterfaces();
                var declaredType = interfaces.Length == 1 ? interfaces[0] : service.GetType();

                if (!_services.TryAdd(declaredType, service)) throw new Exception("Unable to update services dictionary");
            }

            return this;
        }

        public Bus<TContent> WithStation<TStation>()
        {
            _stations.Enqueue(typeof(TStation));

            return this;
        }

        public Bus<TContent> WithStations(params Type[] stationTypes)
        {
            if (stationTypes == null) return this;
            if (stationTypes.Length == 0) return this;

            foreach (var stationType in stationTypes)
            {
                if (!stationType.BaseType?.FullName?.StartsWith("LightPath.Cargo.Station") ?? true) continue;

                _stations.Enqueue(stationType);
            }

            return this;
        }

        public Bus<TContent> WithStationRepeatLimit(int limit) => this.SetAndReturn(nameof(_stationRepeatLimit), limit);

    }
}
