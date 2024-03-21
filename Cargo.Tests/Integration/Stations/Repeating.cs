using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Repeating
    {
        public class Station1 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal = 1;

                return Station.Action.Next();
            }
        }

        public class Station2 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 2;

                return Package.Contents.IntVal <= 100 ? Station.Action.Repeat() : Station.Action.Next();
            }
        }

        public class Station3 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 3;

                return Station.Action.Next();
            }
        }

        public class Station4 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 1;

                return Package.Contents.IntVal <= 1000 ? Station.Action.Repeat() : Station.Action.Next();
            }
        }
    }
}
