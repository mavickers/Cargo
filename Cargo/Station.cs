using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                Repeat
            }

            private string _actionMessage { get; set; }
            private Exception _exception { get; set; }

            private ActionTypes _type { get; }

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

        protected TContent Contents => _package.Contents;
        protected Station.Result LastResult => _package.Results.Last();

        public bool IsErrored => _package.IsErrored;
        public List<Station.Result> PackageResults => _package.Results;

        public TService GetService<TService>()
        {
            if (!_package.Services.ContainsKey(typeof(TService))) return default;

            return (TService)_package.Services[typeof(TService)];
        }

        public static Type Type => MethodBase.GetCurrentMethod()?.DeclaringType;

        public abstract Station.Action Process();
    }
}
