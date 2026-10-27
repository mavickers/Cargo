using System;
using System.Linq;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Validation;

namespace LightPath.Cargo.Tests.Integration
{
    public class Validation
    {
        [Fact]
        public void WithStation_AbstractStation_Throws()
        {
            var bus = Bus.New<ContentModel2>();

            Assert.Throws<ArgumentException>(() => bus.WithStation<AbstractStation>());
        }

        [Fact]
        public void WithStation_NoParameterlessConstructor_Throws()
        {
            var bus = Bus.New<ContentModel2>();

            Assert.Throws<ArgumentException>(() => bus.WithStation<NoDefaultConstructorStation>());
        }

        [Fact]
        public void WithStation_NotAStation_Throws()
        {
            var bus = Bus.New<ContentModel2>();

            Assert.Throws<ArgumentException>(() => bus.WithStation<NotAStation>());
        }

        [Fact]
        public void WithFinalStation_MismatchedContentType_Throws()
        {
            var bus = Bus.New<ContentModel2>();

            Assert.Throws<ArgumentException>(() => bus.WithFinalStation<MismatchedStation>());
        }

        [Fact]
        public void WithStations_MisconfiguredStation_Throws()
        {
            var bus = Bus.New<ContentModel2>();

            Assert.Throws<ArgumentException>(() => bus.WithStations(typeof(OkStation), typeof(NoDefaultConstructorStation)));
        }

        [Fact]
        public void ThrowingConstructor_AbortsAndRunsFinalStation()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<ThrowingConstructorStation>()
                         .WithStation<OkStation>()
                         .WithFinalStation<FinalStation>();

            bus.Go(content);

            var failure = bus.Package.Results.Single(r => r.Station == typeof(ThrowingConstructorStation));

            Assert.True(bus.Package.IsErrored);
            Assert.True(bus.Package.IsAborted);
            Assert.True(failure.WasFailure);
            Assert.IsType<InvalidOperationException>(failure.Exception);
            Assert.Equal("ctor", failure.Exception.Message);
            Assert.Equal(20, content.IntVal);
        }

        [Fact]
        public void ThrowingConstructor_WithNoAbortOnError_RunsNextStation()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithNoAbortOnError()
                         .WithStation<ThrowingConstructorStation>()
                         .WithStation<OkStation>()
                         .WithFinalStation<FinalStation>();

            bus.Go(content);

            Assert.True(bus.Package.IsErrored);
            Assert.False(bus.Package.IsAborted);
            Assert.Equal(24, content.IntVal);
        }
    }
}
