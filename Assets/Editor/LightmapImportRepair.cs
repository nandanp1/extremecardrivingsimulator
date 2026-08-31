using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Repairs texture metadata lost when the serialized Unity player assets were
/// reconstructed.  Asset extractors exported baked lightmaps as Sprites, which
/// makes Unity sample them with the wrong color-space and texture semantics.
/// </summary>
public static class LightmapImportRepair
{
    private static readonly string[] SceneLightmaps =
    {
        "Assets/Scenes/Common/1Menu/LightmapFar-0.png",
        "Assets/Scenes/Common/3Workshop/LightmapFar-0.png",
        "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_City_2/LightmapFar-0.png",
        "Assets/Scenes/Common/2Game/Lightmap_Day_2Game_Tunnel_Airport_Forest.png",
        "Assets/Scenes/Common/2Game/Lightmap_Night_2Game_Tunnel_City_Airport.png",
        "Assets/Scenes/Common/2Game/Lightmap_Night_2Game_Tunnel_City_Forest.png"
    };

    public static void RepairMenuLightmap()
    {
        Repair(new[] { SceneLightmaps[0] });
    }

    public static void RepairAllSceneLightmaps()
    {
        Repair(SceneLightmaps);
    }

    private static void Repair(string[] assetPaths)
    {
        foreach (string assetPath in assetPaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException("No TextureImporter found for " + assetPath);
            }

            importer.textureType = TextureImporterType.Lightmap;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            Debug.Log("Repaired baked-lightmap import settings: " + assetPath);
        }

        AssetDatabase.SaveAssets();
    }
}
