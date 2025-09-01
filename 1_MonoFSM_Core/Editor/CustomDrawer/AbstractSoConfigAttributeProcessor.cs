using System;
using System.Collections.Generic;
using _1_MonoFSM_Core.Runtime._3_FlagData;
using _1_MonoFSM_Core.Runtime.Attributes;
using JetBrains.Annotations;
using MonoFSM.Core.Attributes;
using Sirenix.OdinInspector.Editor;

namespace _1_MonoFSM_Core.Editor.CustomDrawer
{
    /// <summary>
    ///     自動幫 AbstractSOConfig 加上 SOConfigAttribute 和 SOTypeDropdownAttribute
    /// </summary>
    [UsedImplicitly]
    public class AbstractSoConfigAttributeProcessor : OdinAttributeProcessor<AbstractSOConfig>
    {
        public override void ProcessSelfAttributes(
            InspectorProperty property,
            List<Attribute> attributes
        )
        {
            var propertyType = property.ValueEntry.TypeOfValue;
            if (attributes.Exists(x => x is SOConfigAttribute))
                return;
            attributes.Add(new SOConfigAttribute(propertyType.Name));
            attributes.Add(new SOTypeDropdownAttribute());
        }
    }
}
