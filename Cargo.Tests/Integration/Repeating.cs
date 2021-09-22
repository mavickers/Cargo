using Cargo.Tests.Integration.Common;
using Xunit;
using static Cargo.Tests.Integration.Stations.Repeating;

namespace Cargo.Tests.Integration
{
    public class Repeating
    {
        /// <summary>
        /// Simple station repeat until a condition is met
        /// </summary>
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<Station1>()
                         .WithStation<Station2>()
                         .WithStation<Station3>();

            bus.Go(content);

            Assert.False(bus.Package.IsAborted);
            Assert.False(bus.Package.IsErrored);
            Assert.Equal(104, content.IntVal);
        }

        /// <summary>
        /// Station repeat until station iterations exceed limit
        /// </summary>
        [Fact]
        public void Scenario2()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                .WithStation<Station1>()
                .WithStation<Station4>()
                .WithStation<Station3>();

            bus.Go(content);

            Assert.False(bus.Package.IsAborted);
            Assert.True(bus.Package.IsErrored);
            Assert.Equal(101, content.IntVal);
        }

        /// <summary>
        /// Multiple station iteration - total iterations will
        /// exceed threshold for the combined stations, but individually
        /// will not.
        /// </summary>
        [Fact]
        public void Scenario3()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                .WithStation<Station1>()
                .WithStation<Station2>()
                .WithStation<Station1>()
                .WithStation<Station2>()
                .WithStation<Station3>();

            bus.Go(content);

            Assert.False(bus.Package.IsAborted);
            Assert.False(bus.Package.IsErrored);
            Assert.Equal(104, content.IntVal);
        }
    }
}
