namespace RPG.Core
{
    /// <summary>
    /// Provides global access to the service registry (optional).
    /// Prefer passing dependencies explicitly, but this can help bridge Unity code.
    /// </summary>
    public static class ServiceLocator
    {
        public static ServiceRegistry Registry { get; private set; }

        public static void Initialize(ServiceRegistry registry)
        {
            Registry = registry;
        }

        public static T Resolve<T>() where T : class => Registry.Resolve<T>();
    }
}
