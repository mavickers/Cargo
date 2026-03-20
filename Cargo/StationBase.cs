using System.Collections.Generic;
using System.Linq;

namespace LightPath.Cargo
{
    public abstract class StationBase<TContent> where TContent : class
    {
        // package value will be injected by the bus when it runs

        private Package<TContent> _package { get; set; }

        protected Package<TContent> Package => _package;
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
    }
}
