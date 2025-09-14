using System.Linq;
using MonoFSM.Core.Attributes;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Runtime.Interact.EffectHit
{
    [CreateAssetMenu(
        fileName = "GeneralEffectType",
        menuName = "RCGMaker/GeneralEffectType",
        order = 0
    )]
    public class GeneralEffectType : ScriptableObject, IEffectType, IStringKey
    {
#if UNITY_EDITOR
        [TextArea]
        [SerializeField]
        private string _note;

        [PreviewInInspector]
        GeneralEffectDealer[] _bindedDealers;

        [PreviewInInspector]
        GeneralEffectReceiver[] _bindedReceivers;

        [Button]
        private void GetBindingVariables()
        {
            _bindedDealers = FindObjectsByType<GeneralEffectDealer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(v => v._effectType == this)
                .ToArray();
            _bindedReceivers = FindObjectsByType<GeneralEffectReceiver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(v => v._effectType == this)
                .ToArray();
        }
#endif

        public string GetStringKey => name;
    }
}
