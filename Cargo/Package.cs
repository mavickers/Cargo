using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cargo
{
    public class Package<T>
    {
        private bool _abort { get; set; }
        private Exception _abortedWith { get; set; }
        private Exception _exception { get; set; }
        private Guid _executionId { get; }
        private readonly ILogger<T> _logger;
        private readonly T _package;

        public Exception AbortedWith => _abortedWith;
        public T Contents => _package;
        public Exception Exception => _exception;
        internal Guid ExecutionId => _executionId;
        public bool IsAborted => _abort;
        public bool IsErrored => _exception != null;
        public Station.Result<T> LastStationResult => Results?.Last();
        public ILogger Logger => _logger;
        public List<Station.Result<T>> Results { get; }


        public Package(params object[] parameters)
        {
            if (parameters.Count(p => p?.GetType() == typeof(T)) != 1) throw new ArgumentException($"Package parameters must contain a single instance of {typeof(T).FullName}");

            _abort = false;
            _exception = null;
            _executionId = Guid.NewGuid();
            _logger = (ILogger<T>) parameters.FirstOrDefault(p => p is ILogger<T>) ?? new Logger<T>(new NullLoggerFactory());
            _package = (T)parameters.First(p => p.GetType() == typeof(T));

            Results = new List<Station.Result<T>>();
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

        internal void AddResult(Station.Result<T> result)
        {
            if (result == null) throw new ArgumentException("AddResult \"result\" parameter is null");

            Results.Add(result);
        }
    }
}
