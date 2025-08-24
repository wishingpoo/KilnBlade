using UnityEngine;
using RPG.Systems;
using RPG.Presentation;

namespace RPG.Core
{
    /// <summary>
    /// Game bootstrapper.
    /// Initializes the game and registers the services.
    /// MonoBehaviour to manage the game loop.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public PlayerView playerPrefab;

        ServiceRegistry _services;
        ITickProvider _tickProvider;

        void Awake()
        {
            InitializeGraphics();

            _services = new ServiceRegistry();
            ServiceLocator.Initialize(_services);

            _services.Register(new GameState());
            _services.Register(new UnityTimeProvider() as ITimeProvider);
            _services.Register(_tickProvider = new TickProvider());

            _services.RegisterFactory(() =>
                new PlayerController(
                    _services.Resolve<GameState>(),
                    _services.Resolve<ITimeProvider>(),
                    _services.Resolve<ITickProvider>()));

            // Force instantiation of player system.  TODO: consider a better way to do this like zenject's .NonLazy() on a factory registration
            _services.Resolve<PlayerController>();

            RegisterViews(_services);

            Debug.Log("Game bootstrap complete, services registered.");
        }

        void OnDestroy()
        {
            _services.Dispose();
        }

        void Update()
        {
            _tickProvider.Update();
        }

        void InitializeGraphics()
        {
            // Steam Deck resolution: TODO refactor to some kind of settings / graphics system
            Screen.SetResolution(1280, 800, true);
        }

        void RegisterViews(ServiceRegistry services)
        {
            // TODO: consider a better way to do this: what is the lifecycle of the views?  How to manage them?
            // Probably want a ViewFactory or some event-driven spawn system later.
            var playerView = Instantiate(playerPrefab);
            playerView.Bind(services.Resolve<GameState>(), services.Resolve<ITickProvider>());
            services.Register(playerView);
        }
    }
}
