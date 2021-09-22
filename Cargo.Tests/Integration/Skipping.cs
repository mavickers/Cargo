using Cargo.Tests.Integration.Common;
using System.Linq;
using Xunit;
using static Cargo.Tests.Integration.Stations.Skipping;

namespace Cargo.Tests.Integration
{
    public class Skipping
    {
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>()
                .WithStation<Station1>()
                .WithStation<Station2>()
                .WithStation<Station3>();

            bus.Go(content);

            Assert.Equal(4, content.Int1);
            Assert.Equal(1, bus.Package.Results.Count(r => r.WasSkipped));
            Assert.Equal(typeof(Station2), bus.Package.Results.First(r => r.WasSkipped).Station);
        }

        [Fact]
        public void Scenario2()
        {
            var content = new ContentModel1();
            var bus = Bus.New<ContentModel1>()
                .WithStation<Station1>()
                .WithStation<Station2>()
                .WithStation<Station2>()
                .WithStation<Station3>()
                .WithStation<Station2>()
                .WithStation<Station2>()
                .WithStation<Station4>();

            bus.Go(content);

            Assert.Equal(8, content.Int1);
            Assert.Equal(4, bus.Package.Results.Count(r => r.WasSkipped));
        }
    }
}
