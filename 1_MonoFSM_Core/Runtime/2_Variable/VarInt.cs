public interface IIntProvider
{
    public int IntValue { get; }
}

public class VarInt : AbstractFieldVariable<GameFlagInt, FlagFieldInt, int>, IIntProvider
{
    public int IntValue => CurrentValue;

    // public override GameFlagBase FinalData => BindData;
    public override bool IsValueExist => CurrentValue != 0;
}
