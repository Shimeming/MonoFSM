using System;
using _1_MonoFSM_Core.Runtime.Attributes;
using Sirenix.OdinInspector;

namespace MonoFSM.Core.Attributes
{
    [EditorOnly]
    [IncludeMyAttributes]
    [TypeRestrictDropdown] //FIXME: 好像要可以過濾對應的型別, list失敗
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
