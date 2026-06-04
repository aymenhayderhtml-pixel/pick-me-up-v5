using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Phase 1b: Generates placeholder portrait sprites for all HeroDefinitions.
/// Creates 512×512 PNGs with class silhouettes, initials, and rarity borders.
/// Saved to Assets/Resources/Portraits/{HeroDefId}.png and sets PortraitSpritePath.
///
/// Run from Tools > Pick Me Up > Generate Placeholder Portraits
/// </summary>
public static class PortraitGenerator
{
    private const string PortraitsFolder = "Assets/Resources/Portraits";
    private const string HeroesFolder = "Assets/Resources/Heroes";
    private const int TextureSize = 512;
    private const int BorderWidth = 8;

    private static readonly Color GoldBorder = new Color(0.95f, 0.75f, 0.10f);

    // Class → silhouette path data (drawn as filled rects/shapes on the canvas)
    private static readonly Dictionary<HeroClass, System.Action<Texture2D, Color, int>> SilhouetteDrawers =
        new Dictionary<HeroClass, System.Action<Texture2D, Color, int>>
    {
        { HeroClass.Novice, DrawCircleSilhouette },
        { HeroClass.Vanguard, DrawShieldSilhouette },
        { HeroClass.Scout, DrawArrowheadSilhouette },
        { HeroClass.Mage, DrawStaffSilhouette },
        { HeroClass.Berserker, DrawAxeSilhouette },
        { HeroClass.Assassin, DrawDaggerSilhouette },
        { HeroClass.Support, DrawCrossSilhouette },
        { HeroClass.Specialist, DrawGearSilhouette },
    };

    [MenuItem("Tools/Pick Me Up/Generate Placeholder Portraits")]
    public static void Execute()
    {
        Directory.CreateDirectory(PortraitsFolder);
        AssetDatabase.Refresh();

        string[] heroGuids = AssetDatabase.FindAssets("t:HeroDefinition", new[] { HeroesFolder });
        if (heroGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Heroes Found", "No HeroDefinition assets found in Resources/Heroes/.", "OK");
            return;
        }

        int generated = 0, skipped = 0;

        foreach (string guid in heroGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HeroDefinition def = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
            if (def == null) continue;

            if (EditorUtility.DisplayCancelableProgressBar("Generating Portraits",
                    $"Processing {def.HeroName} ({def.HeroId})...",
                    (float)generated / heroGuids.Length))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            string heroId = def.HeroId;

            // Generate the texture
            Texture2D tex = GeneratePortrait(def);
            if (tex == null)
            {
                Debug.LogWarning($"[PortraitGenerator] Could not generate portrait for {heroId}");
                skipped++;
                continue;
            }

            // Save as PNG
            byte[] pngData = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            string pngPath = Path.Combine(PortraitsFolder, heroId + ".png");
            File.WriteAllBytes(pngPath, pngData);
            AssetDatabase.ImportAsset(pngPath);

            // Mark the portrait path on the definition
            string portraitResourcePath = "Portraits/" + heroId;
            if (def.PortraitSpritePath != portraitResourcePath)
            {
                def.PortraitSpritePath = portraitResourcePath;
                EditorUtility.SetDirty(def);
            }

            // Ensure import settings are sprite (2D UI)
            TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            generated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        Debug.Log($"[PortraitGenerator] Done. Generated: {generated}, Skipped: {skipped}");
        EditorUtility.DisplayDialog("Portraits Generated",
            $"Generated {generated} placeholder portraits in {PortraitsFolder}/\nSkipped: {skipped}",
            "OK");
    }

    [MenuItem("Tools/Pick Me Up/Clear Placeholder Portraits")]
    public static void ClearPortraits()
    {
        if (!Directory.Exists(PortraitsFolder))
        {
            EditorUtility.DisplayDialog("Nothing to Clear", "No Portraits folder found.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Clear Portraits?",
                "This will delete all generated placeholder portraits from Resources/Portraits/. Continue?",
                "Yes, delete them", "Cancel"))
            return;

        string[] files = Directory.GetFiles(PortraitsFolder, "*.png");
        foreach (string file in files)
        {
            AssetDatabase.DeleteAsset(file.Replace(Application.dataPath, "Assets"));
        }

        AssetDatabase.Refresh();
        Debug.Log($"[PortraitGenerator] Cleared {files.Length} placeholder portraits.");
        EditorUtility.DisplayDialog("Portraits Cleared", $"Deleted {files.Length} portrait PNGs.", "OK");
    }

    // ── Texture generation ─────────────────────────────────────────────────

    private static Texture2D GeneratePortrait(HeroDefinition def)
    {
        int size = TextureSize;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = Color.clear;

        // Fill transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        // Get class color
        Color classColor = def.BaseClass switch
        {
            HeroClass.Novice => new Color(0.50f, 0.50f, 0.50f),
            HeroClass.Vanguard => new Color(0.90f, 0.30f, 0.30f),
            HeroClass.Scout => new Color(0.20f, 0.70f, 0.60f),
            HeroClass.Mage => new Color(0.30f, 0.30f, 0.90f),
            HeroClass.Berserker => new Color(0.80f, 0.60f, 0.20f),
            HeroClass.Assassin => new Color(0.60f, 0.20f, 0.70f),
            HeroClass.Support => new Color(0.30f, 0.90f, 0.50f),
            HeroClass.Specialist => new Color(0.20f, 0.70f, 0.80f),
            _ => Color.gray
        };

        // Draw class silhouette
        if (SilhouetteDrawers.TryGetValue(def.BaseClass, out var drawer))
            drawer(tex, classColor, size);
        else
            DrawCircleSilhouette(tex, classColor, size);

        // Draw gold border for 4★+ heroes
        if (def.BaseStarRank >= 4)
            DrawGoldBorder(tex, size);

        // Draw initials
        string initials = GetInitials(def.HeroName);
        DrawInitials(tex, initials, size);

        tex.Apply();
        return tex;
    }

    // ── Silhouette shapes ──────────────────────────────────────────────────

    private static void DrawCircleSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2, cy = size / 2;
        int r = size / 3;
        int r2 = r * r;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r2)
                    tex.SetPixel(x, y, color);
            }
        }
    }

    private static void DrawShieldSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2;
        int w = size / 3, h = size / 2;
        int left = cx - w / 2, right = cx + w / 2;
        int top = size / 4, bottom = top + h;
        for (int y = top; y < bottom; y++)
        {
            float t = (float)(y - top) / h;
            int rowHalf = (int)(w * 0.5f * (1f + 0.3f * t));
            int x1 = cx - rowHalf, x2 = cx + rowHalf;
            for (int x = x1; x <= x2; x++)
                tex.SetPixel(x, y, color);
        }
    }

    private static void DrawArrowheadSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2, cy = size / 2;
        int halfH = size / 3, halfW = size / 4;
        for (int y = cy - halfH; y <= cy + halfH; y++)
        {
            float t = (float)(y - (cy - halfH)) / (halfH * 2);
            int rowW = (int)(halfW * (1f - Mathf.Abs(t - 0.5f) * 1.2f));
            for (int x = cx - rowW; x <= cx + rowW; x++)
                tex.SetPixel(x, y, color);
        }
    }

    private static void DrawStaffSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2;
        // Staff line
        int staffW = 6, staffL = size / 4, staffR = size * 3 / 4;
        for (int y = staffL; y <= staffR; y++)
            for (int x = cx - staffW / 2; x <= cx + staffW / 2; x++)
                tex.SetPixel(x, y, color);

        // Crescent top
        int cresCX = cx, cresCY = staffL;
        int cresR = size / 6;
        for (int y = cresCY - cresR; y <= cresCY + cresR; y++)
        {
            for (int x = cresCX - cresR; x <= cresCX + cresR; x++)
            {
                int dx = x - cresCX, dy = y - cresCY;
                int innerR = cresR * 3 / 5;
                int d2 = dx * dx + dy * dy;
                if (d2 <= cresR * cresR && d2 >= innerR * innerR && dx > 0)
                    tex.SetPixel(x, y, color);
            }
        }
    }

    private static void DrawAxeSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2, cy = size / 2;
        int bladeW = size / 3, bladeH = size / 4;
        // Blade
        for (int y = cy - bladeH / 2; y <= cy + bladeH / 2; y++)
        {
            int rowW = (int)(bladeW * (1f - Mathf.Abs((float)(y - cy) / bladeH) * 0.3f));
            for (int x = cx - rowW / 2; x <= cx + rowW / 2; x++)
                tex.SetPixel(x, y, color);
        }
        // Handle
        int handleW = 5, handleTop = cy + bladeH / 2, handleBot = cy + bladeH;
        for (int y = handleTop; y <= handleBot; y++)
            for (int x = cx - handleW / 2; x <= cx + handleW / 2; x++)
                tex.SetPixel(x, y, color);
    }

    private static void DrawDaggerSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2, cy = size / 2;
        int bladeH = size / 3;
        int top = cy - bladeH / 2, bot = cy + bladeH / 2;
        for (int y = top; y <= bot; y++)
        {
            float t = (float)(y - top) / bladeH;
            int rowW = t < 0.3f
                ? (int)(size / 10 * (1f - t / 0.3f * 0.7f))
                : (int)(size / 10 * 0.3f);
            // Add widening at base
            if (t > 0.7f) rowW = (int)(rowW * (1f + (t - 0.7f) / 0.3f * 2f));
            for (int x = cx - rowW / 2; x <= cx + rowW / 2; x++)
                tex.SetPixel(x, y, color);
        }
    }

    private static void DrawCrossSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2, cy = size / 2;
        int armW = size / 8, armH = size / 6;
        int barW = size / 6, barH = size / 8;
        int halfBarW = barW / 2, halfBarH = barH / 2;

        // Vertical bar
        for (int y = cy - armH; y <= cy + armH; y++)
            for (int x = cx - armW / 2; x <= cx + armW / 2; x++)
                tex.SetPixel(x, y, color);

        // Horizontal bar
        for (int y = cy - halfBarH; y <= cy + halfBarH; y++)
            for (int x = cx - halfBarW; x <= cx + halfBarW; x++)
                tex.SetPixel(x, y, color);
    }

    private static void DrawGearSilhouette(Texture2D tex, Color color, int size)
    {
        int cx = size / 2, cy = size / 2;
        int outerR = size / 3, innerR = size / 5;
        int teeth = 8;
        int toothH = size / 20;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                bool inTeeth = false;
                for (int t = 0; t < teeth; t++)
                {
                    float a0 = (float)t / teeth * Mathf.PI * 2f;
                    float a1 = (float)(t + 0.5f) / teeth * Mathf.PI * 2f;
                    if (angle >= a0 && angle <= a1)
                    {
                        inTeeth = true;
                        break;
                    }
                }

                float maxR = inTeeth ? outerR + toothH : outerR;
                if (dist >= innerR && dist <= maxR)
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // ── Border ─────────────────────────────────────────────────────────────

    private static void DrawGoldBorder(Texture2D tex, int size)
    {
        int cx = size / 2, cy = size / 2;
        int outer = size / 2 - 2;
        int inner = outer - BorderWidth;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist >= inner && dist <= outer)
                    tex.SetPixel(x, y, GoldBorder);
            }
        }
    }

    // ── Initials ───────────────────────────────────────────────────────────

    private static string GetInitials(string heroName)
    {
        if (string.IsNullOrEmpty(heroName)) return "?";

        string[] parts = heroName.Split(' ');
        if (parts.Length >= 2)
        {
            string first = parts[0];
            string last = parts[parts.Length - 1];
            if (first.Length > 0 && last.Length > 0)
            {
                // Use first char of both
                char f = char.ToUpperInvariant(first[0]);
                char l = char.ToUpperInvariant(last[0]);
                return $"{f}{l}";
            }
        }

        // Single name: use first 1-2 chars
        string upper = heroName.ToUpperInvariant();
        return upper.Length >= 2 ? upper.Substring(0, 2) : upper;
    }

    private static void DrawInitials(Texture2D tex, string initials, int size)
    {
        // Draw initials as solid white rects (simple pixel-based rendering)
        // Since we can't easily render fonts to Texture2D, draw a stylized block
        Color white = new Color(1f, 1f, 1f, 0.85f);
        int charCount = Mathf.Min(initials.Length, 2);
        int charW = size / 6;
        int charH = size / 4;
        int gap = size / 16;
        int totalW = charCount * charW + (charCount - 1) * gap;
        int startX = (size - totalW) / 2;
        int baseY = size / 2 - charH / 2;

        for (int ci = 0; ci < charCount; ci++)
        {
            int cx = startX + ci * (charW + gap);

            // Simple block for each character
            for (int y = baseY; y < baseY + charH; y++)
            {
                for (int x = cx; x < cx + charW; x++)
                {
                    // Create a basic character shape (vertical bar + hints)
                    bool draw = false;
                    char c = initials[ci];

                    // "I" shape
                    if (c == 'I')
                    {
                        draw = (x == cx || x == cx + charW - 1 || y == baseY || y == baseY + charH - 1);
                    }
                    else
                    {
                        // Fill shape with slight padding for legibility
                        draw = (x > cx + 1 && x < cx + charW - 2 && y > baseY + 1 && y < baseY + charH - 2);
                    }

                    if (draw)
                        tex.SetPixel(x, y, white);
                }
            }
        }
    }
}
