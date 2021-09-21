using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Cargo.Tests.Unit
{
    public class Package
    {
        public class ContentModel
        {
            public string String1 { get; set; }
            public string String2 { get; set; }
            public string String3 { get; set; }
        }

        [Fact]
        public void Instantiation()
        {
            Cargo.Package<ContentModel> package;

            var contents = new ContentModel { String2 = string.Empty, String3 = "test" };

            Assert.Throws<ArgumentException>(() => package = new Package<ContentModel>());

            package = new Package<ContentModel>(contents);

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
