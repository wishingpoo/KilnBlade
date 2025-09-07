using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RPG.Services
{
    /// <summary>
    /// Addressable-based BGM library that lazy-loads AudioClips by key.
    /// - Keys are exactly the Addressables keys.
    /// - Caches loaded clips for the lifetime of the library.
    /// - Releases all handles on Dispose.
    /// </summary>
    public sealed class BgmLibrary : IDisposable
    {
        private readonly Dictionary<string, AudioClip> _cache = new();

        // Handles for every successfully loaded clip (so we can release them)
        private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _handles = new();

        // Prevent duplicate loads of the same key
        private readonly HashSet<string> _loading = new();

        // Pending callbacks for when a key is loaded
        private readonly Dictionary<string, List<Action<AudioClip>>> _pendingCallbacks = new();

        private bool _disposed = false;

        public bool IsLoaded(string key) => _cache.ContainsKey(key);
        public bool IsLoading(string key) => _loading.Contains(key);

        /// <summary>Returns true if the clip is loaded and sets the out parameter to the clip.</summary>
        public bool TryGetLoadedClip(string key, out AudioClip clip)
        {
            return _cache.TryGetValue(key, out clip);
        }

        /// <summary>
        /// Loads a clip asynchronously and calls the callback with the loaded clip on success.
        /// If the clip is already loaded, the callback is invoked immediately.
        /// If the clip is already loading, the callback is added to the pending callbacks.
        /// Non-blocking; completion is handled via Addressables' Completed callback.
        /// </summary>
        public void LoadClip(string key, Action<AudioClip> onLoaded = null)
        {
            if (_disposed) return;
            
            if (TryGetLoadedClip(key, out var clip))  // already loaded
            {
                onLoaded?.Invoke(clip);
                return;
            }
            if (IsLoading(key))  // already loading, add pending callback
            {
                if (onLoaded != null)
                    _pendingCallbacks[key].Add(onLoaded);
                return;
            }

            _loading.Add(key);
            _pendingCallbacks[key] = new List<Action<AudioClip>>();
            if (onLoaded != null)
                _pendingCallbacks[key].Add(onLoaded);

            var handle = Addressables.LoadAssetAsync<AudioClip>(key);
            handle.Completed += h =>
            {
                if (_disposed) return;  // Early exit if disposed
                
                _loading.Remove(key);

                if (h.Status == AsyncOperationStatus.Succeeded && h.Result != null)
                {
                    _cache[key]  = h.Result;
                    _handles[key] = h; // retain handle so we can Release on Dispose

                    // invoke pending callbacks (if any)
                    foreach (var callback in _pendingCallbacks[key])
                        callback(h.Result);

                    _pendingCallbacks.Remove(key);
                }
                else
                {
                    Debug.LogError($"BgmLibrary: Failed to load Addressable '{key}' - Status: {h.Status}");
                    Addressables.Release(h);
                }
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            foreach (var handle in _handles.Values)
                Addressables.Release(handle);

            _handles.Clear();
            _cache.Clear();
            _loading.Clear();
            _pendingCallbacks.Clear();
        }
    }
}
