using MonoFSM.Core;
using MonoFSM.Runtime.Variable;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonoFSM.Runtime.Interact.EffectHit
{
    // 用這個觸發action?
    public sealed class EffectEnterNode : AbstractEventHandler
    {
        //local variable, 這在這個enter下的生命週期
        [Component] //[Component?
        public VarEntity _hittingEntity; //to set

        protected override void Rename()
        {
            base.Rename();
#if UNITY_EDITOR
            if (_hittingEntity != null)
            {
                _hittingEntity.gameObject.name = "[local] hittingEntity";
                _hittingEntity._isRuntimeOnly = true;
                EditorUtility.SetDirty(_hittingEntity.gameObject);
            }
#endif
        }
    }
}
