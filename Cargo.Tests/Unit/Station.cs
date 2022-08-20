using System;
using LightPath.Cargo.Tests.Unit.Common;
using Xunit;
using static LightPath.Cargo.Station;

namespace LightPath.Cargo.Tests.Unit
{
    public class Station
    {
        class TestStation1 : Station<ContentModel1>
        {
            public override Cargo.Station.Action Process()
            {
                throw new NotImplementedException();
            }
        }

        class TestStation2 : Station<ContentModel1>
        {
            public override Cargo.Station.Action Process()
            {
                return Cargo.Station.Action.Next();
            }
        }

        class TestStation3 : Station<ContentModel1>
        {
            public override Cargo.Station.Action Process()
            {
                return Cargo.Station.Action.Repeat();
            }
        }

        [Fact]
        public void Instantiation()
        {
            var station1 = new TestStation1();
            var result = Result.New(station1.GetType(), Cargo.Station.Action.Next(), Output.Succeeded);

            Assert.Null(result.Exception);
            Assert.True(result.WasSuccess);

            result = Result.New(station1.GetType(), Cargo.Station.Action.Abort(), Output.Failed, new Exception("testing"));

            Assert.NotNull(result.Exception);
            Assert.True(result.IsAborting);
            Assert.Equal("testing", result.Exception.Message);

            Assert.Throws<NotImplementedException>(() => station1.Process());
        }

        [Fact]
        public void Operations()
        {
            var station2 = new TestStation2();
            var station3 = new TestStation3();

            Assert.Equal(Cargo.Station.Action.ActionTypes.Next, station2.Process().ActionType);
            Assert.Equal(Cargo.Station.Action.ActionTypes.Repeat, station3.Process().ActionType);
        }
    }
}
