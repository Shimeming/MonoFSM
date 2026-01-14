using _1_MonoFSM_Core.Runtime._1_States;
using MonoFSM.Core;

namespace MonoFSM.Variable
{
    public class SchemaFolder : MonoDictFolder<string, AbstractEntitySchema>
    {
        protected override string DescriptionTag => "SchemaFolder";
    }
}
