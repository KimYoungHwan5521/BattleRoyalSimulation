using UnityEditor;
using UnityEngine;

public static class MissingScriptFinder
{
    [MenuItem("Tools/Find Missing Scripts In Project")]
    private static void FindMissingScriptsInProject()
    {
        int totalCount = 0;

        string[] prefabGuids =
            AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            Transform[] children =
                prefab.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                int missingCount =
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        child.gameObject
                    );

                if (missingCount <= 0)
                    continue;

                Debug.LogError(
                    $"Missing Script: {path} / {GetPath(child)} " +
                    $"({missingCount})",
                    prefab
                );

                totalCount += missingCount;
            }
        }

        Debug.Log($"Prefab Missing Script 검색 완료: {totalCount}개");
    }

    private static string GetPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = $"{target.name}/{path}";
        }

        return path;
    }
}