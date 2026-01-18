using MonoFSM.Core.DataProvider;
using MonoFSM.Foundation;

namespace MonoFSM.Core.Runtime.Pattern.DataProvider.ComponentWrapper
{
    public class StringLiteralComp : AbstractDescriptionBehaviour, IStringProvider
    {
        protected override string DescriptionTag => "String";
        public override string Description => _literal;

        public string GetString()
        {
            return _literal;
        }

        public string _literal;
    }
}
