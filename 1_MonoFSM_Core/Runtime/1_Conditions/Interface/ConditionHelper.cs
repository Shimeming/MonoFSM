using UnityEngine;

public static class ConditionHelper
{
    /// <summary>
    /// 每個frame跑condition會很貴嗎？可以cache?
    /// </summary>
    /// <param name="conditions"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public static bool IsAllValid(this AbstractConditionBehaviour[] conditions, Object owner = null)
    {
        if (conditions == null || conditions.Length == 0)
            return true;
        foreach (var condition in conditions)
        {
            if (condition == null)
                continue;
            if (condition == owner)
            {
                Debug.LogError(
                    "[ConditionHelper] Condition cannot reference itself!, will stackoverflow",
                    condition
                );
                continue;
            }
            if (condition.gameObject.activeSelf == false) //只看自己，可能是parent有人關
                continue;
            if (condition.FinalResult == false)
                return false;
            // Debug.Log($"[ConditionHelper] {condition.name} is valid", condition.gameObject);
        }

        return true;
    }

    public static bool IsAnyValid(this AbstractConditionBehaviour[] conditions, Object owner = null)
    {
        if (conditions == null || conditions.Length == 0)
            return true;

        foreach (var condition in conditions)
        {
            if (condition == null)
                continue;
            if (condition == owner)
            {
                Debug.LogError(
                    "[ConditionHelper] Condition cannot reference itself!, will stackoverflow",
                    condition
                );
                continue;
            }
            if (!condition.gameObject.activeSelf) //只看自己，可能是parent有人關
                continue;
            if (condition.FinalResult)
                return true;
            // Debug.Log($"[ConditionHelper] {condition.name} is valid", condition.gameObject);
        }

        return false;
    }
}
