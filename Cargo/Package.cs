using System;
using System.Collections.Concurrent;
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
        private ConcurrentQueue<string> _messages { get; }
        private Guid _executionId { get; }
        private readonly TContent _contents;

        public TContent Contents => _contents;
        public Guid ExecutionId => _executionId;
        public bool IsAborted => Results?.Any(r => r.IsAborting) ?? false;
        public bool IsErrored => Results?.Any(r => r.WasFailure) ?? false;
        public Station.Result LastStationResult => Results?.LastOrDefault();
        public ConcurrentQueue<Station.Result> Results { get; }
        internal readonly ConcurrentDictionary<Type, object> Services;
        public IList<string> Messages => _messages.ToList().AsReadOnly();

        private Package(params object[] parameters)
        {
            var isInstanceOf = parameters.Count(p => p?.GetType() == typeof(TContent)) == 1;
            var isInheriting = parameters.Count(p => p?.GetType().GetInterfaces().Any(i => i == typeof(TContent)) ?? false) == 1;
            //if (parameters.Count(p => p?.GetType() == typeof(TContents)) != 1) throw new ArgumentException($"Package parameters must contain a single instance of {typeof(TContents).FullName}");

            if (!isInheriting && !isInstanceOf) throw new ArgumentException($"Package parameters must inherit or be an instance of {typeof(TContent).FullName}");

            _executionId = Guid.NewGuid();
            _contents = isInstanceOf 
                ? (TContent)parameters.First(p => p.GetType() == typeof(TContent)) 
                : (TContent)parameters.First(p => p.GetType().GetInterfaces().Any(i => i == typeof(TContent)));
            _messages = new ConcurrentQueue<string>();

            Results = new ConcurrentQueue<Station.Result>();
            Services = (ConcurrentDictionary<Type, object>) parameters.FirstOrDefault(p => p is ConcurrentDictionary<Type, object>) ?? new ConcurrentDictionary<Type, object>();
        }

        public static Package<TContent> New(params object[] parameters)
        {
            return new Package<TContent>(parameters);
        }

        public void Trace() => _messages.Enqueue(string.Empty);
        public void Trace(string message) => _messages.Enqueue(message);
    }
}
