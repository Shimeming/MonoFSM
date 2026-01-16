using MonoFSM.EditorExtension;

public interface IIntProvider
{
    public int IntValue { get; }
}

public class VarInt : AbstractFieldVariable<GameFlagInt, FlagFieldInt, int>, IIntProvider,
    IStringTokenVar, IHierarchyValueInfo
{
    public int IntValue => CurrentValue;

    // public override GameFlagBase FinalData => BindData;
    public override bool IsValueExist => CurrentValue != 0;
    public string ValueInfo => IntValue.ToString();
    public bool IsDrawingValueInfo => true;
}
