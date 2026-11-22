# Fonts

This folder holds the menu font source and the compiled AssetBundles the mod embeds.

- `cuphead-memphis.ttf` — the font source. The bundles below are built from it.
- `Bundles/memphis_5_6.assetbundle` — build in a **Unity 5.6.x** editor; used by game versions **1.0 and 1.1** (they run Unity 5.6.2).
- `Bundles/memphis_2017_4.assetbundle` — build in a **Unity 2017.4.x** editor; used by game versions **1.2 and 1.3+** (they run Unity 2017.4.9).

AssetBundles are not cross-engine-version compatible, hence one bundle per engine. When a bundle for the running game version is embedded, the menu renders with it; otherwise the menu falls back to the game's own fonts (with the glyph remapping).

## Building a bundle (one-time per engine)

1. Open a Unity editor of the matching version (any 5.6.x / any 2017.4.x editor works) and create an empty project.
2. Copy `cuphead-memphis.ttf` into the project's `Assets/` folder. The font asset's **Include Font Data** option must stay enabled (it is by default), so the TTF data is baked into the font asset.
3. Add the matching editor script below to the project's `Assets/Editor/` folder, then run **Tools > Build Font Bundle**.
4. Copy the produced `.assetbundle` file into this `Bundles/` folder with the exact name listed above. The mod's runtime looks for embedded resources named `*5_6*` / `*2017_4*` under `Fonts.Bundles`, so keep those suffixes.

### Unity 5.6.x editor script

```csharp
using UnityEditor;
using UnityEngine;

public static class BuildFontBundle {
    [MenuItem("Tools/Build Font Bundle")]
    public static void Build() {
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/cuphead-memphis.ttf");
        BuildPipeline.BuildAssetBundle(font, null, "memphis_5_6.assetbundle",
            BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }
}
```

### Unity 2017.4.x editor script

```csharp
using UnityEditor;
using UnityEngine;

public static class BuildFontBundle {
    [MenuItem("Tools/Build Font Bundle")]
    public static void Build() {
        AssetBundleBuild build = new AssetBundleBuild {
            assetNames = new[] { "Assets/cuphead-memphis.ttf" },
            assetBundleName = "memphis_2017_4.assetbundle"
        };
        BuildPipeline.BuildAssetBundles("Assets", new[] { build },
            BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }
}
```

The bundle's build target arch does not matter (the bundle only contains a font, no scripts); `StandaloneWindows` works for both.
