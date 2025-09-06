using System;
using UnityEngine;
using RPG.Core;

namespace RPG.Services
{
    class FootstepSystem : IDisposable
    {
        const float STEP_DISTANCE = 1f;
        const string DEFAULT_SURFACE = "grass"; // TODO: make this a parameter

        readonly GameState _state;
        readonly IAudioManager _audio;
        readonly ITickProvider _ticks;
        readonly FootstepLibrary _library;

        public FootstepSystem(GameState state, FootstepLibrary library, IAudioManager audio, ITickProvider ticks)
        {
            _state = state;
            _library = library;
            _audio = audio;
            _ticks = ticks;

            _ticks.AddTick(Tick);
        }

        void Tick()
        {
            if (_state.Player.DistanceSinceLastStep >= STEP_DISTANCE)
            {
                _state.Player.DistanceSinceLastStep -= STEP_DISTANCE;

                AudioClip clip = _library.GetRandomClip(DEFAULT_SURFACE);
                if (clip != null)
                {
                    // Add subtle randomization for more natural footstep sounds
                    float volumeVariance = UnityEngine.Random.Range(0.85f, 1.15f); // ±15% volume variation
                    float pitchVariance = UnityEngine.Random.Range(0.05f, 0.15f); // ±5-15% pitch variation
                    
                    _audio.PlaySFX(clip, volumeVariance, pitchVariance);
                }
            }
        }

        public void Dispose()
        {
            _ticks.RemoveTick(Tick);
        }
    }
}
