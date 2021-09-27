using System.Linq;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Skipping;

namespace LightPath.Cargo.Tests.Integration
{
    public class Skipping
    {
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>()
                .WithStation<Stations.Skipping.Station1>()
                .WithStation<Stations.Skipping.Station2>()
                .WithStation<Stations.Skipping.Station3>();

            bus.Go(content);

            Assert.Equal(4, content.Int1);
            Assert.Equal(1, bus.Package.Results.Count(r => r.WasSkipped));
            Assert.Equal(typeof(Stations.Skipping.Station2), bus.Package.Results.First(r => r.WasSkipped).Station);
        }

        [Fact]
        public void Scenario2()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>()
                .WithStation<Stations.Skipping.Station1>()
                .WithStation<Stations.Skipping.Station2>()
                .WithStation<Stations.Skipping.Station2>()
                .WithStation<Stations.Skipping.Station3>()
                .WithStation<Stations.Skipping.Station2>()
                .WithStation<Stations.Skipping.Station2>()
                .WithStation<Stations.Skipping.Station4>();

            bus.Go(content);

            Assert.Equal(8, content.Int1);
            Assert.Equal(4, bus.Package.Results.Count(r => r.WasSkipped));
        }
    }
}
