using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using static LightPath.Cargo.Station.Output;

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
        private List<Type> _stations { get; } = new List<Type>();
        private Dictionary<Type, object> _services { get; } = new Dictionary<Type, object>();
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
                    if (iteration > _stationRepeatLimit) throw new OverflowException("Bug.Go failed - station execution iterations exceeded repeat limit");

                    _package.Trace();
                    _package.Trace($"{stationPrefix} begin");

                    var action = (Station.Action)processMethod.Invoke(currentStation, null);
                    var result = Station.Result.New(stationType, action, Succeeded);

                    _package.Trace($"{stationPrefix} finished - {action.ActionType} ({action.ActionMessage ?? "N/A"})");
                    _package.Results.Add(result);
                }
                catch (Exception exception)
                {
                    var action = _withAbortOnError ? Station.Action.Abort(exception) : Station.Action.Next(exception);
                    var result = Station.Result.New(stationType, action, Failed, exception);

                    _package.Trace($"{stationPrefix} finished - {action.ActionType} ({action.ActionMessage ?? "N/A"})");
                    _package.Results.Add(result);
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

                currentStationIndex += isRepeating ? 0 : gotoFinalStation ? stationList.Count - 1 : isAborting ? stationList.Count + 1 : 1;
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

        public Bus<TContent> WithService<TService>(TService service)
        {
            if (service != null) _services.Add(typeof(TService), service);

            return this;
        }

        public Bus<TContent> WithStation<TStation>()
        {
            _stations.Add(typeof(TStation));

            return this;
        }

        public Bus<TContent> WithStationRepeatLimit(int limit) => this.SetAndReturn(nameof(_stationRepeatLimit), limit);

    }
}
