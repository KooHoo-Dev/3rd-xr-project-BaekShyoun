using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whack_A_Mole;

// 현재 Game 씬에만 파동 참조를 연결. 구매 에셋에는 접근하지 않음.
[InitializeOnLoad]
public static class WhackRippleSetup
{
    static WhackRippleSetup() { EditorApplication.delayCall += ApplyOnce; }
    private static void ApplyOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Game") return;
        var root = GameObject.Find("NeonEnvironment");
        if (!root || root.GetComponent<NeonHitRipple>()) return;
        const string path = "Assets/03. Environment/";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path + "Neon Ring.asset");
        var material = AssetDatabase.LoadAssetAtPath<Material>(path + "Blue Neon Rim.mat");
        var floor = AssetDatabase.LoadAssetAtPath<Material>(path + "Platform.mat");
        if (!mesh || !material || !floor) { Debug.LogError("Ripple setup missing environment assets."); return; }
        var ripple = Undo.AddComponent<NeonHitRipple>(root);
        var data = new SerializedObject(ripple);
        data.FindProperty("ringMesh").objectReferenceValue = mesh;
        data.FindProperty("ringMaterial").objectReferenceValue = material;
        data.ApplyModifiedProperties();
        Undo.RecordObject(floor, "Darken platform");
        floor.SetColor("_BaseColor", new Color(.012f,.016f,.023f,1));
        floor.SetColor("_Color", new Color(.012f,.016f,.023f,1));
        EditorUtility.SetDirty(floor);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("RIPPLE_SETUP_OK: hit event connected, floor darkened, scene saved.");
    }

    [MenuItem("Tools/Whack a Mole/Preview Hit Wave (Play Mode)")]
    public static void Preview()
    {
        var ripple = Object.FindFirstObjectByType<NeonHitRipple>();
        if (!EditorApplication.isPlaying || !ripple) return;
        ripple.Emit();
        float pauseAt = Time.time + .10f;
        void PausePreview()
        {
            if (!EditorApplication.isPlaying) { EditorApplication.update -= PausePreview; return; }
            if (Time.time < pauseAt) return;
            EditorApplication.update -= PausePreview;
            EditorApplication.isPaused = true;
            Debug.Log("RIPPLE_PREVIEW_OK: paused for visual inspection; press Pause to resume.");
        }
        EditorApplication.update += PausePreview;
    }
}
