using System;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Whack_A_Mole;

// 편집기에서 HUD를 구성하는 도구. 빌드에는 포함되지 않음.
public static class WhackHUDSetup
{
    [MenuItem("Tools/Whack a Mole/Build Status HUD")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        var ui = UnityEngine.Object.FindFirstObjectByType<UIController>();
        var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (ui == null || gm == null) throw new InvalidOperationException("Open the Game scene first.");
        const string path = "Assets/02. UI/Heart.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 256;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        var data = new SerializedObject(ui);
        var source = (TMP_Text)data.FindProperty("announceText").objectReferenceValue;
        if (ui.transform.Find("StatusHUD") != null)
            throw new InvalidOperationException("StatusHUD already exists; adjust its objects directly.");
        var panel = Rect("StatusHUD", ui.transform, Vector2.zero, new Vector2(880, 100));
        panel.anchoredPosition = new Vector2(0, 500);
        var background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.035f, 0.05f, 0.08f, 0.88f);
        background.raycastTarget = false;
        var score = Label("ScoreText", panel, new Vector2(-285, 0), source.font, "점수 0");
        var combo = Label("ComboText", panel, new Vector2(285, 0), source.font, "콤보 0");
        var hearts = new Image[Mathf.Max(1, gm.maxHearts)];
        for (int i = 0; i < hearts.Length; i++)
        {
            var rect = Rect("Heart" + (i + 1), panel,
                new Vector2((i - (hearts.Length - 1) * 0.5f) * 62, 0), new Vector2(56, 56));
            var heart = rect.gameObject.AddComponent<Image>();
            heart.sprite = sprite;
            heart.preserveAspect = true;
            heart.raycastTarget = false;
            hearts[i] = heart;
        }
        data.FindProperty("scoreText").objectReferenceValue = score;
        data.FindProperty("comboText").objectReferenceValue = combo;
        var array = data.FindProperty("heartImages");
        array.arraySize = hearts.Length;
        for (int i = 0; i < hearts.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
        data.ApplyModifiedProperties();
        ui.RefreshHUD(gm.score, 0, gm.maxHearts);
        EditorUtility.SetDirty(ui);
        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        EditorSceneManager.SaveScene(ui.gameObject.scene);
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("HUD_SETUP_OK: score, combo, heart sprites linked and Game scene saved.");
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        Undo.RegisterCreatedObjectUndo(go, "Create status HUD");
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TMP_Text Label(string name, Transform parent, Vector2 position, TMP_FontAsset font, string text)
    {
        var rect = Rect(name, parent, position, new Vector2(270, 76));
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.fontSize = 34;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        label.text = text;
        return label;
    }

    [MenuItem("Tools/Whack a Mole/Verify Status HUD (Play Mode)")]
    public static void Verify()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first.");
        var gm = GameManager.Instance;
        var ui = UIController.Instance;
        var data = new SerializedObject(ui);
        var score = (TMP_Text)data.FindProperty("scoreText").objectReferenceValue;
        var combo = (TMP_Text)data.FindProperty("comboText").objectReferenceValue;
        var array = data.FindProperty("heartImages");
        var hearts = new Image[array.arraySize];
        for (int i = 0; i < hearts.Length; i++) hearts[i] = (Image)array.GetArrayElementAtIndex(i).objectReferenceValue;
        var comboField = typeof(GameManager).GetField("combo", BindingFlags.NonPublic | BindingFlags.Instance);
        var timeField = typeof(GameManager).GetField("currentSpawnTime", BindingFlags.NonPublic | BindingFlags.Instance);
        int oldScore = gm.score, oldHearts = gm.maxHearts;
        var oldCombo = comboField.GetValue(gm);
        var oldTime = timeField.GetValue(gm);
        try
        {
            gm.score = 0;
            gm.maxHearts = 3;
            comboField.SetValue(gm, 0);
            for (int i = 0; i < 5; i++) gm.HitMole();
            if (score.text != "점수 500" || combo.text != "콤보 5") throw new Exception("Hit HUD values mismatch.");
            gm.MissMole();
            if (combo.text != "콤보 0" || score.text != "점수 500" || hearts[2].color == Color.white || hearts[0].color != Color.white)
                throw new Exception("Miss HUD values mismatch.");
            ui.RefreshHUD(500, 0, -2);
            foreach (var heart in hearts) if (heart.color == Color.white) throw new Exception("Negative-heart display mismatch.");
            Debug.Log("HUD_VERIFY_OK: 5 hits = score 500 / combo 5; miss = combo 0 / two hearts; negative hearts display empty. State restored.");
        }
        finally
        {
            gm.score = oldScore;
            gm.maxHearts = oldHearts;
            comboField.SetValue(gm, oldCombo);
            timeField.SetValue(gm, oldTime);
            ui.RefreshHUD(oldScore, (int)oldCombo, oldHearts);
        }
    }
}