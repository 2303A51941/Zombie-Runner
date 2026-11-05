
using UnityEngine;
using UnityEditor;
using System.IO;

public class FixAnimationScale
{
    [MenuItem("Tools/Zombie Runner/Fix Animation Scale")]
    static void FixAnimations()
    {
        Object[] selected = Selection.objects;

        if (selected.Length == 0)
        {
            Debug.LogError("Select your character FBX first!");
            return;
        }

        foreach (Object obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!path.EndsWith(
                ".fbx",
                System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("Please select an FBX file: " + path);
                continue;
            }

            Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object asset in assets)
            {
                AnimationClip source = asset as AnimationClip;

                if (source == null || source.name.StartsWith("__"))
                    continue;

                AnimationClip copy = new AnimationClip();

                EditorUtility.CopySerialized(source, copy);

                EditorCurveBinding[] bindings =
                    AnimationUtility.GetCurveBindings(copy);

                int removedScale = 0;

                foreach (EditorCurveBinding binding in bindings)
                {
                    // Only remove scale animation on the root object.
                    // Keep all rotation, position and bone animation.
                    bool isRootScale =
                        binding.type == typeof(Transform) &&
                        binding.path == "" &&
                        binding.propertyName.StartsWith("m_LocalScale");

                    if (isRootScale)
                    {
                        AnimationUtility.SetEditorCurve(
                            copy,
                            binding,
                            null
                        );

                        removedScale++;
                    }
                }

                string folder = Path.GetDirectoryName(path);

                string safeName = source.name.Replace("|", "_");

                string outputPath =
                    folder + "/" + safeName + "_ScaleFixed.anim";

                outputPath =
                    AssetDatabase.GenerateUniqueAssetPath(outputPath);

                AssetDatabase.CreateAsset(copy, outputPath);

                Debug.Log(
                    "Created: " + outputPath +
                    " | Removed root scale curves: " + removedScale
                );
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Scale correction completed. Original animations preserved.");
    }
}
