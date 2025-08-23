using System;
using System.Collections.Generic;
using System.Diagnostics;
using MonoFSM.Core;
using MonoFSM.Core.Attributes;
using MonoFSM.Runtime;
using MonoFSM.Variable.Attributes;
using MonoFSMCore.Runtime.LifeCycle;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Fusion.Addons.FSM
{
    public interface IStateMachineController
    {
        public float DeltaTime { get; }
    }

    public interface IStateMachineOwner
    {
        void CollectStateMachines(List<IStateMachine> stateMachines);
        string name { get; }
    }

    [DisallowMultipleComponent]
    public class StateMachineLogic : MonoBehaviour, IResetStart
    {
        [AutoParent]
        private MonoEntity _parentEntity;
        public MonoEntity ParentEntity => _parentEntity;

        public float DeltaTime => _stateMachineController.DeltaTime;

        // #if UNITY_EDITOR
        /// <summary>
        /// 確保有controller才會執行
        /// </summary>
        [CompRef]
        [Required]
        [Auto]
        private IStateMachineController _stateMachineController;

        // #endif
        private bool _backingEnableLogging = false;

        public bool EnableLogging
        {
            get => _backingEnableLogging;
            set => _backingEnableLogging = value;
        }

        protected List<IStateMachine> _stateMachinesInternal = new(32);
        public List<IStateMachine> StateMachines => _stateMachinesInternal;

        protected List<IState> _statePool; // Used by CheckDuplicateStates

        [AutoChildren]
        public AnyState anyState;

        [PreviewInDebugMode]
        public bool _stateMachinesCollected { get; protected set; }
        public bool _manualUpdateMode { get; protected set; }

        public bool IsCurrentState(IState state)
        {
            if (state == null)
                return false;
            if (!_stateMachinesCollected)
                return false;
            return _stateMachinesInternal[0].ActiveState == state;
        }

        [ShowInInspector]
        private IState PreviousState
        {
            get
            {
                if (!_stateMachinesCollected)
                    return null;
                // if (_stateMachinesInternal.Count == 0) return null;
                return _stateMachinesInternal[0].PreviousState;
            }
        }

        [ShowInInspector]
        private IState CurrentState
        {
            get
            {
                if (!_stateMachinesCollected)
                    return null;
                // if (_stateMachinesInternal.Count == 0) return null;
                return _stateMachinesInternal[0].ActiveState;
            }
        }

        // Called by controllers to initialize.
        public void InitializeLogic()
        {
            if (!_stateMachinesCollected)
                CollectStateMachines();
            // Debug.Log($"Initializing MonoStateMachineController on {gameObject.name}");
        }

        public void SetManualUpdateMode(bool manualUpdate)
        {
            _manualUpdateMode = manualUpdate;
        }

        //FIXME: 到處亂叫，不爽, InitializeLogic & CollectStateMachines
        public void CollectStateMachines()
        {
            _stateMachinesInternal.Clear();
            if (_statePool != null)
                _statePool.Clear();

            // Get IStateMachineOwner components from children of this GameObject.
            var owners = GetComponentsInChildren<IStateMachineOwner>(true);

            // Assuming ListPool is a static utility class available.
            // If not, replace with: var tempMachines = new List<IStateMachine>(32);
            // var tempMachines = new List<IStateMachine>(32); // Placeholder if ListPool is not found
            var tempMachines = ListPool.Get<IStateMachine>(32);

            for (var i = 0; i < owners.Length; i++)
            {
                owners[i].CollectStateMachines(tempMachines);
                CheckCollectedMachines(owners[i], tempMachines);

                for (var j = 0; j < tempMachines.Count; j++)
                {
                    var stateMachine = tempMachines[j];
                    if (_stateMachinesInternal.Contains(stateMachine))
                    {
                        Debug.LogError(
                            $"Trying to add already collected state machine for second time {stateMachine.Name}",
                            gameObject
                        );
                        continue;
                    }

                    CheckDuplicateStates(stateMachine.Name, stateMachine.States);
                    _stateMachinesInternal.Add(stateMachine);
                }

                tempMachines.Clear();
            }

            _stateMachinesCollected = true;
            // If using a real ListPool:
            ListPool.Return(tempMachines);
        }

        [Conditional("DEBUG")]
        protected void CheckCollectedMachines(
            IStateMachineOwner owner,
            List<IStateMachine> machines
        )
        {
            if (machines.Count == 0)
            {
                var ownerObject = (owner as Component).gameObject;
                Debug.LogWarning(
                    $"No state machines collected from the state machine owner {ownerObject.name}",
                    ownerObject
                );
            }
        }

        [Conditional("DEBUG")]
        protected void CheckDuplicateStates(string stateMachineName, IState[] states)
        {
            if (states == null || states.Length == 0)
                return;

            if (_statePool == null)
                _statePool = new List<IState>(128);

            foreach (var state in states)
            {
                if (state == null)
                    continue;

                if (_statePool.Contains(state) == true)
                    throw new InvalidOperationException(
                        $"State {state.Name} is used for multiple state machines, this is not allowed! State Machine: {stateMachineName}"
                    );

                _statePool.Add(state);
            }
        }

        public void ResetStart()
        {
            InitializeLogic();
            foreach (var stateMachine in StateMachines)
                stateMachine.Reset();

            // else
            //     Debug.LogError("State machines not collected yet, cannot reset.", this);
        }
    }
}
