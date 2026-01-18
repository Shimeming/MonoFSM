using MonoFSM.Core.Runtime;
using MonoFSM.Core;

namespace MonoFSM.Variable
{
    public class SchemaFolder : MonoDictFolder<string, AbstractEntitySchema>
    {
        protected override string DescriptionTag => "SchemaFolder";
    }
}
