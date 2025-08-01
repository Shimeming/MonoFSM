using UnityEngine;
using UnityEditor;
using System.IO;

namespace MonoFSM.Editor
{
    public static class FSMFolderCopyTest
    {
        [MenuItem("Tools/MonoFSM/測試FSM資料夾複製")]
        public static void TestFSMFolderCopy()
        {
            var testSourcePath = "Assets/FSMs/Puzzles/Door";
            
            if (!AssetDatabase.IsValidFolder(testSourcePath))
            {
                Debug.LogError($"測試來源資料夾不存在: {testSourcePath}");
                return;
            }
            
            // 分析資料夾
            var folderData = FSMFolderCopyTool.AnalyzeFolder(testSourcePath);
            
            if (folderData == null)
            {
                Debug.LogError("無法分析資料夾");
                return;
            }
            
            Debug.Log($"=== FSM資料夾分析結果 ===");
            Debug.Log($"資料夾: {folderData.folderName}");
            Debug.Log($"路徑: {folderData.folderPath}");
            Debug.Log($"是否為有效FSM資料夾: {folderData.IsValidFSMFolder}");
            Debug.Log($"Prefab檔案數: {folderData.prefabPaths.Count}");
            Debug.Log($"Animator Controller數: {folderData.animatorPaths.Count}");
            Debug.Log($"動畫檔案數: {folderData.animationPaths.Count}");
            Debug.Log($"其他資產數: {folderData.otherAssetPaths.Count}");
            
            if (!folderData.IsValidFSMFolder)
            {
                Debug.LogWarning("這不是一個有效的FSM資料夾，停止測試");
                return;
            }
            
            // 測試Variant模式複製
            TestVariantCopy(folderData);
            
            // 測試直接複製模式
            TestDirectCopy(folderData);
        }
        
        private static void TestVariantCopy(FSMFolderCopyTool.FSMFolderData sourceData)
        {
            Debug.Log("\n=== 測試智能命名複製（Variant模式） ===");
            
            var copyOptions = new FSMFolderCopyTool.CopyOptions
            {
                prefabMode = FSMFolderCopyTool.CopyOptions.PrefabCopyMode.CreateVariant,
                animatorMode = FSMFolderCopyTool.CopyOptions.AnimatorCopyMode.CreateOverrideController,
                targetFolderPath = "Assets/FSMs/Puzzles",
                newFolderBaseName = "TestGate"
            };
            
            var success = FSMFolderCopyTool.CopyFSMFolder(sourceData, copyOptions);
            
            if (success)
            {
                Debug.Log("✓ 智能命名複製（Variant）成功");
                VerifyCopyResult("Assets/FSMs/Puzzles/TestGate", copyOptions);
            }
            else
            {
                Debug.LogError("✗ 智能命名複製（Variant）失敗");
            }
        }
        
        private static void TestDirectCopy(FSMFolderCopyTool.FSMFolderData sourceData)
        {
            Debug.Log("\n=== 測試智能命名複製（直接複製模式） ===");
            
            var copyOptions = new FSMFolderCopyTool.CopyOptions
            {
                prefabMode = FSMFolderCopyTool.CopyOptions.PrefabCopyMode.DirectCopy,
                animatorMode = FSMFolderCopyTool.CopyOptions.AnimatorCopyMode.DirectCopy,
                targetFolderPath = "Assets/FSMs/Puzzles",
                newFolderBaseName = "TestWindow"
            };
            
            var success = FSMFolderCopyTool.CopyFSMFolder(sourceData, copyOptions);
            
            if (success)
            {
                Debug.Log("✓ 智能命名複製（直接複製）成功");
                VerifyCopyResult("Assets/FSMs/Puzzles/TestWindow", copyOptions);
            }
            else
            {
                Debug.LogError("✗ 智能命名複製（直接複製）失敗");
            }
        }
        
        private static void VerifyCopyResult(string targetFolderPath, FSMFolderCopyTool.CopyOptions options)
        {
            Debug.Log($"\n--- 驗證複製結果: {targetFolderPath} ---");
            
            if (!AssetDatabase.IsValidFolder(targetFolderPath))
            {
                Debug.LogError("目標資料夾不存在");
                return;
            }
            
            // 檢查Prefab
            var prefabs = AssetDatabase.FindAssets("t:GameObject", new[] { targetFolderPath });
            foreach (var prefabGuid in prefabs)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                var animator = prefab.GetComponent<Animator>();
                if (animator != null)
                {
                    if (animator.runtimeAnimatorController != null)
                    {
                        var controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                        Debug.Log($"✓ Prefab [{Path.GetFileName(prefabPath)}] 的Animator Controller: {Path.GetFileName(controllerPath)}");
                        
                        // 檢查Controller是否在同一資料夾
                        if (controllerPath.Contains(targetFolderPath))
                        {
                            Debug.Log("  ✓ Controller引用正確（指向新複製的檔案）");
                        }
                        else
                        {
                            Debug.LogWarning("  ⚠️ Controller引用可能不正確（指向原始檔案）");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Prefab [{Path.GetFileName(prefabPath)}] 的Animator沒有Controller");
                    }
                }
            }
            
            // 檢查Animator Controller
            var controllers = AssetDatabase.FindAssets("t:AnimatorController", new[] { targetFolderPath });
            var overrideControllers = AssetDatabase.FindAssets("t:AnimatorOverrideController", new[] { targetFolderPath });
            
            foreach (var controllerGuid in controllers)
            {
                var controllerPath = AssetDatabase.GUIDToAssetPath(controllerGuid);
                VerifyAnimatorController(controllerPath, targetFolderPath);
            }
            
            foreach (var overrideGuid in overrideControllers)
            {
                var overridePath = AssetDatabase.GUIDToAssetPath(overrideGuid);
                VerifyOverrideController(overridePath, targetFolderPath);
            }
        }
        
        private static void VerifyAnimatorController(string controllerPath, string targetFolderPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
            if (controller == null) return;
            
            Debug.Log($"檢查Animator Controller: {Path.GetFileName(controllerPath)}");
            
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    var motion = state.state.motion;
                    if (motion != null)
                    {
                        var motionPath = AssetDatabase.GetAssetPath(motion);
                        Debug.Log($"  狀態 [{state.state.name}] 動畫: {Path.GetFileName(motionPath)}");
                        
                        if (motionPath.Contains(targetFolderPath))
                        {
                            Debug.Log("    ✓ 動畫引用正確（指向新複製的檔案）");
                        }
                        else
                        {
                            Debug.LogWarning("    ⚠️ 動畫引用可能不正確（指向原始檔案）");
                        }
                    }
                }
            }
        }
        
        private static void VerifyOverrideController(string overridePath, string targetFolderPath)
        {
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
            if (overrideController == null) return;
            
            Debug.Log($"檢查Override Controller: {Path.GetFileName(overridePath)}");
            
            var overrides = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            
            foreach (var pair in overrides)
            {
                if (pair.Value != null)
                {
                    var clipPath = AssetDatabase.GetAssetPath(pair.Value);
                    Debug.Log($"  覆蓋動畫 [{pair.Key.name}] -> {Path.GetFileName(clipPath)}");
                    
                    if (clipPath.Contains(targetFolderPath))
                    {
                        Debug.Log("    ✓ 覆蓋動畫引用正確（指向新複製的檔案）");
                    }
                    else
                    {
                        Debug.LogWarning("    ⚠️ 覆蓋動畫引用可能不正確（指向原始檔案）");
                    }
                }
            }
        }
        
        [MenuItem("Tools/MonoFSM/清理測試檔案")]
        public static void CleanupTestFiles()
        {
            var testFolders = new[]
            {
                "Assets/FSMs/Puzzles/TestGate",
                "Assets/FSMs/Puzzles/TestWindow"
            };
            
            foreach (var folder in testFolders)
            {
                if (AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.DeleteAsset(folder);
                    Debug.Log($"已刪除測試資料夾: {folder}");
                }
            }
            
            AssetDatabase.Refresh();
        }
    }
}