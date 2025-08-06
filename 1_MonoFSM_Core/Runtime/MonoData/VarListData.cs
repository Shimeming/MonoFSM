using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Variable;

namespace _1_MonoFSM_Core.Runtime.MonoData
{
    public class VarListData : VarList<GameData>, IGameDataProvider
    {
        public GameData GameData => CurrentListItem;
    }
}