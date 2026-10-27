using System;
using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Validation
    {
        public abstract class AbstractStation : Station<ContentModel2>
        {
        }

        public class FinalStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 20;

                return Station.Action.Next();
            }
        }

        public class MismatchedStation : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                return Station.Action.Next();
            }
        }

        public class NoDefaultConstructorStation : Station<ContentModel2>
        {
            public NoDefaultConstructorStation(int value)
            {
            }

            public override Station.Action Process()
            {
                return Station.Action.Next();
            }
        }

        public class NotAStation
        {
        }

        public class OkStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 4;

                return Station.Action.Next();
            }
        }

        public class ThrowingConstructorStation : Station<ContentModel2>
        {
            public ThrowingConstructorStation()
            {
                throw new InvalidOperationException("ctor");
            }

            public override Station.Action Process()
            {
                return Station.Action.Next();
            }
        }
    }
}
