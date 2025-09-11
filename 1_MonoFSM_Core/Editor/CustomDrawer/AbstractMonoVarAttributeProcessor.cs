using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MonoFSM.Variable;
using Sirenix.OdinInspector.Editor;

namespace _1_MonoFSM_Core.Editor.CustomDrawer
{
    //FIXME: 一定需要Required?
    /// <summary>
    ///     自動幫 AbstractMonoVariable 加上 DropDownRefAttribute 和 ShowDrawerChainAttribute
    /// </summary>
    [UsedImplicitly]
    public class AbstractMonoVarAttributeProcessor : OdinAttributeProcessor<AbstractMonoVariable>
    {
        public override void ProcessSelfAttributes(
            InspectorProperty property,
            List<Attribute> attributes
        )
        {
            if (property.IsTreeRoot) //ex: VarEntity Component自身也會被處理，略過非property的
                return;
            var backend = property.ValueEntry?.SerializationBackend ?? SerializationBackend.None;
            var isSerialized = backend != SerializationBackend.None; // Unity 或 Odin 都算序列化
            if (!isSerialized)
                return;

            //TODO: 要整？
            var memberInfo = property.Info.GetMemberInfo();
            // if (memberInfo is PropertyInfo)
            //     return;

            // 如果是字段，檢查是否為public或有SerializeField特性
            if (memberInfo is FieldInfo fieldInfo)
            {
                // var hasSerializeField = property.Info.GetAttribute<SerializeField>() != null;
                // if (!fieldInfo.IsPublic && !hasSerializeField)
                //     return;

                if (property.Info.GetAttribute<DropDownRefAttribute>() != null)
                    return;
            }

            attributes.Add(new DropDownRefAttribute());
            // attributes.Add(new ShowDrawerChainAttribute());
            // attributes.Add(new SOTypeDropdownAttribute());
        }
    }
}
