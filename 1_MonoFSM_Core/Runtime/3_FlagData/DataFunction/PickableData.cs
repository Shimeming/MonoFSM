using System;
using _1_MonoFSM_Core.Runtime.Attributes;
using MonoFSM.Core.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

//FIXME: 不好用...valueProvider拿不到，還是要繼承DescriptableData, 或是用變數把值給想辦法資料化? 直接對表格？ json介面？ (SO不能彈性改變檔案型別...)
[Serializable]
public abstract class AbstractDataFunction
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

    [SerializeField]
    private int _stackCount = 1; //這個是用來描述這個物品的堆疊數量
    public int MaxStackCount => _stackCount;

    public void Use()
    {
        //?
        //丟出來？
    }
}

[Serializable]
public class ScoreData : AbstractDataFunction
{
    [SerializeField]
    private int _score = 1; //這個是用來描述這個物品的分數
    public int Score => _score;
}
