using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    /// <summary>
    /// Simple DI registry. Supports singletons and transients.
    /// </summary>
    public class ServiceRegistry : IDisposable
    {
        private readonly Dictionary<Type, Func<object>> _factories = new();
        private readonly List<IDisposable> _disposables = new();

        /// <summary>
        /// Register a concrete instance as a singleton.
        /// Always returns the same instance.
        /// </summary>
        public void Register<T>(T instance) where T : class
        {
            _factories[typeof(T)] = () => instance!;

            if (instance is IDisposable disposable)
                _disposables.Add(disposable);
        }

        /// <summary>
        /// Register a factory that will create a new instance every time.
        /// </summary>
        public void RegisterTransient<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () =>
            {
                var instance = factory();
                if (instance is IDisposable disposable)
                    _disposables.Add(disposable);
                return instance;
            };
        }

        /// <summary>
        /// Register a factory that will lazily create a single instance.
        /// </summary>
        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            T instance = null;
            bool created = false;

            _factories[typeof(T)] = () =>
            {
                if (!created)
                {
                    instance = factory();
                    created = true;

                    if (instance is IDisposable disposable)
                        _disposables.Add(disposable);
                }

                return instance!;
            };
        }

        /// <summary>
        /// Resolve a registered service.
        /// </summary>
        public T Resolve<T>() where T : class
        {
            if (_factories.TryGetValue(typeof(T), out var factory))
            {
                return (T)factory();
            }

            throw new InvalidOperationException($"Service of type {typeof(T)} not registered.");
        }

        public void Dispose()
        {
            int disposed = 0;
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                    disposed++;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            _disposables.Clear();

            Debug.Log($"Disposed {disposed} services.");
        }
    }
}
