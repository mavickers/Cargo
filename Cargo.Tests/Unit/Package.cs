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
            Package<ContentModel1> package1;
            Package<ContentModel2> package2;

            var contents1 = new ContentModel1();

            Assert.Throws<ArgumentException>(() => package1 = Cargo.Package.New<ContentModel1>());

            package1 = Cargo.Package.New<ContentModel1>(contents1);

            Assert.False(package1.IsAborted);
            Assert.False(package1.IsErrored);
            Assert.Null(package1.LastStationResult);
            Assert.NotNull(package1.Results);
            Assert.Empty(package1.Results);
            Assert.Equal(package1.Contents, contents1);
            Assert.Null(package1.AbortedWith);

            var contents2 = new ContentModel2("testing123");

            package2 = Cargo.Package.New<ContentModel2>(contents2);

            Assert.Equal("testing123", package2.Contents.String1);
        }
    }
}
