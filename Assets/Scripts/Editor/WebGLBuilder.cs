using UnityEditor;
using UnityEngine;

// Skrip build WebGL untuk dipanggil via batch mode (-executeMethod WebGLBuilder.Build).
public static class WebGLBuilder
{
    public static void Build()
    {
        string[] scenes = { "Assets/Scenes/Gameplay.unity" };
        string output = "Build/WebGL";
        var opt = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opt);
        Debug.Log("[WebGLBuilder] Hasil build: " + report.summary.result);
    }
}
