namespace _1_MonoFSM_Core.Runtime.FSMCore.Core.StateBehaviour
{
    public interface IRenderBehaiour
    {
        public void OnEnterRender();
        public void OnRender();
        bool isActiveAndEnabled { get; }
    }
}
