using LightPath.Cargo.Tests.Integration.Common;
using LightPath.Cargo.Tests.Integration.Stations;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Simple;

namespace LightPath.Cargo.Tests.Integration
{
    public class SimpleTests
    {
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>()
                               .WithStation<Simple.Station1>()
                               .WithStation<Simple.Station2>()
                               .WithStation<Simple.Station3>();

            bus.Go(content);

            Assert.Equal(6, content.Int1);
            Assert.Equal("1", content.String1);
            Assert.Equal("3", content.String2);
            Assert.Equal("6", content.String3);
            Assert.Equal(3, bus.Package.Results.Count);

            bus.Go(content);

            Assert.Equal(12, content.Int1);
            Assert.Equal("7", content.String1);
            Assert.Equal("9", content.String2);
            Assert.Equal("12", content.String3);
            Assert.Equal(3, bus.Package.Results.Count);

            Assert.False(bus.Package.IsAborted);
            Assert.False(bus.Package.IsErrored);
        }
    }
}
