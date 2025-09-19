using System;
using System.Collections.Generic;
using Auto.Utils;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.Simulate;
using MonoFSM.CustomAttributes;
using MonoFSM.Runtime;
using MonoFSM.Variable.FieldReference;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;

namespace MonoFSMCore.Runtime.LifeCycle
{
    public interface IMonoObjectProvider : ICompProvider<MonoObj> //FIXME:這個不是很好...trace不到，最好還是都過一層？
    {
        //FIXME: 需要提供 EntityTag嗎？還是說MonoPoolObj就有EntityTag了？那從 bindPrefab就要有EntityTag

        //這個是給MonoPoolObj用的
        // MonoPoolObj GetMonoObject();
    }

    //1. 先回狀態
    public interface IResetStateRestore //新規用這個，現在和上面都有call, exitLevelAndDestroy是為了換場景很煩可以拔掉
    {
        void ResetStateRestore();
    }

    //2. 在跑這個
    public interface IResetStart //摸別人,set 變數之類的，要不然會reset掉
    {
        void ResetStart();
    }

    public interface IInstantiated
    {
        void OnInstantiated(WorldUpdateSimulator world);
    }

    /// <summary>
    /// 1.LevelAwake,
    /// 2.LevelAwakeReverse
    /// 3.LevelStart,
    /// 4.LevelStartReverse
    /// </summary>
    //關著也能call
    public interface ISceneAwake //摸自己, Prefab也需要(一次性
    {
        void EnterSceneAwake();
    }

    //FIXME: auto 怎麼處理？cache?
    //這個和MonoEntity結構會類似？但分別做不同的角色？
    [ScriptTiming(-20000)]
    [DisallowMultipleComponent]
    [FormerlyNamedAs("MonoPoolObj")]
    public sealed class MonoObj : MonoBehaviour, IPrefabSerializeCacheOwner, IDropdownRoot
    {
        [field: Auto]
        public MonoEntity Entity { get; }

        // public
        //寫一個show error的Attribute，然後在這裡用
        [InfoBox(
            "WorldUpdateSimulator is required for MonoPoolObj to function properly",
            InfoMessageType.Error,
            nameof(RuntimeCheckNoWorldUpdateSimulator)
        )]
        [ShowInDebugMode]
        public WorldUpdateSimulator WorldUpdateSimulator => _worldUpdateSimulator;

        // set => _worldUpdateSimulator = value;
        public void SetWorldUpdateSimulator(WorldUpdateSimulator world)
        {
            // Debug.Log("SetWorldUpdateSimulator" + name, this);
            _worldUpdateSimulator = world;
        }

        bool RuntimeCheckNoWorldUpdateSimulator =>
            WorldUpdateSimulator == null && Application.isPlaying;

        public void Despawn()
        {
            //會跑兩次嗎？
            // Debug.Log("Despawn" + name, this);
            if (WorldUpdateSimulator == null)
            {
                Debug.LogError(
                    "WorldUpdateSimulator is not set. Cannot despawn MonoPoolObj.",
                    this
                );
                return;
            }

            //回傳root mono Obj
            WorldUpdateSimulator.Despawn(GetMonoObjRoot());
        }

        private MonoObj GetMonoObjRoot()
        {
            if (HasParent)
                return _parentObj.GetMonoObjRoot();
            return this;
        }

        private void OnDestroy()
        {
            //fixme: ??
            //play mode 被刪掉要怎麼處理？
            // Debug.Log("MonoObj OnDestroy" + name, this);
        }

        [PreviewInInspector]
        [AutoChildren]
        private ISceneAwake[] _sceneAwakes;

        [PreviewInInspector]
        [AutoChildren]
        private ISceneStart[] _sceneStarts;

        [PreviewInInspector]
        [AutoChildren]
        private ISceneDestroy[] _sceneDestroys;

        [PreviewInInspector]
        [AutoChildren]
        private IResetStateRestore[] _resetStateRestores;

        [PreviewInInspector]
        [AutoChildren]
        private IResetStart[] _resetStarts;

        [PreviewInInspector]
        [AutoChildren]
        private IInstantiated[] _instantiateds;

        [PreviewInInspector]
        [AutoChildren]
        private IUpdateSimulate[] _updateSimulates;

        [PreviewInInspector]
        [AutoChildren]
        private IBeforeSimulate[] _beforeSimulates;

        //FIXME: PoolBeforeReturnToPool? OnReturnPool?

        [ShowInDebugMode]
        private MonoObj _parentObj;

        [SerializeField]
        private WorldUpdateSimulator _worldUpdateSimulator;

        public bool HasParent => _parentObj != null; //有_parentObj就表示是nested的pool object，不作用，交給parent處理

        private void Awake()
        {
            if (transform.parent != null)
                _parentObj = transform.parent.GetComponentInParent<MonoObj>(true);
            if (HasParent)
                return;
#if UNITY_EDITOR
            AutoAttributeManager.AutoReferenceAllChildren(gameObject);
#endif
            //FIXME: prefab cache restore?
            //把 PrefabSerializeCache 的實作拿過來？
        }

        public void SpawnFromPool() //必定是root吧
        {
            ResetStateRestore();
            ResetStart();
        }

        public void SceneAwake(WorldUpdateSimulator world) //可以自己sceneＡwake吧？
        {
            SetWorldUpdateSimulator(world);
            if (HasParent)
                return;
            HandleIAwake();
            //這可以嗎？
            HandleIInstantiated(world); //和IAwake合併？
        }

        //FIXME: 想把這個拿掉
        private void HandleIInstantiated(WorldUpdateSimulator world)
        {
            if (HasParent)
                return;
            // Debug.Log("HandleIInstantiated",this);
            foreach (var item in _instantiateds)
            {
                if (item == null)
                    continue;
                try
                {
                    item.OnInstantiated(world);
                }
                catch (Exception e)
                {
                    if (item is MonoBehaviour)
                        Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                    else
                        Debug.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        public void ResetStateRestore() //還是要分兩階，先還原，再開始？ 還是說有這種dependency本身就不好...? life cycle集中化
        {
            if (HasParent)
                return;
            HandleIResetStateRestore();
        }

        public void ResetStart()
        {
            if (HasParent)
                return;
            HandleIResetStart();
        }

        private void HandleIResetStateRestore()
        {
            if (HasParent)
                return;
            // Debug.Log("[MonoObj] HandleIResetStateRestore", this);
            foreach (var item in _resetStateRestores)
            {
                if (item == null)
                    continue;
                try
                {
                    item.ResetStateRestore();
                }
                catch (Exception e)
                {
                    if (item is MonoBehaviour)
                        Debug.LogError(e.StackTrace, item as MonoBehaviour);
                    else
                        Debug.LogError(e.StackTrace);
                }
            }
        }

        private void HandleIResetStart()
        {
            if (HasParent)
                return;
            foreach (var item in _resetStarts)
            {
                if (item == null)
                    continue;
                //FIXEM: 用trycatch不好debug?
                // try
                // {
                item.ResetStart();
                // }
                // catch (Exception e)
                // {
                //     if (item is MonoBehaviour)
                //         Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                //     else
                //         Debug.LogError(e.Message + "\n" + e.StackTrace);
                // }
            }
        }

        public bool IsProxy { get; set; }

        public void BeforeSimulate(float deltaTime)
        {
            if (HasParent)
                return;
            if (IsProxy)
                return;
            foreach (var item in _beforeSimulates)
            {
                if (item == null || !item.isActiveAndEnabled)
                    continue;
                // try
                // {
                item.BeforeSimulate(deltaTime);
                // }
                // catch (Exception e)
                // {
                //     if (item is MonoBehaviour)
                //         Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                //     else
                //         Debug.LogError(e.Message + "\n" + e.StackTrace);
                // }
            }
        }

        //理論上沒有註冊就不會call到這個
        public void Simulate(float deltaTime)
        {
            if (HasParent)
                return;
            //如果proxy就跳過？
            if (IsProxy)
                return;
            //要在state machine之後嗎？還是要可以排順序？
            foreach (var item in _updateSimulates)
            {
                if (item == null || !item.IsValid)
                    continue;
                // try
                // {
                item.Simulate(deltaTime);
                // }
                // catch (Exception e)
                // {
                //     if (item is MonoBehaviour)
                //         Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                //     else
                //         Debug.LogError(e.Message + "\n" + e.StackTrace);
                // }
            }
        }

        public void AfterUpdate()
        {
            if (HasParent)
                return;
            if (IsProxy)
                return;
            foreach (var item in _updateSimulates)
            {
                if (item == null || item.isActiveAndEnabled == false)
                    continue;
                //FIXME: 這個很難偵錯耶？集中update的壞處，要怎麼樣
                // try
                // {
                Profiler.BeginSample($"MonoObj AfterUpdate");
                item.AfterUpdate();
                Profiler.EndSample();
                // }
                // catch (Exception e)
                // {
                //     if (item is MonoBehaviour)
                //         Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                //     else
                //         Debug.LogError(e.Message + "\n" + e.StackTrace);
                // }
            }
        }

        /// <summary>
        /// 兩個進入點，SpawnFromPool 和 SceneAwake
        /// 1. SpawnFromPool 是從Pool中取出來的物件，
        /// 2. SceneAwake ?
        /// </summary>
        private void HandleIAwake()
        {
            var iLevelAwakes = new List<ISceneAwake>(_sceneAwakes);
            iLevelAwakes.Reverse();

            foreach (var item in iLevelAwakes)
            {
                if (item == null)
                    continue;
                try
                {
                    item.EnterSceneAwake();
                }
                catch (Exception e)
                {
                    if (item is MonoBehaviour)
                        Debug.LogError(e.StackTrace, item as MonoBehaviour);
                    else
                        Debug.LogError(e.StackTrace);
                }
            }
        }

        public void HandleSceneStart()
        {
            if (HasParent)
                return;
            if (WorldUpdateSimulator.IsReady == false)
                // Debug.LogError("WorldUpdateSimulator is not ready. Cannot proceed with SceneAwake.", this);
                return;
            var iLevelStarts = new List<ISceneStart>(_sceneStarts);
            iLevelStarts.Reverse();

            foreach (var item in iLevelStarts)
            {
                if (item == null)
                    continue;
                try
                {
                    item.EnterSceneStart();
                }
                catch (Exception e)
                {
                    if (item is MonoBehaviour)
                        Debug.LogError(e.Message + "\n" + e.StackTrace, item as MonoBehaviour);
                    else
                        Debug.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        public void OnReturnPool() //會被despawn才需要？ 反註冊用？
        {
            if (HasParent)
                return;
        }
    }
}
