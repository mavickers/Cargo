using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Services;

namespace LightPath.Cargo.Tests.Integration
{
    public class Services
    {
        [Fact]
        public void Scenario1A()
        {
            var content = new ContentModel1();
            var implementation = new Implementation1();
            var bus = Bus.New<ContentModel1>()
                         .WithService<Interface1>(implementation)
                         .WithStation<Station1>()
                         .WithStation<Station2>()
                         .WithStation<Station2>()
                         .WithStation<Station3A>()
                         .WithStation<Station4A>();

            bus.Go(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(11, content.Int1);
        }

        [Fact]
        public void Scenario1B()
        {
            var content1 = new ContentModel1();
            var content2 = new ContentModel1();
            var implementation1 = (Interface1) new Implementation1();
            var implementation2 = new Implementation2();
            var implementation3 = new Implementation3();
            var bus1 = Bus.New<ContentModel1>()
                          .WithServices(implementation1, implementation2, implementation3, NumberedTypes.Third)
                          .WithStation<Station1>()
                          .WithStation<Station2>()
                          .WithStation<Station2>()
                          .WithStation<Station3A>()
                          .WithStation<Station3B>()
                          .WithStation<Station4B>()
                          .WithStation<Station5>();
            var bus2 = Bus.New<ContentModel1>()
                          .WithServices(Strategies.ServiceRegistrationStrategy.AsFirstInterfacePreferred, implementation1, implementation2, implementation3)
                          .WithServices(Strategies.ServiceRegistrationStrategy.AsFirstInterfacePreferred, NumberedTypes.Third, UnNumberedTypes.First)
                          .WithStation<Station1>()
                          .WithStation<Station2>()
                          .WithStation<Station2>()
                          .WithStation<Station3A>()
                          .WithStation<Station3B>()
                          .WithStation<Station4B>()
                          .WithStation<Station5>()
                          .WithStation<Station6>();

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
            Assert.True(station1.TryGetService<NumberedTypes>(out var typeResult));
            Assert.Equal(actual: typeResult, expected: NumberedTypes.None);
        }
    }
}
