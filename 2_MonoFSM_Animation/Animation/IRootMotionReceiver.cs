using UnityEngine;

//FIXME: 改用AbstractClass吧
public interface IRootMotionReceiver
{
    public void OnProcessRootMotion(Vector3 deltaPosition, Quaternion deltaRotation);
}
