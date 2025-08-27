using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVector3Flag", menuName = "GameFlag/Vector3", order = 1)]
public class GameDataVector3 : AbstractScriptableData<FlagFieldVector3, Vector3>
{
    public override Vector3 CurrentValue
    {
        get => base.CurrentValue;
        set { base.CurrentValue = value; }
    }
}
