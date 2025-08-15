using System;

using UnityEngine;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Variable;

public abstract class AbstractFolder : MonoBehaviour
{
    public string IconName => "Folder Icon";
    public bool IsDrawingIcon => true;
}

//FIXME: 這個才該叫做blackboard?，這個是用來放變數的?

public class VariableFolder : MonoDict<VariableTag, AbstractMonoVariable>
{
    protected override bool IsStringDictEnable => true;
    // [ReadOnly] [Component( AddComponentAt.Children, "[Variable]")]
    // public AbstractVariable flag;

    // [Component(typeof(AbstractFlag), "[Variable]")]
    // void AddComponent()
    // {
    //     //按完就沒我的事了??
    // }
    // private void Awake()
    // {
    //     // varDict = GetVariableDict();
    // }
    public AbstractMonoVariable GetVariable(VariableTag type)
    {
        return Get(type);
        // return varDict.GetValueOrDefault(type);
    }

    public AbstractMonoVariable GetVariable(string varName)
    {
        return Get(varName);
    }

    public TVariable GetVariable<TVariable>(VariableTag type) where TVariable : AbstractMonoVariable
    {
        return Get(type) as TVariable;
    }

    public TVariable GetVariable<TVariable>(string varName) where TVariable : AbstractMonoVariable
    {
        return Get(varName) as TVariable;
    }

    //GetConfig?

    public void CommitVariableValues()
    {
        // var variables = GetComponentsInChildren<AbstractVariable>(true);
        //FIXME: 用

        foreach (var variable in _collections)
            if (variable is ISettable settableVariable)
                settableVariable.CommitValue();
    }

    // [PreviewInInspector]
    // [PreviewInInspector] [Component] [AutoChildren]
    // private ISettable[] _variables = Array.Empty<ISettable>();

    // private void OnValidate()
    // {
    //     variables = GetComponentsInChildren<AbstractVariable>(true);
    //     foreach (var variable in variables) variable.transform.localPosition = Vector3.zero;
    // }
#if UNITY_EDITOR

    // [Button]
    public VarBool CreateVariableBool()
    {
        var varBool = gameObject.AddChildrenComponent<VarBool>("[Variable] flag");
        return varBool;
    }

    /// <summary>
    /// 創建指定類型的變數
    /// </summary>
    /// <typeparam name="TVariable">變數類型，必須繼承自 AbstractMonoVariable</typeparam>
    /// <param name="tagName">變數的標籤名稱</param>
    /// <returns>創建的變數實例</returns>
    public TVariable CreateVariable<TVariable>(string tagName) where TVariable : AbstractMonoVariable
    {
        var variable = gameObject.AddChildrenComponent<TVariable>($"[Var] {tagName}");
        
        // 這裡可以進一步設定 VariableTag，如果有需要的話
        // variable._varTag = FindOrCreateVariableTag(tagName);
        
        return variable;
    }

    /// <summary>
    /// 創建變數的通用方法
    /// </summary>
    /// <param name="variableType">變數類型</param>
    /// <param name="tagName">變數的標籤名稱</param>
    /// <returns>創建的變數實例</returns>
    public AbstractMonoVariable CreateVariable(System.Type variableType, string tagName)
    {
        if (!typeof(AbstractMonoVariable).IsAssignableFrom(variableType))
        {
            Debug.LogError($"類型 {variableType} 不是 AbstractMonoVariable 的子類別");
            return null;
        }
        
        var childGameObject = new GameObject($"[Var] {tagName}");
        childGameObject.transform.SetParent(transform);
        
        var variable = childGameObject.AddComponent(variableType) as AbstractMonoVariable;
        
        // 這裡可以進一步設定 VariableTag，如果有需要的話
        // variable._varTag = FindOrCreateVariableTag(tagName);
        
        return variable;
    }

    /// <summary>
    /// 根據 VariableTag 創建變數
    /// </summary>
    /// <typeparam name="TVariable">變數類型，必須繼承自 AbstractMonoVariable</typeparam>
    /// <param name="tag">要綁定的 VariableTag</param>
    /// <returns>創建的變數實例</returns>
    public TVariable CreateVariableWithTag<TVariable>(VariableTag tag) where TVariable : AbstractMonoVariable
    {
        if (tag == null)
        {
            Debug.LogError("VariableTag 不能為 null");
            return null;
        }

        var variable = gameObject.AddChildrenComponent<TVariable>($"[Var] {tag.name}");
        
        // 設定變數的 tag
        variable._varTag = tag;
        
        Debug.Log($"已創建變數 {typeof(TVariable).Name} 並綁定到 VariableTag: {tag.name}", variable);
        
        return variable;
    }

    /// <summary>
    /// 根據 VariableTag 創建變數（非泛型版本）
    /// </summary>
    /// <param name="variableType">變數類型</param>
    /// <param name="tag">要綁定的 VariableTag</param>
    /// <returns>創建的變數實例</returns>
    public AbstractMonoVariable CreateVariableWithTag(System.Type variableType, VariableTag tag)
    {
        if (!typeof(AbstractMonoVariable).IsAssignableFrom(variableType))
        {
            Debug.LogError($"類型 {variableType} 不是 AbstractMonoVariable 的子類別");
            return null;
        }
        
        if (tag == null)
        {
            Debug.LogError("VariableTag 不能為 null");
            return null;
        }
        
        var childGameObject = new GameObject($"[Var] {tag.name}");
        childGameObject.transform.SetParent(transform);
        
        var variable = childGameObject.AddComponent(variableType) as AbstractMonoVariable;
        
        // 設定變數的 tag
        variable._varTag = tag;
        
        Debug.Log($"已創建變數 {variableType.Name} 並綁定到 VariableTag: {tag.name}", variable);
        
        return variable;
    }
#endif
    protected override void AddImplement(AbstractMonoVariable item)
    {
        
    }

    protected override void RemoveImplement(AbstractMonoVariable item)
    {
    }

    protected override bool CanBeAdded(AbstractMonoVariable item)
    {
        return item.gameObject.activeSelf == true;
        //一定要可以加，還是用disable?
        // return true;
    }
}