using UnityEngine;
using RPG.Systems;
using RPG.Presentation;

namespace RPG.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        public PlayerView playerPrefab;

        Game game;

        void Awake()
        {
            // Steam Deck resolution
            Screen.SetResolution(1280, 800, true);

            game = new Game();
            var player = new PlayerSystem();
            game.AddSystem(player);

            // Spawn player view and bind to logic
            var playerView = Instantiate(playerPrefab);
            playerView.Initialize(player);
        }

        void Update()
        {
            game.Tick(Time.deltaTime);
        }
    }
}
