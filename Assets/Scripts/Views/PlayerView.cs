using System;
using UnityEngine;
using RPG.Services;

namespace RPG.Views
{
    public class PlayerView : MonoBehaviour, IDisposable
    {
        GameState _state;
        ITickProvider _ticks;

        public void Bind(GameState state, ITickProvider ticks)
        {
            _state = state;
            _ticks = ticks;

            _ticks.AddTick(Tick, TickPriority.Late);
        }

        public void Dispose()
        {
            _ticks.RemoveTick(Tick);
        }

        void Tick()
        {
            transform.position = _state.Player.Position;
        }
    }
}
