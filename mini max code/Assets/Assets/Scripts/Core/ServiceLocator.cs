using System;
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Game
{
    /// <summary>
    /// Simple service locator for accessing services from non-MonoBehaviour classes.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Registers a service factory.
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () => factory();
        }

        /// <summary>
        /// Gets a registered service instance.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }

            if (_factories.TryGetValue(typeof(T), out var factory))
            {
                var instance = factory() as T;
                _services[typeof(T)] = instance;
                return instance;
            }

            Debug.LogWarning($"Service of type {typeof(T).Name} not found in ServiceLocator");
            return null;
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T)) || _factories.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _factories.Clear();
        }

        /// <summary>
        /// Unregisters a service.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
            _factories.Remove(typeof(T));
        }
    }
}