using System;
using UnityEngine;

namespace RPG.Services
{
    /// <summary>
    /// User controls the player.
    /// Player moves based on input.
    /// </summary>
    public class PlayerController : IDisposable
    {
        readonly GameState _state;
        readonly PlayerControls _controls;
        readonly ITimeProvider _time;
        readonly ITickProvider _ticks;

        public PlayerController(
            GameState state,
            ITimeProvider time,
            ITickProvider ticks)
        {
            _state = state;
            _time = time;
            _ticks = ticks;

            _controls = new PlayerControls();
            _controls.Enable();

            _ticks.AddTick(Tick);
        }

        void Tick()
        {
            Vector2 movement = _controls.Player.Move.ReadValue<Vector2>();
            if (movement.sqrMagnitude <= 0f)
                return;

            Vector2 newPosition = _state.Player.Position + movement.normalized * _state.Player.Speed * _time.DeltaTime;

            float distanceTraveled = Vector2.Distance(_state.Player.Position, newPosition);
            _state.Player.DistanceSinceLastStep += distanceTraveled;
            _state.Player.Position = newPosition;
        }

        public void Dispose()
        {
            _ticks.RemoveTick(Tick);

            _controls.Player.Disable();
            _controls.Dispose();
        }
    }
}
