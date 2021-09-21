using Microsoft.Extensions.Logging;
using System;
using Cargo.Tests.Unit.Common;
using Xunit;

namespace Cargo.Tests.Unit
{
    public class Package
    {
        [Fact]
        public void Instantiation()
        {
            Package<ContentModel1> package;

            var contents = new ContentModel1();

            Assert.Throws<ArgumentException>(() => package = new Package<ContentModel1>());

            package = new Package<ContentModel1>(contents);

            Assert.False(package.IsAborted);
            Assert.False(package.IsErrored);
            Assert.Null(package.LastStationResult);
            Assert.NotNull(package.Logger);
            Assert.IsAssignableFrom<ILogger>(package.Logger);
            Assert.NotNull(package.Results);
            Assert.Empty(package.Results);
            Assert.Equal(package.Contents, contents);
            Assert.Null(package.AbortedWith);
        }
    }
}
