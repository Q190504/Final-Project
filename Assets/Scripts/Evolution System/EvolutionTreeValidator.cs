using System.Linq;
using UnityEditor;
using UnityEngine;

public static class EvolutionTreeValidator
{
    [InitializeOnLoadMethod]

    private static void ValidateTrees()
    {
        string[] guids = AssetDatabase.FindAssets("t:EvolutionTreeSO");

        var trees = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<EvolutionTreeSO>(AssetDatabase.GUIDToAssetPath(g)))
            .ToList();

        if (trees.Count == 0)
            return;

       const int tierCount = 3;

        foreach (var tree in trees)
        {
            if (tree.tiers.Count != tierCount)
            {
                Debug.LogError($"Evolution Tree [{tree.name}] has invalid tier count.");
            }
        }
    }
}