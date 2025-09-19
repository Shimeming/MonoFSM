using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

//第一次記住
//看到dynamic rigidbody就應該要有這個
//要寫一堆restore系列？
//FIXME: 放在這，還是應該放在init state
public class LocalTransformResetter : MonoBehaviour, IResetStateRestore
{
    private Vector3 _initPosition;
    private Quaternion _initRotation;
    private Transform _initParent;
    private Vector3 _initLocalScale;
    private bool _isResetParameterInit;

    private bool _isKinematic;

    //FIXME: 要拆開嗎？
    [AutoParent]
    public Rigidbody _rigidbody;

    // [AutoChildren(false)] private Rigidbody2D rigidbody2D;

    private void Awake()
    {
        // if (rigidbody2D != null && rigidbody2D.mass > 1)
        // {
        //     Debug.LogError("rigidbody2D.mass > 1, 怪怪mass? 檢查一下是scene還是prefab?", this);
        // }
    }

    private bool ParameterInitCheck()
    {
        if (_isResetParameterInit)
            return true;

        InitSaveSnapshot();
        _isResetParameterInit = true;
        return false;
    }

    private void InitSaveSnapshot()
    {
        _initParent = transform.parent;
        _initPosition = transform.localPosition;
        _initRotation = transform.localRotation;
        _initLocalScale = transform.localScale;

        //--
        if (_rigidbody)
            _isKinematic = _rigidbody.isKinematic;
    }

    public void ResetStateRestore()
    {
        if (ParameterInitCheck()) //第一次記下來？還是分開感覺比較好？
        {
            transform.SetParent(_initParent);
            transform.localPosition = _initPosition;
            transform.localRotation = _initRotation;
            transform.localScale = _initLocalScale;
        }

        if (_rigidbody)
        {
            _rigidbody.isKinematic = _isKinematic;
            if (!_isKinematic)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            _rigidbody.ResetInertiaTensor();
        }
    }
}
