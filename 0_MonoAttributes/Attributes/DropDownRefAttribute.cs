using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

// [Required] //FIXME: optional?
// [IncludeMyAttributes]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[Conditional("UNITY_EDITOR")] //這個可以嗎？
public class DropDownRefAttribute : Attribute
{
    public DropDownRefAttribute(
        Type parentType = null,
        string dynamicTypeGetter = "",
        bool findFromParentTransform = false
    ) //FIXME: 寫死在code裏，不好
    {
        _parentType = parentType;
        _dynamicTypeGetter = dynamicTypeGetter;
        //這要做啥？
        _findFromParentTransform = findFromParentTransform;
    }

    public Type _parentType; //default 會用 IVariableOwner, 寫在DropdownRefCompselector
    public string _dynamicTypeGetter;
    public bool _findFromParentTransform = false;
}
