using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whack_A_Mole;

[InitializeOnLoad]
public static class WhackArrowLabelsSetup
{
    static WhackArrowLabelsSetup() { EditorApplication.delayCall += Apply; }
    [MenuItem("Tools/Whack a Mole/Setup Arrow Labels")]
    static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        var ready = Object.FindFirstObjectByType<GameReadyController>();
        var ui = Object.FindFirstObjectByType<UIController>();
        if (!ready || !ui) return;
        var source = (TMP_Text)new SerializedObject(ui).FindProperty("centerMessageText").objectReferenceValue;
        var data = new SerializedObject(ready);
        bool changed = false;
        foreach (var side in new[] { "left", "right" })
        {
            var arrow = (LineRenderer)data.FindProperty(side + "Arrow").objectReferenceValue;
            if (arrow.transform.Find("HandLabel")) continue;
            var go = new GameObject("HandLabel");
            Undo.RegisterCreatedObjectUndo(go, "Add hammer hand label");
            go.transform.SetParent(arrow.transform, false);
            var text = go.AddComponent<TextMeshPro>();
            text.font = source.font;
            text.text = side == "left" ? "L" : "R";
            text.fontSize = 2;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(.15f, .85f, 1);
            text.rectTransform.sizeDelta = new Vector2(.4f, .3f);
            text.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            changed = true;
        }
        if (!changed) return;
        EditorSceneManager.MarkSceneDirty(ready.gameObject.scene);
        EditorSceneManager.SaveScene(ready.gameObject.scene);
        Debug.Log("ARROW_LABELS_OK: L and R labels saved under matching arrows.");
    }
}
