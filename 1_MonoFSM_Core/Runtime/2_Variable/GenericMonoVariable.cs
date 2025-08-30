using System;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.RCGMakerFSMCore.Tracking;
using MonoFSM.Variable;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

//FIXME: autoGen太複雜，可能需要再拆漂亮
//FIXME: 改檔案名
[Searchable]
// [DisallowMultipleComponent]
public abstract class GenericMonoVariable<TScriptableData, TField, TType>
    : TypedMonoVariable<TType>,
        ISettable<TType>,
        IGameStateOwner,
        IDefaultSerializable,
        IReferenceTarget,
        ISceneStart
    where TScriptableData : AbstractScriptableData<TField, TType>
    where TField : FlagField<TType>, new()
    where TType : IEquatable<TType>
{
    // [CompRef] [AutoChildren(DepthOneOnly = true)]
    // private IValueProvider<TType>[] _valueSources; //FIXME: 可能會有多個耶...還要一層resolver嗎？
    //
    //
    // [ShowInPlayMode]
    // private IValueProvider<TType> valueSource
    // {
    //     get
    //     {
    //         if (_valueSources == null || _valueSources.Length == 0)
    //             return null;
    //         foreach (var valueProvider in _valueSources)
    //             if (valueProvider.IsValid)
    //                 return valueProvider;
    //
    //         //[]: 多個的話要怎麼辦？還是說不允許多個？
    //         Debug.LogWarning("condition not met, use default? (last)" + _valueSources[^1], this);
    //         return _valueSources[^1];
    //     }
    // }

    //還要看條件嗎？ conditional value switch
    //想要直接選一個field就拿他的值，應該抽出去做成一個新東西不要放在GenericVariable裡面
    //VariableFloat應該獨立寫？這樣就一定可以有一個最好的abstract class
    public override void CommitValue()
    {
        var (last, current) = Field.CommitValue();
        ValueCommited(last, current);
    }

    //可以用abstract比較好？但目前只用到VarFloat
    protected virtual void ValueCommited(TType lastValue, TType currentValue) { }

    public override void SetValue(object value, MonoBehaviour byWho)
    {
        SetValueInternal((TType)value, byWho);
    }

    public override void SetValue(TType value, MonoBehaviour byWho)
    {
        SetValueInternal(value, byWho);
    }

    [CompRef]
    [Auto]
    private IVarValueSettingProcessor<TType> _beforeSetProcessor;

    private bool PrefabKindMatchTagCheck()
    {
#if UNITY_EDITOR
        if (myPrefabKind == PrefabKind.NonPrefabInstance) //場景上的非prefab給過
            return true;
        var tag = GetComponent<GameStateRequireAtPrefabKind>();

        if (tag == null)
            return false; //[]: 該給過嗎？ 不該，要不然prefab會很吵
        if ((tag.prefabKind & myPrefabKind) != 0)
            return true;
#endif
        return false; //不是那個環境就不用顯示了
    }

    private bool IsCheckingPrefabKind => GetComponent<GameStateRequireAtPrefabKind>() != null;

    private void GenData()
    {
#if UNITY_EDITOR
        //get type of scriptableData field using reflection
        var type = GetType().GetField("scriptableData").FieldType;
        _bindData = type.CreateGameStateSO(this) as TScriptableData;
        this.SetDirty();
        Debug.Log("自動生成flag修正" + _bindData, _bindData);
#endif
        //FIXME: 用validator檢查，然後自動Fix?
        //[]:已經在Auto那邊用OnBeforeSerialize全部做掉了
    }

    [TabGroup("GameState")]
    [LabelText("自動生成")]
    [ShowInInspector]
    private bool IsAutoGen => GetComponent<AutoGenGameState>() != null; //TODO: IsAutoGen?
#if UNITY_EDITOR
    private bool IsAutoGenButNotYet() => IsAutoGen && _bindData == null;

    private bool IsGameStateRequiredButMissing()
        //FIXME: default不需要存檔，標記需要存檔的流程是什麼？
        =>
        PrefabKindMatchTagCheck() && _bindData == null;

    private bool IsSuggestingAutoGen() => !IsAutoGen && _bindData == null;

    private bool IsSuggestingDesignTag()
    {
        return gameObject.IsInPrefab() || myPrefabKind == PrefabKind.NonPrefabInstance;
    }
#endif

    //TODO: 可以直接弄到drawer上？
    [TabGroup("GameState")]
    [HideInInlineEditors]
    [EnableIf("IsSuggestingDesignTag")]
    [HideIf("IsAutoGen")] //[]: 已經裝了的話要藏嗎？ 還是應該要透明
    [Button("[Prefab設計]Add AutoGen GameState")]
    private void AddTag() => this.TryGetCompOrAdd<AutoGenGameState>();

    [TabGroup("GameState")]
    [HideIf("IsCheckingPrefabKind")] //[]: 已經裝了的話要藏嗎？
    [EnableIf("IsSuggestingDesignTag")]
    [Button("[Prefab設計]Add GameState Require Tag")]
    private void AddRequireInPrefab() => this.TryGetCompOrAdd<GameStateRequireAtPrefabKind>();

    //  MustGenScriptableDataTag mustGenTag; //提醒一定要gen flag
#if UNITY_EDITOR


    //lazy get prefabKind
    private PrefabKind _myPrefabKind;

    [ShowInInspector]
    private PrefabKind myPrefabKind => OdinPrefabUtility.GetPrefabKind(this);
    //FIXME: 這個可以cache嗎...
#endif

    // [MCPExtractable]
    [FormerlySerializedAs("localField")]
    [TabGroup("Value")]
    [InlineField]
    [HideIf(nameof(_bindData))]
    [HideIf(nameof(HasValueProvider))]
    public TField _localField; // = new();

    private bool HasValueProvider => _valueSources is { Length: > 0 };

    //這個值會被蓋掉???

    // [TabGroup("Value")]
    public TField Field => BindData != null ? BindData.field : _localField;

    //給非Auto的人看的，要綁，Auto自己就會生，就結束了

    public void EnterSceneStart()
    {
        RegisterValueChange();
    }

    // public override void AddListener<T>(UnityAction<T> action)
    // {
    //     if (action == null) return;
    //     // this.Log("[Variable] AddListener", action);
    //     if (action is UnityAction<TType> actionT)
    //         Field.AddListener(actionT, this);
    //     else
    //         Debug.LogError("AddListener Type Error", this);
    // }


    protected virtual void RegisterValueChange()
    {
        Field.AddListener(
            (value) =>
            {
                OnValueChanged();
            },
            this
        );
    }

    [FormerlySerializedAs("scriptableData")]
    //FIXME: 這個錯了...要有特定設計tag，才是在prefab上不要gen
    // [EnableIn(PrefabKind.InstanceInScene | PrefabKind.NonPrefabInstance)] //scriptable binding, 只想要在景裡編輯
    [TabGroup("GameState")]
    [Header("存檔")]
    [GameState]
    [InlineEditor]
    [EnableIf(nameof(PrefabKindMatchTagCheck))]
#if  UNITY_EDITOR
    [InfoBox("SaveID不一致, 清掉重綁", InfoMessageType.Error, nameof(IsGameStateSaveIDNotMatch))]
    [InfoBox("GameState的類型不對", InfoMessageType.Error, nameof(IsGameStateTypeNotMatch))]
    [InfoBox("需要綁GameState!", InfoMessageType.Error, nameof(IsGameStateRequiredButMissing))]
    [InlineButton(nameof(GenData), "Auto Gen Fix", ShowIf = nameof(IsGenDataRequired))]
#endif
    // [ValidateInput("AutoGenCheck", "自動生成檢查失敗")]
    public TScriptableData _bindData;

#if UNITY_EDITOR
    private bool IsGameStateSaveIDNotMatch() //需檢查情境：複製時，造成綁到同一個gameState ref, 檢查saveID
    {
        if (!IsAutoGen)
            return false;
        var autoComp = GetComponent<AutoGenGameState>();
        if (autoComp == null || _bindData == null)
            return false;
        return autoComp.SaveID != _bindData.GetSaveID;
        // Debug.LogError("SaveID不一致", this);
    }

    // <summary> 用來檢查是否有auto gen, 但是type不對 </summary>
    private bool IsGameStateTypeNotMatch()
    {
        if (_bindData == null)
            return false;

        var autoComp = GetComponent<AutoGenGameState>();
        if (autoComp != null)
        {
            //有auto gen, 但是type不對
            if (_bindData.gameStateType != GameFlagBase.GameStateType.AutoUnique)
                return true;
        }
        else
        {
            if (_bindData.gameStateType != GameFlagBase.GameStateType.Manual)
                return true;
        }

        return false;
    }
#endif

    public virtual TScriptableData BindData => _bindData; //FIXME:

    //不同type不同類型的modifier
    [PreviewInInspector]
    [Component]
    [AutoChildren]
    protected AbstractVariableModifier<TType>[] _modifiers; //bound modifier?

    // [TabGroup("Data")]
    // [PreviewInInspector]
    public virtual TType FinalValue => CurrentValue;

    // [TabGroup("Value")]
    [ShowInDebugMode]
    public virtual TType LastValue => Field.LastValue; //FIXME: 這裡沒有過到modifier

    // [MCPExtractable]
    public TType Value
    {
        get => CurrentValue;
        // set //給reflection用的
        // // this.Log("[Variable] Set", value);
        // {
        //     if (!Application.isPlaying)
        //         EditorValue = value;
        //     else
        //         SetValueExecution(value);
        // }
    }

    public TType EditorValue
    {
        get => Field.ProductionValue;
        set
        {
            // Field.ProductionValue = value;
            // Field.DevValue = value;
            _localField.ProductionValue = value;
            _localField.DevValue = value;
            Debug.Log("Set EditorValue" + value, this);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    [ShowInPlayMode]
    public virtual TType CurrentValue //FIXME: 改成Value?
    {
        get
        {
            if (!Application.isPlaying) //FIXME: 先隨便寫一下..
                return EditorValue;

            if (valueSource != null) //用外部source getter, 這樣原本一坨都不需要了吧？
                return valueSource.Value;

            Profiler.BeginSample("Variable GetValue");
            var tempValue = _localField.CurrentValue;

            //FIXME: 這裡就有proxy? 而且還是直接reference...
            // if (VariableSource != null)
            // {
            //     var v = VariableSource as GenericMonoVariable<TScriptableData, TField, TType>;
            //     tempValue = v.CurrentValue;
            // }
            if (BindData != null)
                tempValue = BindData.CurrentValue;

            Profiler.EndSample();
            Profiler.BeginSample("AfterGetValueModifyCheck");
            //FIXME: 這個是不是有點貴？有需要在這層做嗎？應該在set時就做掉了？不需要ㄅ
            // if (_modifiers != null)
            //     foreach (var modifier in _modifiers)
            //         tempValue = modifier.AfterGetValueModifyCheck(tempValue);
            Profiler.EndSample();
            // this.Log("[Variable] Get", tempValue);
            return tempValue;
        }

        //         set //FIXME: 拿掉，用SetValue(
        //         {
        //             var tempValue = value;
        //             //先檢查會被修改
        //
        //             if (_modifiers != null)
        //                 foreach (var modifier in _modifiers)
        //                     tempValue = modifier.BeforeSetValueModifyCheck(tempValue);
        //             // this.Log("[Variable] Set", value);
        //             if (BindData == null)
        //             {
        //                 if (_localField.CurrentValue.Equals(tempValue)) return;
        //                 // if (localField == null)
        //                 //     localField = default(TField);
        //                 _localField.CurrentValue = tempValue;
        //             }
        //
        //             else
        //             {
        //                 if (BindData.CurrentValue.Equals(tempValue)) return;
        //                 if (FinalData == null) return;
        // #if MIXPANEL
        //                 _trackValue.OnRecycle();
        //                 _trackValue["Data"] = FinalData ? FinalData.name : "null";
        //                 _trackValue["value"] = tempValue switch
        //                 {
        //                     bool valueBool => valueBool,
        //                     int valueInt => valueInt,
        //                     float valueFloat => valueFloat,
        //                     _ => _trackValue["value"]
        //                 };
        //                 this.Track("Variable Changed", _trackValue);
        // #endif
        //                 // Debug.Log("Set Value" + tempValue);
        //
        //                 BindData.CurrentValue = tempValue;
        //             }
        //         }
    }

    // private MonoBehaviour lastValueSetter;


    protected override void SetValueInternal<T>(T value, Object byWho)
    {
        if (value is TType type)
        {
            var (result, tempValue) = SetValueExecution(type, byWho as MonoBehaviour);
            if (result)
                RecordSetbyWho(byWho, tempValue);
        }
        else
            Debug.LogError("SetValueInternal Type Error", this);
    }

    //FIXME: protected?
    private (bool, TType) SetValueExecution(TType value, MonoBehaviour byWho)
    {
        // if (_beforeSetProcessor != null)
        _beforeSetProcessor?.BeforeSetValue(value); //練線處理？
        // lastValueSetter = byWho;
        var tempValue = value;
        //先檢查會被修改

        if (_modifiers != null)
            foreach (var modifier in _modifiers)
                tempValue = modifier.BeforeSetValueModifyCheck(tempValue);
        //after?
        // Debug.Log("[Variable] Set" + value + "tempValue:" + tempValue + ", Value:" + CurrentValue, byWho);
        if (tempValue.Equals(CurrentValue))
            return (false, tempValue); //沒有變化就不需要處理

        Field.SetCurrentValue(tempValue, byWho);
        TrackValue(tempValue, byWho);
        return (true, tempValue);
        // #if MIXPANEL
        //         _trackValue.OnRecycle();
        //         _trackValue["Data"] = FinalData ? FinalData.name : "null";
        //         _trackValue["byWho"] = byWho ? byWho.name : "null";
        //         _trackValue["value"] = tempValue switch
        //         {
        //             bool valueBool => valueBool,
        //             int valueInt => valueInt,
        //             float valueFloat => valueFloat,
        //             _ => _trackValue["value"]
        //         };
        //         this.Log("Set Value byWho", tempValue, "byWho", byWho);
        //
        //         this.Track("Variable Changed", _trackValue);
        // #endif
    }

    private void TrackValue(TType value, MonoBehaviour byWho)
    {
        var trackValue = UserDataTracker.BorrowTrackableValue;
        if (trackValue == null)
            return;
        // trackValue.SetProperty("Data", FinalData ? FinalData.name : "null");
        trackValue.SetProperty("byWho", byWho ? byWho.name : "null");
        trackValue.SetProperty("value", value);
        //FIXME: 還是這裡應該用trackValue.Track(...?)既然都包了
        UserDataTracker.Track("Variable Changed", trackValue);
    }

#if MIXPANEL
    private readonly Value _trackValue = new();
#endif

    //FIXME: 還需要這個嗎？
    // [AutoParent()] private IGameEntity gameEntity;
    //
    // [ShowInPlayMode]
    // private string GameStateID => gameEntity != null
    //     ? $"{gameObject.scene.name}_{gameEntity.name}_{gameObject.name}"
    //     : $"{gameObject.scene.name}_{gameObject.name}";

    //為了讀檔後才能設定？reset又要重置參數...


    // void IResetter.EnterLevelReset()
    // {
    //     // this.Log("[VariableType] Before local Reset" + localField.CurrentValue, gameObject);
    //     //Scene裡的物件沒有要存檔的必要，重置
    //     if (TestModeGameFlag.Instance)
    //         localField.Init(TestModeGameFlag.Instance.mode, this);
    //     else
    //     {
    //         localField.Init(TestMode.EditorDevelopment, this);
    //     }
    //     localField.ResetToDefault();
    //     this.Log("[VariableType] After local Reset" , localField.CurrentValue, gameObject);
    // }

    public void ExitLevelAndDestroy()
    {
        return;
    }

    public int GetPriority()
    {
        return -1;
    }

    //FIXME 不該用這個？
    // [HideInInlineEditors] public UnityEvent<TType> OnValueChanged = new();

    //     public void Validate(SelfValidationResult result)
    //     {
    // #if UNITY_EDITOR
    //         if (IsAutoGen)
    //         {
    //             //不在景裏，不需要
    //             if ((OdinPrefabUtility.GetPrefabKind(this) & PrefabKind.InstanceInScene) == 0) return;
    //             if (IsAutoGenButNotYet()) result.AddError("需要GameState Not Gen").WithFix(GenData);
    //         }
    //
    //         if (IsGameStateSaveIDNotMatch()) result.AddError("SaveID不一致, 清掉重綁").WithFix(GenData);
    // #endif
    //     }

#if UNITY_EDITOR
    private bool IsGenDataRequired()
    {
        if (IsAutoGen)
        {
            //不在景裏，不需要
            if ((OdinPrefabUtility.GetPrefabKind(this) & PrefabKind.InstanceInScene) == 0)
                return false;
            if (IsAutoGenButNotYet())
                return true;
        }

        return IsGameStateSaveIDNotMatch();
    }
#endif

    public override Type ValueType => typeof(TType);
    public override object objectValue => CurrentValue;

    public string Serialize()
    {
        return GetType().Name + ":" + _localField.ProductionValue;
    }

    public void Deserialize(string data)
    {
        throw new NotImplementedException();
    }

    public override void ResetStateRestore() //應該放下去，然後這裡override實作？
    {
        //FIXME: if not init, restore? 應該要弄個ISceneAwake?
        _localField.Init(TestMode.Production, this);
        // Field.ResetToDefault();
    }

    public override void OnBeforePrefabSave()
    {
        base.OnBeforePrefabSave();
        if (_varTag == null)
            Debug.LogError("No VarTag: " + this, this);
        else
            name = _varTag.name;
    }
}

public interface ISettable //FIXME: 有點蠢
{
    void CommitValue();

    //FIXME: 用T?
    void SetValue(object value, MonoBehaviour byWho = null);
}

public interface ISettable<in T> : ISettable
{
    void SetValue(T value, MonoBehaviour byWho = null);
}
