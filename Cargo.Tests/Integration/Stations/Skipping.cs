using Cargo.Tests.Integration.Common;

namespace Cargo.Tests.Integration.Stations
{
    public class Skipping
    {
        public class Station1 : Station<ContentModel1>
        {
            public override void Process()
            {
                Contents.Int1 += 1;
            }
        }

        public class Station2 : Station<ContentModel1>
        {
            public override void Process()
            {
                Skip();

                Contents.Int1 += 2;
            }
        }

        public class Station3 : Station<ContentModel1>
        {
            public override void Process()
            {
                Contents.Int1 += 3;
            }
        }

        public class Station4 : Station<ContentModel1>
        {
            public override void Process()
            {
                Contents.Int1 += 4;
            }
        }
    }
}
