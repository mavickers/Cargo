using System.Threading.Tasks;
using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class RepeatLimit
    {
        public class AsyncLimit2Station : StationAsync<ContentModel2>
        {
            protected override int? RepeatLimit => 2;

            public override Task<Station.Action> ProcessAsync()
            {
                Package.Contents.IntVal++;

                return Task.FromResult(Station.Action.Repeat());
            }
        }

        public class Limit3Station : Station<ContentModel2>
        {
            protected override int? RepeatLimit => 3;

            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Station.Action.Repeat();
            }
        }

        public class Limit5Station : Station<ContentModel2>
        {
            protected override int? RepeatLimit => 5;

            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Station.Action.Repeat();
            }
        }

        public class NoOverrideStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal++;

                return Station.Action.Repeat();
            }
        }
    }
}
