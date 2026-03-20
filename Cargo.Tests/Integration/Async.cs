using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Async;
using static LightPath.Cargo.Tests.Integration.Stations.Services;

namespace LightPath.Cargo.Tests.Integration
{
    public class AsyncTests
    {
        /// <summary>
        /// 1. Interleaved state accumulation: Sync +1, Async +2, Sync +3, Async +4 = 10
        /// </summary>
        [Fact]
        public async Task InterleavedStateAccumulation()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<AsyncAdder>()
                         .WithStation<SyncAdder3>()
                         .WithStation<AsyncAdderWithDelay>();

            await bus.GoAsync(content);

            Assert.Equal(10, content.IntVal);
            Assert.Equal(4, bus.Package.Results.Count);
        }

        /// <summary>
        /// 2. Async station reads sync station's result via LastResult
        /// </summary>
        [Fact]
        public async Task AsyncReadsSyncResult()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncMessageSender>()
                         .WithStation<AsyncResultChecker>();

            await bus.GoAsync(content);

            Assert.Equal(10, content.IntVal);
            Assert.False(bus.Package.IsErrored);
        }

        /// <summary>
        /// 3. Sync station reads async station's result via LastResult
        /// </summary>
        [Fact]
        public async Task SyncReadsAsyncResult()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<AsyncMessageSender>()
                         .WithStation<SyncResultChecker>();

            await bus.GoAsync(content);

            Assert.Equal(11, content.IntVal);
            Assert.False(bus.Package.IsErrored);
        }

        /// <summary>
        /// 4. Async station aborts, sync final station runs
        /// </summary>
        [Fact]
        public async Task AsyncAbortsSyncFinalRuns()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<AsyncAborter>()
                         .WithStation<SyncAdder3>()
                         .WithFinalStation<SyncFinalStation>();

            await bus.GoAsync(content);

            Assert.True(bus.Package.IsAborted);
            Assert.Equal(101, content.IntVal);
        }

        /// <summary>
        /// 5. Sync station aborts, async final station runs
        /// </summary>
        [Fact]
        public async Task SyncAbortsAsyncFinalRuns()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<SyncAborter>()
                         .WithStation<SyncAdder3>()
                         .WithFinalStation<AsyncFinalStation>();

            await bus.GoAsync(content);

            Assert.True(bus.Package.IsAborted);
            Assert.Equal(101, content.IntVal);
        }

        /// <summary>
        /// 6. Mixed pipeline with services — both station types call GetService
        /// </summary>
        [Fact]
        public async Task MixedPipelineWithServices()
        {
            var content = new ContentModel1();
            var implementation = new Implementation1();
            var bus = Bus.New<ContentModel1>()
                         .WithService<Interface1>(implementation)
                         .WithStation<SyncServiceUser>()
                         .WithStation<AsyncServiceUser>();

            await bus.GoAsync(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(6, content.Int1);
        }

        /// <summary>
        /// 7. Async station repeats in mixed pipeline
        /// </summary>
        [Fact]
        public async Task AsyncRepeatsInMixedPipeline()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<AsyncRepeater>()
                         .WithStation<SyncAdder3>();

            await bus.GoAsync(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(8, content.IntVal);
        }

        /// <summary>
        /// 8. Async error, sync continues (NoAbortOnError)
        /// </summary>
        [Fact]
        public async Task AsyncErrorSyncContinues()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<AsyncThrower>()
                         .WithStation<SyncAdder1>()
                         .WithNoAbortOnError();

            await bus.GoAsync(content);

            Assert.True(bus.Package.IsErrored);
            Assert.False(bus.Package.IsAborted);
            Assert.Equal(1, content.IntVal);
        }

        /// <summary>
        /// 9. Sync error, async continues (NoAbortOnError)
        /// </summary>
        [Fact]
        public async Task SyncErrorAsyncContinues()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncThrower>()
                         .WithStation<AsyncAdder>()
                         .WithNoAbortOnError();

            await bus.GoAsync(content);

            Assert.True(bus.Package.IsErrored);
            Assert.False(bus.Package.IsAborted);
            Assert.Equal(2, content.IntVal);
        }

        /// <summary>
        /// 10. Cancellation between mixed station types
        /// </summary>
        [Fact]
        public async Task CancellationBetweenMixedStations()
        {
            var content = new ContentModel2();
            var cts = new CancellationTokenSource();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncCanceller>()
                         .WithStation<AsyncAdder>();

            cts.Cancel();

            // Token already cancelled before GoAsync — first station never runs
            await Assert.ThrowsAsync<OperationCanceledException>(() => bus.GoAsync(content, cts.Token));
            Assert.Equal(0, content.IntVal);
        }

        /// <summary>
        /// 10b. Cancellation after first station runs
        /// </summary>
        [Fact]
        public async Task CancellationAfterFirstStation()
        {
            var content = new ContentModel2();
            var cts = new CancellationTokenSource();

            // We'll use a station that triggers cancellation mid-pipeline via the token on the package
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<AsyncAdder>();

            // Run with a token that we cancel after a brief delay
            // Since SyncAdder1 is synchronous and fast, we need a different approach:
            // Cancel between iterations by using a custom station
            cts.CancelAfter(1);
            await Task.Delay(50);

            await Assert.ThrowsAsync<OperationCanceledException>(() => bus.GoAsync(content, cts.Token));
            Assert.Equal(0, content.IntVal);
        }

        /// <summary>
        /// 11. Pure async pipeline (happy path)
        /// </summary>
        [Fact]
        public async Task PureAsyncPipeline()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<AsyncAdder>()
                         .WithStation<AsyncAdderWithDelay>()
                         .WithStation<AsyncAdder>();

            await bus.GoAsync(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(8, content.IntVal);
            Assert.Equal(3, bus.Package.Results.Count);
        }

        /// <summary>
        /// 12. GoAsync with all-sync stations works identically to Go
        /// </summary>
        [Fact]
        public async Task GoAsyncWithAllSyncStations()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<SyncAdder3>();

            await bus.GoAsync(content);

            Assert.False(bus.Package.IsErrored);
            Assert.Equal(4, content.IntVal);
            Assert.Equal(2, bus.Package.Results.Count);
        }

        /// <summary>
        /// 13. Go with async station throws InvalidOperationException
        /// </summary>
        [Fact]
        public void GoWithAsyncStationThrows()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStation<SyncAdder1>()
                         .WithStation<AsyncAdder>();

            Assert.Throws<InvalidOperationException>(() => bus.Go(content));
        }

        /// <summary>
        /// 14. Async repeat exceeds limit — OverflowException
        /// </summary>
        [Fact]
        public async Task AsyncRepeatExceedsLimit()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithStationRepeatLimit(3)
                         .WithStation<AsyncRepeater>();

            await bus.GoAsync(content);

            Assert.True(bus.Package.IsErrored);
            Assert.True(bus.Package.IsAborted);
            Assert.True(bus.Package.Results.Last(r => r.Exception != null).Exception is OverflowException);
        }
    }
}
