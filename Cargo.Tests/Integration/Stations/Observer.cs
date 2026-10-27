using System;
using System.Collections.Generic;
using System.Threading;
using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Observer
    {
        public class FinalStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 20;

                return Station.Action.Next();
            }
        }

        public class AbortStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Abort();
            }
        }

        public class Limit2Station : Station<ContentModel2>
        {
            protected override int? RepeatLimit => 2;

            public override Station.Action Process()
            {
                return Station.Action.Repeat();
            }
        }

        public class RecordingObserver : IStationObserver
        {
            public List<Station.Result> Finished { get; } = new List<Station.Result>();
            public List<(Type Station, int Iteration)> Starts { get; } = new List<(Type Station, int Iteration)>();
            public Exception ThrowOnFinished { get; set; }
            public Exception ThrowOnStarting { get; set; }
            public int StartingSleepMilliseconds { get; set; }

            void IStationObserver.Finished(Station.Result result)
            {
                Finished.Add(result);

                if (ThrowOnFinished != null) throw ThrowOnFinished;
            }

            void IStationObserver.Starting(Type station, int iteration)
            {
                Starts.Add((station, iteration));

                if (StartingSleepMilliseconds > 0) Thread.Sleep(StartingSleepMilliseconds);

                if (ThrowOnStarting != null) throw ThrowOnStarting;
            }
        }

        public class Station1 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Thread.Sleep(50);

                return Station.Action.Next();
            }
        }

        public class Station2 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Package.Contents.IntVal < 2 ? Station.Action.Repeat() : Station.Action.Next();
            }
        }

        public class Station3 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Next();
            }
        }
    }
}
