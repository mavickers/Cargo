using System;
using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Delay
    {
        public class FinalStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 100;

                return Station.Action.Next();
            }
        }

        public class FinalRepeatDelayStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Station.Action.Repeat().WithDelay(TimeSpan.FromMilliseconds(500));
            }
        }

        public class FinalRepeatStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Station.Action.Repeat();
            }
        }

        public class OutOfRangeDelayStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Repeat().WithDelay(TimeSpan.FromDays(100));
            }
        }

        public class LongDelayStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Repeat().WithDelay(TimeSpan.FromSeconds(2));
            }
        }

        public class NextWithDelayStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Next().WithDelay(TimeSpan.FromMilliseconds(500));
            }
        }

        public class RepeatWithDelayStation : Station<ContentModel2>
        {
            public static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(150);

            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Package.Contents.IntVal <= 2 ? Station.Action.Repeat().WithDelay(Delay) : Station.Action.Next();
            }
        }
    }
}
