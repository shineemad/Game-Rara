using UnityEngine;

/// <summary>
/// Aset bersama untuk tata letak box dialog.
///
/// CARA PAKAI:
///   1. Project view → klik kanan → Create → RARA → Dialog Box Layout.
///   2. Drag aset ini ke field "Layout" pada komponen Day1Intro, NpcDialog,
///      atau DialogManager. Saat Play, semua komponen akan menyalin nilainya
///      dari aset ini → ubah sekali, semua box dialog ikut berubah.
///   3. Tekan tombol ContextMenu pada aset untuk reset ke preset 8.png.
/// </summary>
[CreateAssetMenu(menuName = "RARA/Dialog Box Layout", fileName = "DialogBoxLayout")]
public class DialogBoxLayout : ScriptableObject
{
    [Header("Sprite")]
    [Tooltip("Sprite kotak dialog utama. Kosong = pakai panel solid warna.")]
    public Sprite boxSprite;
    [Tooltip("Sprite banner nama pembicara (lencana kayu). Kosong = warna solid.")]
    public Sprite nameBannerSprite;

    [Header("Posisi & Ukuran Panel (fraksi layar 0–1)")]
    [Range(0f, 1f)]   public float panelCenterX    = 0.50f;
    [Range(0f, 1f)]   public float panelCenterY    = 0.215f;
    [Range(0.1f, 1f)] public float panelWidthFrac  = 0.96f;
    [Range(0.02f, 0.5f)] public float panelHeightFrac = 0.395f;

    [Header("Portrait (anchor fraksi panel 0–1)")]
    [Range(0f, 1f)]    public float portraitCenterX = 0.153f;
    [Range(0f, 1f)]    public float portraitCenterY = 0.625f;
    [Range(0.02f, 0.6f)] public float portraitSizeW = 0.192f;
    [Range(0.02f, 1f)]   public float portraitSizeH = 0.494f;
    public bool        portraitPreserveAspect       = true;

    [Header("Banner Nama (anchor 0–1)")]
    public Vector2 bannerAnchorMin = new Vector2(0.057f, 0.196f);
    public Vector2 bannerAnchorMax = new Vector2(0.253f, 0.333f);

    [Header("Area Teks (anchor 0–1)")]
    public Vector2 textAnchorMin = new Vector2(0.345f, 0.20f);
    public Vector2 textAnchorMax = new Vector2(0.955f, 0.78f);

    [Header("Petunjuk Lanjut (anchor 0–1)")]
    [Range(0f, 1f)]   public float hintCenterX = 0.82f;
    [Range(0f, 1f)]   public float hintCenterY = 0.13f;
    [Range(0.05f, 1f)] public float hintSizeW  = 0.30f;
    [Range(0.02f, 0.5f)] public float hintSizeH = 0.12f;

    // ══════════════════════════════════════════════════════════════════════
    // Helper untuk konversi (center, size) ke pasangan anchorMin/Max
    // ══════════════════════════════════════════════════════════════════════
    public Vector2 PanelAnchorMin    => new Vector2(panelCenterX    - panelWidthFrac  * 0.5f, panelCenterY    - panelHeightFrac * 0.5f);
    public Vector2 PanelAnchorMax    => new Vector2(panelCenterX    + panelWidthFrac  * 0.5f, panelCenterY    + panelHeightFrac * 0.5f);
    public Vector2 PortraitAnchorMin => new Vector2(portraitCenterX - portraitSizeW  * 0.5f, portraitCenterY - portraitSizeH   * 0.5f);
    public Vector2 PortraitAnchorMax => new Vector2(portraitCenterX + portraitSizeW  * 0.5f, portraitCenterY + portraitSizeH   * 0.5f);
    public Vector2 HintAnchorMin     => new Vector2(hintCenterX     - hintSizeW      * 0.5f, hintCenterY     - hintSizeH       * 0.5f);
    public Vector2 HintAnchorMax     => new Vector2(hintCenterX     + hintSizeW      * 0.5f, hintCenterY     + hintSizeH       * 0.5f);

    // ══════════════════════════════════════════════════════════════════════
    // Preset untuk sprite UI day 1/8.png
    // ══════════════════════════════════════════════════════════════════════

    [ContextMenu("▶ Reset ke Preset 8.png")]
    public void ResetToPreset8()
    {
        panelCenterX    = 0.50f;
        panelCenterY    = 0.215f;
        panelWidthFrac  = 0.96f;
        panelHeightFrac = 0.395f;

        portraitCenterX        = 0.153f;
        portraitCenterY        = 0.625f;
        portraitSizeW          = 0.192f;
        portraitSizeH          = 0.494f;
        portraitPreserveAspect = true;

        bannerAnchorMin = new Vector2(0.057f, 0.196f);
        bannerAnchorMax = new Vector2(0.253f, 0.333f);

        textAnchorMin = new Vector2(0.345f, 0.20f);
        textAnchorMax = new Vector2(0.955f, 0.78f);

        hintCenterX = 0.82f;
        hintCenterY = 0.13f;
        hintSizeW   = 0.30f;
        hintSizeH   = 0.12f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /// <summary>
    /// Preset Day Intro — banner & teks digeser sedikit dari preset 8,
    /// portrait sedikit lebih kecil agar pas frame kayu. Diambil dari
    /// tuning manual user (gambar Inspector Day1Intro).
    /// </summary>
    [ContextMenu("▶ Reset ke Preset Day Intro")]
    public void ResetToPresetDayIntro()
    {
        panelCenterX    = 0.50f;
        panelCenterY    = 0.219f;
        panelWidthFrac  = 0.939f;
        panelHeightFrac = 0.395f;

        portraitCenterX        = 0.14f;
        portraitCenterY        = 0.584f;
        portraitSizeW          = 0.189f;
        portraitSizeH          = 0.56f;
        portraitPreserveAspect = false;

        bannerAnchorMin = new Vector2(0.10f,  0.10f);
        bannerAnchorMax = new Vector2(0.253f, 0.333f);

        textAnchorMin = new Vector2(0.31f, 0.55f);
        textAnchorMax = new Vector2(0.84f, 0.76f);

        hintCenterX = 0.798f;
        hintCenterY = 0.242f;
        hintSizeW   = 0.296f;
        hintSizeH   = 0.12f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("▶ Muat Sprite Default (UI day 1/8.png)")]
    void LoadDefaultSprite()
    {
        var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/UI day 1/8.png");
        if (sp != null) { boxSprite = sp; UnityEditor.EditorUtility.SetDirty(this); }
        else Debug.LogWarning("[DialogBoxLayout] Sprite tidak ditemukan: Assets/sprites/UI day 1/8.png");
    }

    // ══════════════════════════════════════════════════════════════════════
    // PUSH ke semua consumer di scene yang sedang terbuka — agar saat aset
    // ini diedit di Inspector, semua NpcDialog / Day1Intro yang memakai
    // aset ini langsung ikut update di Edit Mode (tidak perlu Play dulu).
    // ══════════════════════════════════════════════════════════════════════
    void OnValidate()
    {
        // Tunda ke frame berikut: writing ke serialized field di dalam
        // OnValidate dapat memicu OnValidate berulang → stack overflow.
        UnityEditor.EditorApplication.delayCall += PushToConsumers;
    }

    void PushToConsumers()
    {
        if (this == null) return;

        bool anyChanged = false;

        var npcs = Object.FindObjectsByType<NpcDialog>(FindObjectsSortMode.None);
        foreach (var n in npcs)
        {
            if (n != null && n.layout == this)
            {
                n.ApplyLayoutAsset();
                UnityEditor.EditorUtility.SetDirty(n);
                anyChanged = true;
            }
        }

        var intros = Object.FindObjectsByType<Day1Intro>(FindObjectsSortMode.None);
        foreach (var d in intros)
        {
            if (d != null && d.layout == this)
            {
                d.ApplyLayoutAsset();
                UnityEditor.EditorUtility.SetDirty(d);
                anyChanged = true;
            }
        }

        if (anyChanged && !Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
#endif
}
