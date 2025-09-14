using System;
using _1_MonoFSM_Core.Runtime.Attributes;
using Sirenix.OdinInspector;

namespace MonoFSM.Core.Attributes
{
    [EditorOnly]
    [IncludeMyAttributes]
    [SOTypeDropdown] //FIXME: Drawer應該要合併嗎？
    // [ShowDrawerChain]
    [ListDrawerSettings(ShowFoldout = false)]
    public class SOConfigAttribute : Attribute
    {
        public string SubFolderPath = "";
        public string PostProcessMethodName = "";
        public bool UseVarTagRestrictType = false;

        //FIXME: 空路徑，就放在原本同一個資料夾？
        public SOConfigAttribute(
            string subFolderPath,
            string PostProcessMethodName = "",
            bool useVarTagRestrictType = false
        )
        {
            SubFolderPath = subFolderPath;
            this.PostProcessMethodName = PostProcessMethodName;
            UseVarTagRestrictType = useVarTagRestrictType;
        }

        // public string GetPathFromOwnerObj(GameObject gObj, string configName)
        // {
        //     //命名
        //     var finalName = $"{gObj.name}";
        //     if (SubFolderPath == "")
        //         return $"{finalName}.asset";
        //     else
        //         return $"{SubFolderPath}/{finalName}.asset";
        // }
        //
        // public string GetFilePath(string configName)
        // {
        //     var finalName = $"{configName}";
        //     if (SubFolderPath == "")
        //         return $"{finalName}.asset";
        //     else
        //         return $"{SubFolderPath}/{finalName}.asset";
        // }
    }
}
