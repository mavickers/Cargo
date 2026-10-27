using System;
using System.Linq;
using System.Threading;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Observer;

namespace LightPath.Cargo.Tests.Integration
{
    public class Observer
    {
        [Fact]
        public void Observer_ReceivesStartingAndFinishedForEveryRun()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver();
            var bus = Bus.New<ContentModel2>()
                         .WithStationObserver(observer)
                         .WithStation<Station1>()
                         .WithStation<Station2>()
                         .WithStation<Station3>();

            bus.Go(content);

            var expectedStarts = new[] { (typeof(Station1), 1), (typeof(Station2), 1), (typeof(Station2), 2), (typeof(Station3), 1) };

            Assert.Equal(expectedStarts, observer.Starts.ToArray());
            Assert.Equal(4, observer.Finished.Count);
            Assert.Equal(new[] { 1, 1, 2, 1 }, observer.Finished.Select(r => r.Iteration).ToArray());
            Assert.All(observer.Finished, r => Assert.True(r.Elapsed >= TimeSpan.Zero));
            Assert.True(observer.Finished[0].Elapsed >= TimeSpan.FromMilliseconds(40), $"elapsed {observer.Finished[0].Elapsed.TotalMilliseconds} ms");
        }

        [Fact]
        public void Observer_ThrowingInStarting_EscapesGo()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver { ThrowOnStarting = new InvalidOperationException("observer") };
            var bus = Bus.New<ContentModel2>()
                         .WithStationObserver(observer)
                         .WithStation<Station1>();

            var exception = Assert.Throws<InvalidOperationException>(() => bus.Go(content));

            Assert.Equal("observer", exception.Message);
            Assert.DoesNotContain(bus.Package.Results, r => r.Station == typeof(Station1));
        }

        [Fact]
        public void Observer_PreCancelledToken_FinishedCalledForSyntheticAbort()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver();
            var bus = Bus.New<ContentModel2>()
                         .WithAbortOnCancel()
                         .WithStationObserver(observer)
                         .WithStation<Station1>()
                         .WithFinalStation<FinalStation>();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                bus.GoAsync(content, cts.Token).GetAwaiter().GetResult();
            }

            var aborted = observer.Finished.Single(r => r.Station == typeof(Station1));

            Assert.True(aborted.IsAborting);
            Assert.Equal(1, aborted.Iteration);
            Assert.Equal(TimeSpan.Zero, aborted.Elapsed);
            Assert.DoesNotContain(observer.Starts, s => s.Station == typeof(Station1));
            Assert.Contains(observer.Starts, s => s.Station == typeof(FinalStation));
            Assert.Equal(20, content.IntVal);
        }

        [Fact]
        public void Observer_ThrowingInFinished_EscapesGoAfterResultEnqueued()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver { ThrowOnFinished = new InvalidOperationException("finished") };
            var bus = Bus.New<ContentModel2>()
                         .WithStationObserver(observer)
                         .WithStation<Station3>();

            var exception = Assert.Throws<InvalidOperationException>(() => bus.Go(content));

            Assert.Equal("finished", exception.Message);
            Assert.Contains(bus.Package.Results, r => r.Station == typeof(Station3));
        }

        [Fact]
        public void Observer_RepeatOverflow_ReportsFailedThirdIteration()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver();
            var bus = Bus.New<ContentModel2>()
                         .WithStationObserver(observer)
                         .WithStation<Limit2Station>();

            bus.Go(content);

            Assert.Equal(new[] { 1, 2, 3 }, observer.Starts.Select(s => s.Iteration).ToArray());
            Assert.Equal(3, observer.Finished.Count);
            Assert.True(observer.Finished[2].WasFailure);
            Assert.IsType<OverflowException>(observer.Finished[2].Exception);
            Assert.Equal(3, observer.Finished[2].Iteration);
        }

        [Fact]
        public void Observer_AbortToFinalStation_ReportsBoth()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver();
            var bus = Bus.New<ContentModel2>()
                         .WithStationObserver(observer)
                         .WithStation<AbortStation>()
                         .WithFinalStation<FinalStation>();

            bus.Go(content);

            Assert.Equal(new[] { (typeof(AbortStation), 1), (typeof(FinalStation), 1) }, observer.Starts.ToArray());
            Assert.Equal(2, observer.Finished.Count);
        }

        [Fact]
        public void Observer_SlowStarting_DoesNotInflateElapsed()
        {
            var content = new ContentModel2();
            var observer = new RecordingObserver { StartingSleepMilliseconds = 100 };
            var bus = Bus.New<ContentModel2>()
                         .WithStationObserver(observer)
                         .WithStation<Station3>();

            bus.Go(content);

            Assert.True(observer.Finished.Single().Elapsed < TimeSpan.FromMilliseconds(80), $"elapsed {observer.Finished.Single().Elapsed.TotalMilliseconds} ms");
        }
    }
}
