using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MonoFSM.Core.Attributes;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MonoFSM.Runtime.Vote
{
    public interface IRuntimeConditionImplementation //這個interface的目的是？
    {
        ConditionType GetConditionType();
        void OnValueChange(bool value);
        void Vote(Object voter, bool vote);
        bool GetDefaultValue();
    }

    public enum ConditionType
    {
        AND,
        OR,
    }

    public interface IVoteChild
    {
        public MonoBehaviour VoteOwner { get; }
    }

    //只有bool
    //[]: 如果想要放在Scriptable上，需要FlagInit時把資料清乾淨，如果沒有reload domain會殘留
    [Serializable]
    public class RuntimeConditionVote : IRuntimeConditionImplementation //這個只有Bool
    {
        //default用And?
        public RuntimeConditionVote(
            ConditionType type = ConditionType.AND,
            bool defaultValue = false,
            OnValueChangeDelegate onValueChangeDelegate = null,
            ChangeResultTiming updateTiming = ChangeResultTiming.OnVote
        )
        {
            _getConditionTypeDelegate = () => type;
            _getDefaultValueDelegate = () => defaultValue;
            _onValueChangeDelegate = onValueChangeDelegate;
            _currentResult = GetDefaultValue();
            _changeChangeResultTiming = updateTiming;
            OnValueChange(_currentResult);
            voteDict = new Dictionary<Object, VoteRecord>();
        }

        public RuntimeConditionVote(
            GetConditionTypeDelegate getConditionTypeDelegate,
            GetDefaultValueDelegate getDefaultValueDelegate,
            OnValueChangeDelegate onValueChangeDelegate = null,
            ChangeResultTiming updateTiming = ChangeResultTiming.OnVote
        )
        {
            _getConditionTypeDelegate = getConditionTypeDelegate;
            _getDefaultValueDelegate = getDefaultValueDelegate;
            _onValueChangeDelegate = onValueChangeDelegate;
            _changeChangeResultTiming = updateTiming;
            _currentResult = GetDefaultValue();
            OnValueChange(_currentResult);
            voteDict = new Dictionary<Object, VoteRecord>();
        }

        public void Reset()
        {
            voteDict.Clear();
            _currentResult = GetDefaultValue();
            OnValueChange(_currentResult);
        }

        [ShowInPlayMode]
        private Object[] keys
        {
            get
            {
                if (voteDict == null)
                {
                    Debug.Log("voteDict is null");
                    return Array.Empty<Object>();
                }

                return voteDict.Count > 0 ? voteDict.Keys.ToArray() : Array.Empty<Object>();
            }
        }

        [ShowInPlayMode]
        private VoteRecord[] values
        {
            get
            {
                if (voteDict == null)
                    return Array.Empty<VoteRecord>();
                return voteDict.Count > 0 ? voteDict.Values.ToArray() : Array.Empty<VoteRecord>();
            }
        }

        public Dictionary<Object, VoteRecord> voteDict = new();

        [Serializable]
        public struct VoteRecord
        {
            //FIXME: 不要用string, keep ref?
            // private string _voterName;
            private bool _vote;
            private Object _voterRef;

#if UNITY_EDITOR
            [ShowInPlayMode]
            public string Voter => _voterRef.name;
#endif

            [ShowInPlayMode]
            public bool Vote => _vote;

            public VoteRecord(Object voter, bool vote)
            {
                _voterRef = voter;
                _vote = vote;
            }
        }

        public ConditionType GetConditionType()
        {
            return _getConditionTypeDelegate();
        }

        public bool GetDefaultValue()
        {
            return _getDefaultValueDelegate.Invoke();
        }

        public void OnValueChange(bool value)
        {
            _onValueChangeDelegate?.Invoke(value);
            OnVoteChange?.Invoke(value);
        }

        //FIXME: 有做這個耶！ 統一事件的實作
        public UnityEvent<bool> OnVoteChange = new UnityEvent<bool>();

        private GetDefaultValueDelegate _getDefaultValueDelegate; //有必要用delegate嗎？
        private OnValueChangeDelegate _onValueChangeDelegate;
        private GetConditionTypeDelegate _getConditionTypeDelegate;

        public delegate bool GetDefaultValueDelegate();
        public delegate void OnValueChangeDelegate(bool value);
        public delegate ConditionType GetConditionTypeDelegate();

        public enum ChangeResultTiming
        {
            OnVote, //一觸發就更新, 線性call
            OnManualUpdate, //類似ECS, 應該就叫做OnUpdate
            //LazySolve, 取用的時候才solve, 這個就不是event driven了
        }

        private ChangeResultTiming _changeChangeResultTiming = ChangeResultTiming.OnVote;

        public void ManualUpdate() //投票的時候還沒solve, 在update (或是真的要手動Lazy Solve)
        {
            CheckResult();
        }

        public void Vote(Object voter, bool vote) //FIXME: 直接拿對象來vote比較好？IVoter?
        {
            //FIXME: 可能會有相同owner所以進入點不同但想要A綁B解綁？
            // if (voter is IVoteChild voteChild)
            //     voter = voteChild.VoteOwner;

            //不需樣Add?
            voteDict[voter] = new VoteRecord(voter, vote);
            // Debug.Log($"Vote {voter} bool:{vote}");

            if (_changeChangeResultTiming == ChangeResultTiming.OnVote)
                CheckResult();
            //ManualUpdate
            //投票的時候還沒solve
        }

        public void Revoke(Object voter)
        {
            // if (voter is IVoteChild voteChild)
            // voter = voteChild.VoteOwner;
            voteDict.Remove(voter);

            if (_changeChangeResultTiming == ChangeResultTiming.OnVote)
                CheckResult();
        }

        public async UniTask AddForSeconds(MonoBehaviour m, float seconds)
        {
            Vote(m, true);
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.DeltaTime);
            Vote(m, false);
        }

        private List<Object> _toRemove = new();

        private void CheckResult()
        {
            var newResult = GetDefaultValue();

            //clear null key

            _toRemove.Clear();
            var iterator = voteDict.GFIterator();
            while (iterator.MoveNext())
            {
                if (iterator.Current.Key == null)
                {
                    _toRemove.Add(iterator.Current.Key);

                    // Debug.LogError("null key !!????: 後面有被destroy的東西嗎？" + iterator.Current.Value.Voter);
                }
            }

            foreach (var key in _toRemove)
            {
                voteDict.Remove(key);
            }

            if (GetConditionType() == ConditionType.AND)
            {
                if (voteDict.Count != 0)
                    newResult = true;
                iterator = voteDict.GFIterator();
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.Vote == false)
                    {
                        newResult = false;
                        break;
                    }
                }

                // foreach (var vote in votes.Values)
                // {
                //     if (vote == false)
                //     {
                //         newResult = false;
                //     }
                // }
            }
            else if (GetConditionType() == ConditionType.OR)
            {
                iterator = voteDict.GFIterator();
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.Vote != true)
                        continue;
                    newResult = true;
                    break;
                }
            }

            if (_currentResult != newResult)
            {
                _currentResult = newResult;
                OnValueChange(newResult);
            }
        }

        private bool _currentResult = false;

        // public bool VoteResult => _currentResult;
        public bool Result => _currentResult;
    }
}
