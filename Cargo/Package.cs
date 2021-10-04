using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightPath.Cargo
{
    public static class Package
    {
        public static Package<TContents> New<TContents>(params object[] parameters) where TContents : new()
        {
            return Package<TContents>.New(parameters);
        }
    }

    public class Package<TContents>
    {
        private bool _abort { get; set; }
        private Exception _abortedWith { get; set; }
        private Exception _exception { get; }
        private Guid _executionId { get; }
        private readonly ILogger<TContents> _logger;
        private readonly TContents _contents;
        private readonly Dictionary<Type, object> _services;

        public Exception AbortedWith => _abortedWith;
        public TContents Contents => _contents;
        public Exception Exception => _exception;
        internal Guid ExecutionId => _executionId;
        public bool IsAborted => _abort;
        public bool IsErrored => Results?.Any(r => r.WasFail) ?? false;
        public Station.Result<TContents> LastStationResult => Results?.LastOrDefault();
        public ILogger Logger => _logger;
        public List<Station.Result<TContents>> Results { get; }


        private Package(params object[] parameters)
        {
            if (parameters.Count(p => p?.GetType() == typeof(TContents)) != 1) throw new ArgumentException($"Package parameters must contain a single instance of {typeof(TContents).FullName}");

            _abort = false;
            _contents = (TContents)parameters.First(p => p.GetType() == typeof(TContents));
            _exception = null;
            _executionId = Guid.NewGuid();
            _logger = (ILogger<TContents>) parameters.FirstOrDefault(p => p is ILogger<TContents>) ?? new Logger<TContents>(new NullLoggerFactory());
            _services = new Dictionary<Type, object>();

            Results = new List<Station.Result<TContents>>();
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

        internal void AddResult(Station.Result<TContents> result)
        {
            if (result == null) throw new ArgumentException("AddResult \"result\" parameter is null");

            Results.Add(result);
        }

        public Package<TContents> AddService<TService>(TService service)
        {
            if (service != null) _services.Add(typeof(TService), service);

            return this;
        }

        public TService GetService<TService>() where TService : new()
        {
            if (!_services.ContainsKey(typeof(TService))) return default;

            return (TService) _services[typeof(TService)];
        }

        public static Package<TContents> New(params object[] parameters)
        {
            return new Package<TContents>(parameters);
        }
    }
}
