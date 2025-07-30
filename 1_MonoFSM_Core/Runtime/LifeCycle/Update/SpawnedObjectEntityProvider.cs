using System;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSMCore.Runtime.LifeCycle
{
    //FIXME: 好像很trivial, 不好懂
    //
    public class SpawnedObjectEntityProvider : AbstractEntityProvider, IEntityProvider, ICompProvider<MonoEntity>
    {
        [Required] [ShowInInspector] [AutoParent]
        private IMonoObjectProvider _monoObjectProvider; //FIXME: 怪怪的，還是應該統一進入點SpawnAction就好？還有可能有別種嗎？

        [PreviewInInspector]
        public override MonoEntity monoEntity
        {
            get
            {
                //editor time應該沒有ㄅ
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                    _monoObjectProvider = GetComponentInParent<IMonoObjectProvider>(true);
#endif
                // return _monoObjectProvider.
                
                return _monoObjectProvider?.Get()?.GetComponent<MonoEntity>();
            }
        }


        //FIXME: Editor 不一定有啊..runtime才有？
        // [Required]
        // [PreviewInInspector]
        // public MonoEntityTag entityTag => _monoObjectProvider?.Get()?.GetComponent<MonoEntityTag>();

        // public T GetComponentOfOwner<T>()
        // {
        //     var owner = monoEntity;
        //     if (owner == null)
        //         return default;
        //     return owner.gameObject.GetComponent<T>();
        // }

        public MonoEntity Get()
        {
            return monoEntity;
        }

        public object GetValue()
        {
            return monoEntity;
        }

        public Type ValueType => typeof(MonoBlackboard);

        public string Description => "SpawnedObjectProvider: " + _monoObjectProvider?.Get()?.name;
    }
}