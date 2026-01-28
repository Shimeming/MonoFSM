namespace MonoFSM.Core.Module
{
    public class AnimationEnableHandle : EnableHandle, IBeforePrefabSaveCallbackReceiver
    {
        //用type來自動match? hard reference?
        //TODO: 要存檔時自動 disable 嗎？
        public void OnBeforePrefabSave()
        {
            //disable 掉？
        }
    }
}
