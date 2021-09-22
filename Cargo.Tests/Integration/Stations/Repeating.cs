using Cargo.Tests.Integration.Common;

namespace Cargo.Tests.Integration.Stations
{
    public class Repeating
    {
        public class Station1 : Station<ContentModel2>
        {
            public override void Process()
            {
                Contents.IntVal = 1;
            }
        }

        public class Station2 : Station<ContentModel2>
        {
            public override void Process()
            {
                Contents.IntVal += 2;

                if (Contents.IntVal <= 100) Repeat();
            }
        }

        public class Station3 : Station<ContentModel2>
        {
            public override void Process()
            {
                Contents.IntVal += 3;
            }
        }

        public class Station4 : Station<ContentModel2>
        {
            public override void Process()
            {
                Contents.IntVal += 1;

                if (Contents.IntVal <= 1000) Repeat();
            }
        }
    }
}
