using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Native Pool 物件介面
/// </summary>
public interface INativePool
{
    public void Clear();
}

/// <summary>
/// Native 物件的 Pool 管理器
/// 用於管理非 Unity GameObject 的原生物件
/// </summary>
public class PoolNativeObjectManager<T> where T : INativePool, new()
{
    public PoolNativeObjectManager(int prepareCount)
    {
        _objList = new List<T>();
        for (var i = 0; i < prepareCount; i++)
            _objList.Add(new T());
        // Debug.Log("PoolNativeObjectManager init" + typeof(T));
    }

    private int _index = 0;
    private readonly List<T> _objList;

    public T Borrow()
    {
        // Debug.Log("Borrow " + _objList.Count + ",index:" + _index);
        if (_index >= _objList.Count) _index = 0;
        return _objList[_index++];
    }

    public void Clear()
    {
        foreach (var _obj in _objList)
        {
            _obj.Clear();
        }
    }
}