using MonoFSM.Core.Condition;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime._1_Conditions.Activator
{
    public class GameObjectActivateChecker : AbstractConditionActivateTarget
    {
        [PropertyOrder(-1)]
        [Required]
        public GameObject _target;

        protected override void ActivateCheckImplement(bool isValid)
        {
            if (_target == null)
            {
                Debug.LogError("GameObjectActivateChecker: Target is null", this);
                return;
            }
            _target.SetActive(isValid);
        }
    }
}
