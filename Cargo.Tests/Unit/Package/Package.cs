using LightPath.Cargo.Tests.Unit.Common;
using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace LightPath.Cargo.Tests.Unit.Package
{
    public class Instantiation
    {
        private readonly ITestOutputHelper console;
        public Instantiation(ITestOutputHelper output)
        {
            console = output;
        }

        [Fact]
        public void Basic()
        {
            Assert.Throws<ArgumentException>(() => Cargo.Package.New<ContentModel1>());

            var contents1 = new ContentModel1();
            var package1 = Cargo.Package.New<ContentModel1>(contents1);

            Assert.False(package1.IsAborted);
            Assert.False(package1.IsErrored);
            Assert.Null(package1.LastStationResult);
            Assert.NotNull(package1.Results);
            Assert.Empty(package1.Results);
            Assert.Equal(package1.Contents, contents1);
            Assert.Null(package1.AbortedWith);
            Assert.NotEqual(package1.ExecutionId, Guid.Empty);

            var contents2 = new ContentModel2("testing123");
            var package2 = Cargo.Package.New<ContentModel2>(contents2);

            Assert.Equal("testing123", package2.Contents.String1);
        }
    }
}
