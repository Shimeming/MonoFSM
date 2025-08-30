using System.Collections.Generic;
using MonoFSM.Core.DataProvider;
using MonoFSM.Foundation;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Interact.EffectHit;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.EffectHit.ValueGetter
{
    public class ListEntityFromEffectDealer
        : AbstractDescriptionBehaviour,
            IValueProvider<List<MonoEntity>>
    {
        [SerializeField]
        [DropDownRef]
        private GeneralEffectDealer _effectDealer;

        public override string Description =>
            _effectDealer != null
                ? $"Get hitting entities from {_effectDealer.name}"
                : "No EffectDealer";

        protected override string DescriptionTag => "GetList";
        public List<MonoEntity> Value => _effectDealer.GetHittingEntities();
    }
}
