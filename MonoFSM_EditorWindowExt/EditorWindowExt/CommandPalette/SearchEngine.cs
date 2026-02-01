#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandPalette
{
    /// <summary>
    /// 搜尋結果介面
    /// </summary>
    public interface ISearchResult<T>
    {
        T Item { get; }
        float Score { get; }
        string MatchedField { get; } // 用於顯示匹配的欄位
    }

    /// <summary>
    /// 搜尋結果實作
    /// </summary>
    public class SearchResult<T> : ISearchResult<T>
    {
        public T Item { get; }
        public float Score { get; }
        public string MatchedField { get; }

        public SearchResult(T item, float score, string matchedField = "")
        {
            Item = item;
            Score = score;
            MatchedField = matchedField;
        }
    }

    /// <summary>
    /// 多層次搜尋引擎
    /// 層次 1：精確匹配（名稱完全相等）→ 1.0 分
    /// 層次 2：前綴匹配（名稱以關鍵字開頭）→ 0.9 分
    /// 層次 3：包含匹配（名稱包含關鍵字）→ 0.8 分
    /// 層次 4：路徑匹配（路徑包含關鍵字）→ 0.6 分
    /// 層次 5：Fuzzy 匹配（Levenshtein、子序列、首字母縮寫）
    /// </summary>
    public static class SearchEngine
    {
        /// <summary>
        /// 搜尋 AssetEntry 列表
        /// </summary>
        public static List<SearchResult<AssetEntry>> Search(string query, IEnumerable<AssetEntry> assets, int maxResults = 100)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // 空查詢時返回所有資源（依名稱排序）
                return assets
                    .OrderBy(a => a.name)
                    .Take(maxResults)
                    .Select(a => new SearchResult<AssetEntry>(a, 1.0f))
                    .ToList();
            }

            var queryLower = query.Trim().ToLower();
            var results = new List<SearchResult<AssetEntry>>();

            foreach (var asset in assets)
            {
                var result = CalculateAssetScore(queryLower, asset);
                if (result != null && result.Score > 0)
                {
                    results.Add(result);
                }
            }

            return results
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Item.name)
                .Take(maxResults)
                .ToList();
        }

        /// <summary>
        /// 搜尋 EditorWindowEntry 列表
        /// </summary>
        public static List<SearchResult<EditorWindowEntry>> Search(string query, IEnumerable<EditorWindowEntry> windows, int maxResults = 100)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return windows
                    .OrderBy(w => w.DisplayName)
                    .Take(maxResults)
                    .Select(w => new SearchResult<EditorWindowEntry>(w, 1.0f))
                    .ToList();
            }

            var queryLower = query.Trim().ToLower();
            var results = new List<SearchResult<EditorWindowEntry>>();

            foreach (var window in windows)
            {
                var result = CalculateWindowScore(queryLower, window);
                if (result != null && result.Score > 0)
                {
                    results.Add(result);
                }
            }

            return results
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Item.DisplayName)
                .Take(maxResults)
                .ToList();
        }

        /// <summary>
        /// 搜尋 MenuItemEntry 列表
        /// </summary>
        public static List<SearchResult<MenuItemEntry>> Search(string query, IEnumerable<MenuItemEntry> menuItems, int maxResults = 100)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return menuItems
                    .OrderBy(m => m.displayName)
                    .Take(maxResults)
                    .Select(m => new SearchResult<MenuItemEntry>(m, 1.0f))
                    .ToList();
            }

            var queryLower = query.Trim().ToLower();
            var results = new List<SearchResult<MenuItemEntry>>();

            foreach (var menuItem in menuItems)
            {
                var result = CalculateMenuItemScore(queryLower, menuItem);
                if (result != null && result.Score > 0)
                {
                    results.Add(result);
                }
            }

            return results
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Item.displayName)
                .Take(maxResults)
                .ToList();
        }

        private static SearchResult<AssetEntry> CalculateAssetScore(string query, AssetEntry asset)
        {
            var nameLower = asset.name?.ToLower() ?? "";
            var pathLower = asset.path?.ToLower() ?? "";

            // 層次 1：精確匹配
            if (nameLower == query)
            {
                return new SearchResult<AssetEntry>(asset, 1.0f, "name");
            }

            // 層次 2：前綴匹配
            if (nameLower.StartsWith(query))
            {
                var score = 0.9f + (float)query.Length / nameLower.Length * 0.05f;
                return new SearchResult<AssetEntry>(asset, score, "name");
            }

            // 層次 3：包含匹配
            if (nameLower.Contains(query))
            {
                var score = 0.8f + (float)query.Length / nameLower.Length * 0.05f;
                return new SearchResult<AssetEntry>(asset, score, "name");
            }

            // 層次 4：路徑匹配
            if (pathLower.Contains(query))
            {
                var score = 0.6f + (float)query.Length / pathLower.Length * 0.1f;
                return new SearchResult<AssetEntry>(asset, score, "path");
            }

            // 層次 5：Fuzzy 匹配
            var fuzzyScore = CalculateFuzzyScore(query, nameLower);
            if (fuzzyScore > 0.3f)
            {
                return new SearchResult<AssetEntry>(asset, fuzzyScore * 0.5f, "fuzzy");
            }

            return null;
        }

        private static SearchResult<EditorWindowEntry> CalculateWindowScore(string query, EditorWindowEntry window)
        {
            var displayNameLower = window.DisplayName?.ToLower() ?? "";
            var typeNameLower = window.Type?.Name?.ToLower() ?? "";
            var categoryLower = window.Category?.ToLower() ?? "";

            // 層次 1：精確匹配（DisplayName 或 TypeName）
            if (displayNameLower == query || typeNameLower == query)
            {
                return new SearchResult<EditorWindowEntry>(window, 1.0f, "name");
            }

            // 層次 2：前綴匹配
            if (displayNameLower.StartsWith(query) || typeNameLower.StartsWith(query))
            {
                var score = 0.9f;
                return new SearchResult<EditorWindowEntry>(window, score, "name");
            }

            // 層次 3：包含匹配
            if (displayNameLower.Contains(query) || typeNameLower.Contains(query))
            {
                var score = 0.8f;
                return new SearchResult<EditorWindowEntry>(window, score, "name");
            }

            // 層次 4：Category 匹配
            if (categoryLower.Contains(query))
            {
                return new SearchResult<EditorWindowEntry>(window, 0.6f, "category");
            }

            // 層次 5：Fuzzy 匹配
            var fuzzyScore = Math.Max(
                CalculateFuzzyScore(query, displayNameLower),
                CalculateFuzzyScore(query, typeNameLower)
            );
            if (fuzzyScore > 0.3f)
            {
                return new SearchResult<EditorWindowEntry>(window, fuzzyScore * 0.5f, "fuzzy");
            }

            return null;
        }

        private static SearchResult<MenuItemEntry> CalculateMenuItemScore(string query, MenuItemEntry menuItem)
        {
            var displayNameLower = menuItem.displayName?.ToLower() ?? "";
            var menuPathLower = menuItem.menuPath?.ToLower() ?? "";
            var categoryLower = menuItem.category?.ToLower() ?? "";

            // 層次 1：精確匹配
            if (displayNameLower == query)
            {
                return new SearchResult<MenuItemEntry>(menuItem, 1.0f, "name");
            }

            // 層次 2：前綴匹配
            if (displayNameLower.StartsWith(query))
            {
                var score = 0.9f;
                return new SearchResult<MenuItemEntry>(menuItem, score, "name");
            }

            // 層次 3：包含匹配
            if (displayNameLower.Contains(query))
            {
                var score = 0.8f;
                return new SearchResult<MenuItemEntry>(menuItem, score, "name");
            }

            // 層次 4：Path 匹配
            if (menuPathLower.Contains(query))
            {
                return new SearchResult<MenuItemEntry>(menuItem, 0.6f, "path");
            }

            // 層次 5：Fuzzy 匹配
            var fuzzyScore = CalculateFuzzyScore(query, displayNameLower);
            if (fuzzyScore > 0.3f)
            {
                return new SearchResult<MenuItemEntry>(menuItem, fuzzyScore * 0.5f, "fuzzy");
            }

            return null;
        }

        /// <summary>
        /// 計算模糊搜尋分數（複用 TypeFinderUtility 的演算法）
        /// </summary>
        private static float CalculateFuzzyScore(string query, string target)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(target))
                return 0f;

            // 精確匹配最高分
            if (target == query)
                return 1.0f;

            // 包含匹配得高分
            if (target.Contains(query))
                return 0.8f + (1.0f - (float)query.Length / target.Length) * 0.2f;

            // 計算 Levenshtein 距離相似度
            var levenshteinScore = 1.0f - (float)LevenshteinDistance(query, target) / Math.Max(query.Length, target.Length);

            // 計算子序列匹配分數
            var subsequenceScore = CalculateSubsequenceScore(query, target);

            // 計算首字母縮寫匹配分數
            var acronymScore = CalculateAcronymScore(query, target);

            // 返回最高分數
            return Math.Max(Math.Max(levenshteinScore, subsequenceScore), acronymScore);
        }

        /// <summary>
        /// 計算 Levenshtein 距離（編輯距離）
        /// </summary>
        private static int LevenshteinDistance(string s1, string s2)
        {
            if (s1.Length == 0) return s2.Length;
            if (s2.Length == 0) return s1.Length;

            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (var i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            for (var j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (var i = 1; i <= s1.Length; i++)
            {
                for (var j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(
                            matrix[i - 1, j] + 1,      // deletion
                            matrix[i, j - 1] + 1),    // insertion
                        matrix[i - 1, j - 1] + cost); // substitution
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        /// <summary>
        /// 計算子序列匹配分數（字符順序匹配）
        /// </summary>
        private static float CalculateSubsequenceScore(string query, string target)
        {
            if (query.Length > target.Length)
                return 0f;

            var matches = 0;
            var targetIndex = 0;

            for (var queryIndex = 0; queryIndex < query.Length && targetIndex < target.Length; queryIndex++)
            {
                while (targetIndex < target.Length && target[targetIndex] != query[queryIndex])
                    targetIndex++;

                if (targetIndex < target.Length)
                {
                    matches++;
                    targetIndex++;
                }
            }

            return matches == query.Length ? (float)matches / target.Length : 0f;
        }

        /// <summary>
        /// 計算首字母縮寫匹配分數（例如 "PC" 匹配 "PlayerController"）
        /// </summary>
        private static float CalculateAcronymScore(string query, string target)
        {
            if (query.Length > target.Length)
                return 0f;

            var acronym = "";
            var wasLower = false;

            for (var i = 0; i < target.Length; i++)
            {
                var c = target[i];
                // 加入首字母或大寫字母（在小寫字母之後）
                if (i == 0 || (char.IsUpper(c) && wasLower))
                    acronym += char.ToLower(c);
                wasLower = char.IsLower(c);
            }

            if (acronym.Contains(query))
                return 0.7f + (float)query.Length / acronym.Length * 0.3f;

            return 0f;
        }
    }
}
#endif
