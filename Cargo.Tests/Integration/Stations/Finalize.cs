using System;
using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Finalize
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
                Skip();
            }
        }

        public class Station3 : Station<ContentModel2>
        {
            public override void Process()
            {
                Abort();
            }
        }

        public class Station4 : Station<ContentModel2>
        {
            public override void Process()
            {
                Contents.IntVal += 4;
            }
        }

        public class Station5 : Station<ContentModel2>
        {
            public override void Process()
            {
                throw new Exception("Testing");
            }
        }

        public class FinalStation : Station<ContentModel2>
        {
            public override void Process()
            {
                Contents.IntVal += 20;
            }
        }

        /// <summary>
        /// Purposefully throw an exception in the final station
        /// https://github.com/mavickers/Cargo/issues/5
        /// </summary>
        public class FinalStationCrasher : Station<ContentModel2>
        {
            public override void Process()
            {
                throw new NotImplementedException();
            }
        }
    }
}
