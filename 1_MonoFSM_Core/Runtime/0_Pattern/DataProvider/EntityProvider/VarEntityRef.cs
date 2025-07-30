using MonoFSM.Core.DataProvider;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Attributes;
using MonoFSM.Runtime.Mono;
using MonoFSM.Runtime.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Runtime
{
    //這個也是 IBlackboardProvider的一種，會打架？
    //這個不該繼承VariableProviderRef？應該自己獨立？

    //FIXME: 好像可以留著喔？重寫？ 
    //provider是從variable拿到的MonoEntity，這個MonoEntity是
    // [Obsolete]
    public class VarEntityRef : MonoBehaviour, IEntityProvider
    {
        // [ValueTypeValidate(typeof(MonoEntity))] [SerializeField]
        // private ValueProvider _varEntityProvider;

        //FIXME: 必定從自身拿到？
        [DropDownRef] [SerializeField] private VarEntity _varEntity;

        [ValueTypeValidate(typeof(MonoEntity))] [SerializeField]
        private ValueProvider _monoEntityProvider;

        public MonoEntity monoEntity => _varEntity?.Value;

        [ShowInInspector]
        public MonoEntityTag entityTag =>
            monoEntity?.DefaultTag ?? _varEntity?._monoEntityTag; //FIXME: monoEntity是null, 這樣 _varEntity要先知道有什麼tag?

        public T GetComponentOfOwner<T>()
        {
            if (monoEntity == null)
            {
                Debug.LogError("MonoEntity is null, cannot get component.", this);
                return default;
            }

            return monoEntity.GetComponent<T>();
            // if (Value == null)
            // {
            //     Debug.LogError("VariableOwner is null, cannot get component.");
            //     return default;
            // }
            //
            // return Value.GetComponent<T>();
        }
    }
}