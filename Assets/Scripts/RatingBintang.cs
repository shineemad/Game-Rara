using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RatingBintang — Sistem rating bintang 1–3 ala game umumnya.
///
/// Dipakai di layar selesai Hari 1, Hari 2, dan hasil akhir Hari 3 untuk
/// memberi "feel" game: makin tinggi skor relatif target, makin banyak bintang.
///
/// Bintang digambar secara prosedural (tidak butuh sprite/font) sehingga aman
/// dari masalah glyph font yang tidak punya karakter bintang.
///
/// Cara pakai:
///   int b = RatingBintang.HitungBintang(skor, target);     // 1..3
///   RatingBintang.Bangun(parentTransform, b,
///       new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f),
///       new Vector2(0f, -160f), 48f, 14f, this);
/// </summary>
public static class RatingBintang
{
    // Cache sprite bintang supaya tidak generate ulang tiap layar.
    static Sprite _spriteBintang;

    // Warna default bintang.
    static readonly Color WarnaTerisi  = new Color(1f,   0.83f, 0.22f, 1f);   // emas
    static readonly Color WarnaKosong  = new Color(0.28f, 0.26f, 0.20f, 1f);  // abu gelap

    // ══════════════════════════════════════════════════════════════════════
    // PERHITUNGAN
    // ══════════════════════════════════════════════════════════════════════

    /// Hitung jumlah bintang (1..3) dari skor relatif target.
    /// >=100% target = 3, >=60% = 2, sisanya = 1 (minimal 1 karena menyelesaikan hari).
    public static int HitungBintang(int skor, int target)
    {
        float rasio = target <= 0 ? 1f : (float)skor / target;
        if (rasio >= 1f)   return 3;
        if (rasio >= 0.6f) return 2;
        return 1;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PEMBANGUN UI
    // ══════════════════════════════════════════════════════════════════════

    /// Bangun satu baris 3 bintang (terisi + kosong) di bawah <paramref name="parent"/>.
    /// <paramref name="animHost"/> opsional: bila diberikan, bintang muncul satu per satu
    /// dengan efek pop + SFX.
    public static GameObject Bangun(Transform parent, int jumlahTerisi,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos,
        float ukuranBintang = 48f, float jarak = 14f, MonoBehaviour animHost = null)
    {
        jumlahTerisi = Mathf.Clamp(jumlahTerisi, 0, 3);

        var holder = new GameObject("BarisBintang");
        holder.transform.SetParent(parent, false);
        var hRT = holder.AddComponent<RectTransform>();
        hRT.anchorMin = anchorMin;
        hRT.anchorMax = anchorMax;
        hRT.pivot     = pivot;
        hRT.sizeDelta = new Vector2(ukuranBintang * 3f + jarak * 2f, ukuranBintang);
        hRT.anchoredPosition = anchoredPos;

        var transforms = new RectTransform[3];
        float totalW = ukuranBintang * 3f + jarak * 2f;
        float startX = -totalW * 0.5f + ukuranBintang * 0.5f;

        for (int i = 0; i < 3; i++)
        {
            bool terisi = i < jumlahTerisi;
            var b = new GameObject("Bintang" + i);
            b.transform.SetParent(holder.transform, false);
            var img = b.AddComponent<Image>();
            img.sprite        = GetSpriteBintang();
            img.color         = terisi ? WarnaTerisi : WarnaKosong;
            img.raycastTarget = false;
            var rt = b.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(ukuranBintang, ukuranBintang);
            rt.anchoredPosition = new Vector2(startX + i * (ukuranBintang + jarak), 0f);

            // Bintang terisi sedikit lebih besar + glow halus.
            if (terisi)
            {
                var sh = b.AddComponent<Shadow>();
                sh.effectColor    = new Color(1f, 0.65f, 0.10f, 0.55f);
                sh.effectDistance = new Vector2(0f, -2f);
            }
            transforms[i] = rt;
        }

        if (animHost != null && animHost.isActiveAndEnabled)
            animHost.StartCoroutine(AnimasiPop(transforms, jumlahTerisi));

        return holder;
    }

    static IEnumerator AnimasiPop(RectTransform[] bintang, int jumlahTerisi)
    {
        // Sembunyikan dulu lalu munculkan satu per satu dengan overshoot.
        for (int i = 0; i < bintang.Length; i++)
            if (bintang[i] != null) bintang[i].localScale = Vector3.zero;

        yield return new WaitForSecondsRealtime(0.15f);

        for (int i = 0; i < bintang.Length; i++)
        {
            if (bintang[i] == null) continue;
            // SFX hanya untuk bintang yang terisi.
            if (i < jumlahTerisi && AudioManager.Instance != null)
                AudioManager.Instance.Correct();

            float t = 0f;
            const float dur = 0.22f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / dur);
                // Overshoot: 0 → 1.25 → 1.
                float s = p < 0.6f
                    ? Mathf.Lerp(0f, 1.25f, p / 0.6f)
                    : Mathf.Lerp(1.25f, 1f, (p - 0.6f) / 0.4f);
                if (bintang[i] != null) bintang[i].localScale = Vector3.one * s;
                yield return null;
            }
            if (bintang[i] != null) bintang[i].localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(0.10f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // SPRITE BINTANG PROSEDURAL
    // ══════════════════════════════════════════════════════════════════════

    static Sprite GetSpriteBintang()
    {
        if (_spriteBintang != null) return _spriteBintang;

        const int N = 128;
        const float outerR = 0.96f;
        const float innerR = 0.42f;

        // 10 titik bintang (selang-seling jari-jari luar & dalam), ujung tunggal di ATAS.
        // Sumbu Y dibalik (-Sin) supaya titik luar pertama (i=0) berada di atas —
        // tanpa ini bintang tergambar terbalik (ujung menghadap ke bawah, mirip panah).
        var titik = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float sudut = Mathf.Deg2Rad * (-90f + i * 36f);
            float r = (i % 2 == 0) ? outerR : innerR;
            titik[i] = new Vector2(Mathf.Cos(sudut) * r, -Mathf.Sin(sudut) * r);
        }

        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var px  = new Color32[N * N];
        const int SS = 2; // supersample 2x2 untuk tepi lebih halus

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                {
                    for (int sx = 0; sx < SS; sx++)
                    {
                        float fx = (x + (sx + 0.5f) / SS) / N * 2f - 1f;
                        float fy = (y + (sy + 0.5f) / SS) / N * 2f - 1f;
                        if (DiDalamPoligon(fx, fy, titik)) hit++;
                    }
                }
                float a = hit / (float)(SS * SS);
                px[y * N + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        _spriteBintang = Sprite.Create(tex, new Rect(0, 0, N, N),
            new Vector2(0.5f, 0.5f), 100f);
        return _spriteBintang;
    }

    static bool DiDalamPoligon(float px, float py, Vector2[] poly)
    {
        bool dalam = false;
        int n = poly.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((poly[i].y > py) != (poly[j].y > py)) &&
                (px < (poly[j].x - poly[i].x) * (py - poly[i].y) /
                       (poly[j].y - poly[i].y) + poly[i].x))
            {
                dalam = !dalam;
            }
        }
        return dalam;
    }
}
