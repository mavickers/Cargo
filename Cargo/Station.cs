using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightPath.Cargo
{
    public static class Station
    {
        public enum Output
        {
            Unknown = 1,
            Succeeded,
            Failed,
            Skipped,
            Aborted,
        }

        private static readonly Dictionary<Type, Output> OutputMaps = new Dictionary<Type, Output>
        {
            { typeof(AbortException), Output.Aborted },
            { typeof(SkipException), Output.Skipped }
        };

        //public static class Result
        //{
            //public static Result<T> New<T>(Station<T> station, Output output, Exception exception = null)
            //{
            //    return new Result<T>(station, output, exception);
            //}
        //}

        public class Result
        {
            internal Type _station { get; }
            internal Output _output { get; }
            internal Exception _exception { get; }

            public Exception Exception => _exception;
            public Type Station => _station?.GetType();
            public bool WasAborted => _output == Output.Aborted;
            public bool WasFail => _output == Output.Failed;
            public bool WasSkipped => _output == Output.Skipped;
            public bool WasSuccess => _output == Output.Succeeded;
            public bool WasUnknown => _output == Output.Unknown;

            internal Result(Type stationType, Output output, Exception exception = null)
            {
                _station = stationType ?? throw new ArgumentException(nameof(stationType));
                _output = output;
                _exception = exception;
            }

            public static Result New(Type stationType, Output output, Exception exception = null)
            {
                return new Result(stationType, output, exception);
            }
        }

        public class AbortException : Exception
        {
            public AbortException(string message) : base(message) { }
        }

        public class SkipException : Exception
        {
            public SkipException(string message) : base(message) { }
        }
    }

    public abstract class Station<T>
    {
        // package value will be injected by the bus when it runs

        private Package<T> _package { get; set; }
        private bool _repeat { get; set; }

        protected T Contents => _package.Contents;
        protected Station.Result LastResult => _package.Results.Last();

        public bool IsErrored => _package.IsErrored;
        public bool IsRepeat => _repeat;
        public bool NotRepeat => !_repeat;
        public List<Station.Result> PackageResults => _package.Results;

        public TService GetService<TService>()
        {
            if (!_package.Services.ContainsKey(typeof(TService))) return default;

            return (TService)_package.Services[typeof(TService)];
        }

        public static Type Type => MethodBase.GetCurrentMethod().DeclaringType;

        public void Abort(string message = "Aborted")
        {
            _package.Abort(message);

            throw new Station.AbortException(message);
        }
        public abstract void Process();
        public void Skip(string message = "Skipped") => throw new Station.SkipException(message);

        public void NoRepeat() => _repeat = false;
        public void Repeat() => _repeat = true;
    }
}
