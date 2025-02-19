namespace LightPath.Cargo
{
    public static class Strategies
    {
        public enum ServiceRegistrationStrategy
        {
            /// <summary>
            /// Default strategy. Register the service with the first interface found on its type. If no
            /// interfaces are found, the service is registered with its declared type.
            /// </summary>
            /// <remarks>
            /// Remember to perform a service check using HasService or TryGetService inside the station!
            /// </remarks>
            AsFirstInterfacePreferred = 0,
            /// <summary>
            /// Register the service with its declared type.
            /// </summary>
            AsDeclaredType = 1,
        }
    }
}
