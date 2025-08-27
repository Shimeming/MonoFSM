using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Variable.Attributes;
using MonoFSM.VarRefOld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.Backpack.Actions
{
    //從receiver那邊拿到變數，然後設定到自己的變數上 (有點像rebind了)
    //FIXME: 直接set?
    //FIXME: Assign Value to Variable?
    public class AssignValueAction : AbstractStateAction //, IRCGArgEventReceiver<IEffectHitData>
    {
        // public MonoValueProvider TestVariable;
        //SourceValueWrapper?
        //TargetValueWrapper?

        [InlineEditor]
        [AutoChildren]
        [CompRef]
        private TargetVarRef _targetVarRef;

        [InlineEditor]
        [AutoChildren]
        [CompRef]
        private SourceValueRef _sourceValueRef;

        // [AutoChildren] IConfigVar SourceValue; //FIXME; 怎麼用component...要手動assgin了嗎
        // [AutoChildren] IVariableProvider TargetVariable;
        [PreviewInInspector]
        private IEffectReceiver _lastReceiver;

        public override string Description =>
            $"Assign {_sourceValueRef.Description} to {_targetVarRef.Description}";

        protected override void OnActionExecuteImplement()
        {
            if (_sourceValueRef == null)
            {
                Debug.LogError("AssignValueAction: Source value is null", _sourceValueRef);
                return;
            }
            //FIXME: 在TargetRef就先檢查了？

            // 新功能：支援直接設定屬性值（不只是變數）
            if (_targetVarRef.VarRaw != null)
            {
                // 原有的變數設定模式
                var targetVar = _targetVarRef.VarRaw;
                targetVar.SetValueByRef(_sourceValueRef, this);
                Debug.Log(
                    $"AssignValueAction: Set variable value {_sourceValueRef} to {targetVar}",
                    this
                );
            }
            else
            {
                // 新的屬性設定模式：直接透過 ValueProvider 設定屬性
                var targetValueProvider = _targetVarRef.GetComponent<ValueProvider>();

                if (targetValueProvider != null)
                {
                    // 檢查是否支援設定
                    //FIXME: 是錯的
                    if (!targetValueProvider.CanSetProperty)
                    {
                        Debug.LogError(
                            $"AssignValueAction: 選擇的欄位 '{targetValueProvider.Description}' 不支援設定值（可能是唯讀屬性或常數）",
                            this
                        );
                        return;
                    }

                    //FIXME: 應該用generic做到？
                    var sourceValue = _sourceValueRef.objectValue;
                    // 直接呼叫 SetProperty 方法（不用反射）
                    targetValueProvider.SetProperty(sourceValue);

                    Debug.Log(
                        $"AssignValueAction: Set property value {sourceValue} to {targetValueProvider.Description}",
                        this
                    );
                }
                else
                {
                    Debug.LogError("AssignValueAction: No variable or property target found", this);
                }
            }
        }

        // protected override void OnArgEventReceived(GeneralEffectHitData arg)
        // {
        //     if (_targetVarRef == null)
        //     {
        //         Debug.LogError("AssignValueAction: No target variable reference", this);
        //         return;
        //     }
        //
        //     var variable = _targetVarRef.VarRaw;
        //     if (variable == null)
        //     {
        //         Debug.LogError("AssignValueAction: No variable found", this);
        //         return;
        //     }
        //
        //     variable.SetValueByRef(_sourceValueRef, this);
        // }

        // public VariableTag refVariableTag => varTag;
    }
}
