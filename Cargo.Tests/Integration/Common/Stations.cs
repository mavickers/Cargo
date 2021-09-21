namespace Cargo.Tests.Integration.Common
{
    public class Stations
    {
        public class Simple
        {
            public class Station1 : Station<ContentModel1>
            {
                public override void Process()
                {
                    Contents.Int1 += 1;
                    Contents.String1 = Contents.Int1.ToString();
                }
            }

            public class Station2 : Station<ContentModel1>
            {
                public override void Process()
                {
                    Contents.Int1 += 2;
                    Contents.String2 = Contents.Int1.ToString();
                }
            }

            public class Station3 : Station<ContentModel1>
            {
                public override void Process()
                {
                    Contents.Int1 += 3;
                    Contents.String3 = Contents.Int1.ToString();
                }
            }
        }
    }
}
