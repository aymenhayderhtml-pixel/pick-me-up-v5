#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class HeroPortraitGenerator : EditorWindow
{
    [MenuItem("PickMeUp/Generate Hero Portraits")]
    public static void ShowWindow()
    {
        GetWindow<HeroPortraitGenerator>("Portrait Gen");
    }

    private int _size = 256;

    private void OnGUI()
    {
        GUILayout.Label("Hero Portrait Generator", EditorStyles.boldLabel);
        GUILayout.Label("Creates colored silhouette textures per class", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        _size = EditorGUILayout.IntField("Texture Size", _size);
        _size = Mathf.Clamp(_size, 64, 512);

        if (GUILayout.Button("GENERATE ALL PORTRAITS", GUILayout.Height(50)))
        {
            GenerateAll();
        }
    }

    private void GenerateAll()
    {
        HeroDefinition[] defs = Resources.LoadAll<HeroDefinition>("Heroes");
        if (defs == null || defs.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No HeroDefinition assets found in Resources/Heroes/", "OK");
            return;
        }

        string folder = "Assets/Resources/HeroPortraits";
        if (!System.IO.Directory.Exists(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        foreach (HeroDefinition def in defs)
        {
            if (def == null) continue;

            Color baseColor = GetClassColor(def.BaseClass);
            Texture2D tex = CreateSilhouetteTexture(_size, baseColor);

            string path = $"{folder}/{def.HeroId}_portrait.png";
            byte[] png = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, png);
            Object.DestroyImmediate(tex);

            Debug.Log($"[PortraitGen] Created {path}");
        }

        AssetDatabase.Refresh();

        // Now assign textures to hero definitions
        foreach (HeroDefinition def in defs)
        {
            string texPath = $"HeroPortraits/{def.HeroId}_portrait";
            Sprite sprite = Resources.Load<Sprite>(texPath);
            if (sprite != null)
            {
                def.Portrait = sprite;
                def.PortraitSpritePath = texPath;
                EditorUtility.SetDirty(def);
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", $"Generated {defs.Length} portraits", "OK");
    }

    private Texture2D CreateSilhouetteTexture(int size, Color baseColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // Fill background transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Draw a rounded-rectangle silhouette in center
        int margin = size / 8;
        int centerX = size / 2;
        int centerY = size / 2;
        int radiusX = (size - margin * 2) / 2;
        int radiusY = (size - margin * 2) / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - centerX) / (float)radiusX;
                float dy = (y - centerY) / (float)radiusY;
                float dist = dx * dx + dy * dy;

                if (dist <= 1.0f)
                {
                    // Gradient from light at top to dark at bottom
                    float gradient = 1f - ((float)y / size) * 0.5f;
                    Color pixelColor = baseColor * gradient;
                    pixelColor.a = 0.95f;

                    // Add subtle noise
                    pixelColor.r += Random.Range(-0.05f, 0.05f);
                    pixelColor.g += Random.Range(-0.05f, 0.05f);
                    pixelColor.b += Random.Range(-0.05f, 0.05f);

                    pixels[y * size + x] = pixelColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private Color GetClassColor(HeroClass heroClass)
    {
        switch (heroClass)
        {
            case HeroClass.Warrior: return new Color(0.85f, 0.25f, 0.15f);
            case HeroClass.Mage: return new Color(0.25f, 0.35f, 0.9f);
            case HeroClass.Ranger: return new Color(0.15f, 0.75f, 0.25f);
            case HeroClass.Healer: return new Color(0.9f, 0.8f, 0.15f);
            case HeroClass.Tank: return new Color(0.5f, 0.5f, 0.55f);
            case HeroClass.Vanguard: return new Color(0.8f, 0.2f, 0.2f);
            case HeroClass.Scout: return new Color(0.2f, 0.7f, 0.3f);
            case HeroClass.Berserker: return new Color(0.9f, 0.1f, 0.1f);
            case HeroClass.Assassin: return new Color(0.5f, 0.1f, 0.5f);
            case HeroClass.Support: return new Color(0.2f, 0.6f, 0.9f);
            case HeroClass.Specialist: return new Color(0.7f, 0.5f, 0.2f);
            default: return new Color(0.7f, 0.7f, 0.75f);
        }
    }
}
#endif
