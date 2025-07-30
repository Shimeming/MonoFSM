using MonoFSM.Core.Detection;
using MonoFSM.Runtime.Interact.EffectHit;
using UnityEngine;

public interface IHitDataProvider
{
   public IEffectHitData GetHitData();
   public GeneralEffectHitData GetGeneralHitData();
}

public interface ICollisionDataProvider
{
   public Collision GetCollision();
}

public interface IDetectDataProvider
{
   public DetectData? GetDetectData();
}