using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 池管理器介面 - 定義池管理的核心操作，減少具體實作的耦合
/// </summary>
public interface IPoolManager
{
    /// <summary>
    /// 借用或實例化物件
    /// </summary>
    T BorrowOrInstantiate<T>(
        T obj,
        Vector3 position = default,
        Quaternion rotation = default,
        Transform parent = null,
        Action<PoolObject> handler = null
    )
        where T : MonoBehaviour;

    /// <summary>
    /// 歸還物件到池中
    /// </summary>
    void ReturnToPool(PoolObject instance);

    /// <summary>
    /// 通知池物件被銷毀
    /// </summary>
    void PoolObjectDestroyed(PoolObject poolObject);

    /// <summary>
    /// 重新計算池大小
    /// </summary>
    void ReCalculatePools();

    /// <summary>
    /// 驗證系統完整性
    /// </summary>
    bool ValidateSystemIntegrity();
}

/// <summary>
/// 物件池介面 - 定義單個池的核心操作
/// </summary>
public interface IObjectPool
{
    /// <summary>
    /// 池中的總物件數
    /// </summary>
    int TotalObjectCount { get; }

    /// <summary>
    /// 使用中的物件數
    /// </summary>
    int InUseObjectCount { get; }

    /// <summary>
    /// 可用的物件數
    /// </summary>
    int AvailableObjectCount { get; }

    /// <summary>
    /// 借用物件
    /// </summary>
    PoolObject Borrow(
        Vector3 position,
        Quaternion rotation,
        Transform parent = null,
        Action<PoolObject> beforeHandler = null
    );

    /// <summary>
    /// 歸還物件
    /// </summary>
    void ReturnToPool(PoolObject obj);

    /// <summary>
    /// 歸還所有物件
    /// </summary>
    void ReturnAllObjects();

    /// <summary>
    /// 縮放池到新的最大值
    /// </summary>
    void ScalePoolToNewMaximum();

    /// <summary>
    /// 銷毀池
    /// </summary>
    void DestroyPool();

    /// <summary>
    /// 檢查是否有受保護的物件
    /// </summary>
    bool HasProtectedObjects();

    /// <summary>
    /// 獲取受保護物件數量
    /// </summary>
    int GetProtectedObjectCount();
}

/// <summary>
/// 池物件介面 - 定義池物件的核心功能
/// </summary>
public interface IPoolableObject
{
    /// <summary>
    /// 原始預製體
    /// </summary>
    PoolObject OriginalPrefab { get; }

    /// <summary>
    /// 是否來自池
    /// </summary>
    bool IsFromPool { get; }

    /// <summary>
    /// 是否在場景中
    /// </summary>
    bool isOnScene { get; }

    /// <summary>
    /// 是否受保護
    /// </summary>
    bool IsProtected();

    /// <summary>
    /// 設置保護狀態
    /// </summary>
    void SetProtectionState(bool isProtected);

    /// <summary>
    /// 歸還到池
    /// </summary>
    void ReturnToPool();

    /// <summary>
    /// 重置Transform
    /// </summary>
    void TransformReset();

    /// <summary>
    /// 池物件準備完成回調
    /// </summary>
    void OnPrepare();
}

/// <summary>
/// 場景生命週期管理介面 - 減少與PoolManager的直接耦合
/// </summary>
public interface ISceneLifecycleManager
{
    /// <summary>
    /// 準備池物件實作
    /// </summary>
    void PreparePoolObjectImplementation(PoolObject obj);

    /// <summary>
    /// 重置重載
    /// </summary>
    void ResetReload(GameObject root);

    /// <summary>
    /// 場景銷毀前處理
    /// </summary>
    void OnBeforeDestroyScene(Scene scene);
}

/// <summary>
/// Transform重置管理介面
/// </summary>
public interface ITransformResetManager
{
    /// <summary>
    /// 重置Transform
    /// </summary>
    void ResetTransform(Transform transform, TransformResetHelper.TransformData resetData);

    /// <summary>
    /// 設置Transform並記錄重置資料
    /// </summary>
    TransformResetHelper.TransformData SetupTransform(
        Transform transform,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Transform parent
    );

    /// <summary>
    /// 捕獲Transform資料
    /// </summary>
    TransformResetHelper.TransformData CaptureTransformData(Transform transform);
}
