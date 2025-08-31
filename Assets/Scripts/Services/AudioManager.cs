using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using RPG.Services;

namespace RPG.Services
{
    public interface IAudioManager : IDisposable
    {
        void PlayMusic(AudioClip clip, float fadeTime = 1f);
        void StopMusic(float fadeTime = 1f);
        void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariance = 0f);
    }

    public class AudioManager : IAudioManager
    {
        readonly MusicChannel _musicChannel;
        readonly SFXChannel _sfxChannel;
        readonly GameObject _audioRoot;

        public AudioManager(
            GameObject audioRoot,
            ITimeProvider timeProvider,
            ITickProvider tickProvider,
            int sfxPoolSize = 10)
        {
            _audioRoot = audioRoot;
            _musicChannel = new MusicChannel(audioRoot, timeProvider, tickProvider);
            _sfxChannel = new SFXChannel(audioRoot, timeProvider, tickProvider, sfxPoolSize);
        }

        public void PlayMusic(AudioClip clip, float fadeTime = 1f)
            => _musicChannel.Play(clip, fadeTime);

        public void StopMusic(float fadeTime = 1f)
            => _musicChannel.Stop(fadeTime);

        public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariance = 0f)
            => _sfxChannel.Play(clip, volume, pitchVariance);

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_audioRoot);
            _musicChannel.Dispose();
            _sfxChannel.Dispose();
        }
    }

    class MusicChannel : IDisposable
    {
        class FadeJob
        {
            public AudioSource From;
            public AudioSource To;
            public float Duration;
            public float Progress;
            public bool SwapOnComplete;
        }

        readonly AudioSource _a;
        readonly AudioSource _b;
        readonly ITimeProvider _time;
        readonly ITickProvider _ticks;

        AudioSource _active;
        AudioSource _inactive;

        FadeJob _fadeJob;

        public MusicChannel(GameObject root, ITimeProvider time, ITickProvider ticks)
        {
            _time = time;
            _ticks = ticks;

            _a = CreateSource(root, "MusicA");
            _b = CreateSource(root, "MusicB");
            _a.loop = _b.loop = true;

            _active = _a;
            _inactive = _b;
        }

        public void Play(AudioClip clip, float fadeTime)
        {
            if (clip == null)
                return;

            if (!_active.isPlaying || fadeTime <= 0f)
            {
                CancelFade();

                _active.clip = clip;
                _active.volume = 1f;
                _active.Play();
                return;
            }

            // Prepare crossfade
            _inactive.clip = clip;
            _inactive.volume = 0f;
            _inactive.Play();

            StartFade(_active, _inactive, fadeTime, swapOnComplete: true);
        }

        public void Stop(float fadeTime)
        {
            if (!_active.isPlaying)
                return;

            if (fadeTime <= 0f)
            {
                CancelFade();
                _active.Stop();
                return;
            }

            StartFade(_active, null, fadeTime, swapOnComplete: false);
        }

        void StartFade(AudioSource from, AudioSource to, float duration, bool swapOnComplete)
        {
            CancelFade(); // kill any in-progress fade

            _fadeJob = new FadeJob
            {
                From = from,
                To = to,
                Duration = Mathf.Max(0.01f, duration),
                Progress = 0f,
                SwapOnComplete = swapOnComplete
            };

            _ticks.AddTick(UpdateFade);
        }

        void UpdateFade()
        {
            if (_fadeJob == null)
                return;

            _fadeJob.Progress += _time.DeltaTime;
            float t = Mathf.Clamp01(_fadeJob.Progress / _fadeJob.Duration);

            if (_fadeJob.From != null)
                _fadeJob.From.volume = Mathf.Lerp(1f, 0f, t);

            if (_fadeJob.To != null)
                _fadeJob.To.volume = Mathf.Lerp(0f, 1f, t);

            if (t >= 1f)
            {
                if (_fadeJob.From != null)
                    _fadeJob.From.Stop();

                if (_fadeJob.SwapOnComplete && _fadeJob.To != null)
                    (_active, _inactive) = (_fadeJob.To, _fadeJob.From);

                CancelFade();
            }
        }

        void CancelFade()
        {
            if (_fadeJob != null)
            {
                _ticks.RemoveTick(UpdateFade);
                _fadeJob = null;
            }
        }

        static AudioSource CreateSource(GameObject root, string name)
        {
            var go = new GameObject(name);
            go.transform.parent = root.transform;
            return go.AddComponent<AudioSource>();
        }

        public void Dispose()
        {
            CancelFade();

            UnityEngine.Object.Destroy(_a.gameObject);
            UnityEngine.Object.Destroy(_b.gameObject);
        }
    }

    class SFXChannel : IDisposable
    {
        readonly ITimeProvider _time;
        readonly ITickProvider _ticks;

        readonly Queue<AudioSource> _pool;
        readonly List<AudioSource> _active;
        readonly int _poolSize;

        public SFXChannel(GameObject root, ITimeProvider time, ITickProvider ticks, int poolSize)
        {
            _poolSize = poolSize;
            _time = time;
            _ticks = ticks;

            _pool = new Queue<AudioSource>(poolSize);
            _active = new List<AudioSource>(poolSize);

            for (int i = 0; i < poolSize; i++)
                _pool.Enqueue(CreateSource(root, $"SFX_{i}"));

            _ticks.AddTick(Update);
        }

        public void Play(AudioClip clip, float volume = 1f, float pitchVariance = 0f)
        {
            if (clip == null) return;

            AudioSource src = _pool.Count > 0 ? _pool.Dequeue() : GetOldest();
            src.clip = clip;
            src.volume = Mathf.Clamp01(volume);
            src.pitch = 1f + UnityEngine.Random.Range(-pitchVariance, pitchVariance);
            src.Play();
            _active.Add(src);
        }

        void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var src = _active[i];
                if (!src.isPlaying)
                {
                    _active.RemoveAt(i);
                    _pool.Enqueue(src);
                }
            }
        }

        AudioSource GetOldest()
        {
            if (_active.Count == 0)
                throw new InvalidOperationException("No active AudioSources to reuse.");

            var src = _active[0];
            _active.RemoveAt(0);
            return src;
        }

        static AudioSource CreateSource(GameObject root, string name)
        {
            var go = new GameObject(name);
            go.transform.parent = root.transform;
            return go.AddComponent<AudioSource>();
        }

        public void Dispose()
        {
            _ticks.RemoveTick(Update);

            foreach (var src in _pool) UnityEngine.Object.Destroy(src.gameObject);
            foreach (var src in _active) UnityEngine.Object.Destroy(src.gameObject);
        }
    }

}
