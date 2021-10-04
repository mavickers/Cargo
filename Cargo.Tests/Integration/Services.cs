using LightPath.Cargo.Tests.Integration.Common;
using Xunit;

namespace LightPath.Cargo.Tests.Integration
{
    public class Services
    {
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel1();
            var implementation = new Stations.Services.Implementation1();
            var bus = Bus.New<ContentModel1>()
                         .WithService<Stations.Services.Interface1>(implementation)
                         .WithStation<Stations.Services.Station1>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station2>()
                         .WithStation<Stations.Services.Station3>();

            bus.Go(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(8, content.Int1);
        }
    }
}
