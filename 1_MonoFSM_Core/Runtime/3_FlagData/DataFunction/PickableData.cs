using System;
using MonoFSM.Core.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

[Serializable]
public class PickableData : IItemData //寫死還是有點不爽？
{
    public MonoPoolObj EntityPrefab => _entityPrefab;
    [SerializeField] private MonoPoolObj _entityPrefab; //這個是用來生成實體的
    [SerializeField] private int _stackCount = 1; //這個是用來描述這個物品的堆疊數量
    public int MaxStackCount => _stackCount;
    public void Use()
    {
        //?
    }

    //FIXME: 抽出去
    [ShowInDebugMode]
    [SerializeField] private DescriptableData _owner; //這個是用來描述這個物品的擁有者
    public DescriptableData Owner => _owner;
    public void SetOwner(DescriptableData owner)
    {
        if (owner == null)
        {
            Debug.LogError("Owner cannot be null");
            return;
        }
        _owner = owner;
    }
}