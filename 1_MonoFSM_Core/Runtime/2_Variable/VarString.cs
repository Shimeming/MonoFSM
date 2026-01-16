public class VarString : AbstractFieldVariable<GameFlagString, FlagFieldString, string>,
    IStringTokenVar
{
    // public override GameFlagBase FinalData => BindData;
    public override bool IsValueExist => !string.IsNullOrEmpty(CurrentValue);
    public string ValueInfo => CurrentValue;
}
