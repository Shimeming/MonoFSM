using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVector2Flag", menuName = "GameFlag/Vector2", order = 1)]
public class GameDataVector2 : AbstractScriptableData<FlagFieldVector2, Vector2>
{
    public override Vector2 CurrentValue
    {
        get => base.CurrentValue;
        set
        {
            base.CurrentValue = value;
        }
    }
}
