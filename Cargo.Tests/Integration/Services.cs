using System;
using LightPath.Cargo.Tests.Integration.Common;
using Moq;
using Xunit;

namespace LightPath.Cargo.Tests.Integration
{
    public class Services
    {
        [Fact]
        public void Scenario1A()
        {
            var content = new ContentModel1();
            var implementation = new Stations.Services.Implementation1();
            var bus = Bus.New<ContentModel1>()
                         .WithService<Stations.Services.Interface1>(implementation)
                         .WithStation<Stations.Services.Station1>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station3A>()
                         .WithStation<Stations.Services.Station4A>();

            bus.Go(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(11, content.Int1);
        }

        [Fact]
        public void Scenario1B()
        {
            var content = new ContentModel1();
            var implementation1 = (Stations.Services.Interface1) new Stations.Services.Implementation1();
            var implementation2 = new Stations.Services.Implementation2();
            var implementation3 = new Stations.Services.Implementation3();
            var bus = Bus.New<ContentModel1>()
                         .WithServices(implementation1, implementation2, implementation3)
                         .WithStation<Stations.Services.Station1>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station3A>()
                         .WithStation<Stations.Services.Station3B>()
                         .WithStation<Stations.Services.Station4B>();

            bus.Go(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(16, content.Int1);
        }

        [Fact]
        public void TryGetService()
        {
            var station = new Stations.Simple.Station1();

            Assert.False(station.TryGetService<string>(out _));
        }
    }
}
