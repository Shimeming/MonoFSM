using System;
using JetBrains.Annotations;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Runtime.Action;
using MonoFSM.Core.Variable;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Interact.EffectHit;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.LifeCycle
{
    public interface IAfterSpawnProcess //action在Spawn之後執行 也有種event的概念？AfterSpawnEventHandler?
    {
        public void AfterSpawn(
            MonoObj obj,
            Vector3 position,
            Quaternion rotation,
            [CanBeNull] GeneralEffectHitData hitData
        ); //對著某個東西spawn?
    }

    //重寫FXPlayer
    public class SpawnAction : AbstractArgEventHandler<GeneralEffectHitData>, IMonoObjectProvider //ICompProvider<MonoPoolObj>
    {
        //FIXME: 下面要有各種preProcess action?
        // [Required]
        // [CompRef]
        // [AutoChildren(DepthOneOnly = true)]
        // [ValueTypeValidate(typeof(MonoObj))]
        // private ValueProvider _poolObjProvider; //使用VarPoolObj來存儲目標物件

        // [Required] [SerializeField] private VarEntity _poolObjVar; //用來存取剛spawn的物件
        [Required]
        [SerializeField]
        private VarMonoObj _poolObjVar;

        [CompRef]
        [AutoChildren]
        private IAfterSpawnProcess[] _preSpawnActions;

        //檢查？SerializeClass的話？

        //型別篩選 vs attribute篩選？
        //FIXME: 應該要分ConfigPrefabProvider, 不可以都用ICompProvider，因為會有Runtime的Prefab
        //Prefab特殊，不該runtime去Instantiate對ㄅ，都應該從prefab來，就算要也是複製狀態

        [PreviewInDebugMode]
        private MonoObj Prefab => _poolObjVar?.Value; // _poolObjProvider?.Get<MonoObj>();

        [CompRef]
        [AutoChildren]
        private SpawnEventHandler _spawnEventHandler;

        //FIXME: preview scale & rotation
        protected override void OnActionExecuteImplement()
        {
            Debug.Log("SpawnAction OnStateEnterImplement", this);
            //FIXME: 時機點？FixedUpdateNetwork?
            Spawn(Prefab, transform.position, transform.rotation, null);
        }

        public bool _isUsingSpawnTransformScale;

        [PreviewInInspector]
        private MonoObj _lastSpawnedObj;

        [AutoParent]
        private MonoObj _parentObj;

        // [Required] [SerializeField] private VarMonoObj _spawnedObjVar; //用來存取剛spawn的物件
        [Required]
        [SerializeField]
        private VarEntity _spawnedEntityVar;

        private void Spawn(
            MonoObj prefab,
            Vector3 position,
            Quaternion rotation,
            [CanBeNull] GeneralEffectHitData hitData
        )
        {
            //FIXME: canspawn?
            if (prefab == null)
            {
                Debug.LogError("SpawnAction: Prefab is null", this);
                return;
            }

            if (_parentObj == null)
            {
                Debug.LogError("SpawnAction: No MonoPoolObj found in parent", this);
                return;
            }

            if (_parentObj.WorldUpdateSimulator == null)
            {
                Debug.LogError(
                    "SpawnAction: No WorldUpdateSimulator found in MonoPoolObj",
                    _parentObj
                );
                return;
            }

            var newObj = _parentObj.WorldUpdateSimulator.Spawn(prefab, position, rotation); //Runner.spawn?
            if (newObj == null)
                return;
            //用目前這個action的transform的scale,fixme; 可能需要別種？物件本身的scale?還是應該避免
            //fixme: 為什麼要這樣？
            if (_isUsingSpawnTransformScale)
                newObj.transform.localScale = transform.lossyScale;

            newObj.gameObject.SetActive(true);
            _spawnedEntityVar.SetValue(newObj.GetComponent<MonoEntity>(), this); //更新變數
            //Rotation呢？
            _lastSpawnedObj = newObj;
            _spawnEventHandler?.OnSpawn(newObj, position, rotation);

            foreach (var preSpawnAction in _preSpawnActions)
                preSpawnAction.AfterSpawn(newObj, position, rotation, hitData);
        }

        protected override void OnArgEventReceived(GeneralEffectHitData arg)
        {
            // base.EventReceived(arg);
            //噴Receiver的位置?
            var receiverTrans = arg.Receiver.transform;

            var pos = arg.hitPoint ?? receiverTrans.position; //如果沒有hitPoint，就用Receiver的位置
            Debug.Log(
                "SpawnAction EventReceived, pos: " + pos + ", hitPoint: " + arg.hitPoint,
                this
            );

            //FIXME: arg是EffectHitData...point和normal都放過來嗎？
            var rotation = receiverTrans.rotation;
            if (arg.hitNormal != null)
            {
                // up 改為 Vector3.up，避免參考 receiverTrans
                var normal = arg.hitNormal.Value.normalized;
                var up = Vector3.up;
                // 避免 up 跟 normal 太接近導致 LookRotation 不穩定
                if (Mathf.Abs(Vector3.Dot(normal, up)) > 0.99f)
                    up = Vector3.right;
                rotation = Quaternion.LookRotation(normal, up);
                Debug.Log("hitNormal is not null, using it for rotation " + rotation, this);
            }

            Spawn(Prefab, pos, rotation, arg);
            // var newObj = PoolManager.Instance.BorrowOrInstantiate(target, t.position, t.rotation);
        }

        public MonoObj GetMonoObject()
        {
            if (_lastSpawnedObj != null)
            {
                return _lastSpawnedObj;
            }
            else
            {
                Debug.LogError("No object spawned yet, returning null", this);
                return null; //或許可以拋出異常？
            }
        }

        public object GetValue()
        {
            if (_lastSpawnedObj != null)
            {
                return _lastSpawnedObj;
            }
            else
            {
                Debug.LogError("No object spawned yet, returning null", this);
                return null; //或許可以拋出異常？
            }
        }

        public Type ValueType => typeof(MonoObj);

        public MonoObj Get()
        {
            if (!Application.isPlaying)
                return Prefab;
            if (_lastSpawnedObj != null)
            {
                return _lastSpawnedObj;
            }
            else
            {
                if (Application.isPlaying)
                    Debug.LogError("No object spawned yet, returning null", this);
                return null; //或許可以拋出異常？
            }
        }
    }

    public interface ISpawnProcessor //想找一個static的對象來生成物件 (但不能真的static，multi peer的話)
    {
        MonoObj Spawn(MonoObj obj, Vector3 position, Quaternion rotation);
        public void Despawn(MonoObj obj);
    }
}
