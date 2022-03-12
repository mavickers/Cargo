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

        /// <summary>
        /// Scenario2 repeats Scenario1 but uses an interface for the bus
        /// package type and implementations of the interface as station
        /// package types.
        /// </summary>
        [Fact]
        public void Scenario2()
        {
            var content = new ContentModel3();
            var bus = Bus.New<IContentModel3>()
                .WithStation<Simple.Station4>()
                .WithStation<Simple.Station5>()
                .WithStation<Simple.Station6>()
                .WithStation<Simple.Station7>();

            bus.Go(content);
        }

        /// <summary>
        /// Scenario2 repeats Scenario1 but uses an interface for the bus
        /// package type and implementations of the interface as station
        /// package types.
        /// </summary>
        [Fact]
        public void Scenario3()
        {
            var content = new ContentModel3();
            var bus = Bus.New<ContentModel3>()
                .WithStation<Simple.Station4>()
                .WithStation<Simple.Station5>()
                .WithStation<Simple.Station6>()
                .WithStation<Simple.Station7>();

            bus.Go(content);
        }
    }
}
