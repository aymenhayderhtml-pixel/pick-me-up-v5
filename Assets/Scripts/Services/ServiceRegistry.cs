using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceRegistry : MonoBehaviour
{
    private static ServiceRegistry _instance;
    public static ServiceRegistry Instance => _instance;

    private Dictionary<Type, object> _services;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Auto-wire / initialize internal state
        _services = new Dictionary<Type, object>();
    }

    public void Register<T>(T service)
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
        }
    }

    public T Resolve<T>()
    {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object service))
        {
            return (T)service;
        }
        
        throw new Exception($"Service of type {type.Name} is not registered in ServiceRegistry.");
    }
    
    public bool HasService<T>()
    {
        return _services.ContainsKey(typeof(T));
    }
}
