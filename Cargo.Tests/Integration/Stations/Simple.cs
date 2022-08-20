using LightPath.Cargo.Tests.Integration.Common;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Simple
    {
        public class Station1 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 1;
                Contents.String1 = Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station2 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 2;
                Contents.String2 = Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station3 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 3;
                Contents.String3 = Contents.Int1.ToString();
                
                return Station.Action.Next();
            }
        }

        public class Station4 : Station<IContentModel3>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 2;
                Contents.String3 = Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station5 : Station<IContentModel3>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 4;
                Contents.String2 = Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station6 : Station<IContentModel3>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 6;
                Contents.String3 = Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station7 : Station<ContentModel3>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 8;
                Contents.String4 = Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }
    }
}
