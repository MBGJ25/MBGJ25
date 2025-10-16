using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Marker interface for services that can be registered with the ServiceLocator.
/// Prevents accidental registration of MonoBehaviours and other inappropriate types.
/// </summary>
public interface IGameService { }

/// <summary>
/// Global service locator for managing game services.
/// Provides centralized access to core systems without tight coupling.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();
    private static bool _isInitialized;

    #region Initialization
    public static void Initialize()
    {
        if (_isInitialized)
        {
            LogWarning("ServiceLocator has already been initialized.");
            return;
        }

        _services.Clear();
        _isInitialized = true;
    }

    /// <summary>
    /// Check if the ServiceLocator has been initialized.
    /// </summary>
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// Register a service by its concrete type.
    /// </summary>
    public static void Register<T>(T service) where T : class, IGameService
    {
        ValidateService(service);

        var type = typeof(T);
        RegisterInternal(type, service);
    }

    /// <summary>
    /// Register a service implementation against an interface type.
    /// This is the preferred method for supporting Dependency Inversion.
    /// </summary>
    public static void Register<TInterface, TImplementation>(TImplementation service) 
        where TInterface : class, IGameService
        where TImplementation : class, TInterface
    {
        ValidateService(service);

        var type = typeof(TInterface);
        RegisterInternal(type, service);
    }

    private static void RegisterInternal(Type type, object service)
    {
        if (_services.ContainsKey(type))
        {
            LogWarning($"Service of type {type.Name} is already registered. Overwriting...");
        }

        _services[type] = service;
    }

    private static void ValidateService(object service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service), "Cannot register null service");
        }

        // CS NOTE: MonoBehaviours should be managed by Unity and not registered as services
        if (service is MonoBehaviour)
        {
            throw new ArgumentException(
                $"Cannot register MonoBehaviour '{service.GetType().Name}' as a service. " +
                "Services should be plain C# classes implementing IGameService.",
                nameof(service));
        }
    }

    #endregion

    #region Retrieval

    /// <summary>
    /// Get a registered service. Throws if service is not found.
    /// </summary>
    public static T Get<T>() where T : class, IGameService
    {
        var type = typeof(T);
        
        if (!_services.TryGetValue(type, out var service))
        {
            throw new KeyNotFoundException(
                $"Service of type {type.Name} is not registered. " +
                "Ensure the service is registered before attempting to retrieve it.");
        }

        return (T)service;
    }

    /// <summary>
    /// Try to get a registered service. Returns false if not found.
    /// Preferred over Get() when service existence is uncertain.
    /// </summary>
    public static bool TryGet<T>(out T service) where T : class, IGameService
    {
        service = null;
        var type = typeof(T);
        
        if (_services.TryGetValue(type, out var registeredService))
        {
            service = (T)registeredService;
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Get a service or return null if not registered.
    /// Alternative to TryGet for null-checking patterns.
    /// </summary>
    public static T GetOrNull<T>() where T : class, IGameService
    {
        TryGet<T>(out var service);
        return service;
    }

    /// <summary>
    /// Check if a service is registered without retrieving it.
    /// </summary>
    public static bool IsRegistered<T>() where T : class, IGameService
    {
        return _services.ContainsKey(typeof(T));
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Unregister a service and dispose it if it implements IDisposable.
    /// </summary>
    public static void Unregister<T>() where T : class, IGameService
    {
        var type = typeof(T);
        
        if (_services.TryGetValue(type, out var service))
        {
            // Dispose if the service implements IDisposable
            (service as IDisposable)?.Dispose();
            
            _services.Remove(type);
        }
    }

    /// <summary>
    /// Clear all registered services and dispose any IDisposable services.
    /// Call this during application shutdown or scene transitions if needed.
    /// </summary>
    public static void Clear()
    {
        // Dispose all IDisposable services before clearing
        foreach (var service in _services.Values)
        {
            (service as IDisposable)?.Dispose();
        }

        _services.Clear();
        _isInitialized = false;
    }

    #endregion

    #region Utilities

    private static void LogWarning(string message)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"[ServiceLocator] {message}");
#endif
    }

    #endregion
}