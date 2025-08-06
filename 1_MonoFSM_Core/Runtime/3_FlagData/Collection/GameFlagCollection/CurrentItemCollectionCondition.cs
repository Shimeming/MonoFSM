using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CurrentItemCollectionCondition : AbstractConditionBehaviour
{
    public AbstractGameFlagCollection collection;
    //FIXME: 用dropdown選

    private IEnumerable<GameData> GetCollection()
    {
        return collection.rawCollection;
    }

    [ValueDropdown("GetCollection", IsUniqueList = true)]
    public GameData targetItem;


    protected override bool IsValid => collection.currentItem == targetItem;
}