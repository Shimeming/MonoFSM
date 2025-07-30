using MonoFSM.Core.Runtime.Action;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction
{
    public class AssignMonoVarFromOwner : AbstractStateAction
    {
        [CompRef] [Auto] private IEntityProvider _ownerProvider;

        [FormerlySerializedAs("_varBlackboard")] [FormerlySerializedAs("_varMono")] [SerializeField] [DropDownRef]
        private VarEntity _varEntity;

        protected override void OnActionExecuteImplement()
        {
            Debug.Log($"AssignMonoVarFromOwner: Assigning {_ownerProvider.Description} to {_varEntity.name}");
            var source = _ownerProvider.GetComponentOfOwner<MonoEntity>();
            _varEntity.SetValue(source, this);
        }

        public override string Description =>
            $"Assign {_ownerProvider?.Description} to {_varEntity?.name}";
    }
}