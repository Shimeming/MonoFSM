using System;
using System.Diagnostics.CodeAnalysis;
using MonoDebugSetting;
using MonoFSM.Core.Attributes;
using MonoFSM.EditorExtension;
using MonoFSM.Foundation;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ConditionGroup //封裝的蠻好的...? 但是auto可能會遇到問題...
{
    public bool IsValid => _conditions.IsAllValid();

    [CompRef]
    [AutoChildren(DepthOneOnly = true)]
    [SerializeField]
    private AbstractConditionBehaviour[] _conditions;
}

//還是Condition要用Is開頭？
public abstract class AbstractConditionBehaviour
    : AbstractDescriptionBehaviour,
        IBoolProvider,
        IOverrideHierarchyIcon,
        // IValueProvider<bool>, //FIXME: 不該作為ValueProvider? 要的話另外轉換好了？
        IHierarchyValueInfo
{
#if UNITY_EDITOR
    [ExcludeFromCodeCoverage]
    public string IconName => Selection.activeGameObject == gameObject ? "d__Help@2x" : "_Help@2x"; //UnityEditor.EditorGUIUtility.ObjectContent(null, typeof(AbstractConditionBehaviour)).image.name;

    [ExcludeFromCodeCoverage]
    public bool IsDrawingIcon => true;

    [ExcludeFromCodeCoverage]
    public Texture2D CustomIcon => null;
    // UnityEditor.EditorGUIUtility.ObjectContent(null, typeof(AbstractConditionBehaviour)).image as Texture2D;
    //UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.rcgmaker.fsm/RCGMakerFSMCore/Runtime/2_Variable/VarFloatIcon.png");
#endif

    //     [Button]
    //     [ShowIf("IsShowRenameButton")]
    //     protected void RenameOfGameObject()
    //     {
    //         try
    //         {
    //             var text = "[Condition] " + Description;
    //             if (FinalResultInverted)
    //                 text = "[Condition] Not " + Description;
    //             gameObject.name = text;
    // #if UNITY_EDITOR
    //             UnityEditor.EditorUtility.SetDirty(gameObject);
    // #endif
    //         }
    //         catch (System.Exception e)
    //         {
    //             Debug.LogError(e,this);
    //         }
    //     }
    protected override string DescriptionTag => "Condition";

    protected override string DescriptionPreprocess(string description)
    {
        //FIXME: formatName會把這個尬爛...
        var text = base.DescriptionPreprocess(description);
        return FinalResultInverted ? " Not " + text : text;
    }

    // protected override string Description =>


    // protected virtual bool IsShowRenameButton => Description != "";
    //
    // //FIXME: AI 可以解釋性？
    // //FIXME: 整合 Description, interface?
    // protected virtual string Description => this.GetType().Name;


    //FIXME: 可是 _parentTransition等著被call
    // public Action OnConditionChanged; //要用這個？還是用polling就好了
    //直接用interface往上叫好像不錯？
    private bool _isConditionChanged = false;

    //用類似statData 檢查dirty來決定要不要重新檢查condition
    public bool IsDirty => _isConditionChanged;

    public virtual bool IsInvertResultOptionAvailable => true;

    [ShowIf(nameof(IsInvertResultOptionAvailable))]
    [Tooltip(
        "If true, the final result will be inverted. For example, if the condition is fulfilled when pressing a button normally, setting this to true will make it fulfilled when the button is not pressed."
    )]
    public bool FinalResultInverted = false;

    protected abstract bool IsValid { get; }

    [ShowInPlayMode]
    public bool FinalResult
    {
        get
        {
            if (Application.isPlaying == false)
                return false;
#if UNITY_EDITOR

            //Debug用，暫時強迫覆蓋值 (ex: 裝備可以在路上換)
            if (_debugConditionResultOverrider != null && IsDebugMode)
                return _debugConditionResultOverrider.OverrideResultValue;
#endif
            //之前都沒有...
            // if (isActiveAndEnabled == false)
            //     return false;
            //FIXME: 關著表示不判...

            if (FinalResultInverted)
                return !IsValid;

            return IsValid;
        }
    }

#if UNITY_EDITOR
    [ShowIf("IsDebugMode")]
    [PropertyOrder(1)]
    [TabGroup("Debug")]
    [Component]
    [AutoChildren(false)]
    protected DebugConditionResultOverrider _debugConditionResultOverrider;

    [ShowIf("IsDebugMode")]
    [ShowInInspector]
    [TabGroup("Debug")]
    public bool OverrideValue =>
        _debugConditionResultOverrider != null
        && _debugConditionResultOverrider.OverrideResultValue;

    private static bool IsDebugMode => RuntimeDebugSetting.IsDebugMode;
#endif

    //For Cheat Code
    public virtual void CheatComplete()
    {
        Debug.LogError("This Condition Can't ForceSetValid");
    }

    public bool IsTrue => FinalResult;
    public bool Value => FinalResult;

    //interface & implementation的關係，所以我也可以說安裝一個schema, 然後下面再補variable....可能自動補掉就好了？(有就自動撈)
    public virtual string ValueInfo => FinalResult.ToString();
    public virtual bool IsDrawingValueInfo => Application.isPlaying && isActiveAndEnabled;
}
