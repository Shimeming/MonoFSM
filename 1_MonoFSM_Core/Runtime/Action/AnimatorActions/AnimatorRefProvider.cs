using MonoFSM.Foundation;
using MonoFSM.Variable;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.AnimatorActions
{
    public class AnimatorRefProvider : AbstractValueProvider<Animator>
    {
        [SerializeField]
        [DropDownRef]
        private VarComp _animatorRef;
        public override Animator Value => _animatorRef?.Value as Animator;
    }
}
