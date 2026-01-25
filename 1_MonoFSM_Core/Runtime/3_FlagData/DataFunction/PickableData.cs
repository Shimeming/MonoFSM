using System;
using _1_MonoFSM_Core.Runtime.Attributes;
using MonoFSM.Core.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;
using UnityEngine.Serialization;

//要用MovedFrom來改名字
[Serializable]
public abstract class AbstractDataFunction : IDataFeature
{
    //FIXME: 抽出去
    [ShowInDebugMode]
    [SerializeField]
    private GameData _owner; //這個是用來描述這個物品的擁有者
    public GameData Owner => _owner;

    public void SetOwner(GameData owner)
    {
        if (owner == null)
        {
            Debug.LogError("Owner cannot be null");
            return;
        }

        _owner = owner;
    }
}

[Serializable]
public class PickableData : AbstractDataFunction, IItemData //寫死還是有點不爽？
{
    public MonoObj EntityPrefab => _entityPrefab;

    [PrefabFilter]
    [SerializeField]
    private MonoObj _entityPrefab; //這個是用來生成實體的

    [FormerlySerializedAs("_stackCount")] [SerializeField]
    private float _maxStackCount = 1; //這個是用來描述這個物品的堆疊數量

    public float MaxStackCount => _maxStackCount;

    public void Use()
    {
        //?
        //丟出來？
    }
}

[Serializable]
public class ScoreData : AbstractDataFunction
{
    [SerializeField] private float _score = 1; //這個是用來描述這個物品的分數
    public float Score => _score;
}
