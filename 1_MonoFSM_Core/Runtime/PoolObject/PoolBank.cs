
using MonoFSM.Core;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;

public class PoolBank : MonoBehaviour,ISceneSavingCallbackReceiver,ISceneAwake
{
    [InlineButton("FindOrCreatePoolPrewarmData","Create")]
    public PoolPrewarmData BindPrewarmData;
    
    void FindOrCreatePoolPrewarmData()
    {
        FindPoolPrewarmDataFor(this);
    }

    public static PoolPrewarmData FindPoolPrewarmDataFor(PoolBank poolbank)
    {
        if (poolbank.BindPrewarmData != null)
        {
            return poolbank.BindPrewarmData;
        }

        var path = "Assets/15_PoolManagerPrewarm/" + poolbank.gameObject.scene.name + "_Prewarm.asset";
        string resourcesId = poolbank.gameObject.scene.name + "_Prewarm";

        PoolPrewarmData prewarmData = Resources.Load<PoolPrewarmData>(resourcesId);

        if (prewarmData == null)
        {
#if UNITY_EDITOR
            prewarmData = AssetDatabase.LoadAssetAtPath<PoolPrewarmData>(path);
#endif
        }
        else
        {
            
        }

        if (prewarmData == null)
        {
            Debug.LogWarning("create new prewarm by resources!");
#if UNITY_EDITOR
            prewarmData = ScriptableObject.CreateInstance<PoolPrewarmData>();
            AssetDatabase.CreateAsset(prewarmData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        poolbank.BindPrewarmData = prewarmData;
#if UNITY_EDITOR
        EditorUtility.SetDirty(poolbank);
#endif
        return prewarmData;
    }
    
    public static PoolPrewarmData FindGlobalPrewarmData()
    {
        var folderPath = "Assets/15_PoolManagerPrewarm";
        var assetName = "_Global_Prewarm.asset";
        var path = $"{folderPath}/{assetName}";
        string resourcesId = "_Global_Prewarm";

        // 先嘗試從 Resources 載入
        PoolPrewarmData prewarmData = Resources.Load<PoolPrewarmData>(resourcesId);

        if (prewarmData == null)
        {
#if UNITY_EDITOR
            // 若 Resources 沒有，再從 AssetDatabase 找
            prewarmData = AssetDatabase.LoadAssetAtPath<PoolPrewarmData>(path);
#endif
        }

        if (prewarmData == null)
        {
            Debug.LogWarning("create new prewarm by resources!");
#if UNITY_EDITOR
            // 資料夾不存在時建立
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = "Assets";
                string[] split = folderPath.Split('/');
                for (int i = 1; i < split.Length; i++)
                {
                    string currentPath = $"{parent}/{split[i]}";
                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(parent, split[i]);
                    }
                    parent = currentPath;
                }
            }

            // 建立 ScriptableObject 資產
            prewarmData = ScriptableObject.CreateInstance<PoolPrewarmData>();
            AssetDatabase.CreateAsset(prewarmData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        return prewarmData;
    }

    public void OnBeforeSceneSave()
    {
        FindGlobalPrewarmData();
        // 確保當前 PoolBank 的 BindPrewarmData 也被處理
        if (BindPrewarmData == null)
        {
            FindOrCreatePoolPrewarmData();
        }
    }

    public void EnterSceneAwake()
    {
        if (BindPrewarmData == null)
            return;
        
        Debug.Log("EnterSceneAwake?"+this.gameObject);

        Profiler.BeginSample("Prewarm GameLevel PoolObjects");
        PoolManager.Instance.PrepareGlobalPrewarmData();
        PoolManager.Instance.SetPrewarmData(BindPrewarmData,this);
        PoolManager.Instance.ReCalculatePools();
        Profiler.EndSample();
    }
}
