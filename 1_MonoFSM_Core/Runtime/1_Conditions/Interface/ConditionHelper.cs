public static class ConditionHelper
{
    public static bool IsAllValid(this AbstractConditionBehaviour[] conditions)
    {
        if (conditions == null || conditions.Length == 0)
            return true;
        foreach (var condition in conditions)
        {
            if (condition == null)
                continue;
            if (condition.gameObject.activeSelf == false) //只看自己，可能是parent有人關
                continue;
            if (condition.FinalResult == false)
                return false;
            // Debug.Log($"[ConditionHelper] {condition.name} is valid", condition.gameObject);
        }

        return true;
    }

    public static bool IsAnyValid(this AbstractConditionBehaviour[] conditions)
    {
        if (conditions == null || conditions.Length == 0)
            return true;

        foreach (var condition in conditions)
        {
            if (condition == null)
                continue;
            if (!condition.gameObject.activeSelf) //只看自己，可能是parent有人關
                continue;
            if (condition.FinalResult)
                return true;
            // Debug.Log($"[ConditionHelper] {condition.name} is valid", condition.gameObject);
        }

        return false;
    }
}
