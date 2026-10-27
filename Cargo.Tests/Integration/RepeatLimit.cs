using System;
using System.Linq;
using System.Threading.Tasks;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.RepeatLimit;

namespace LightPath.Cargo.Tests.Integration
{
    public class RepeatLimit
    {
        private static void AssertOverflowed(Package<ContentModel2> package)
        {
            Assert.True(package.IsAborted);
            Assert.IsType<OverflowException>(package.Results.Last(r => r.WasFailure).Exception);
        }

        [Fact]
        public void StationOverride_AppliesLimit()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStation<Limit3Station>();

            bus.Go(content);

            Assert.Equal(3, content.IntVal);
            AssertOverflowed(bus.Package);
        }

        [Fact]
        public void StationOverride_ReplacesBusWideLimit()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStationRepeatLimit(2).WithStation<Limit5Station>();

            bus.Go(content);

            Assert.Equal(5, content.IntVal);
            AssertOverflowed(bus.Package);
        }

        [Fact]
        public void StationWithoutOverride_UsesBusWideLimit()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStationRepeatLimit(2).WithStation<NoOverrideStation>();

            bus.Go(content);

            Assert.Equal(2, content.IntVal);
            AssertOverflowed(bus.Package);
        }

        [Fact]
        public async Task AsyncStationOverride_AppliesLimit()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStationRepeatLimit(10).WithStation<AsyncLimit2Station>();

            await bus.GoAsync(content);

            Assert.Equal(2, content.IntVal);
            AssertOverflowed(bus.Package);
        }
    }
}
