using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MonoFSM.Editor
{
    public static class FSMFolderCopyTool
    {
        [Serializable]
        public class FSMFolderData
        {
            public string folderPath;
            public string folderName;
            public List<string> prefabPaths = new List<string>();
            public List<string> animatorPaths = new List<string>();
            public List<string> animationPaths = new List<string>();
            public List<string> otherAssetPaths = new List<string>();

            public bool IsValidFSMFolder => prefabPaths.Count > 0 && animatorPaths.Count > 0;
        }

        [Serializable]
        public class CopyOptions
        {
            public enum PrefabCopyMode
            {
                DirectCopy,    // 直接複製
                CreateVariant  // 建立Variant
            }

            public enum AnimatorCopyMode
            {
                DirectCopy,           // 直接複製
                CreateOverrideController // 建立Override Controller
            }

            public PrefabCopyMode prefabMode = PrefabCopyMode.CreateVariant;
            public AnimatorCopyMode animatorMode = AnimatorCopyMode.CreateOverrideController;
            public string targetFolderPath = "";
            public string newFolderBaseName = "";
        }

        public static FSMFolderData AnalyzeFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                return null;

            var data = new FSMFolderData
            {
                folderPath = folderPath,
                folderName = Path.GetFileName(folderPath)
            };

            var allAssets = AssetDatabase.FindAssets("", new[] { folderPath });

            foreach (var guid in allAssets)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var extension = Path.GetExtension(assetPath).ToLower();

                switch (extension)
                {
                    case ".prefab":
                        data.prefabPaths.Add(assetPath);
                        break;
                    case ".controller":
                        data.animatorPaths.Add(assetPath);
                        break;
                    case ".anim":
                        data.animationPaths.Add(assetPath);
                        break;
                    default:
                        if (!string.IsNullOrEmpty(extension))
                            data.otherAssetPaths.Add(assetPath);
                        break;
                }
            }

            return data;
        }

        public static bool CopyFSMFolder(FSMFolderData sourceData, CopyOptions options)
        {
            if (sourceData == null || !sourceData.IsValidFSMFolder)
            {
                Debug.LogError("無效的FSM資料夾");
                return false;
            }

            if (string.IsNullOrEmpty(options.targetFolderPath))
            {
                Debug.LogError("目標資料夾路徑不能為空");
                return false;
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                // 建立目標資料夾
                var newFolderName = GetNewFolderName(sourceData.folderName, options);
                var targetFolder = options.targetFolderPath + "/" + newFolderName;

                if (AssetDatabase.IsValidFolder(targetFolder))
                {
                    Debug.LogError($"資料夾已存在: {targetFolder}");
                    AssetDatabase.StopAssetEditing();
                    return false;
                }

                CreateFolderRecursive(targetFolder);

                // 複製資產的映射表
                var assetMapping = new Dictionary<string, string>();

                // 階段1: 複製所有基礎資產（不更新引用）

                // 複製動畫檔案（總是複製）
                foreach (var animPath in sourceData.animationPaths)
                {
                    var newAnimPath = CopyAnimationClip(animPath, targetFolder, options, sourceData.folderName);
                    if (!string.IsNullOrEmpty(newAnimPath))
                        assetMapping[animPath] = newAnimPath;
                }

                // 複製其他資產
                foreach (var otherPath in sourceData.otherAssetPaths)
                {
                    var newOtherPath = CopyOtherAsset(otherPath, targetFolder, options);
                    if (!string.IsNullOrEmpty(newOtherPath))
                        assetMapping[otherPath] = newOtherPath;
                }

                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 階段2: 複製Animator Controller（不立即更新引用）
                if (sourceData.animatorPaths.Count > 0)
                {
                    var newAnimatorPath = CopyAnimatorControllerRaw(sourceData.animatorPaths[0], targetFolder, options, sourceData.folderName);
                    if (!string.IsNullOrEmpty(newAnimatorPath))
                        assetMapping[sourceData.animatorPaths[0]] = newAnimatorPath;
                }

                // 階段3: 複製Prefab（不立即更新引用）
                if (sourceData.prefabPaths.Count > 0)
                {
                    var newPrefabPath = CopyPrefabRaw(sourceData.prefabPaths[0], targetFolder, options, sourceData.folderName);
                    if (!string.IsNullOrEmpty(newPrefabPath))
                        assetMapping[sourceData.prefabPaths[0]] = newPrefabPath;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 階段4: 更新所有引用關係
                UpdateAllReferences(assetMapping, options);

                Debug.Log($"FSM資料夾複製成功: {targetFolder}");
                Debug.Log($"資產映射: {string.Join(", ", assetMapping.Select(kvp => $"{Path.GetFileName(kvp.Key)} -> {Path.GetFileName(kvp.Value)}"))}");

                return true;
            }
            catch (Exception ex)
            {
                AssetDatabase.StopAssetEditing();
                Debug.LogError($"複製FSM資料夾時發生錯誤: {ex.Message}");
                return false;
            }
        }

        private static string GetNewFolderName(string originalName, CopyOptions options)
        {
            // 如果有指定新的基礎名稱，直接使用
            if (!string.IsNullOrEmpty(options.newFolderBaseName))
            {
                return options.newFolderBaseName;
            }

            // 否則保持原名
            return originalName;
        }

        private static void CreateFolderRecursive(string folderPath)
        {
            var folders = folderPath.Split('/');
            var currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                var nextPath = currentPath + "/" + folders[i];

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }

                currentPath = nextPath;
            }
        }

        private static string CopyAnimationClip(string sourcePath, string targetFolder, CopyOptions options, string originalFolderName)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var newFileName = GetNewAssetName(fileName, options, originalFolderName);
            var newPath = targetFolder + "/" + newFileName + ".anim";

            if (AssetDatabase.CopyAsset(sourcePath, newPath))
            {
                return newPath;
            }

            return null;
        }

        private static string CopyAnimatorControllerRaw(string sourcePath, string targetFolder, CopyOptions options, string originalFolderName)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var newFileName = GetNewAssetName(fileName, options, originalFolderName);

            if (options.animatorMode == CopyOptions.AnimatorCopyMode.CreateOverrideController)
            {
                // 建立 Animator Override Controller
                var newPath = targetFolder + "/" + newFileName + " Override.overrideController";
                var sourceController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(sourcePath);

                if (sourceController != null)
                {
                    var overrideController = new AnimatorOverrideController(sourceController);
                    AssetDatabase.CreateAsset(overrideController, newPath);
                    return newPath;
                }
            }
            else
            {
                // 直接複製 Animator Controller
                var newPath = targetFolder + "/" + newFileName + ".controller";
                if (AssetDatabase.CopyAsset(sourcePath, newPath))
                {
                    return newPath;
                }
            }

            return null;
        }

        private static string CopyPrefabRaw(string sourcePath, string targetFolder, CopyOptions options, string originalFolderName)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var newFileName = GetNewAssetName(fileName, options, originalFolderName);
            var newPath = targetFolder + "/" + newFileName + ".prefab";

            if (options.prefabMode == CopyOptions.PrefabCopyMode.CreateVariant)
            {
                // 建立 Prefab Variant
                var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (sourcePrefab != null)
                {
                    var variant = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
                    var variantPrefab = PrefabUtility.SaveAsPrefabAsset(variant, newPath);
                    UnityEngine.Object.DestroyImmediate(variant);
                    return newPath;
                }
            }
            else
            {
                // 直接複製 Prefab
                if (AssetDatabase.CopyAsset(sourcePath, newPath))
                {
                    return newPath;
                }
            }

            return null;
        }

        private static void UpdateAllReferences(Dictionary<string, string> assetMapping, CopyOptions options)
        {
            AssetDatabase.StartAssetEditing();

            try
            {
                // 更新Animator Controllers中的動畫引用
                foreach (var mapping in assetMapping)
                {
                    if (mapping.Key.EndsWith(".controller") || mapping.Value.EndsWith(".overrideController"))
                    {
                        if (options.animatorMode == CopyOptions.AnimatorCopyMode.CreateOverrideController)
                        {
                            UpdateOverrideControllerReferences(mapping.Value, assetMapping);
                        }
                        else
                        {
                            UpdateAnimatorControllerReferences(mapping.Value, assetMapping);
                        }
                    }
                }

                // 更新Prefab中的Animator Controller引用
                foreach (var mapping in assetMapping)
                {
                    if (mapping.Key.EndsWith(".prefab"))
                    {
                        UpdatePrefabReferences(mapping.Value, assetMapping);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }


        private static void UpdateOverrideControllerReferences(string overrideControllerPath, Dictionary<string, string> assetMapping)
        {
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overrideControllerPath);
            if (overrideController == null)
            {
                Debug.LogError($"無法載入Override Controller: {overrideControllerPath}");
                return;
            }

            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            var hasChanges = false;

            for (int i = 0; i < overrides.Count; i++)
            {
                var originalClip = overrides[i].Key;
                if (originalClip != null)
                {
                    // 查找對應的新動畫檔案
                    var originalClipPath = AssetDatabase.GetAssetPath(originalClip);
                    if (assetMapping.ContainsKey(originalClipPath))
                    {
                        var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetMapping[originalClipPath]);
                        if (newClip != null)
                        {
                            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(originalClip, newClip);
                            hasChanges = true;
                            Debug.Log($"Override Controller更新動畫: {originalClip.name} -> {newClip.name}");
                        }
                    }
                }
            }

            if (hasChanges)
            {
                overrideController.ApplyOverrides(overrides);
                EditorUtility.SetDirty(overrideController);
                Debug.Log($"成功更新Override Controller引用: {overrideControllerPath}");
            }
        }

        private static void UpdateAnimatorControllerReferences(string controllerPath, Dictionary<string, string> assetMapping)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogError($"無法載入Animator Controller: {controllerPath}");
                return;
            }

            var hasChanges = false;

            // 更新所有層級的狀態機
            foreach (var layer in controller.layers)
            {
                if (UpdateStateMachineReferences(layer.stateMachine, assetMapping))
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                EditorUtility.SetDirty(controller);
                Debug.Log($"成功更新Animator Controller引用: {controllerPath}");
            }
        }

        private static bool UpdateStateMachineReferences(AnimatorStateMachine stateMachine, Dictionary<string, string> assetMapping)
        {
            var hasChanges = false;

            // 更新狀態中的動畫引用
            foreach (var state in stateMachine.states)
            {
                var motion = state.state.motion;
                if (motion != null && motion is AnimationClip)
                {
                    var currentPath = AssetDatabase.GetAssetPath(motion);
                    if (assetMapping.ContainsKey(currentPath))
                    {
                        var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetMapping[currentPath]);
                        if (newClip != null)
                        {
                            state.state.motion = newClip;
                            hasChanges = true;
                            Debug.Log($"Animator Controller更新動畫: {motion.name} -> {newClip.name}");
                        }
                    }
                }
            }

            // 遞歸處理子狀態機
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                if (UpdateStateMachineReferences(childStateMachine.stateMachine, assetMapping))
                {
                    hasChanges = true;
                }
            }

            return hasChanges;
        }


        private static void UpdatePrefabReferences(string prefabPath, Dictionary<string, string> assetMapping)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"無法載入Prefab: {prefabPath}");
                return;
            }

            var hasChanges = false;

            // 使用PrefabUtility來正確處理Prefab修改
            var tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            try
            {
                // 更新所有Animator組件的Controller引用
                var animators = tempInstance.GetComponentsInChildren<Animator>(true);
                foreach (var animator in animators)
                {
                    if (animator.runtimeAnimatorController != null)
                    {
                        var currentControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                        if (assetMapping.ContainsKey(currentControllerPath))
                        {
                            var newController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetMapping[currentControllerPath]);
                            if (newController != null)
                            {
                                animator.runtimeAnimatorController = newController;
                                hasChanges = true;
                                Debug.Log($"Prefab更新Animator Controller: {Path.GetFileName(currentControllerPath)} -> {Path.GetFileName(assetMapping[currentControllerPath])}");
                            }
                        }
                    }
                }

                // 如果有變更，保存Prefab
                if (hasChanges)
                {
                    PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
                    Debug.Log($"成功更新Prefab引用: {prefabPath}");
                }
            }
            finally
            {
                // 清理臨時實例
                UnityEngine.Object.DestroyImmediate(tempInstance);
            }
        }

        private static string CopyOtherAsset(string sourcePath, string targetFolder, CopyOptions options)
        {
            var fileName = Path.GetFileName(sourcePath);
            var newPath = targetFolder + "/" + fileName;

            if (AssetDatabase.CopyAsset(sourcePath, newPath))
            {
                return newPath;
            }

            return null;
        }

        private static string GetNewAssetName(string originalName, CopyOptions options, string originalFolderName = "")
        {
            // 如果沒有指定新基礎名稱，保持原名
            if (string.IsNullOrEmpty(options.newFolderBaseName))
                return originalName;

            var newName = originalName;

            // 智能替換：檢查原始檔名是否包含原資料夾名稱
            if (!string.IsNullOrEmpty(originalFolderName))
            {
                var index = newName.IndexOf(originalFolderName, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var beforePart = newName.Substring(0, index);
                    var afterPart = newName.Substring(index + originalFolderName.Length);
                    newName = beforePart + options.newFolderBaseName + afterPart;
                    Debug.Log($"智能替換檔名: {originalName} -> {newName}");
                    return newName;
                }
            }

            // 如果沒有找到匹配項，在檔名前加上新基礎名稱
            newName = options.newFolderBaseName + " " + originalName;
            Debug.Log($"智能命名（添加前綴）: {originalName} -> {newName}");
            return newName;
        }
    }
}
