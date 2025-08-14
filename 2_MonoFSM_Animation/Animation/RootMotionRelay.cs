using MonoFSM.EditorExtension;
using MonoFSM.Variable.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

public class RootMotionRelay : MonoBehaviour, IOverrideHierarchyIcon, IDrawHierarchyBackGround, IHierarchyValueInfo
{
    [ReadOnly][CompRef][Required][Auto][ShowInInspector]
    private IRootMotionReceiver _rootMotionReceiver;

    [ReadOnly][Required][Auto][ShowInInspector]
    private Animator _animator = null;

    private void OnAnimatorMove()
    {
        _rootMotionReceiver.OnProcessRootMotion( _animator.deltaPosition,  _animator.deltaRotation);
    }

#if UNITY_EDITOR
    // IOverrideHierarchyIcon 實作
    public string IconName => "AnimatorController Icon";
    public bool IsDrawingIcon => true;
    public Texture2D CustomIcon => null;
    public bool IsPosAtHead => false; // 圖示在右邊

    // IDrawHierarchyBackGround 實作
    public Color BackgroundColor
    {
        get
        {
            if (_rootMotionReceiver == null)
                return new Color(0.8f, 0.2f, 0.2f, 0.15f); // 紅色 - 設定錯誤
            else
                return new Color(0.2f, 0.5f, 0.8f, 0.15f); // 淡藍色 - 正常
        }
    }
    public bool IsDrawGUIHierarchyBackground => true;

    // IHierarchyValueInfo 實作
    public string ValueInfo
    {
        get
        {
            if (_rootMotionReceiver == null)
            {
                return "⚠ No Receiver";
            }

            // 檢查接收端類型
            string receiverType = "";
            if (_rootMotionReceiver is TransformMotionReceiver)
                receiverType = "Transform";
            else if (_rootMotionReceiver is RigidbodyMotionReceiver)
                receiverType = "Rigidbody";
            else if (_rootMotionReceiver is Rigidbody2DMotionReceiver)
                receiverType = "Rigidbody2D";
            else
                receiverType = _rootMotionReceiver.GetType().Name;

            return $"Motion→{receiverType}";
        }
    }
    public bool IsDrawingValueInfo => true;
#endif
}
