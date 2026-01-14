using System.Collections.Generic;
using MonoFSM.Core;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonoFSM.InputAction
{
    [CreateAssetMenu(
        fileName = "InputActionDataCollection",
        menuName = "MonoFSM/Input/InputActionDataCollection"
    )]
    public class InputActionDataCollection : DynamicScriptableCollection
    {
        private static InputActionDataCollection _instance;

        /// <summary>
        ///     Singleton 實例，使用 lazy initialization
        /// </summary>
        public static InputActionDataCollection Instance =>
            _instance ??= LazyInstanceHelper.GetOrCreateInstance<InputActionDataCollection>();

        /// <summary>
        ///     手動指派實例（用於特殊情況）
        /// </summary>
        public void ManuallyAssign()
        {
            _instance = this;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // 自動設定目標類型為 InputActionData
            if (targetType.RestrictType != typeof(InputActionData))
                targetType.SetType(typeof(InputActionData));
        }

#if UNITY_EDITOR
        [Button]
        public void FindAllInputActionsInProject()
        {
            // 確保目標類型設定正確
            targetType.SetType(typeof(InputActionData));

            collection.Clear();
            var allProjectInputActions = AssetDatabase.FindAssets("t:InputActionData");

            for (var i = 0; i < allProjectInputActions.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(allProjectInputActions[i]);
                var inputActionData = AssetDatabase.LoadAssetAtPath<InputActionData>(path);
                if (inputActionData != null && FlagBelongThisCollection(inputActionData))
                    collection.Add(inputActionData);
            }

            // 自動分配 actionID
            AssignActionIDs();
            EditorUtility.SetDirty(this);
        }

        [Button]
        public void AssignActionIDs()
        {
            var inputActionList = GetInputActionDataList();
            Debug.Log("Assigning action IDs to InputActionData..." + inputActionList.Count);
            for (var i = 0; i < inputActionList.Count; i++)
            {
                inputActionList[i].actionID = i;
                EditorUtility.SetDirty(inputActionList[i]);
            }
        }
#endif

        /// <summary>
        ///     取得所有 InputActionData 的型別安全列表
        /// </summary>
        public List<InputActionData> GetInputActionDataList()
        {
            return GetCollectionAs<InputActionData>();
        }

        /// <summary>
        ///     根據 actionID 取得 InputActionData
        /// </summary>
        public InputActionData GetInputActionDataByID(int actionID)
        {
            foreach (var item in collection)
                if (item is InputActionData inputActionData && inputActionData.actionID == actionID)
                    return inputActionData;

            return null;
        }

#if UNITY_EDITOR
        public void AddInputActionData(InputActionData inputActionData)
        {
            if (inputActionData == null || collection.Contains(inputActionData))
                return;
            collection.Add(inputActionData);

            // 自動分配 actionID
            inputActionData.actionID = GetInputActionDataList().Count - 1;

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(inputActionData);
        }

        public override void OnHeavySceneSaving()
        {
            Debug.Log(
                "OnHeavySceneSaving: Finding all InputActionData in project and assigning action IDs."
            );
            FindAllInputActionsInProject();
            // 在Custom 保存時自動分配 actionID
            AssignActionIDs();
        }
#endif
    }
}
