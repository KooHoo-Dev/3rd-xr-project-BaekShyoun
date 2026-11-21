using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 기존에 생성된 장식 발판의 CapsuleCollider만 비활성화합니다.
[InitializeOnLoad]
public static class WhackPlatformColliderFix
{
    static WhackPlatformColliderFix()
    {
        EditorApplication.delayCall += Apply;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Apply;
        };
    }
    private static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Game") return;
        var platform = GameObject.Find("NeonEnvironment/Black Circular Platform");
        if (!platform || !platform.TryGetComponent<CapsuleCollider>(out var capsule) || !capsule.enabled) return;
        Debug.Log("PLATFORM_COLLIDER_BEFORE: world bounds=" + capsule.bounds);
        Undo.RecordObject(capsule, "Disable decorative platform collision");
        capsule.enabled = false;
        Physics.SyncTransforms();
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(capsule);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        var plane = GameObject.Find("Plane");
        var floor = plane ? plane.GetComponent<Collider>() : null;
        Debug.Log("PLATFORM_COLLIDER_FIXED: decorative collider disabled; original floor enabled=" + (floor && floor.enabled));
    }
}
