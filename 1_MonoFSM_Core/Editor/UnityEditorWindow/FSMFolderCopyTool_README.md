# FSM資料夾複製工具

這是一個Unity Editor工具，用於複製包含FSM Prefab、Animator Controller和Animation Clips的完整資料夾，並自動維護它們之間的引用關係。

## 功能特色

### 🎯 智能資料夾分析
- 自動檢測FSM資料夾結構
- 識別Prefab、Animator Controller、Animation Clips等資產
- 驗證資料夾是否包含有效的FSM組件

### 📋 多種複製模式

#### Prefab複製方式：
- **直接複製**：建立完全獨立的Prefab副本
- **建立Variant**：新Prefab繼承原始Prefab，只覆蓋差異部分

#### Animator複製方式：
- **直接複製**：建立完全獨立的Animator Controller
- **建立Override Controller**：使用Animator Override Controller，可以單獨替換動畫

### 🔗 自動引用維護
- 自動更新Prefab中的Animator Controller引用
- 自動更新Animator Controller中的Animation Clip引用
- 支援Override Controller的動畫覆蓋設定
- 使用SerializedObject確保引用關係正確

## 使用方法

### 方法1：右鍵選單（推薦）

1. 在Project視窗中右鍵點擊FSM資料夾
2. 選擇 **Assets > MonoFSM > 複製FSM資料夾**
3. 在彈出的視窗中設定複製選項：
   - 選擇目標資料夾
   - 輸入新基礎名稱（例如："Gate"）
   - 選擇Prefab複製模式（直接複製/建立Variant）
   - 選擇Animator複製模式（直接複製/建立Override Controller）
4. 點擊「開始複製」

### 方法2：工具選單

1. 選擇 **Tools > MonoFSM > FSM資料夾複製工具**
2. 手動選擇來源資料夾
3. 設定複製選項
4. 執行複製

### 方法3：分析資料夾

右鍵選擇 **Assets > MonoFSM > 分析FSM資料夾** 可以查看資料夾的詳細分析結果。

## 複製選項說明

### 命名設定
- **新基礎名稱**：輸入新的基礎名稱，工具會智能替換檔案名稱中的原資料夾名稱
  - 例如：原資料夾名稱為"Door"，輸入"Gate"
  - `General FSM Variant - Door.prefab` → `General FSM Variant - Gate.prefab`
  - 如果檔案名稱中沒有原資料夾名稱，會在前面添加新基礎名稱

### 複製模式對比

| 特性 | 直接複製 | Variant/Override |
|------|----------|------------------|
| 獨立性 | 完全獨立 | 繼承原始資產 |
| 檔案大小 | 較大 | 較小 |
| 修改影響 | 互不影響 | 原始修改會影響副本 |
| 適用場景 | 需要大幅修改 | 只需微調差異 |

## 支援的資料夾結構

工具會自動識別以下檔案類型：
- `.prefab` - Prefab檔案
- `.controller` - Animator Controller
- `.anim` - Animation Clip
- 其他資產檔案

### 有效FSM資料夾要求：
- 至少包含1個Prefab檔案
- 至少包含1個Animator Controller檔案

## 錯誤處理

### 常見問題：
1. **「無效的FSM資料夾」** - 確保資料夾包含Prefab和Animator Controller
2. **「資料夾已存在」** - 修改名稱設定或選擇其他目標位置
3. **「複製失敗」** - 檢查Console輸出獲取詳細錯誤訊息

### 故障排除：
- 確保所有資產沒有被其他程序鎖定
- 檢查目標資料夾的寫入權限
- 確保Unity Editor處於正常狀態（沒有編譯錯誤）

## 技術實現

### 核心類別：
- `FSMFolderCopyTool` - 核心複製邏輯
- `FSMFolderCopyWindow` - UI介面視窗
- `FSMFolderContextMenuExtensions` - 右鍵選單擴展

### 引用更新策略：
1. **階段性複製**：先複製基礎資產，再建立引用關係
2. **SerializedObject更新**：使用Unity的序列化系統更新引用
3. **延遲執行**：使用EditorApplication.delayCall確保資產完全載入

## 範例用途

### 場景1：創建門的變體
```
原始: Assets/FSMs/Puzzles/Door/
複製: Assets/FSMs/Puzzles/Gate/
新基礎名稱: "Gate"
模式: Variant + Override Controller
用途: 大門使用不同的動畫，但基本邏輯相同
結果: General FSM Variant - Door.prefab → General FSM Variant - Gate.prefab
```

### 場景2：創建完全獨立的副本
```
原始: Assets/FSMs/Puzzles/Door/
複製: Assets/FSMs/Puzzles/Window/
新基礎名稱: "Window"
模式: 直接複製
用途: 窗戶需要完全不同的行為和動畫
結果: General FSM Variant - Door.prefab → General FSM Variant - Window.prefab
```

## 版本資訊

- 初始版本：支援基本的FSM資料夾複製功能
- 支援Unity 2022.3+
- 相容MonoFSM框架

## 開發者說明

此工具是MonoFSM框架的一部分，位於 `MonoFSM/1_MonoFSM_Core/Editor/UnityEditorWindow/` 目錄下。

如需修改或擴展功能，請參考現有的Editor工具實現方式。