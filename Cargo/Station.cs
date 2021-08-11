﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Cargo
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

        private static readonly Dictionary<Type, Output> OutputMaps = new()
        {
            { typeof(AbortException), Output.Aborted },
            { typeof(SkipException), Output.Skipped }
        };

        public static class Result
        {
            public static Result<T> New<T>(Station<T> station, Output output, Exception exception = null) where T : new()
            {
                return new()
                {
                    _station = station,
                    _output = output,
                    _exception = exception
                };

            }
        }

        public class Result<T>
        {

            internal Station<T> _station { get; init; }
            internal Output _output { get; init; }
            internal Exception _exception { get; init; }

            public Exception Exception => _exception;
            public Type Station => _station?.GetType();
            public bool WasAborted => _output == Output.Aborted;
            public bool WasFail => _output == Output.Failed;
            public bool WasSkipped => _output == Output.Skipped;
            public bool WasSuccess => _output == Output.Succeeded;
            public bool WasUnknown => _output == Output.Unknown;
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
        private Package<T> _package { get; set; }
        private bool _repeat { get; set; }

        protected T Contents => _package.Contents;
        protected Station.Result<T> LastResult => _package.Results.Last();
        public static Type Type => typeof(Station<T>);

        public bool IsRepeat => _repeat;
        public bool NotRepeat => !_repeat;

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