#if false
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using MonoFSM.Core.Attributes; // PreviewInInspectorAttribute
using MonoFSM.Variable.Attributes; // CompRefAttribute

// 暫停啟用：RequiredFieldLayoutProcessor（等待進一步評估與調整）
namespace MonoFSM.Core.Attributes.Editor
{
    public class RequiredFieldLayoutProcessor : OdinAttributeProcessor
    {
        private static readonly Type RequiredAttrType = typeof(RequiredAttribute);
        private static readonly Type DropDownRefAttrType = typeof(DropDownRefAttribute);
        private static readonly Type ComponentAttrType = typeof(ComponentAttribute);

        private const string RequiredGroupName = "必要設定";
        private const string OptionalGroupName = "選填設定";
        private const string PreviewGroupName = "狀態預覽";
        private const string OthersGroupName = "其他";

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (member is not FieldInfo fieldInfo)
                return;

            bool hasComponentButton = member.GetAttribute<ComponentAttribute>() != null ||
                                       member.GetAttribute<CompRefAttribute>() != null;
            bool isRequired = member.GetAttribute<RequiredAttribute>() != null || member.GetAttribute<DropDownRefAttribute>() != null;

            if (isRequired)
            {
                var order = hasComponentButton ? -10050 : -10000;
                attributes.Add(new PropertyOrderAttribute(order));
                attributes.Add(new FoldoutGroupAttribute(RequiredGroupName, expanded: true));
                return;
            }

            if (hasComponentButton)
            {
                attributes.Add(new PropertyOrderAttribute(-5000));
                attributes.Add(new FoldoutGroupAttribute(OptionalGroupName, expanded: false));
                return;
            }

            bool isPreview = member.GetAttribute<PreviewInInspectorAttribute>() != null;
            if (isPreview)
            {
                attributes.Add(new PropertyOrderAttribute(-4000));
                attributes.Add(new FoldoutGroupAttribute(PreviewGroupName, expanded: false));
                return;
            }

            attributes.Add(new PropertyOrderAttribute(-3000));
            attributes.Add(new FoldoutGroupAttribute(OthersGroupName, expanded: false));
        }
    }
}
#endif
