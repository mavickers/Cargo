using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Delay;

namespace LightPath.Cargo.Tests.Integration
{
    public class Delay
    {
        // two 150 ms delays; allow timer slack below the 300 ms nominal
        private static readonly TimeSpan MinimumElapsed = TimeSpan.FromMilliseconds(250);

        [Fact]
        public void Go_RepeatWithDelay_WaitsBetweenRuns()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStation<RepeatWithDelayStation>();
            var stopwatch = Stopwatch.StartNew();

            bus.Go(content);

            stopwatch.Stop();

            Assert.Equal(3, content.IntVal);
            Assert.True(stopwatch.Elapsed >= MinimumElapsed, $"elapsed {stopwatch.ElapsedMilliseconds} ms");
            Assert.False(bus.Package.IsErrored);
        }

        [Fact]
        public async Task GoAsync_RepeatWithDelay_WaitsBetweenRuns()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStation<RepeatWithDelayStation>();
            var stopwatch = Stopwatch.StartNew();

            await bus.GoAsync(content);

            stopwatch.Stop();

            Assert.Equal(3, content.IntVal);
            Assert.True(stopwatch.Elapsed >= MinimumElapsed, $"elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public void RepeatResult_ExposesActionDelay()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStation<RepeatWithDelayStation>();

            bus.Go(content);

            var repeats = bus.Package.Results.Where(r => r.IsRepeating).ToList();

            Assert.Equal(2, repeats.Count);
            Assert.All(repeats, r => Assert.Equal(RepeatWithDelayStation.Delay, r.ActionDelay));
            Assert.Equal(TimeSpan.Zero, bus.Package.Results.Last().ActionDelay);
        }

        [Fact]
        public async Task CancelDuringDelay_ByDefault_Throws()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStation<LongDelayStation>();
            var stopwatch = Stopwatch.StartNew();

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(50);

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => bus.GoAsync(content, cts.Token));
            }

            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1.5), $"elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task CancelDuringDelay_WithAbortOnCancel_RunsFinalStation()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithAbortOnCancel()
                         .WithStation<LongDelayStation>()
                         .WithFinalStation<FinalStation>();
            var stopwatch = Stopwatch.StartNew();

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(50);

                await bus.GoAsync(content, cts.Token);
            }

            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1.5), $"elapsed {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(bus.Package.IsAborted);
            Assert.Equal(100, content.IntVal);
        }

        [Fact]
        public async Task FinalStationRepeatWithDelay_AfterCancel_RunsOnceThenAborts()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithAbortOnCancel()
                         .WithStation<LongDelayStation>()
                         .WithFinalStation<FinalRepeatDelayStation>();
            var stopwatch = Stopwatch.StartNew();

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(100);

                await bus.GoAsync(content, cts.Token);
            }

            var last = bus.Package.Results.Last();

            Assert.Equal(1, content.IntVal);
            Assert.True(last.IsAborting);
            Assert.True(last.WasFailure);
            Assert.Equal(2, last.Iteration);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1.5), $"elapsed {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task FinalStationRepeat_PreCancelled_RunsOnceThenAborts()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>()
                         .WithAbortOnCancel()
                         .WithFinalStation<FinalRepeatStation>();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                await bus.GoAsync(content, cts.Token);
            }

            var last = bus.Package.Results.Last();

            Assert.Equal(1, content.IntVal);
            Assert.True(last.IsAborting);
            Assert.True(last.WasFailure);
            Assert.Equal(2, last.Iteration);
        }

        [Fact]
        public void NextWithDelay_DoesNotWait()
        {
            var content = new ContentModel2();
            var bus = Bus.New<ContentModel2>().WithStation<NextWithDelayStation>();
            var stopwatch = Stopwatch.StartNew();

            bus.Go(content);

            stopwatch.Stop();

            Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(200), $"elapsed {stopwatch.ElapsedMilliseconds} ms");
            Assert.Equal(TimeSpan.FromMilliseconds(500), bus.Package.Results.Single().ActionDelay);
        }
    }
}
