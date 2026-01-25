using MonoFSM.Core.Runtime;
using MonoFSM.Core;

namespace MonoFSM.Variable
{
    //FIXME: 應該用MonoTypeTag當作key?
    public class SchemaFolder : MonoDictFolder<string, AbstractEntitySchema>
    {
        protected override string DescriptionTag => "SchemaFolder";
    }
}
