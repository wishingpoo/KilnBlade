using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Services
{
    /// <summary>
    /// Provides a way to add and remove ticks from the game.
    /// Ticks are called every frame.
    /// Ticks are called in the order they were added, with the highest priority being called first.
    /// </summary>
    public interface ITickProvider
    {
        void AddTick(Action tick, TickPriority priority = TickPriority.Normal);
        void RemoveTick(Action tick);
        void Clear();

        /// <summary>
        /// Call every frame by the Game Loop MonoBehaviour.
        /// </summary>
        void Update();
    }

    /// <summary>
    /// The priority of a tick.
    /// Ticks with a higher priority are called before ticks with a lower priority.
    /// Abstraction to not tie us to Unity's Update() and LateUpdate() methods.
    /// </summary>
    public enum TickPriority
    {
        Normal = 0,
        Late = 1,
    }

    public class TickProvider : ITickProvider, IDisposable
    {
        readonly List<Action>[] _ticks;
        int _numPriorities;

        public TickProvider()
        {
            _numPriorities = Enum.GetValues(typeof(TickPriority)).Length;

            _ticks = new List<Action>[_numPriorities];
            for (int i = 0; i < _numPriorities; i++)
                _ticks[i] = new List<Action>();
        }

        public void AddTick(Action tick, TickPriority priority = TickPriority.Normal)
        {
            _ticks[(int)priority].Add(tick);
        }

        public void RemoveTick(Action tick)
        {
            for (int i = 0; i < _numPriorities; i++)
                _ticks[i].Remove(tick);
        }

        // Stable tick order first by priority, then by order of addition.
        public void Update()
        {
            for (int i = 0; i < _numPriorities; i++)
            {
                var ticks = _ticks[i];
                foreach (var tick in ticks)
                {
                    try
                    {
                        tick.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _numPriorities; i++)
                _ticks[i].Clear();
        }

        public void Dispose() => Clear();
    }
}
