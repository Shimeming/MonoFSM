using System.Collections.Generic;
using System.IO;
using _1_MonoFSM_Core.Runtime._3_FlagData;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoFSM.Core
{
    public abstract class DynamicScriptableCollection : AbstractSOConfig
    {
        [Header("目標類型設定")]
        [PropertyOrder(-1)]
        public MySerializedType<ScriptableObject> targetType = new();

        protected virtual void OnValidate()
        {
            targetType._bindObject = this;
        }

        protected virtual bool FlagBelongThisCollection(ScriptableObject obj) //用一些條件去篩掉特定flag, 例如要做百科分類
        {
            return true;
        }

        /// <summary>
        ///     取得指定類型的集合項目
        /// </summary>
        public List<T> GetCollectionAs<T>()
            where T : ScriptableObject
        {
            var result = new List<T>();
            foreach (var item in collection)
                if (item is T typedItem)
                    result.Add(typedItem);

            return result;
        }

#if UNITY_EDITOR
        [Button("Clear")]
        public void Clear()
        {
            collection.Clear();
            EditorUtility.SetDirty(this);
        }

        [Button("Find Under Folder With OverrideTypeName")]
        public void FindUnderFolder()
        {
            var type = targetType.RestrictType;
            if (type == null)
            {
                Debug.LogError("請先設定目標類型");
                return;
            }

            collection = ScriptableHelper.FindAllSO(this, type);
            EditorUtility.SetDirty(this);
        }

        [TextArea]
        public string note;

        [Button("FindAllFlags")]
        public void FindAllFlags()
        {
            var type = targetType.RestrictType;
            if (type == null)
            {
                Debug.LogError("請先設定目標類型");
                return;
            }

            collection.Clear();
            var myPath = AssetDatabase.GetAssetPath(this);
            var dirPath = Path.GetDirectoryName(myPath);
            var scriptables = AssetDatabase.FindAssets("t:" + type.Name, new[] { dirPath });

            foreach (var guid in scriptables)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (
                    obj != null
                    && type.IsAssignableFrom(obj.GetType())
                    && FlagBelongThisCollection(obj)
                )
                    collection.Add(obj);
            }

            EditorUtility.SetDirty(this);
        }
#endif

        [FormerlySerializedAs("data")]
        [FormerlySerializedAs("gameFlagDataList")]
        [InlineEditor()]
        [SerializeField]
        public List<ScriptableObject> collection = new();
    }

    // 保持向後兼容性的原始泛型版本
    public abstract class ScriptableCollection<T> : ScriptableObject
        where T : ScriptableObject
    {
        protected virtual bool FlagBelongThisCollection(T t) //用一些條件去篩掉特定flag, 例如要做百科分類
        {
            return true;
        }

#if UNITY_EDITOR
        [Button("Clear")]
        public void Clear()
        {
            collection.Clear();
            EditorUtility.SetDirty(this);
        }

        [Button("Find Under Folder With OverrideTypeName")]
        public void FindUnderFolder()
        {
            collection = ScriptableHelper.FindAllSO<T>(this);
            EditorUtility.SetDirty(this);
        }

        [TextArea]
        public string note;

        [Button("FindAllFlags")]
        public void FindAllFlags()
        {
            collection.Clear();
            var myPath = AssetDatabase.GetAssetPath(this);
            var dirPath = Path.GetDirectoryName(myPath);
            var scriptables = AssetDatabase.FindAssets(
                "t:" + typeof(T).FullName,
                new[] { dirPath }
            );

            foreach (var t in scriptables)
            {
                var path = AssetDatabase.GUIDToAssetPath(t);
                var flag = AssetDatabase.LoadAssetAtPath<T>(path);

                if (FlagBelongThisCollection(flag))
                    collection.Add(flag);
            }

            EditorUtility.SetDirty(this);
        }
#endif

        [FormerlySerializedAs("data")]
        [FormerlySerializedAs("gameFlagDataList")]
        [InlineEditor]
        [SerializeField]
        public List<T> collection = new();
    }
}
