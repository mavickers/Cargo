﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LightPath.Cargo
{
    public static class Station
    {
        public struct Action
        {
            public enum ActionTypes
            {
                Abort = 1,
                Next,
                Repeat,
                Trace
            }

            private string _actionMessage { get; set; }
            private Exception _exception { get; set; }

            private ActionTypes _type { get; }

            public Exception ActionException => _exception;
            public string ActionMessage => _actionMessage;

            public ActionTypes ActionType => _type;

            private Action(ActionTypes type)
            {
                _actionMessage = null;
                _exception = null;
                _type = type;
            }

            public static Action Abort() => new Action(ActionTypes.Abort);
            public static Action Abort(Exception abortException) => Abort().WithException(abortException);
            public static Action Abort(string abortMessage) => Abort().WithMessage(abortMessage);
            public static Action Next() => new Action(ActionTypes.Next);
            public static Action Next(Exception nextException) => Next().WithException(nextException);
            public static Action Next(string nextMessage) => Next().WithMessage(nextMessage);
            public static Action Repeat() => new Action(ActionTypes.Repeat);
            public static Action Repeat(Exception repeatException) => Next().WithException(repeatException);
            public static Action Repeat(string repeatMessage) => Next().WithMessage(repeatMessage);

            public Action WithException(Exception exception)
            {
                _exception = exception;

                return this;
            }

            public Action WithMessage(string message)
            {
                _actionMessage = message;

                return this;
            }
        }

        public enum Output
        {
            Succeeded,
            Failed
        }

        public class Result
        {
            internal Action _action { get; }
            internal Type _station { get; }
            internal Output _output { get; }
            internal Exception _exception { get; }

            public Exception ActionException => _action.ActionException;
            public string ActionMessage => _action.ActionMessage;
            public Exception Exception => _exception;
            public Type Station => _station?.GetType();
            public bool IsAborting => _action.ActionType == Action.ActionTypes.Abort;
            public bool IsRepeating => _action.ActionType == Action.ActionTypes.Repeat;
            public bool WasFailure => _output == Output.Failed;
            public bool WasSuccess => _output == Output.Succeeded;

            internal Result(Type stationType, Action action, Output output, Exception exception = null)
            {
                _action = action;
                _station = stationType ?? throw new ArgumentException(nameof(stationType));
                _output = output;
                _exception = exception;
            }

            public static Result New(Type stationType, Action action, Output output, Exception exception = null)
            {
                return new Result(stationType, action, output, exception);
            }
        }
    }

    public abstract class Station<TContent> where TContent : class
    {
        // package value will be injected by the bus when it runs

        private Package<TContent> _package { get; set; }

        protected Package<TContent> Package => _package;
        [Obsolete("Use Package.Contents")]
        protected TContent Contents => _package.Contents;
        protected Station.Result LastResult => _package.Results.Last();

        public bool IsErrored => _package.IsErrored;
        public IList<Station.Result> PackageResults => _package.Results.ToList().AsReadOnly();

        public TService GetService<TService>()
        {
            if (!HasService<TService>()) return default;
            if (_package == null) return default;

            return (TService)_package.Services[typeof(TService)];
        }

        public bool HasService<TService>() => _package?.Services?.ContainsKey(typeof(TService)) ?? false;

        public IList<string> Messages => _package.Messages;

        public void Trace(string message) => _package.Trace(message);

        public bool TryGetService<TService>(out TService output)
        {
            output = default;

            try
            {
                output = GetService<TService>();

                return output != null;
            }
            catch
            {
                return false;
            }
        }

        public abstract Station.Action Process();
    }
}
