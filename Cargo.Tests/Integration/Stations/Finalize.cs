using System;

namespace Cargo.Tests.Integration.Stations
{
    public class Finalize
    {
        public class Station1 : Station<ContentModel>
        {
            public override void Process()
            {
                Contents.IntVal = 1;
            }
        }

        public class Station2 : Station<ContentModel>
        {
            public override void Process()
            {
                Skip();
            }
        }

        public class Station3 : Station<ContentModel>
        {
            public override void Process()
            {
                Abort();
            }
        }

        public class Station4 : Station<ContentModel>
        {
            public override void Process()
            {
                Contents.IntVal += 4;
            }
        }

        public class Station5 : Station<ContentModel>
        {
            public override void Process()
            {
                throw new Exception("Testing");
            }
        }

        public class FinalStation : Station<ContentModel>
        {
            public override void Process()
            {
                Contents.IntVal += 20;
            }
        }
    }
}
