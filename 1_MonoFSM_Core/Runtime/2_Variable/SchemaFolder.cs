using _1_MonoFSM_Core.Runtime._1_States;
using MonoFSM.Core;

namespace MonoFSM.Variable
{
    public class SchemaFolder : MonoDict<AbstractEntitySchema, AbstractEntitySchema>
    {
        protected override void AddImplement(AbstractEntitySchema item) { }

        protected override void RemoveImplement(AbstractEntitySchema item)
        {
            // throw new NotImplementedException();
        }

        protected override bool CanBeAdded(AbstractEntitySchema item)
        {
            return true;
        }

        protected override string DescriptionTag => "SchemaFolder";
    }
}
