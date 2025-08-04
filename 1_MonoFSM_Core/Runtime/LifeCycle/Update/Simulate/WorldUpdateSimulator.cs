using System.Collections.Generic;
using System.Linq;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.LifeCycle;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace MonoFSM.Core.Simulate
{
    public interface ISimulateRunner
    {
    }
    
    public static class WorldUpdateSimulatorExtensions
    {
        public static MonoObj Spawn(this GameObject gObj, MonoObj obj, Vector3 position, Quaternion rotation)
        {
            if (gObj == null)
            {
                Debug.LogError("Cannot spawn a MonoPoolObj from a null GameObject.", obj);
                return null;
            }

            var simulator = WorldUpdateSimulator.GetWorldUpdateSimulator(gObj);
            // var worldUpdateSimulator = gObj.GetComponent<WorldUpdateSimulator>();
            if (simulator == null)
            {
                Debug.LogError("WorldUpdateSimulator not found on the GameObject.", gObj);
                return null;
            }

            return simulator.Spawn(obj, position, rotation);
        }
    }

    //要當作世界系統中心嗎？但如果是runner旁邊的話，就不在scene上喔

    //NOTE: 放在runner上!
    //場上可以有收集器？還是另外自己做掉?
    [DefaultExecutionOrder(10000)] //確保在所有Update之後執行
    public sealed class WorldUpdateSimulator : MonoBehaviour
    {
        //反綁？
        //fsm reset?, simulate runner 
        [Required][CompRef][Auto] private ISimulateRunner _simulateRunner;

        //FIXME: Spawn要不要過我？
        [Required][CompRef][Auto] private ISpawnProcessor _spawnProcessor;

        private void Awake()
        {
            _spawnProcessor = GetComponent<ISpawnProcessor>();
            _simulateRunner = GetComponent<ISimulateRunner>();
            // _simulators.AddRange(_localSimulators); //不需要了？
        }
        
        public static WorldUpdateSimulator GetWorldUpdateSimulator(GameObject me)
        {
            //這個是用來獲取當前的WorldUpdateSimulator
            var monoPoolObj = me.GetComponent<MonoObj>();
            if (monoPoolObj == null)
            {
                Debug.LogError("MonoPoolObj not found on the GameObject.", me);
                return null;
            }
            if (monoPoolObj.WorldUpdateSimulator == null)
            {
                Debug.LogError("WorldUpdateSimulator not set on MonoPoolObj.", monoPoolObj);
                return null;
            }
            return monoPoolObj.WorldUpdateSimulator;
        }

        //全世界都該透過這個spawn?
        public MonoObj Spawn(MonoObj obj, Vector3 position, Quaternion rotation)
        {
            if (obj == null)
            {
                Debug.LogError("Cannot spawn a null MonoPoolObj.", this);
                return null;
            }

            //Spawn Strategy? 透過 Fusion的PoolObject 系統...那何不都用他的就好?
            var result = _spawnProcessor.Spawn(obj, position, rotation);
#if UNITY_EDITOR
            //Editor裡還是直接使用AutoAttributeManager，cache太容易髒掉了
            Debug.Log($"AutoReferenceAllChildren: {result.name}", result);
            AutoAttributeManager.AutoReferenceAllChildren(result.gameObject);
#endif
            //重置狀態
            result.ResetStateRestore();
            result.ResetStart();
            
            RegisterMonoObject(result);
            //poolObject spawn lifecycle? 可以整進去？
            result.SpawnFromPool();
            return result;
        }

        public void Despawn(MonoObj obj)
        {
            if (obj == null) return;
            // Unregister the object from the world update simulator
            UnregisterMonoObject(obj);
            obj.ResetStateRestore();
            // Return the object to the pool
            //FIXME: 要先做事？OnReturnPool? OnDespawn
            _spawnProcessor.Despawn(obj); //看實作
        }

        public void RegisterMonoObject(MonoObj target)
        {
            if(_monoObjectSet.Add(target))
                target.WorldUpdateSimulator = this;
        }

        public void UnregisterMonoObject(MonoObj target)
        {
            _monoObjectSet.Remove(target);
            target.WorldUpdateSimulator = null; //清除引用
        }

        private void SceneAwake()
        {
            //這個是用來做初始化的？
            foreach (var monoObject in _monoObjectSet) monoObject.SceneAwake(this);
            Debug.Log($"WorldUpdateSimulator SceneAwake called with {_monoObjectSet.Count} MonoPoolObjs.", this);
        }

        private void SceneStart()
        {
            foreach (var monoObject in _monoObjectSet) monoObject.HandleSceneStart();
        }
        
        

        //從player進入？
        public void ResetLevelRestore()
        {
            //Pool回收會
            PoolManager.Instance.ReturnAllObjects();
            foreach (var mono in _monoObjectSet) mono.ResetStateRestore();
            Debug.Log($"WorldUpdateSimulator ResetStateRestore called with {_monoObjectSet.Count} MonoPoolObjs.", this);
        }

        public void ResetLevelStart()
        {
            foreach (var mono in _monoObjectSet) mono.ResetStart();
        }

        public void WorldInit()
        {
            //SceneAwake可以自己做ㄅ？
            IsReady = true;
            SceneAwake();
            SceneStart();
            ResetLevelRestore();
            ResetLevelStart();
        }

        //FIXME: 可能會動態移除
        // [PreviewInInspector] [AutoChildren] private IUpdateSimulate[] _localSimulators;

        // private readonly HashSet<IUpdateSimulate> _simulators = new(); //HashSet?

        // [PreviewInInspector] [AutoChildren] private IMonoObject[] _localMonoObjects; //FIXME這顆要掛在？
        private readonly HashSet<MonoObj> _monoObjectSet = new(); //這個是用來做reset的？還是要有一個MonoObjectRunner?

#if UNITY_EDITOR
        // [PreviewInInspector] private IUpdateSimulate[] PreviewSimulators => _simulators.ToArray();
        [PreviewInInspector] private MonoObj[] PreviewMonoObjects => _monoObjectSet.ToArray();
#endif
        [ShowInInspector]
        public bool IsReady { get; private set; } = false;


        private readonly HashSet<MonoObj> _currentUpdatingObjs = new();

        /// <summary>
        /// 需要依照環境決定怎麼simulate
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Simulate(float deltaTime)
        {
            if (!IsReady)
                return;
         
            _currentUpdatingObjs.Clear();
            _currentUpdatingObjs.AddRange(_monoObjectSet); 
            

            //FIXME: isProxy? 要ㄇ 跳過模擬，或是regiester要兩階段
            foreach (var monoObject in _currentUpdatingObjs)
                if (monoObject is { isActiveAndEnabled: true })
                    monoObject.Simulate(deltaTime);
            
                    
            // else
            //     Debug.LogWarning("A mono object is null or not active and enabled, skipping simulation.");

            // foreach (var simulator in _simulators)
            //     if (simulator is { isActiveAndEnabled: true })
            //         simulator.Simulate(deltaTime);
        }

        public void AfterUpdate()
        {
            if (!IsReady)
                return;
            foreach (var monoObject in _monoObjectSet)
                if (monoObject is { isActiveAndEnabled: true })
                    monoObject.AfterUpdate();
            // else
            //     Debug.LogWarning("A mono object is null or not active and enabled, skipping after update.");
        }


#if UNITY_EDITOR
        [MenuItem("MonoFSM/ResetLevel %R")]
        public static void TestResetLevel() //Cheat Reset?
        {
            if (Application.isPlaying)
            {
                Debug.Log("ResetLevel CMD+Shift+R");
                var simulators = FindObjectsByType<WorldUpdateSimulator>(FindObjectsSortMode.None);
                //FIXME: 會拿到Temporary Runner Prefab所以才全拿
                if (simulators.Length == 0)
                    Debug.LogError(
                        "No WorldUpdateSimulator found in the scene. Ensure it is present for proper reset.");
                else
                    foreach (var simulator in simulators)
                        //這樣就可以reset了
                        simulator.ResetLevelRestore();
            }
            else
            {
                CompilationPipeline.RequestScriptCompilation();
            }
        }
#endif
        public void Render(float runnerLocalRenderTime)
        {
            // throw new System.NotImplementedException();
        }
    }
}