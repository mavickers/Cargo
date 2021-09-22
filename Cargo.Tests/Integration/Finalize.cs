using System.Linq;
using Cargo.Tests.Integration.Common;
using Xunit;
using static Cargo.Tests.Integration.Stations.Finalize;

namespace Cargo.Tests.Integration
{
    public class Finalize
    {
        /// <summary>
        /// A bunch of skipping on the way to finalize
        /// </summary>
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<Station1>()
                         .WithStation<Station2>()
                         .WithStation<Station4>()
                         .WithStation<Station2>()
                         .WithStation<Station4>()
                         .WithStation<Station2>()
                         .WithFinalStation<FinalStation>();

            bus.Go(content);

            Assert.False(bus.Package.IsErrored);
            Assert.False(bus.Package.IsAborted);
            Assert.Equal(29, content.IntVal);
            Assert.Equal(3, bus.Package.Results.Count(r => r.WasSkipped));
        }

        /// <summary>
        /// Abort in the middle of the route
        /// </summary>
        [Fact]
        public void Scenario2()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<Station1>()
                         .WithStation<Station3>()
                         .WithStation<Station2>()
                         .WithStation<Station4>()
                         .WithFinalStation<FinalStation>();



            bus.Go(content);

            Assert.False(bus.Package.IsErrored);
            Assert.True(bus.Package.IsAborted);
            Assert.Equal(21, content.IntVal);
            Assert.Equal(0, bus.Package.Results.Count(r => r.WasSkipped));
            Assert.Equal(1, bus.Package.Results.Count(r => r.WasAborted));
        }

        /// <summary>
        /// Raise an error in the middle of the route
        /// </summary>
        [Fact]
        public void Scenario3()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<Station1>()
                         .WithStation<Station2>()
                         .WithStation<Station4>()
                         .WithStation<Station4>()
                         .WithStation<Station5>()
                         .WithStation<Station4>()
                         .WithStation<Station1>()
                         .WithFinalStation<FinalStation>();

            bus.Go(content);

            Assert.True(bus.Package.IsErrored);
            Assert.False(bus.Package.IsAborted);
            Assert.Equal(29, content.IntVal);

            bus.WithNoAbortOnError().Go(content);

            Assert.True(bus.Package.IsErrored);
            Assert.False(bus.Package.IsAborted);
            Assert.Equal(21, content.IntVal);
        }
    }
}
