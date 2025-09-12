using System;
using System.Collections.Generic;
using System.Reflection;
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
            if (property.IsTreeRoot)
                return;
            var backend = property.ValueEntry?.SerializationBackend ?? SerializationBackend.None;
            var isSerialized = backend != SerializationBackend.None; // Unity 或 Odin 都算序列化
            if (!isSerialized)
                return;

            var memberInfo = property.Info.GetMemberInfo();
            // if (memberInfo is PropertyInfo)
            //     return;

            // 如果是字段，檢查是否為public或有SerializeField特性
            if (memberInfo is FieldInfo fieldInfo)
            {
                // var hasSerializeField = property.Info.GetAttribute<SerializeField>() != null;
                // if (!fieldInfo.IsPublic && !hasSerializeField)
                //     return;

                if (property.Info.GetAttribute<SOConfigAttribute>() != null)
                    return;
            }

            var propertyType = property.ValueEntry.TypeOfValue;

            //useVarTagRestrictType??
            attributes.Add(new SOConfigAttribute(propertyType.Name));
            attributes.Add(new SOTypeDropdownAttribute()); //需要再加一次，includeMyAttribute不會自動處理
        }
    }
}
