#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Utility editor untuk menambah komponen SpriteShadow ke NPC dengan satu klik.
///
/// Cara pakai:
///   1. Pilih satu atau lebih GameObject NPC di Hierarchy
///      (mis. NPC penjual, NPC nenek, bapak penjual, penjual kopi, anak beli).
///   2. Menu: GameObject → 2D → RARA → Add Sprite Shadow
///      ATAU klik kanan di Hierarchy → 2D → RARA → Add Sprite Shadow.
///   3. Untuk setiap NPC terpilih:
///      - Dibuatkan child bernama "Shadow" (kalau belum ada).
///      - Child diberi SpriteRenderer + SpriteShadow.
///      - SpriteShadow.characterRenderer otomatis diset ke parent NPC.
///   4. Pilih child "Shadow" → atur warna, panjang, arah matahari di Inspector.
/// </summary>
public static class AddSpriteShadowMenu
{
    const string MENU_PATH = "GameObject/2D/RARA/Add Sprite Shadow";

    [MenuItem(MENU_PATH, false, 10)]
    static void AddShadowToSelected()
    {
        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Add Sprite Shadow",
                "Pilih dulu satu atau lebih GameObject NPC di Hierarchy.", "OK");
            return;
        }

        int added = 0, skipped = 0;
        foreach (var go in selected)
        {
            if (go == null) continue;

            // Hanya proses GameObject yang punya SpriteRenderer (karakter 2D)
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning($"[AddSpriteShadow] '{go.name}' tidak punya SpriteRenderer — dilewati.");
                skipped++;
                continue;
            }

            // Cek apakah sudah ada child "Shadow"
            Transform existing = go.transform.Find("Shadow");
            if (existing != null && existing.GetComponent<SpriteShadow>() != null)
            {
                Debug.Log($"[AddSpriteShadow] '{go.name}' sudah punya Shadow — dilewati.");
                skipped++;
                continue;
            }

            // Buat child "Shadow"
            var shadowGO = new GameObject("Shadow");
            Undo.RegisterCreatedObjectUndo(shadowGO, "Add Sprite Shadow");
            shadowGO.transform.SetParent(go.transform, false);
            shadowGO.transform.localPosition = Vector3.zero;

            // Komponen: SpriteRenderer + SpriteShadow
            var shadowSR = shadowGO.AddComponent<SpriteRenderer>();
            shadowSR.sprite       = sr.sprite;
            shadowSR.sortingLayerID = sr.sortingLayerID;
            shadowSR.sortingOrder = sr.sortingOrder - 1;

            var shadow = shadowGO.AddComponent<SpriteShadow>();
            shadow.characterRenderer = sr;

            EditorUtility.SetDirty(shadowGO);
            EditorUtility.SetDirty(go);
            added++;
        }

        if (added > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        Debug.Log($"[AddSpriteShadow] Selesai. Ditambahkan: {added}, dilewati: {skipped}.");
    }

    [MenuItem(MENU_PATH, true)]
    static bool AddShadowToSelected_Validate()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }
}
#endif
