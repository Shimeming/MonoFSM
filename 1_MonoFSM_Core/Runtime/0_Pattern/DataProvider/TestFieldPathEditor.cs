using System;
using System.Collections.Generic;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using UnityEngine;

namespace MonoFSM.Core.Test
{
    /// <summary>
    ///     測試 FieldPathEditorAttribute 的範例類別
    /// </summary>
    public class TestFieldPathEditor : MonoBehaviour, IFieldPathRootTypeProvider
    {
        [Header("範例1: 使用根型別名稱")]
        [FieldPathEditor("UnityEngine.Transform")]
        public List<FieldPathEntry> _transformPath = new();

        [Header("範例2: 使用根型別提供方法")]
        [FieldPathEditor("GetTestRootType", true)]
        public List<FieldPathEntry> _testPath = new();

        [Header("範例3: 使用當前物件型別")]
        [FieldPathEditor("TestFieldPathEditor")]
        public List<FieldPathEntry> _selfPath = new();

        [Header("範例4: 使用動態介面")]
        [FieldPathEditor] // 預設會使用 IFieldPathRootTypeProvider
        public List<FieldPathEntry> _dynamicPath = new();

        /// <summary>
        ///     提供根型別的方法範例
        /// </summary>
        public Type GetTestRootType()
        {
            // 這裡可以返回任何你想要的型別
            return typeof(TestFieldPathEditor);
        }

        /// <summary>
        ///     當路徑變更時會被調用（可選）
        /// </summary>
        public void OnPathEntriesChanged()
        {
            Debug.Log("Path entries changed!");

            // 可以在這裡進行一些驗證或更新邏輯
            if (_transformPath != null && _transformPath.Count > 0)
                Debug.Log($"Transform path: {BuildPathString(_transformPath)}");

            if (_testPath != null && _testPath.Count > 0)
                Debug.Log($"Test path: {BuildPathString(_testPath)}");

            if (_selfPath != null && _selfPath.Count > 0)
                Debug.Log($"Self path: {BuildPathString(_selfPath)}");
        }

        private string BuildPathString(List<FieldPathEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return "";

            var segments = new List<string>();
            foreach (var entry in entries)
                if (entry.IsArray)
                    segments.Add($"{entry._propertyName}[{entry.index}]");
                else
                    segments.Add(entry._propertyName);

            return string.Join(".", segments);
        }

        /// <summary>
        ///     實作 IFieldPathRootTypeProvider 介面
        ///     為不同欄位提供動態根型別
        /// </summary>
        /*
        public Type GetFieldPathRootType(string fieldName)
        {
            return fieldName switch
            {
                "_dynamicPath" => typeof(TestFieldPathEditor), // 動態路徑使用自己的型別
                "_transformPath" => typeof(Transform), // Transform 路徑
                "_testPath" => typeof(Vector3), // 測試路徑使用 Vector3
                "_selfPath" => GetType(), // 自身路徑
                _ => typeof(GameObject) // 預設使用 GameObject
            };
        }
        */
        public Type GetFieldPathRootType(string fieldName)
        {
            return typeof(TestFieldPathEditor);
        }

        [SerializeField]
        private int testInt;
    }
}
