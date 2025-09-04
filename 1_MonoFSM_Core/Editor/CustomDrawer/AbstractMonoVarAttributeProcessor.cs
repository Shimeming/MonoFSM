using System;
using System.Collections.Generic;
using System.Reflection;
using MonoFSM.Variable;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace _1_MonoFSM_Core.Editor.CustomDrawer
{
    public class AbstractMonoVarAttributeProcessor : OdinAttributeProcessor<AbstractMonoVariable>
    {
        public override void ProcessSelfAttributes(
            InspectorProperty property,
            List<Attribute> attributes
        )
        {
            //TODO: 要整？
            var memberInfo = property.Info.GetMemberInfo();
            if (memberInfo is PropertyInfo)
                return;

            // 如果是字段，檢查是否為public或有SerializeField特性
            if (memberInfo is FieldInfo fieldInfo)
            {
                var hasSerializeField = property.Info.GetAttribute<SerializeField>() != null;
                if (!fieldInfo.IsPublic && !hasSerializeField)
                    return;

                if (property.Info.GetAttribute<DropDownRefAttribute>() != null)
                    return;
            }

            attributes.Add(new DropDownRefAttribute());
            // attributes.Add(new SOTypeDropdownAttribute());
        }
    }
}
