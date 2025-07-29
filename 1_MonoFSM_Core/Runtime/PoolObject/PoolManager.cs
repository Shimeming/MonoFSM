using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Auto.Utils;
using Cysharp.Threading.Tasks;
using MonoFSMCore.Runtime.LifeCycle;
using MonoFSM.AddressableAssets;
using MonoFSM.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;


public delegate void BeforeActiveHandler(PoolObject obj);

/// <summary>
/// FIXME: 重寫！
/// </summary>
public class PoolManager : SingletonBehaviour<PoolManager>
{
    public static void PreparePoolObjectImplementation(PoolObject obj)
    {
        if (obj.TryGetComponent<PrefabSerializeCache>(out var cache))
        {
            cache.RestoreReferenceCache();
        }
        else
        {
            AutoAttributeManager.AutoReferenceAllChildren(obj.gameObject); //
        }

        HandleGameLevelAwakeReverse(obj.gameObject);
        HandleGameLevelAwake(obj.gameObject);
        HandleGameLevelStartReverse(obj.gameObject);
        HandleGameLevelStart(obj.gameObject);
        LevelResetChildrenReload(obj.gameObject);
        obj.OnPrepare();
        // obj.PoolObjectResetAndStart();
    }

    // public static void HandleGameLevelConfigSetting(MonoBehaviour level)
    // {
    //     var ILevelConfigs = new List<ILevelConfig>(level.GetComponentsInChildren<ILevelConfig>(true));
    //
    //     foreach (var item in ILevelConfigs)
    //     {
    //         if (item == null)
    //             continue;
    //         try
    //         {
    //             item.SetLevelConfig();
    //         }
    //         catch (Exception e)
    //         {
    //             if (item is MonoBehaviour)
    //                 Debug.LogError(e.StackTrace, item as MonoBehaviour);
    //             else
    //                 Debug.LogError(e.StackTrace);
    //         }
    //     }
    // }

    //LevelReset, 重職關卡時，一換scene時
    //開放世界用不到？死掉復活？
    public  static void ResetReload(GameObject root)
    {
        //每次重置都要做的, LevelReset, LevelResetAfter?
        LevelResetChildrenReload(root);
        //大便！
        // HandleEnterLevelReset(root);
        //FIXME: 再重整一下
        LevelResetStart(root);
    }


    public static void OnBeforeDestroyScene(Scene s)
    {
        
        foreach (var g in s.GetRootGameObjects())
        {
            foreach (var rcgOnDestroy in g.GetComponentsInChildren<ISceneDestroy>(true))
            {
                try
                {
                    rcgOnDestroy.OnSceneDestroy();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

            }
        }
    }


    public static void LevelResetChildrenReload(GameObject gObj)
    {
        var levelResets = new List<IResetStateRestore>();
        gObj.GetComponentsInChildren(true, levelResets);
        levelResets.Reverse();
        foreach (var item in levelResets)
        {
            if (item == null)
                continue;
            try
            {
                item.ResetStateRestore();
            }
            catch (Exception e)
            {
                if (item is MonoBehaviour behaviour)
                    Debug.LogError(e.Message + "\n" + e.StackTrace, behaviour);
                else
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
        }
    }

    /// <summary>
    /// 摸別人，綁定事件
    /// </summary>
    /// <param name="gObj"></param>
    //最新規，九日沒在用？
    public static void LevelResetStart(GameObject gObj) //由下往上
    {
        var levelResets = new List<IResetStart>();
        gObj.GetComponentsInChildren(true, levelResets);
        levelResets.Reverse();
        foreach (var item in levelResets)
        {
            if (item == null)
                continue;
            // try
            // {
            item.ResetStart();
            // }
            // catch (Exception e)
            // {
            //     if (item is MonoBehaviour behaviour)
            //         Debug.LogError(e.Message + "\n" + e.StackTrace, behaviour);
            //     else
            //         Debug.LogError(e.Message + "\n" + e.StackTrace);
            // }
        }
    }

    public static void HandleGameLevelAwake(GameObject level) //FIXME: 不要放這？
    {
        var levelAwakes = new List<ISceneAwake>(level.GetComponentsInChildren<ISceneAwake>(true));
        // ILevelAwakes.Reverse();
        foreach (var item in levelAwakes)
        {
            if (item == null)
                continue;
            Profiler.BeginSample(item.ToString());
            try
            {
                item.EnterSceneAwake();
            }
            catch (Exception e)
            {
                if (item is MonoBehaviour behaviour)
                    Debug.LogError(e.StackTrace, behaviour);
                else
                    Debug.LogError(e.StackTrace);
            }

            Profiler.EndSample();
        }
    }

    // public static void HandleEnterLevelReset(GameObject level)
    // {
    //     var ILevelResets = new List<IResetter>(level.GetComponentsInChildren<IResetter>(true));
    //     // ILevelAwakes.Reverse();
    //     foreach (var item in ILevelResets)
    //     {
    //         if (item == null)
    //             continue;
    //         try
    //         {
    //             item.EnterLevelReset();
    //         }
    //         catch (Exception e)
    //         {
    //             if (item is MonoBehaviour behaviour)
    //                 Debug.LogError(e.StackTrace, behaviour);
    //             else
    //                 Debug.LogError(e.StackTrace);
    //         }
    //     }
    // }


    public static void HandleGameLevelAwakeReverse(GameObject level)
    {
        var ILevelAwakes = new List<ISceneAwakeReverse>(level.GetComponentsInChildren<ISceneAwakeReverse>(true));
        ILevelAwakes.Reverse();
        foreach (var item in ILevelAwakes)
        {
            if (item == null)
                continue;
            Profiler.BeginSample(item.ToString());
            try
            {
                item.EnterSceneAwakeReverse();
            }
            catch (Exception e)
            {
                if (item is MonoBehaviour)
                    Debug.LogError(e.StackTrace, item as MonoBehaviour);
                else
                    Debug.LogError(e.StackTrace);
            }

            Profiler.EndSample();
        }
    }

    public static void HandleGameLevelStart(GameObject level)
    {
        var levelStarts = new List<ISceneStart>(level.GetComponentsInChildren<ISceneStart>(true));

        //巢狀RCGArgEventBinder  要從下面往上組
        // ILevelStarts.Reverse();

        foreach (var item in levelStarts)
        {
            if (item == null)
                continue;
            try
            {
                item.EnterSceneStart();
            }
            catch (Exception e)
            {
                if (item is MonoBehaviour)
                    Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                else
                    Debug.LogError(e.StackTrace);
            }
        }
    }

    public static void HandleGameLevelStartReverse(GameObject level)
    {
        var ILevelStarts = new List<ISceneStartReverse>(level.GetComponentsInChildren<ISceneStartReverse>(true));

        //巢狀RCGArgEventBinder  要從下面往上組
        ILevelStarts.Reverse();

        foreach (var item in ILevelStarts)
        {
            if (item == null)
                continue;
            try
            {
                item.EnterSceneStartReverse();
            }
            catch (Exception e)
            {
                if (item is MonoBehaviour)
                    Debug.LogError(e.StackTrace, item as MonoBehaviour);
                else
                    Debug.LogError(e.StackTrace);
            }
        }
    }

    // public bool IsReady = false;
    [Header("PrewarmData Logger")] 
    public Transform poolbjects;
    public PoolPrewarmData prewarmDataLogger;
    public PoolPrewarmData globalPrewarmDataLogger;

    
    

   


    public void RegisterPoolPrewarmData(MonoBehaviour requester, PoolPrewarmData data)
    {
        if (data == null)
            return;

        foreach (var entry in data.objectEntries)
            records.Add(new PoolObjectRequestRecords(requester, entry.prefab,
                entry.DefaultMaximumCount));
    }


    private void ReCalculatePoolObjectEntries()
    {
        foreach (var loadedAsset in this.allLoadedRCGRefereces)
        {
            loadedAsset.Release();
        }

        allLoadedRCGRefereces.Clear();

        //沒有人需要用了。
        for (int i = records.Count - 1; i >= 0; i--)
        {
            if (records[i]._requester == null)
            {
                records[i].Clear();
                records.RemoveAt(i);
            }
        }

        foreach (var e in PoolObjectEntries)
        {
            e.Clear();
        }

        PoolObjectEntries.Clear();

        for (var i = 0; i < records.Count; i++)
            AddEntry(PoolObjectEntries, records[i]._prefab, records[i]._count);
    }

    public void PoolObjectDestroyed(PoolObject poolobj)
    {
        if (PoolDictionary.ContainsKey(poolobj.OriginalPrefab))
            PoolDictionary[poolobj.OriginalPrefab].PoolObjectOnDestroySignal(poolobj);
    }

    private void AddEntry(List<PoolObjectEntry> list, PoolObject poolObject, int count)
    {
        for (var i = 0; i < list.Count; i++)
            if (list[i].prefab == poolObject)
            {
                list[i].DefaultMaximumCount += count;
                return;
            }

        var entry = new PoolObjectEntry();

        entry.prefab = poolObject;
        entry.DefaultMaximumCount = count;

        list.Add(entry);
    }

    public List<PoolObjectRequestRecords> records = new();

    private List<PoolObjectEntry> PoolObjectEntries;

    [Header("Run Time Data")] public Dictionary<PoolObject, ObjectPool> PoolDictionary;
    public List<ObjectPool> allPools;
    

    protected void Awake()
    {
        PoolObjectEntries = new List<PoolObjectEntry>();
        PoolDictionary = new Dictionary<PoolObject, ObjectPool>();
        allPools = new List<ObjectPool>();

        poolbjects = new GameObject("PoolObjects").transform;
        poolbjects.parent = transform;
        poolbjects.localPosition = Vector3.zero;
        poolbjects.gameObject.SetActive(false);
        
        
    }

    public void PrepareGlobalPrewarmData()
    {
        if (this.globalPrewarmDataLogger == null)
        {
            this.globalPrewarmDataLogger = PoolBank.FindGlobalPrewarmData();
            //CleanUp 沒用的資料
#if UNITY_EDITOR
            PoolManager.Instance.globalPrewarmDataLogger.objectEntries.RemoveAll((a) => a.prefab == null);
            PoolManager.Instance.globalPrewarmDataLogger.objectEntries.RemoveAll((a) => !a.prefab.IsGlobalPool);
            EditorUtility.SetDirty(PoolManager.Instance.globalPrewarmDataLogger);
#endif
            
            this.globalPrewarmDataLogger.PrewarmObjects(this,this);
        }
        
        
    }

    public void SetPrewarmData(PoolPrewarmData prewarmData,PoolBank bank)
    {
        this.prewarmDataLogger = prewarmData;
        prewarmData.PrewarmObjects(this, bank);
    }
    

    //
    //private bool _poolCreated = false;

    public GameObject BorrowOrInstantiate(GameObject obj, Vector3 position = default, Quaternion rotation = default,
        Transform parent = null, Action<PoolObject> handler = null)
    {
        var hasRequest = obj.TryGetComponent(out PoolRequest request);
        var hasPoolObj = obj.TryGetComponent<PoolObject>(out var poolObj);

        if (hasRequest)
        {
            return Borrow(request.PoolObjectRequests.prefab, position, rotation, parent, handler).gameObject;
        }
        else if (hasPoolObj)
        {
            return Borrow(poolObj, position, rotation, parent, handler).gameObject;
        }
        else
        {
            Debug.LogError("RunTime Instantiate");
            return Instantiate(obj, position, rotation, parent);
        }
    }

    public async UniTask<GameObject> BorrowOrInstantiateRcgAssetReference(RCGAssetReference obj,
        Vector3 position = default, Quaternion rotation = default,
        Transform parent = null, Action<PoolObject> handler = null)
    {
        GameObject poolObject = null;
        if (prewarmDataLogger != null)
        {
            poolObject = prewarmDataLogger.TryFindPrefab(obj.AssetReference);

            if (poolObject != null)
                return BorrowOrInstantiate(poolObject, position, rotation, parent, handler);
        }


#if UNITY_EDITOR
        Debug.LogError("Please Prewarm this:" + obj.editorAsset, obj.editorAsset);
#endif
        Debug.LogError("Please Prewarm this:" + obj.AssetReference.AssetGUID);

        if (allLoadedRCGRefereces.Contains(obj) == false)
        {
            allLoadedRCGRefereces.Add(obj);
        }

        poolObject = await obj.GetAssetAsync<GameObject>();

        if (this.prewarmDataLogger != null)
            this.prewarmDataLogger.RegisterEntry(poolObject, obj.AssetReference);

        return BorrowOrInstantiate(poolObject, position, rotation, parent, handler);

        return null;
    }

    public List<RCGAssetReference> allLoadedRCGRefereces = new List<RCGAssetReference>();

    public T BorrowOrInstantiate<T>(T obj, Transform parent) where T : MonoBehaviour
    {
        return BorrowOrInstantiate(obj, Vector3.zero, Quaternion.identity, parent);
    }

    public T BorrowOrInstantiate<T>(T obj, Vector3 position = default, Quaternion rotation = default,
        Transform parent = null, Action<PoolObject> handler = null) where T : MonoBehaviour
    {
        if (obj == null)
        {
            Debug.LogError("BorrowOrInstantiate: obj is null");
            return null;
        }

        if (obj.TryGetComponent<PoolObject>(out var poolObj))
        {
            return Borrow(poolObj, position, rotation, parent, handler).GetComponent<T>();
        }
        else
        {
            Debug.LogError("It's not a pool object...", obj);
            return Instantiate(obj, position, rotation, parent);
        }
    }

    private PoolObject Borrow(PoolObject prefab, Vector3 position, Quaternion rotation, Transform parent = null,
        Action<PoolObject> handler = null)
    {
        //FIXME: 這很糟
        if (_recalculating)
        {
            Debug.LogError("為何會在ReCalculating的時候借東西？" + prefab, prefab);
            return null;
        }


        if (prefab.UseSceneAsPool)
        {
            prefab.TransformReset();
            //FIXME:這裡又跑一次...
            prefab.PoolObjectResetAndStart();

            var transform1 = prefab.transform;
            transform1.parent = parent;
            transform1.rotation = rotation;
            transform1.position = position;


            prefab.OnBorrowFromPool(null); //OnPoolReset

            // 這會影響設定黨 樹上有結構

            //FIXME: 為什麼要做這件事？？

            //先reset, 後面才
            prefab.OverrideTransformSetting(position, rotation, parent, prefab.transform.localScale);
            prefab.TransformReset();

            prefab.gameObject.SetActive(true);
            // prefab.ResetAnim();
            //FIXME:這裡又跑一次...
            prefab.PoolObjectResetAndStart();

            handler?.Invoke(prefab); //TODO: 這個比較後面call的？

            // Debug.Log("Use Scene As Pool");

            return prefab;
        }
        // prefab

        if (!PoolDictionary.ContainsKey(prefab))
        {
#if RCG_DEV
            //FIXME: 先拿掉的註解
            // Debug.LogError("PoolManager: " + prefab.name + " is not in the pool dictionary");
#endif
            AddAPool(prefab);
            PoolDictionary[prefab].UpdatePoolEntry();
        }

        return PoolDictionary[prefab].Borrow(position, rotation, parent, handler);
    }

    public void ReturnToPool(PoolObject prefab)
    {
        PoolDictionary[prefab.OriginalPrefab].ReturnToPool(prefab);
    }

    //FIXME: 
    public void ReturnToPool(MonoPoolObj obj)
    {
        // PoolDictionary[obj.OriginalPrefab].ReturnToPool(prefab);
        throw new NotImplementedException("ReturnToPool for MonoPoolObj is not implemented yet.");
    }
    
    //

    private bool _recalculating = false;
    
    //FIXME: 有可能保留AG_S2的pool, 或是在特定scene不做清除之類的動作
    public void ReCalculatePools()
    {
        _recalculating = true;
        
        Profiler.BeginSample("ReCalculatePools");

        //把null game level 清掉了
        ReCalculatePoolObjectEntries();

        Profiler.BeginSample("DestroyNonUsedPool");
        for (var i = allPools.Count - 1; i >= 0; i--)
        {
            var currentPool = allPools[i];
            //檢查是否應該保持池存活（考慮明確請求和受保護物件）
            var entry = ShouldKeepPoolAlive(currentPool._prefab);

            //移除沒用到的pool
            if (entry == null)
            {
                // 額外的安全檢查：確保沒有受保護物件被意外銷毀
                if (currentPool.HasProtectedObjects())
                {
                    var protectedCount = currentPool.GetProtectedObjectCount();
                    Debug.LogError($"[PoolManager] 嘗試銷毀包含 {protectedCount} 個受保護物件的池: {currentPool._prefab.name}！已阻止銷毀。");
                    
                    // 創建緊急 Entry 以保護這些物件
                    var emergencyEntry = new PoolObjectEntry
                    {
                        prefab = currentPool._prefab,
                        DefaultMaximumCount = protectedCount
                    };
                    allPools[i]._bindingEntry = emergencyEntry;
                    continue;
                }
                
                Debug.Log($"[PoolManager] 銷毀未使用的池: {currentPool._prefab.name}");
                PoolDictionary.Remove(currentPool._prefab);
                allPools[i].DestroyPool();
                allPools[i] = null;
                allPools.RemoveAt(i);
            }
            //綁定新的Entry
            else
            {
                allPools[i]._bindingEntry = entry;
            }
        }

        Profiler.EndSample();

        Profiler.BeginSample("ScalePoolToNewMaximum");
        for (var i = 0; i < allPools.Count; i++)
            allPools[i].ScalePoolToNewMaximum();
        Profiler.EndSample();

        Profiler.BeginSample("Add New Entry");
        //增加新的沒看過的Entry
        for (var i = 0; i < PoolObjectEntries.Count; i++)
        {
            if (PoolDictionary.ContainsKey(PoolObjectEntries[i].prefab) == false)
            {
                var pool = new ObjectPool(PoolObjectEntries[i], this);
                allPools.Add(pool);
                pool.Init();
                PoolDictionary.Add(PoolObjectEntries[i].prefab, pool);
            }
        }

        Profiler.EndSample();

        // sw.Stop();
        // Debug.Log("[PoolManager] Prepare ElapsedMilliseconds:" + sw.ElapsedMilliseconds);
        // UnityEngine.Debug.LogFormat("[Auto] Assigned <color={5}><b>{4}/{2}</b></color> [Auto*] variables in <color=#cc3300><b>{3} Milliseconds </b></color> - Analized {0} MonoBehaviours and {1} variables",
        //    monoBehavioursInSceneWithAuto.Count(), variablesAnalized, variablesWithAuto, sw.ElapsedMilliseconds, autoVarialbesAssigned_count, autoVarialbesAssigned_count + autoVarialbesNotAssigned_count, result_color);
        Profiler.EndSample();
        
        _recalculating = false;
    }

    public void ReturnAllObjects(Scene withScene)
    {
        // Debug.Log("Return All PoolObj With Scene");
        for (var i = 0; i < allPools.Count; i++)
            allPools[i].ReturnAllObjects(withScene);
    }

    public delegate bool PoolPredicate(PoolObject p);

    public void ReturnAllObjects(Scene withScene, PoolPredicate poolPredicate)
    {
        var StillOnUses = new List<PoolObject>();

        for (var i = 0; i < allPools.Count; i++)
        {
            if (poolPredicate(allPools[i]._prefab))
            {
                allPools[i].ReturnAllObjects(withScene);
            }
        }
    }

    private void AddAPool(PoolObject obj)
    {
        if (PoolDictionary.ContainsKey(obj))
            return;

        var entry = new PoolObjectEntry();
        entry.prefab = obj;
        entry.DefaultMaximumCount = 1;

        var pool = new ObjectPool(entry, this);

        allPools.Add(pool);
        pool.Init();

        PoolDictionary.Add(obj, pool);
    }

    /// <summary>
    /// 檢查是否應該保持池存活
    /// 考慮兩個因素：1) 明確的請求 2) 受保護的物件
    /// </summary>
    public PoolObjectEntry ShouldKeepPoolAlive(PoolObject prefab)
    {
        // 首先檢查是否有明確的請求
        for (var i = PoolObjectEntries.Count - 1; i >= 0; i--)
            if (PoolObjectEntries[i].prefab == prefab)
                return PoolObjectEntries[i];

        // 如果沒有明確請求，檢查是否有受保護的物件
        if (PoolDictionary.ContainsKey(prefab))
        {
            var pool = PoolDictionary[prefab];
            if (pool.HasProtectedObjects())
            {
                int protectedCount = pool.GetProtectedObjectCount();
                Debug.Log($"[PoolManager] 保持池 {prefab.name} 存活，因為有 {protectedCount} 個受保護物件");
                
                // 為受保護物件創建一個虛擬的 Entry
                return new PoolObjectEntry
                {
                    prefab = prefab,
                    DefaultMaximumCount = Mathf.Max(1, protectedCount) // 至少保持足夠容納受保護物件的大小
                };
            }
        }

        return null;
    }
    
    /// <summary>
    /// 舊方法名稱的向後兼容性（已過時）
    /// </summary>
    [System.Obsolete("Use ShouldKeepPoolAlive instead")]
    public PoolObjectEntry isInRequest(PoolObject prefab)
    {
        return ShouldKeepPoolAlive(prefab);
    }
    
    /// <summary>
    /// 獲取整個池系統的受保護物件狀態報告
    /// </summary>
    public string GetSystemProtectedObjectsReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Pool System Protected Objects Report ===");
        
        int totalProtected = 0;
        int poolsWithProtected = 0;
        
        foreach (var pool in allPools)
        {
            if (pool != null && pool.HasProtectedObjects())
            {
                poolsWithProtected++;
                int protectedCount = pool.GetProtectedObjectCount();
                totalProtected += protectedCount;
                
                report.AppendLine($"Pool '{pool._prefab.name}': {protectedCount} protected objects");
            }
        }
        
        report.AppendLine($"Summary: {totalProtected} protected objects across {poolsWithProtected} pools (total pools: {allPools.Count})");
        return report.ToString();
    }
    
    /// <summary>
    /// 驗證整個池系統的完整性
    /// </summary>
    public bool ValidateSystemIntegrity()
    {
        bool allValid = true;
        
        Debug.Log("[PoolManager] Starting system integrity validation...");
        
        foreach (var pool in allPools)
        {
            if (pool != null)
            {
                // Basic validation: check if pool has protected objects and report
                if (pool.HasProtectedObjects())
                {
                    int protectedCount = pool.GetProtectedObjectCount();
                    Debug.Log($"[PoolManager] Pool {pool._prefab.name} has {protectedCount} protected objects");
                }
                
                // Validate basic object counts
                int totalObjects = pool.AllObjs?.Count ?? 0;
                int inUseObjects = pool.OnUseObjs?.Count ?? 0;
                int availableObjects = pool.DisabledObjs?.Count ?? 0;
                
                if (totalObjects != inUseObjects + availableObjects)
                {
                    Debug.LogError($"[PoolManager] Pool {pool._prefab.name} has inconsistent object counts: Total={totalObjects}, InUse={inUseObjects}, Available={availableObjects}");
                    allValid = false;
                }
            }
        }
        
        if (allValid)
        {
            Debug.Log("[PoolManager] System integrity validation passed");
        }
        else
        {
            Debug.LogError("[PoolManager] System integrity validation failed - check individual pool errors above");
        }
        
        return allValid;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only method to log detailed pool status
    /// </summary>
    [UnityEditor.MenuItem("Tools/Pool System/Log Protected Objects Report")]
    public static void LogProtectedObjectsReport()
    {
        if (Instance != null)
        {
            Debug.Log(Instance.GetSystemProtectedObjectsReport());
        }
        else
        {
            Debug.LogWarning("PoolManager instance not found");
        }
    }
    
    [UnityEditor.MenuItem("Tools/Pool System/Validate System Integrity")]
    public static void ValidateSystem()
    {
        if (Instance != null)
        {
            Instance.ValidateSystemIntegrity();
        }
        else
        {
            Debug.LogWarning("PoolManager instance not found");
        }
    }
#endif

}