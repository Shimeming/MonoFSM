using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonoFSM.Core.Simulate
{
    /// <summary>
    /// Component cache for better performance when getting components repeatedly
    /// </summary>
    public class ComponentCache
    {
        // 使用 null 作為 flag 來標記已查找但不存在的 component
        private readonly Dictionary<Type, object> _componentCache = new();

        public T GetComponent<T>(GameObject gameObject)
        {
            var type = typeof(T);

            // 如果已經查找過（包含 null 的情況），直接返回
            if (_componentCache.TryGetValue(type, out var cached))
            {
                return (T)cached;
            }

            // 使用 TryGetComponent 效能更好
            if (gameObject.TryGetComponent<T>(out var component))
            {
                _componentCache[type] = component;
                return component;
            }

            // 查找失敗，cache null 避免下次重複查找
            _componentCache[type] = null;
            return default;
        }

        public void Clear()
        {
            _componentCache.Clear();
        }
    }
}
