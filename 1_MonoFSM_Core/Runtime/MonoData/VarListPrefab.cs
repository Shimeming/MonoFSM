using MonoFSMCore.Runtime.LifeCycle;

namespace MonoFSM.Core.Variable
{
    //還是要吃scriptableObject, 如果不會動的話，collection
    public class VarListPrefab : VarList<MonoObj>, ICompProvider<MonoObj> //, IPrefabSerializeCacheOwner
    {
        public MonoObj Get()
        {
            return CurrentListItem;
        }
    }
}