using Cargo.Tests.Unit.Common;
using System;
using Xunit;

namespace Cargo.Tests.Unit
{
    public class Station
    {
        class TestStation1 : Station<ContentModel1>
        {
            public override void Process()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Instantiation()
        {
            var station = new TestStation1();
            var result = Cargo.Station.Result.New(station, Cargo.Station.Output.Skipped);

            Assert.False(station.IsRepeat);
            Assert.True(station.NotRepeat);
            Assert.Null(result.Exception);
            Assert.True(result.WasSkipped);

            result = Cargo.Station.Result.New(station, Cargo.Station.Output.Aborted, new Exception("testing"));

            Assert.NotNull(result.Exception);
            Assert.True(result.WasAborted);
            Assert.Equal("testing", result.Exception.Message);

            Assert.Throws<NullReferenceException>(() => station.Abort());
            Assert.Throws<NotImplementedException>(() => station.Process());
        }

        [Fact]
        public void Operations()
        {
            var station = new TestStation1();

            Assert.Throws<Cargo.Station.SkipException>(() => station.Skip());
            
            station.Repeat();

            Assert.True(station.IsRepeat);

            station.NoRepeat();

            Assert.False(station.IsRepeat);
        }
    }
}
