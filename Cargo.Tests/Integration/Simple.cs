using LightPath.Cargo.Tests.Integration.Common;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static LightPath.Cargo.Tests.Integration.Stations.Simple;

namespace LightPath.Cargo.Tests.Integration
{
    public class SimpleTests
    {
        private readonly ITestOutputHelper _outputHelper;
        public SimpleTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void Scenario1A()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>()
                         .WithStation<Station1>()
                         .WithStation<Station2>()
                         .WithStation<Station3>();

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

            // make sure the trace messages are being capture

            Assert.EndsWith("begin trace", bus.Package.Messages.First());
            Assert.EndsWith("end trace", bus.Package.Messages.Last());
        }

        [Fact]
        public void Scenario1B()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>().WithStations(typeof(Station1), typeof(Station2), typeof(Station3));

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

        [Fact]
        public void Scenario1C()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>().WithStations(typeof(string), typeof(Station1), typeof(Station2), typeof(Station3));

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
                         .WithStation<Station4>()
                         .WithStation<Station5>()
                         .WithStation<Station6>()
                         .WithStation<Station7>();

            Assert.Throws<System.Reflection.TargetException>(() => bus.Go(content));
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
                         .WithStation<Station4>()
                         .WithStation<Station5>()
                         .WithStation<Station6>()
                         .WithStation<Station7>();

            // maybe someday we can mix interfaces/implementation in the stations
            Assert.Throws<System.Reflection.TargetException>(() => bus.Go(content));
        }

        [Fact]
        public void Scenario4()
        {
            var content4 = new ContentModel4();
            var content5 = new ContentModel5();
            var busA = Bus.New<ContentModel4>().WithStation<Station8>();
            var busB = Bus.New<ContentModel5>().WithStation<Station9>();
            
            busA.Go(content4);
            busB.Go(content5);
        }
    }
}
