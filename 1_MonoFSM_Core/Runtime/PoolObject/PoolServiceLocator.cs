using UnityEngine;

/// <summary>
/// 池服務定位器 - 提供松耦合的服務訪問方式
/// 避免直接依賴具體的池管理器實作
/// </summary>
public static class PoolServiceLocator
{
    private static IPoolManager _poolManager;
    private static ISceneLifecycleManager _sceneLifecycleManager;
    private static ITransformResetManager _transformResetManager;

    /// <summary>
    /// 獲取池管理器服務
    /// </summary>
    public static IPoolManager PoolManager
    {
        get
        {
            if (_poolManager == null)
            {
                _poolManager = Object.FindObjectOfType<PoolManager>();
                if (_poolManager == null)
                {
                    PoolLogger.LogError("No PoolManager found in scene");
                }
            }
            return _poolManager;
        }
    }

    /// <summary>
    /// 獲取場景生命週期管理器服務
    /// </summary>
    public static ISceneLifecycleManager SceneLifecycleManager
    {
        get
        {
            if (_sceneLifecycleManager == null)
            {
                _sceneLifecycleManager = new SceneLifecycleManagerImpl();
            }
            return _sceneLifecycleManager;
        }
    }

    /// <summary>
    /// 獲取Transform重置管理器服務
    /// </summary>
    public static ITransformResetManager TransformResetManager
    {
        get
        {
            if (_transformResetManager == null)
            {
                _transformResetManager = new TransformResetManagerImpl();
            }
            return _transformResetManager;
        }
    }

    /// <summary>
    /// 註冊池管理器服務
    /// </summary>
    public static void RegisterPoolManager(IPoolManager poolManager)
    {
        _poolManager = poolManager;
        PoolLogger.LogInfo("PoolManager service registered");
    }

    /// <summary>
    /// 註冊場景生命週期管理器服務
    /// </summary>
    public static void RegisterSceneLifecycleManager(ISceneLifecycleManager sceneLifecycleManager)
    {
        _sceneLifecycleManager = sceneLifecycleManager;
        PoolLogger.LogInfo("SceneLifecycleManager service registered");
    }

    /// <summary>
    /// 註冊Transform重置管理器服務
    /// </summary>
    public static void RegisterTransformResetManager(ITransformResetManager transformResetManager)
    {
        _transformResetManager = transformResetManager;
        PoolLogger.LogInfo("TransformResetManager service registered");
    }

    /// <summary>
    /// 清除所有服務註冊
    /// </summary>
    public static void ClearServices()
    {
        _poolManager = null;
        _sceneLifecycleManager = null;
        _transformResetManager = null;
        PoolLogger.LogInfo("All pool services cleared");
    }

    /// <summary>
    /// 檢查服務是否可用
    /// </summary>
    public static bool IsPoolManagerAvailable => _poolManager != null;
    public static bool IsSceneLifecycleManagerAvailable => _sceneLifecycleManager != null;
    public static bool IsTransformResetManagerAvailable => _transformResetManager != null;
}

/// <summary>
/// 場景生命週期管理器實作 - 包裝靜態方法
/// </summary>
internal class SceneLifecycleManagerImpl : ISceneLifecycleManager
{
    public void PreparePoolObjectImplementation(PoolObject obj)
    {
        MonoFSM.Runtime.SceneLifecycleManager.PreparePoolObjectImplementation(obj);
    }

    public void ResetReload(GameObject root)
    {
        MonoFSM.Runtime.SceneLifecycleManager.ResetReload(root);
    }

    public void OnBeforeDestroyScene(UnityEngine.SceneManagement.Scene scene)
    {
        MonoFSM.Runtime.SceneLifecycleManager.OnBeforeDestroyScene(scene);
    }
}

/// <summary>
/// Transform重置管理器實作 - 包裝靜態方法
/// </summary>
internal class TransformResetManagerImpl : ITransformResetManager
{
    public void ResetTransform(Transform transform, TransformResetHelper.TransformData resetData)
    {
        TransformResetHelper.ResetTransform(transform, resetData);
    }

    public TransformResetHelper.TransformData SetupTransform(Transform transform, Vector3 position, 
        Quaternion rotation, Vector3 scale, Transform parent)
    {
        return TransformResetHelper.SetupTransform(transform, position, rotation, scale, parent);
    }

    public TransformResetHelper.TransformData CaptureTransformData(Transform transform)
    {
        return TransformResetHelper.CaptureTransformData(transform);
    }
}