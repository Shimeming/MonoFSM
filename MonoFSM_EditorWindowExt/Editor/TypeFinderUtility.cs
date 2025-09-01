#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class TypeFinderUtility
{
    private static Dictionary<string, Type> _cachedExactName = new();
    private static List<Type> _cachedAllMonoBehaviourTypes;

    /// <summary>
    /// 透過完整名稱或簡名查找 MonoBehaviour（含自訂類別）。
    /// </summary>
    public static Type FindMonoBehaviourType(string typeName, bool allowPartialMatch = false)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        typeName = typeName.Trim().ToLower();
        // Try cached
        if (_cachedExactName.TryGetValue(typeName, out var found))
        {
            Debug.Log($"Found MonoBehaviour type from cache: {found.Name} for name: {typeName}");
            return found;
        }

        // Lazy load all MonoBehaviour-derived types
        _cachedAllMonoBehaviourTypes ??= TypeCache.GetTypesDerivedFrom<MonoBehaviour>().ToList();

        // First pass: exact name
        found = _cachedAllMonoBehaviourTypes.FirstOrDefault(t => t.Name.ToLower() == typeName);
        if (found != null)
        {
            _cachedExactName[typeName] = found;
            Debug.Log($"Found MonoBehaviour type: {found.Name} for name: {typeName}");
            return found;
        }

        // Second pass: fuzzy search (optional)
        if (allowPartialMatch)
        {
            var fuzzyMatches = _cachedAllMonoBehaviourTypes
                .Select(t => new
                {
                    Type = t,
                    Score = CalculateFuzzyScore(typeName, t.Name.ToLower()),
                })
                .Where(x => x.Score > 0.3f) // Minimum similarity threshold
                .OrderByDescending(x => x.Score)
                .ToList();

            if (fuzzyMatches.Count > 0)
            {
                found = fuzzyMatches.First().Type;
                Debug.Log(
                    $"Found MonoBehaviour type via fuzzy search: {found.Name} for name: {typeName} (score: {fuzzyMatches.First().Score:F2})"
                );
                return found;
            }
        }

        return null;
    }

    /// <summary>
    ///     計算模糊搜尋分數，結合多種算法以提供更好的匹配結果
    /// </summary>
    private static float CalculateFuzzyScore(string query, string target)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(target))
            return 0f;

        // Exact match gets highest score
        if (target == query)
            return 1.0f;

        // Contains match gets high score
        if (target.Contains(query))
            return 0.8f + (1.0f - (float)query.Length / target.Length) * 0.2f;

        // Calculate Levenshtein distance based similarity
        var levenshteinScore =
            1.0f
            - (float)LevenshteinDistance(query, target) / Math.Max(query.Length, target.Length);

        // Calculate subsequence match score
        var subsequenceScore = CalculateSubsequenceScore(query, target);

        // Calculate acronym match score (e.g., "PC" matches "PlayerController")
        var acronymScore = CalculateAcronymScore(query, target);

        // Return the highest score from all methods
        return Math.Max(Math.Max(levenshteinScore, subsequenceScore), acronymScore);
    }

    /// <summary>
    ///     計算 Levenshtein 距離（編輯距離）
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        if (s1.Length == 0)
            return s2.Length;
        if (s2.Length == 0)
            return s1.Length;

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;
        for (var j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= s1.Length; i++)
        for (var j = 1; j <= s2.Length; j++)
        {
            var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(
                    matrix[i - 1, j] + 1, // deletion
                    matrix[i, j - 1] + 1
                ), // insertion
                matrix[i - 1, j - 1] + cost
            ); // substitution
        }

        return matrix[s1.Length, s2.Length];
    }

    /// <summary>
    ///     計算子序列匹配分數（字符順序匹配）
    /// </summary>
    private static float CalculateSubsequenceScore(string query, string target)
    {
        if (query.Length > target.Length)
            return 0f;

        var matches = 0;
        var targetIndex = 0;

        for (
            var queryIndex = 0;
            queryIndex < query.Length && targetIndex < target.Length;
            queryIndex++
        )
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
    ///     計算首字母縮寫匹配分數
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
            // Add first character or uppercase characters after lowercase
            if (i == 0 || (char.IsUpper(c) && wasLower))
                acronym += char.ToLower(c);
            wasLower = char.IsLower(c);
        }

        if (acronym.Contains(query))
            return 0.7f + (float)query.Length / acronym.Length * 0.3f;

        return 0f;
    }

    /// <summary>
    /// 回傳所有繼承自 T 的型別（cached）。
    /// </summary>
    public static List<Type> GetAllTypesDerivedFrom<T>()
        where T : class
    {
        return TypeCache.GetTypesDerivedFrom<T>().ToList();
    }
}
#endif
