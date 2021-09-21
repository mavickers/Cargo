using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Cargo.Station.Output;

namespace Cargo
{
    public class Bus
    {
        public static Bus<T> New<T>() where T : new()
        {
            return Bus<T>.New();
        }
    }

    public class Bus<T> where T : new()
    {
        private Type _finalStation;
        private Package<T> _package;
        private readonly List<Type> _stations = new List<Type>();

        public Package<T> Package => _package;

        public T Go(T content, ILogger<T> logger = null, Func<T, T> callback = null)
        {
            if (content == null) throw new ArgumentException("\"content\" parameter is null");

            var currentStationIndex = 0;
            var packageProperty = typeof(Station<T>).GetProperty("_package", BindingFlags.NonPublic | BindingFlags.Instance);
            var stationList = _stations.Append(_finalStation).Where(s => s != null).ToList();
            
            if (packageProperty == null) throw new Exception("Unable to access package property");

            _package = new Package<T>(content, logger);
            callback = callback ?? (arg => arg);

            while (currentStationIndex < stationList.Count)
            {
                // if the package indicates that we're aborted and we don't
                // have a final station, break out of the loop; setting the 
                // current station to the final station is handled at the bottom
                // of the loop.

                //if (_package.IsAborted && stationList.Last() != _finalStation) break;

                var currentStation = (Station<T>) Activator.CreateInstance(stationList[currentStationIndex]);
                
                if (currentStation == null) throw new Exception($"Unable to instantiate {stationList[currentStationIndex].FullName}");

                packageProperty.SetValue(currentStation, _package);

                try
                {
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

                // if we are repeating the station then do not change the index and continue.
                if (currentStation.IsRepeat) continue;

                // if we just aborted and we have a final station, set the next run to the final
                // station; otherwise set the index outside the loop so no more stations are processed.
                if (_package.LastStationResult.WasAborted) { currentStationIndex = stationList.Count + (stationList.Last() == _finalStation ? -1 : 1); continue; }
                
                // just process the next station; if we've arrived at the last or final
                // station we'll bail out of the loop.
                currentStationIndex++;
            }

            return callback(content);
        }

        public static Bus<T> New()
        {
            return new Bus<T>();
        }

        public Bus<T> WithFinalStation(Type station)
        {
            if (station?.BaseType?.FullName != typeof(Station<T>).FullName) throw new ArgumentException("\"station\" parameter is invalid");

            _finalStation = station;

            return this;
        }

        public Bus<T> WithStation(Type station)
        {
            if (station?.BaseType?.FullName != typeof(Station<T>).FullName) throw new ArgumentException("\"station\" parameter is invalid");

            _stations.Add(station);

            return this;
        }

        public Bus<T> WithStations(params Type[] stations)
        {
            foreach (var station in stations)
            {
                if (station?.BaseType?.FullName != typeof(Station<T>).FullName) throw new ArgumentException("\"stations\" parameter contained invalid class");

                _stations.Add(station);
            }

            return this;
        }
    }
}
