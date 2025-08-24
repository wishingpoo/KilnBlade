using UnityEngine;

namespace RPG.Systems
{
    /// <summary>
    /// Root game state.
    /// </summary>
    public class GameState
    {
        public PlayerState Player { get; set; } = new();
    }

    /// <summary>
    /// PlayerSystem state.
    /// </summary>
    public class PlayerState
    {
        public Vector2 Position { get; set; } = Vector2.zero;
        public float Speed { get; set; } = 4f;
    }
}
