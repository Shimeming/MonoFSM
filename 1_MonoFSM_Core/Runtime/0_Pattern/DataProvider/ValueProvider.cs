using System;
using System.Collections.Generic;
using _1_MonoFSM_Core.Runtime._1_States;
using Cysharp.Text;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime;
using MonoFSM.Core.Utilities;
using MonoFSM.EditorExtension;
using MonoFSM.Runtime;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace MonoFSM.Core.DataProvider
{
    public class ValueProvider<T> : ValueProvider
    {
        public override Type ValueType => typeof(T);

        public T Get()
        {
            return Get<T>();
        }
    }

    //用這顆就夠了，其他應該都不需要了？除了literal
    public class ValueProvider
        : AbstractVariableProviderRef,
            IOverrideHierarchyIcon,
            IHierarchyValueInfo
    {
        //自引用？
        // [SerializeField] ValueProvider _valueProviderRef;
        //Entity?
        // [DropDownRef] [SerializeField] private ValueProvider _valueProviderRef; //還是是entity想要Ref?

        [PropertyOrder(-1)]
        [BoxGroup("varTag")]
        [ShowInInspector]
        [ValueDropdown(nameof(GetVarTagsFromEntity), NumberOfItemsBeforeEnablingSearch = 5)]
        public VariableTag DropDownVarTag
        {
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError(
                        "VarRef: Cannot set varTag in play mode. Please set it in edit mode.",
                        this
                    );
                    return;
                }

                _varTag = value;
            }
            //only editor Set? 另外開？
            get => _varTag;
        }

        public IEnumerable<ValueDropdownItem<VariableTag>> GetVarTagsFromEntity()
        {
            //FIXME: 不看Parent entity的entity tag嗎？
            if (entityProvider != null)
                if (entityProvider.entityTag == null)
                {
                    return entityProvider.GetVariableTagDropdownItems<AbstractMonoVariable>();
                    // Debug.LogError("EntityProvider has no entity tag, returning empty variable tags.", this);
                    return new List<ValueDropdownItem<VariableTag>>();
                }

            return entityProvider?.entityTag?.GetVariableTagItems()
                ?? ParentEntity?.GetVarTagOptions();
        }

        [ShowInDebugMode]
        [BoxGroup("varTag")]
        // [Required]
        [SerializeField]
        private VariableTag _varTag;

        public void ClearVarTag()
        {
            _varTag = null;
            // Debug.Log("VarRef: Cleared varTag.", this);
        }

        // private bool TypeCheckFail()
        // {
        //     if (_varTag == null) return false;
        //     return typeof(TValueType).IsAssignableFrom(_varTag._valueFilterType.RestrictType) == false;
        // }

        //代表這個ValueProvider可以拿到對應的AbstractMonoVariable


        [ShowInPlayMode]
        public override AbstractMonoVariable VarRaw //可以去拿MonoEntity的資料？而不是一定要透過Var?
        {
            get
            {
                if (entityProvider != null) //這個可以是null...hmmm
                    return entityProvider?.monoEntity?.GetVar(_varTag);

                // if (_monoVariable != null)
                //     return _monoVariable;
                // Debug.LogError("VarRef: No variable found", this);
                return ParentEntity?.GetVar(_varTag); //如果沒有黑板就從parent entity拿
            }
        }

        private MonoEntity ParentEntity //fixme; 避免這個？ fsm結構裡一定要放this.(ParentEntity)嗎？
        {
            get
            {
                this.EnsureComponentInParent(ref _parentEntity);
                return _parentEntity;
            }
        }

        public MonoEntity fromEntity //tag?
        {
            get
            {
                if (_entityProvider != null)
                    return _entityProvider.monoEntity;
                return ParentEntity;
            }
        }

        [AutoParent]
        private MonoEntity _parentEntity;

        //可auto? 有ref就不覆蓋？還是身上的比較大？
        //自己也是？
        [DropDownRef]
        [SerializeField]
        public AbstractEntityProvider _entityProvider;

        private AbstractEntityProvider entityProvider
        {
            get
            {
                this.EnsureComponent(ref _entityProvider, false); //不一定需要這個物件
                return _entityProvider;
            }
        }

        public string _declarationName; //可以有default name?

        public string DeclarationName
        {
            get
            {
                if (!_declarationName.IsNullOrWhitespace())
                    return _declarationName;
                if (entityProvider?.SuggestDeclarationName != null)
                    return entityProvider?.SuggestDeclarationName;
                // Debug.Log($"VarRef: Using entityProvider declaration name: {_declarationName}", this);
                return _declarationName;
            }
        }

        public override string Description
        {
            get
            {
                var stringBuilder = ZString.CreateStringBuilder();
                if (!DeclarationName.IsNullOrWhitespace())
                {
                    // Debug.Log($"VarRef: Using declaration name: {DeclarationName}", this);
                    stringBuilder.Append(DeclarationName);
                    // stringBuilder.Append(".");
                }
                if (entityProvider != null && entityProvider.entityTag != null)
                {
                    //world Entity => world.player
                    //parentEntity => this(Player)
                    // stringBuilder.Append(entityProvider.GetType());
                    // Debug.Log($"VarRef: EntityProvider found: {entityProvider.entityTag}", this);
                    stringBuilder.Append("(");
                    stringBuilder.Append(entityProvider.entityTag.name.Split("_")[^1]);
                    stringBuilder.Append(")");
                }

                // stringBuilder.Append('.');
                if (varTag != null)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append('.');
                    stringBuilder.Append(varTag.name.Split("_")[^1]);
                    // stringBuilder.Append('.');
                }

                if (stringBuilder.Length > 0 && HasFieldPath)
                    stringBuilder.Append('.');
                stringBuilder.Append(PropertyPath);
                var final = stringBuilder.ToString();
                //把final裡的_都拿掉
                // final = final.Replace("_", " ");
                return final;
            }
        }

        // public override AbstractMonoVariable VarRaw => _monoVariable;

        [PreviewInInspector]
        public override Type ValueType =>
            HasFieldPath
                ? lastPathEntryType
                :
                //GetTarget()?.ValueType ??
                varTag?.ValueType
                    ?? entityProvider?.entityTag?.RestrictType
                    ?? typeof(MonoEntity);

        [PreviewInInspector]
        public string ValueTypeSourceFrom
        {
            get
            {
                if (HasFieldPath)
                    return "Field Path";
                else if (_varTag != null)
                    return "Var Tag";
                else if (entityProvider != null && entityProvider.entityTag != null)
                    return "Entity Tag";
                else if (ParentEntity != null)
                    return "Parent Entity";
                return "Unknown";
            }
        }

        //FIXME: 型別有可能和實際不符合嗎？
        //選了VarRaw.Value後反而變成原本的type...這樣外面就沒有提示了

        private IValueProvider GetTarget()
        {
            if (_varTag == null)
            {
                if (entityProvider != null)
                    return entityProvider.monoEntity;
                return ParentEntity;
            }

            return VarRaw;
        }

        // public override Type GetValueType =>
        [PropertyOrder(-1)]
        [PreviewInInspector]
        public override Type GetObjectType //FIXME:
        {
            get
            {
                //varType (tag
                if (_varTag != null)
                    return _varTag.VariableMonoType; //hmm少 var value type...
                //entityType (tag)
                if (entityProvider != null)
                    return entityProvider.entityTag?._entityType?.RestrictType
                        ?? typeof(MonoEntity);
                //parentEntityType (instance)
                if (ParentEntity != null)
                    return ParentEntity.GetType();

                Debug.LogError("VarRef: No target entity or variable tag found.", this);
                return typeof(object); // 如果沒有找到目標，返回 object 類型
            }
        }

        public override VariableTag varTag => _varTag;

        public override TVariable GetVar<TVariable>()
        {
            if (VarRaw is TVariable variable)
                return variable;
            if (VarRaw == null)
            {
                Debug.LogError("VarRef: VarRaw is null, cannot get variable.", this);
            }
            else
                Debug.LogError("VarRef: VarRaw is not of type " + typeof(TVariable), this);

            return null;
        }

        public MonoEntity GetMonoEntity()
        {
            if (entityProvider != null)
                return entityProvider.monoEntity;
            return ParentEntity;
        }

        public TSchema GetSchema<TSchema>()
            where TSchema : AbstractEntitySchema
        {
            var entity = GetMonoEntity();
            if (entity == null)
            {
                Debug.LogError("VarRef: No target entity found.", this);
                return null;
            }

            return entity.GetSchema<TSchema>();
        }

        public override T1 Get<T1>() //GetAs?
        {
            if (ValueType == null)
                // Debug.LogError("VarRef: ValueType is null, cannot get value.", this);
                return default;
            if (!typeof(T1).IsAssignableFrom(ValueType))
            {
                Debug.LogError(
                    $"無法將 {ValueType} 轉換為 {typeof(T1)}，請檢查變數類型或欄位路徑設定。",
                    this
                );
                return default;
            }

            var target = GetTarget();
            if (target == null)
                // Debug.LogError("VarRef: 目標變數或實體為 null，無法獲取值。", this);
                return default;
            // 如果沒有設定欄位路徑，直接回傳變數值
            if (!HasFieldPath)
            {
                // Debug.Log($"VarRef: 直接從變數取得值: {target}", this);
                return target.Get<T1>();
            }

            // 不選varTag的話就用Entity?
            // 使用欄位路徑存取特定欄位值
            var fieldValue = ReflectionUtility.GetFieldValueFromPath(
                target,
                _pathEntries,
                gameObject
            );

            if (fieldValue is T1 tValue)
                return tValue;

            // 嘗試轉型
            if (fieldValue != null)
                try
                {
                    return (T1)Convert.ChangeType(fieldValue, typeof(T1));
                }
                catch (Exception e)
                {
                    if (Application.isPlaying)
                        Debug.LogError(
                            $"無法將欄位值 {fieldValue} (型別: {fieldValue.GetType()}) 轉換為 {typeof(T1)}: {e.Message}",
                            this
                        );
                }
            else
            {
                return default; // 如果欄位值為 null，直接返回預設值
            }

            Debug.LogError($"VarRef: 轉換失敗 Var:{target}", this);
            return default;
        }

        public string IconName => "Linked@2x";
        public bool IsDrawingIcon => true;
        public Texture2D CustomIcon => null;
        public string ValueInfo => $"{DeclarationName}"; //一play會call這個...

        // public string ValueInfo => $"{ValueType.Name} {DeclarationName}"; //一play會call這個...
        public bool IsDrawingValueInfo => true;

        protected override string DescriptionTag
        {
            get
            {
                //Value, Var, Entity
                if (HasFieldPath)
                    return "Value";
                if (_varTag != null)
                    return "Var";
                if (entityProvider != null)
                    return "Entity";
                return "Unknown";
            }
        }
    }
}
