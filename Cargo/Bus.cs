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

            var busPackageType = typeof(TContent);
            var currentStationIndex = 0;
            var packageProperty = typeof(Station<TContent>).GetProperty("_package", BindingFlags.NonPublic | BindingFlags.Instance);
            var isRepeatProperty = typeof(Station<TContent>).GetProperty("_repeat", BindingFlags.NonPublic | BindingFlags.Instance);
            var processMethod = typeof(Station<TContent>).GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
            var stationList = _stations.Append(_finalStation).Where(s => s != null).ToList();
            var iteration = 1;
            
            if (packageProperty == null) throw new Exception("Unable to access package property");

            _package = Cargo.Package.New<TContent>(content, _services);
            callback = callback ?? (arg => arg);

            while (currentStationIndex < stationList.Count)
            {
                var stationType = stationList[currentStationIndex];
                var stationPackageType = stationType.BaseType.GenericTypeArguments[0];
                var isRepeat = false;
                var converter = TypeDescriptor.GetConverter(stationPackageType);

                //if (busPackageType != stationPackageType)
                //{
                //    if (busPackageType.IsInterface)
                //    {
                //        if (busPackageType.IsAssignableFrom(stationPackageType))
                //        {
                //        }
                //        else
                //        {
                //            throw new NotImplementedException();
                //        }
                //    }
                //    else
                //    {
                //        throw new NotImplementedException();
                //    }
                //}

                var currentStation = Activator.CreateInstance(stationList[currentStationIndex]);

                if (currentStation == null) throw new Exception($"Unable to instantiate {stationList[currentStationIndex].FullName}");

                var convertedPackage = converter.ConvertFrom(_package);

                packageProperty.SetValue(currentStation, _package);

                try
                {
                    if (iteration > _stationRepeatLimit) throw new OverflowException("Station execution iterations exceeded repeat limit");

                    processMethod.Invoke(currentStation, null);
                    isRepeat = (bool) isRepeatProperty.GetValue(currentStation);
                    
                    //currentStation.Process();

                    var result = Station.Result.New(stationType, Succeeded);

                    _package.Results.Add(result);
                }
                catch (Exception exception)
                {
                    // if we have an abort or skip exception we do not want to 
                    // pass the exception value to the results.

                    var output = Failed;

                    if (exception is Station.AbortException) output = Aborted;
                    if (exception is Station.SkipException) output = Skipped;

                    var result = Station.Result.New(stationType, output, exception);

                    _package.Results.Add(result);
                }

                //iteration = currentStation.IsRepeat ? iteration + 1 : 1;
                iteration = isRepeat ? iteration + 1 : 1;

                // if we are repeating the station then do not change the index and continue.

                if (isRepeat) continue;

                // if we just aborted, or if the station threw an exception and the bus is configured to abort
                // on exception, then set the next run to the final station if it exists; if those conditions 
                // are not met then just continue on to the next station; if those conditions are met but
                // there is no final station configured then set the index to exceed the station count so that
                // no more stations are processed.

                currentStationIndex = _package.LastStationResult.WasAborted || (_package.LastStationResult.WasFail && _withAbortOnError && !isFinalStation)
                    ? stationList.Count + (stationList.Last() == _finalStation ? -1 : 1)
                    : currentStationIndex + 1;
            }

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
