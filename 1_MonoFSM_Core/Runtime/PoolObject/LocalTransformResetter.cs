using MonoFSMCore.Runtime.LifeCycle;
using UnityEngine;

//第一次記住
//看到dynamic rigidbody就應該要有這個
//要寫一堆restore系列？
public class LocalTransformResetter : MonoBehaviour, IResetStateRestore
{
    private Vector3 initPosition;
    private Quaternion initRotation;
    private Transform initParent;
    private Vector3 initlocalScale;
    private bool isResetParametterInit = false;

    [AutoParent] public Rigidbody _rigidbody;
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
        if (isResetParametterInit)
            return true;

        initParent = transform.parent;
        initPosition = transform.localPosition;
        initRotation = transform.localRotation;
        initlocalScale = transform.localScale;

        isResetParametterInit = true;

        return false;
    }

    public void ResetStateRestore()
    {
        if (ParameterInitCheck()) //第一次記下來？還是分開感覺比較好？
        {
            transform.SetParent(initParent);
            transform.localPosition = initPosition;
            transform.localRotation = initRotation;
            transform.localScale = initlocalScale;
        }

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
}
