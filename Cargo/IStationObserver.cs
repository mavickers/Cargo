using System;

namespace LightPath.Cargo
{
    /// <summary>
    /// Receives live notifications as the bus runs each station. Calls happen outside
    /// the station's error handling, so an exception thrown here escapes Go/GoAsync
    /// untouched; it is never recorded as a station failure.
    /// </summary>
    public interface IStationObserver
    {
        /// <summary>Fires before a station runs; <paramref name="iteration"/> starts at 1 and counts repeats.</summary>
        void Starting(Type station, int iteration);

        /// <summary>Fires after each result is recorded, including the synthetic results for cancellation and repeat overflow.</summary>
        void Finished(Station.Result result);
    }
}
