using System;
using System.Collections.Generic;
using System.Linq;

namespace LightPath.Cargo
{
    public static class Package
    {
        public static Package<TContent> New<TContent>(params object[] parameters) where TContent : class
        {
            return Package<TContent>.New(parameters);
        }
    }

    public class Package<TContent> where TContent : class
    {
        private bool _abort { get; set; }
        private Exception _abortedWith { get; set; }
        private Exception _exception { get; }
        private Guid _executionId { get; }
        private readonly TContent _contents;

        public Exception AbortedWith => _abortedWith;
        public TContent Contents => _contents;
        public Exception Exception => _exception;
        internal Guid ExecutionId => _executionId;
        public bool IsAborted => Results?.Any(r => r.IsAborting) ?? false;
        public bool IsErrored => Results?.Any(r => r.WasFailure) ?? false;
        public Station.Result LastStationResult => Results?.LastOrDefault();
        public List<Station.Result> Results { get; }
        internal readonly Dictionary<Type, object> Services;

        private Package(params object[] parameters)
        {
            var isInstanceOf = parameters.Count(p => p?.GetType() == typeof(TContent)) == 1;
            var isInheriting = parameters.Count(p => p?.GetType().GetInterfaces().Any(i => i == typeof(TContent)) ?? false) == 1;
            //if (parameters.Count(p => p?.GetType() == typeof(TContents)) != 1) throw new ArgumentException($"Package parameters must contain a single instance of {typeof(TContents).FullName}");

            if (!isInheriting && !isInstanceOf) throw new ArgumentException($"Package parameters must inherit or be an instance of {typeof(TContent).FullName}");

            _abort = false;
            _exception = null;
            _executionId = Guid.NewGuid();
            _contents = isInstanceOf 
                ? (TContent)parameters.First(p => p.GetType() == typeof(TContent)) 
                : (TContent)parameters.First(p => p.GetType().GetInterfaces().Any(i => i == typeof(TContent))); 

            Results = new List<Station.Result>();
            Services = (Dictionary<Type, object>) parameters.FirstOrDefault(p => p is Dictionary<Type, object>) ?? new Dictionary<Type, object>();
        }

        internal void Abort(Exception exception = null)
        {
            _abort = true;
            _abortedWith = exception ?? new Exception("N/A");
        }

        internal void Abort(string exceptionMessage)
        {
            var message = string.IsNullOrEmpty(exceptionMessage) ? "N/A" : exceptionMessage;

            _abort = true;
            _abortedWith = new Exception(message);
        }

        internal void AbortIf(bool condition, Exception exception = null)
        {
            if (condition) Abort(exception);
        }

        internal void AbortIf(bool condition, string exceptionMessage)
        {
            if (condition) Abort(exceptionMessage);
        }

        internal void AddResult(Station.Result result)
        {
            if (result == null) throw new ArgumentException("AddResult \"result\" parameter is null");

            Results.Add(result);
        }

        public static Package<TContent> New(params object[] parameters)
        {
            return new Package<TContent>(parameters);
        }
    }
}
