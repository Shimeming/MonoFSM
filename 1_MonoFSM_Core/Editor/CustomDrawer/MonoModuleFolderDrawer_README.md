# MonoModuleFolder 一鍵添加功能

## 功能說明

`MonoModuleFolder` 現在配備了 Odin Inspector 自定義介面，提供了方便的一鍵添加功能，可以快速將選定的 Prefab 模組實例化為
GameObject 的子物件。

## 使用方法

### 1. 設置 MonoModuleFolder

在 Unity 場景中選擇一個包含 `MonoModuleFolder` 組件的 GameObject。

### 2. 選擇 Prefab 模組

在 Inspector 面板中，你會看到 `Prefab Modules` 字段：

- 使用 `PrefabFilter` 屬性自動篩選出符合條件的 MonoModulePack Prefab
- 可以添加多個 Prefab 到列表中

### 3. 使用一鍵添加功能

設置好 Prefab 列表後，Inspector 面板會顯示以下按鈕：

#### **添加所有模組** (綠色按鈕)

- 圖標：➕
- 功能：將所有選定的 Prefab 實例化並添加為當前 GameObject 的子物件
- 特性：
    - 自動重置子物件的 Transform（位置、旋转、縮放）
    - 保持 Prefab 連接
    - 支持 Undo/Redo 操作

#### **清除子物件** (紅色按鈕)

- 圖標：🗑️
- 功能：刪除當前 GameObject 下的所有子物件
- 特性：
    - 需要確認對話框
    - 支持 Undo/Redo 操作（可以使用 Ctrl+Z 復原）
    - 只有在有子物件時才會顯示

### 4. 狀態顯示

Inspector 面板會自動顯示以下資訊：

- **無 Prefab 選擇時**：提示 "請先選擇要添加的 Prefab 模組"
- **選擇了無效 Prefab 時**：警告 "所選的 Prefab 模組列表中沒有有效的 Prefab"
- **有子物件時**：顯示 "當前子物件數量: X"

## 技術實現

### 自定義 Drawer

位置：`MonoFSM/1_MonoFSM_Core/Editor/CustomDrawer/MonoModuleFolderDrawer.cs`

核心類：`MonoModuleFolderDrawer : OdinValueDrawer<MonoModuleFolder>`

### 主要功能

1. **AddAllPrefabsAsChildren()**
    - 使用 `PrefabUtility.InstantiatePrefab()` 保持 Prefab 連接
    - 自動設置父子關係
    - 重置 Transform 屬性
    - 註冊 Undo 操作

2. **ClearAllChildren()**
    - 從後往前遍歷刪除子物件
    - 避免索引問題
    - 使用 `Undo.DestroyObjectImmediate()` 支持撤銷

### 相關組件

- **MonoModuleFolder**：Runtime 組件，位於 `MonoFSM/1_MonoFSM_Core/Runtime/MonoData/ModulePack/MonoModuleFolder.cs`
- **PrefabFilterAttribute**：Prefab 篩選屬性，支持基於組件類型過濾

## 注意事項

- ⚠️ 添加的 Prefab 會保持與原 Prefab 的連接
- ⚠️ 清除操作雖然支持 Undo，但建議操作前先保存場景
- ⚠️ PrefabFilter 會自動搜尋專案中所有 MonoObj 類型的 Prefab

## 示例工作流程

```
1. 在 Hierarchy 中選擇或創建一個 GameObject
2. 添加 MonoModuleFolder 組件
3. 在 Prefab Modules 列表中選擇需要的模組 Prefab
4. 點擊 "添加所有模組 (X)" 按鈕
5. 所有 Prefab 會自動實例化為子物件
6. 如需重新設置，點擊 "清除子物件 (X)" 後重新添加
```

## 日誌輸出

操作時會在 Console 輸出以下日誌：

- 成功添加：`[MonoModuleFolder] 成功添加 X 個模組到 [GameObject名稱]`
- 清除完成：`[MonoModuleFolder] 已清除 [GameObject名稱] 的 X 個子物件`
- 警告訊息：如果沒有有效的 Prefab
