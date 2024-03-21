using System;
using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Finalize
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
                return Station.Action.Next();
            }
        }

        public class Station3 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Abort();
            }
        }

        public class Station4 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 4;

                return Station.Action.Next();
            }
        }

        public class Station5 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                throw new Exception("Testing");
            }
        }

        public class FinalStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 20;

                return Station.Action.Next();
            }
        }

        /// <summary>
        /// Purposefully throw an exception in the final station
        /// https://github.com/mavickers/Cargo/issues/5
        /// </summary>
        public class FinalStationCrasher : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                throw new NotImplementedException();
            }
        }
    }
}
