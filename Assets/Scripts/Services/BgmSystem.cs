using System;
using UnityEngine;
using RPG.Core;

namespace RPG.Services
{
    public class BgmSystem : IDisposable
    {
        const string DEFAULT_BGM = "River";

        readonly IAudioManager _audio;
        readonly BgmLibrary _library;
        readonly GameState _state;
        readonly ITickProvider _ticks;

        string _currentKey;

        public BgmSystem(
            GameState state,
            BgmLibrary library,
            IAudioManager audio,
            ITickProvider ticks)
        {
            _state = state;
            _library = library;
            _audio = audio;
            _ticks = ticks;

            _ticks.AddTick(Tick);
        }

        void Tick()
        {
            // For now just always ensure "River" is playing
            // TODO: base this on state, e.g. current level/scene, game events, etc.
            if (_currentKey != "River")
                Play("River");
        }

        public void Play(string key, float fade = 1f)
        {
            var clip = _library.GetClip(key);
            if (clip == null) return;

            _currentKey = key;
            _audio.PlayMusic(clip, fade);
        }

        public void Stop(float fade = 1f)
        {
            _currentKey = null;
            _audio.StopMusic(fade);
        }

        public void Dispose()
        {
            _ticks.RemoveTick(Tick);
        }
    }
}
