using MonoFSM.Runtime.Interact.EffectHit;
using UnityEngine;

namespace MonoFSM.Core.Detection
{
    public interface IDetectableTarget //需要嗎？好像要
    {
        GameObject TargetObject { get; }

        bool IsValidTarget { get; }
    }

    public interface IColliderProvider
    {
        Collider GetCollider();
    }
}
