using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Cung cấp Font Vector Dynamic sắc nét hỗ trợ tiếng Việt đầy đủ (Segoe UI / Arial)
/// giúp toàn bộ UI TextMeshPro không bị mờ nhòe hay vỡ hạt ở mọi độ phân giải.
/// </summary>
public static class UIFontHelper
{
    private static TMP_FontAsset cachedFont;

    public static TMP_FontAsset GetCrispFont()
    {
        if (cachedFont != null)
            return cachedFont;

        try
        {
            // Nạp font hệ thống Windows có bộ ký tự tiếng Việt Unicode hoàn chỉnh
            Font osFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Arial", "Calibri", "Tahoma" }, 48);
            if (osFont != null)
            {
                cachedFont = TMP_FontAsset.CreateFontAsset(
                    osFont,
                    48,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic
                );
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"UIFontHelper: Không thể khởi tạo dynamic font ({ex.Message}), dùng mặc định.");
        }

        if (cachedFont == null)
        {
            cachedFont = TMP_Settings.defaultFontAsset;
        }

        return cachedFont;
    }
}
