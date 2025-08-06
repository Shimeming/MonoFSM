using System.Collections.Generic;

namespace MonoFSM.Stat
{
    public class StatModifierData : GameData
    {
        public List<StatModifierEntry> effectModifiers;
        public GameData countProvider;
    }
}