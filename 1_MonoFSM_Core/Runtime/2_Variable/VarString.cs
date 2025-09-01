public class VarString : AbstractFieldVariable<GameFlagString, FlagFieldString, string>
{
    public string StringValue => CurrentValue;

    // public override GameFlagBase FinalData => BindData;
    public override bool IsValueExist => !string.IsNullOrEmpty(CurrentValue);
}
