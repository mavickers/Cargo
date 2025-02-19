using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Services.Types;

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
            var content1 = new ContentModel1();
            var content2 = new ContentModel1();
            var implementation1 = (Stations.Services.Interface1) new Stations.Services.Implementation1();
            var implementation2 = new Stations.Services.Implementation2();
            var implementation3 = new Stations.Services.Implementation3();
            var bus1 = Bus.New<ContentModel1>()
                         .WithServices(implementation1, implementation2, implementation3, Third)
                         .WithStation<Stations.Services.Station1>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station3A>()
                         .WithStation<Stations.Services.Station3B>()
                         .WithStation<Stations.Services.Station4B>()
                         .WithStation<Stations.Services.Station5>();
            var bus2 = Bus.New<ContentModel1>()
                         .WithServices(Strategies.ServiceRegistrationStrategy.AsFirstInterfacePreferred, implementation1, implementation2, implementation3, Third)
                         .WithStation<Stations.Services.Station1>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station3A>()
                         .WithStation<Stations.Services.Station3B>()
                         .WithStation<Stations.Services.Station4B>()
                         .WithStation<Stations.Services.Station5>();

            bus1.Go(content1);
            bus2.Go(content2);

            Assert.False(bus1.Package.IsErrored);
            Assert.Equal(16, content1.Int1);

            Assert.False(bus2.Package.IsErrored);
            Assert.Equal(16, content2.Int1);
        }

        [Fact]
        public void TryGetService()
        {
            var station1 = new Stations.Simple.Station1();

            Assert.False(station1.TryGetService<string>(out _));
            Assert.True(station1.TryGetService<Stations.Services.Types>(out var typeResult));
            Assert.Equal(actual: typeResult, expected: None);
        }
    }
}
