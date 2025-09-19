using System;
using UnityEngine;

//TODO: 要把場上的物件回到原本的位置
/// <summary>
/// Transform 重置輔助類 - 簡化和統一 Transform 重置邏輯
/// </summary>
public static class TransformResetHelper
{
    /// <summary>
    /// Transform 重置資料結構
    /// </summary>
    [Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Transform parent;

        public TransformData(Transform transform)
        {
            position = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;
            parent = transform.parent;
        }

        public static TransformData Create(
            Vector3 pos,
            Quaternion rot,
            Vector3 scale,
            Transform parent
        )
        {
            return new TransformData
            {
                position = pos,
                rotation = rot,
                scale = scale,
                parent = parent,
            };
        }
    }

    /// <summary>
    /// 重置 Transform 到指定狀態
    /// </summary>
    public static void ResetTransform(Transform transform, TransformData resetData)
    {
        if (transform == null)
            return;

        transform.SetParent(resetData.parent);
        transform.localPosition = resetData.position;
        transform.localRotation = resetData.rotation;
        transform.localScale = resetData.scale;
    }

    /// <summary>
    /// 設置 Transform 並返回重置資料
    /// </summary>
    public static TransformData SetupTransform(
        Transform transform,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Transform parent
    )
    {
        if (transform == null)
            return default;

        transform.SetParent(parent);
        transform.SetPositionAndRotation(position, rotation);
        // Debug.Log(
        //     $"Setting Transform: Position={position}, Rotation={rotation}, Scale={scale}, Parent={parent?.name ?? "null"}",
        //     transform);
        // 返回本地座標作為重置資料
        return new TransformData
        {
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = scale,
            parent = parent,
        };
    }

    /// <summary>
    /// 記錄當前 Transform 狀態
    /// </summary>
    public static TransformData CaptureTransformData(Transform transform)
    {
        return transform != null ? new TransformData(transform) : default;
    }

    /// <summary>
    /// 從 PoolObject 的原始 Prefab 獲取預設 Scale
    /// </summary>
    public static Vector3 GetDefaultScale(PoolObject poolObject)
    {
        return poolObject?.OriginalPrefab?.transform.localScale ?? Vector3.one;
    }

    /// <summary>
    /// 為池物件設置完整的 Transform 配置
    /// </summary>
    public static TransformData SetupPoolObjectTransform(
        PoolObject poolObject,
        Vector3 position,
        Quaternion rotation,
        Transform parent
    )
    {
        if (poolObject == null)
            return default;

        var defaultScale = GetDefaultScale(poolObject);
        return SetupTransform(poolObject.transform, position, rotation, defaultScale, parent);
    }
}
