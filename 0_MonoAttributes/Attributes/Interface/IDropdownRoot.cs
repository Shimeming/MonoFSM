using UnityEngine;

namespace MonoFSM.CustomAttributes
{
    public interface IDropdownRoot
    {
        public string name { get; }
        GameObject gameObject { get; }
    }
}
