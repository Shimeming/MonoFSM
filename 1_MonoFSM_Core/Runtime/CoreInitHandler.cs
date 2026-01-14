using System;
using _1_MonoFSM_Core.Runtime._3_FlagData;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CoreInitHandler
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BeforeGameLevelLoadAndPrepareCores()
    {
        LoadCore();
        LoadAllFlags();
    }

    private static void LoadAllFlags()
    {
        var allFlagCollection = AllFlagCollection.Instance;
        Debug.Log("Loading AllFlagCollection..." + allFlagCollection.Flags.Count, allFlagCollection);
        allFlagCollection.AllFlagAwake(TestMode.Production);
    }

    public static ApplicationCore LoadCore()
    {
        if (ApplicationCore.IsAvailable())
            return ApplicationCore.Instance;
        var applicationCoreCandidate =
            Resources.Load<GameObject>("Configs/ApplicationCore_Custom"); // Custom 版優先
        if (applicationCoreCandidate == null)
            applicationCoreCandidate =
                Resources.Load<GameObject>("Configs/ApplicationCore"); //Fallback 回原版
        try
        {
           //fixme: 要放在package裡面?
            if(applicationCoreCandidate == null)
            {
                Debug.LogError("Can't found: Configs/ApplicationCore.prefab, make sure you have it in the Resources folder");
                return null;
            }
            applicationCoreCandidate.gameObject.SetActive(false);
            var applicationCoreInstance = Object.Instantiate(applicationCoreCandidate);

            //Auto Reference & Awake
            AutoAttributeManager.AutoReferenceAllChildren(applicationCoreInstance);
            applicationCoreCandidate.gameObject.SetActive(true);
            applicationCoreInstance.gameObject.SetActive(true);

            Object.DontDestroyOnLoad(applicationCoreInstance);
            return applicationCoreInstance.GetComponent<ApplicationCore>();
        }
        catch (Exception e)
        {
            Debug.LogError("Something wrong: Configs/ApplicationCore.prefab",applicationCoreCandidate);
            return null;
        }
    }
}
