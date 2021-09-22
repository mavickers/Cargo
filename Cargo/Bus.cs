using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Cargo.Station.Output;

namespace Cargo
{
    public static class Bus
    {
        public static Bus<TContent> New<TContent>() where TContent : new()
        {
            return Bus<TContent>.New();
        }

        public static Bus<TContent> SetAndReturn<TContent>(this Bus<TContent> bus, string propertyName, object value) where TContent : new()
        {
            var property = typeof(Bus<TContent>).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null) throw new Exception($"Cound not find property named '{propertyName}'");

            property.SetValue(bus, value);

            return bus;
        }
    }

    public class Bus<TContent> where TContent : new()
    {
        private Type _finalStation { get; set; }
        private Package<TContent> _package { get; set; }
        private int _stationRepeatLimit { get; set; } = 100;
        private List<Type> _stations { get; } = new List<Type>();
        private bool _withAbortOnError { get; set; } = true;

        public Package<TContent> Package => _package;

        private Bus() { }

        public TContent Go(TContent content, ILogger<TContent> logger = null, Func<TContent, TContent> callback = null)
        {
            if (content == null) throw new ArgumentException("\"content\" parameter is null");

            var currentStationIndex = 0;
            var packageProperty = typeof(Station<TContent>).GetProperty("_package", BindingFlags.NonPublic | BindingFlags.Instance);
            var stationList = _stations.Append(_finalStation).Where(s => s != null).ToList();
            var iteration = 1;
            
            if (packageProperty == null) throw new Exception("Unable to access package property");

            _package = Cargo.Package.New<TContent>(content, logger);
            callback = callback ?? (arg => arg);

            while (currentStationIndex < stationList.Count)
            {
                var currentStation = (Station<TContent>) Activator.CreateInstance(stationList[currentStationIndex]);
                
                if (currentStation == null) throw new Exception($"Unable to instantiate {stationList[currentStationIndex].FullName}");

                packageProperty.SetValue(currentStation, _package);

                try
                {
                    if (iteration > _stationRepeatLimit) throw new OverflowException("Station execution iterations exceeded repeat limit");
                    
                    currentStation.Process();

                    var result = Station.Result.New(currentStation, Succeeded);

                    _package.Results.Add(result);
                }
                catch (Exception exception)
                {
                    // if we have an abort or skip exception we do not want to 
                    // pass the exception value to the results.

                    var output = Failed;

                    if (exception is Station.AbortException) output = Aborted;
                    if (exception is Station.SkipException) output = Skipped;

                    var result = Station.Result.New(currentStation, output, exception);

                    _package.Results.Add(result);
                }

                iteration = currentStation.IsRepeat ? iteration + 1 : 1;

                // if we are repeating the station then do not change the index and continue.
                if (currentStation.IsRepeat) continue;

                // if we just aborted and we have a final station, or if the station threw an exception
                // and the bus is configured to abort on exception then set the next run to the final
                // station; otherwise set the index outside the loop so no more stations are processed.
                if (_package.LastStationResult.WasAborted || (_package.LastStationResult.WasFail && _withAbortOnError))
                {
                    currentStationIndex = stationList.Count + (stationList.Last() == _finalStation ? -1 : 1); 
                    continue;
                }
                
                // just process the next station; if we've arrived at the last or final
                // station we'll bail out of the loop.
                currentStationIndex++;
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

        public Bus<TContent> WithStation<TStation>() where TStation : new()
        {
            _stations.Add(typeof(TStation));

            return this;
        }

        public Bus<TContent> WithStationRepeatLimit(int limit) => this.SetAndReturn(nameof(_stationRepeatLimit), limit);

    }
}
