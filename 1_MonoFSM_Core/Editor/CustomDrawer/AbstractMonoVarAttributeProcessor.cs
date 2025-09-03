using System;
using System.Collections.Generic;
using MonoFSM.Variable;
using Sirenix.OdinInspector.Editor;

namespace _1_MonoFSM_Core.Editor.CustomDrawer
{
    public class AbstractMonoVarAttributeProcessor : OdinAttributeProcessor<AbstractMonoVariable>
    {
        public override void ProcessSelfAttributes(
            InspectorProperty property,
            List<Attribute> attributes
        )
        {
            // var propertyType = property.ValueEntry.TypeOfValue;
            if (attributes.Exists(x => x is DropDownRefAttribute))
                return;
            attributes.Add(new DropDownRefAttribute());
            // attributes.Add(new SOTypeDropdownAttribute());
        }
    }
}
