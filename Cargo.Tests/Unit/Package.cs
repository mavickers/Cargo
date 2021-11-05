using System;
using LightPath.Cargo.Tests.Unit.Common;
using Xunit;

namespace LightPath.Cargo.Tests.Unit
{
    public class Package
    {
        [Fact]
        public void Instantiation()
        {
            Package<ContentModel1> package;

            var contents = new ContentModel1();

            Assert.Throws<ArgumentException>(() => package = Cargo.Package.New<ContentModel1>());

            package = Cargo.Package.New<ContentModel1>(contents);

            Assert.False(package.IsAborted);
            Assert.False(package.IsErrored);
            Assert.Null(package.LastStationResult);
            Assert.NotNull(package.Results);
            Assert.Empty(package.Results);
            Assert.Equal(package.Contents, contents);
            Assert.Null(package.AbortedWith);
        }
    }
}
