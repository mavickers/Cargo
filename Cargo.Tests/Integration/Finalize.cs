using System.Linq;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Finalize;

namespace LightPath.Cargo.Tests.Integration
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
                         .WithStation<Stations.Finalize.Station1>()
                         .WithStation<Stations.Finalize.Station2>()
                         .WithStation<Stations.Finalize.Station4>()
                         .WithStation<Stations.Finalize.Station2>()
                         .WithStation<Stations.Finalize.Station4>()
                         .WithStation<Stations.Finalize.Station2>()
                         .WithFinalStation<Stations.Finalize.FinalStation>();

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
                         .WithStation<Stations.Finalize.Station1>()
                         .WithStation<Stations.Finalize.Station3>()
                         .WithStation<Stations.Finalize.Station2>()
                         .WithStation<Stations.Finalize.Station4>()
                         .WithFinalStation<Stations.Finalize.FinalStation>();



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
                         .WithStation<Stations.Finalize.Station1>()
                         .WithStation<Stations.Finalize.Station2>()
                         .WithStation<Stations.Finalize.Station4>()
                         .WithStation<Stations.Finalize.Station4>()
                         .WithStation<Stations.Finalize.Station5>()
                         .WithStation<Stations.Finalize.Station4>()
                         .WithStation<Stations.Finalize.Station1>()
                         .WithFinalStation<Stations.Finalize.FinalStation>();

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
