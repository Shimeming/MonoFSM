using MonoFSM.Core;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime._3_FlagData
{
    public class MonoSOConfig : ScriptableObject, ISceneSavingCallbackReceiver, ISceneSavingAfterCallbackReceiver,
        ICustomHeavySceneSavingCallbackReceiver
    {
        public virtual void OnBeforeSceneSave() //hmm需要嗎 這反而不好？
        {
        }

        public virtual void OnAfterSceneSave()
        {
        }

        public virtual void OnHeavySceneSaving()
        {
        }
    }
}