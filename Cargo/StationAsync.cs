using System.Threading.Tasks;

namespace LightPath.Cargo
{
    public abstract class StationAsync<TContent> : StationBase<TContent> where TContent : class
    {
        public abstract Task<Station.Action> ProcessAsync();
    }
}
