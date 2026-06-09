#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Day2SetupMenu — Setup UI Day 2 OTOMATIS dengan satu klik dari menu Editor.
///
/// Membuat GameObject "[Day2Preset]" + komponen Day2Preset di scene aktif.
/// Saat Play, Day2Preset.Bootstrap() akan otomatis membangun seluruh UI Day 2:
///   DayTransitionManager, Day2_Root + 8 fase (Day2Controller, HalteDialog,
///   AngkotSeatPicker, ZonaTubuhQuiz, ChatSimWhatsApp, LaporTeriakButton,
///   EduCardDay2, Day2SummaryScreen), lalu di-nonaktifkan supaya tidak ganggu Day 1.
///
/// Setelah setup, GameObject Day2Controller dipilih otomatis supaya kamu bisa
/// langsung mengisi/menambahkan sprite latar per fase di Inspector
/// (header "Background Sprite Per Fase").
///
/// Cara pakai:
///   Menu: GameObject → 2D → RARA → Setup Day 2 (Otomatis)
///   ATAU klik kanan di Hierarchy → 2D → RARA → Setup Day 2 (Otomatis)
/// </summary>
public static class Day2SetupMenu
{
    const string MENU_PATH = "GameObject/2D/RARA/Setup Day 2 (Otomatis)";

    [MenuItem(MENU_PATH, false, 11)]
    static void SetupDay2()
    {
        // 1. Cari Day2Preset yang sudah ada (jangan buat dobel).
        var preset = Object.FindFirstObjectByType<Day2Preset>(FindObjectsInactive.Include);
        if (preset == null)
        {
            var go = new GameObject("[Day2Preset]");
            Undo.RegisterCreatedObjectUndo(go, "Setup Day 2");
            preset = go.AddComponent<Day2Preset>();
            preset.autoRunSaatAwake = true;
            preset.autoDiscoverDay1 = true;
            preset.autoBuildDay2    = true;
            Debug.Log("[Day2Setup] [Day2Preset] dibuat. UI Day 2 akan dibangun otomatis saat Play.");
        }
        else
        {
            Debug.Log("[Day2Setup] [Day2Preset] sudah ada di scene — tidak dibuat ulang.");
        }

        // 2. Cari Day2Controller yang mungkin sudah ada untuk dipilih (biar gampang isi sprite).
        var controller = Object.FindFirstObjectByType<Day2Controller>(FindObjectsInactive.Include);
        if (controller != null)
        {
            Selection.activeGameObject = controller.gameObject;
            EditorGUIUtility.PingObject(controller.gameObject);
        }
        else
        {
            Selection.activeGameObject = preset.gameObject;
            EditorGUIUtility.PingObject(preset.gameObject);
        }

        // 3. Tandai scene berubah supaya bisa di-save.
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Setup Day 2",
            "Setup Day 2 selesai.\n\n" +
            "\u2022 [Day2Preset] siap \u2014 UI Day 2 dibangun otomatis saat klik Play.\n" +
            "\u2022 Untuk MENAMBAH SPRITE: pilih Day2Controller, buka header\n" +
            "  \"Background Sprite Per Fase\" di Inspector, lalu drag sprite-mu\n" +
            "  (Intro/Halte/Angkot/Quiz/ChatSim/Lapor/EduCard).\n\n" +
            "Kalau Day2Controller belum muncul, klik Play sekali supaya komponen\n" +
            "fase dibuat, lalu Stop dan isi sprite-nya.", "OK");
    }
}
#endif
