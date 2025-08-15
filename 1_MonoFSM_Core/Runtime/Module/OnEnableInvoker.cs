using MonoFSM.Core.Module;
using MonoFSM.Core.Simulate;
using MonoFSM.Variable.Attributes;
using UnityEngine;

// internal interface IUnityEventHolder
// {
//     void PrepareUnityEvent();
// }
//
// [Serializable]
// public class TransformEvent : UnityEvent<Transform>
// {
// }


public class OnEnableInvoker : MonoBehaviour, IUpdateSimulate
{
    [CompRef] [AutoChildren] private OnEnableNode _onEnableNode;
    [CompRef] [AutoChildren] private OnDisableNode _onDisableNode;

    private bool _isCachedEnabled;
    private bool _isCachedDisabled;
    private void OnEnable()
    {
        this.Log("OnEnable");
        _isCachedEnabled = true;
    }

    private void OnDisable()
    {
        _isCachedDisabled = true;
        this.Log("OnDisable");

    }

    public void Simulate(float deltaTime)
    {
        //FIXME: 應該要下個frame做？ 先記下來？
        if (_isCachedEnabled)
        {
            _isCachedEnabled = false;
            if (_onEnableNode.gameObject.activeSelf)
                _onEnableNode.EventHandle();
            else
                Debug.LogError("OnEnableNode is not active", this);
        }

        if (_isCachedDisabled && _onDisableNode != null)
        {
            _isCachedDisabled = false;
            if (_onDisableNode.gameObject.activeSelf)
                _onDisableNode.EventHandle();
            else
                Debug.LogError("OnDisableNode is not active", this);
        }
    }

    public void AfterUpdate()
    {
    }
}
