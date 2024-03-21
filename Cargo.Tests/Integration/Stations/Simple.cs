using System;
using System.Linq;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Simple
    {
        public class Station1 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 1;
                Package.Contents.String1 = Package.Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station2 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 2;
                Package.Contents.String2 = Package.Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station3 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 3;
                Package.Contents.String3 = Package.Contents.Int1.ToString();
                
                return Station.Action.Next();
            }
        }

        public class Station4 : Station<IContentModel3>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 2;
                Package.Contents.String3 = Package.Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station5 : Station<IContentModel3>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 4;
                Package.Contents.String2 = Package.Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station6 : Station<IContentModel3>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 6;
                Package.Contents.String3 = Package.Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station7 : Station<ContentModel3>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 8;
                Package.Contents.String4 = Package.Contents.Int1.ToString();

                return Station.Action.Next();
            }
        }

        public class Station8 : Station<ContentModel4>
        {
            public override Station.Action Process()
            {
                return Station.Action.Next();
            }
        }

        public class Station9 : Station<ContentModel5>
        {
            public override Station.Action Process()
            {
                return Station.Action.Next();
            }
        }

        public class Station10 : Station<ContentModel5>
        {
            public override Station.Action Process()
            {
                throw new NotImplementedException();
            }
        }

        public class Station11 : Station<ContentModel5>
        {
            public override Station.Action Process()
            {
                var message = "this is a test";

                Assert.True(Package.Contents == Contents);
                Assert.True(LastResult.WasFailure);
                Assert.True(IsErrored);
                Assert.True(PackageResults.Count == 1);

                var result = PackageResults.First();

                Assert.True(Package.Messages?.Any() ?? false);
                Assert.True(Messages?.Any() ?? false);
                Assert.True(result.WasFailure);
                Assert.False(result.WasSuccess);
                Assert.NotNull(result.ActionException);
                Assert.True(result.ActionException is NotImplementedException);

                Trace(message);

                Assert.Equal(message, Messages.Last());

                return Station.Action.Next();
            }
        }
    }
}
