namespace MonoFSM.Variable.Condition
{
    //FIXME: 並沒有註冊唷
    public class IsUnityObjectVariableNullCondition : AbstractConditionBehaviour
    {
        [DropDownRef]
        public AbstractMonoVariable unityObjectVariable;

        //FIXME: Variable Tag？
        protected override bool IsValid => unityObjectVariable.objectValue == null;
    }
}
