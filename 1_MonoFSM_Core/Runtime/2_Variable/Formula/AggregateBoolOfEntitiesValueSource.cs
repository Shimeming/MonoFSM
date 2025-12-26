using MonoFSM.Core.Attributes;
using MonoFSM.Core.Variable;
using MonoFSM.Foundation;
using MonoFSM.Variable;

namespace MonoFSM.Core.Formula
{
    public class AggregateBoolOfEntitiesValueSource : AbstractValueSource<bool>
    {
        public VarListEntity _entities;

        //這個怎麼dropdown找？
        [SOConfig("VariableType")]
        public VariableTag _boolVarTag; //hmm??

        //and, 需要 or?
        public override bool Value
        {
            get
            {
                if (_entities == null || _boolVarTag == null)
                    return false;

                foreach (var entity in _entities.GetList())
                {
                    if (entity == null)
                        continue;

                    var boolVar = entity.GetVar<VarBool>(_boolVarTag);
                    if (boolVar != null && !boolVar.Value)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
