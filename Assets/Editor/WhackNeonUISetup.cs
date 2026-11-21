using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Whack_A_Mole;

// 기존 UI 참조와 버튼 이벤트를 유지하며 편집기에서 디자인만 적용합니다.
[InitializeOnLoad]
public static class WhackNeonUISetup
{
    const string Folder = "Assets/02. UI/";
    static readonly Color Cyan = new Color(0.22f, 0.87f, 1f);
    static Material imageMaterial;
    static WhackNeonUISetup() { EditorApplication.delayCall += ApplyOnce; }
    static void ApplyOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (EditorSceneManager.GetActiveScene().name != "Game") return;
        var ui = UnityEngine.Object.FindFirstObjectByType<UIController>();
        if (ui && !ui.transform.Find("StatusHUD/NeonHeader")) Apply();
    }

    [MenuItem("Tools/Whack a Mole/Apply Neon UI")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var ui = UnityEngine.Object.FindFirstObjectByType<UIController>();
        if (!ui) return;
        var data = new SerializedObject(ui);
        var score = (TMP_Text)data.FindProperty("scoreText").objectReferenceValue;
        var combo = (TMP_Text)data.FindProperty("comboText").objectReferenceValue;
        var announce = (TMP_Text)data.FindProperty("announceText").objectReferenceValue;
        var button = (Button)data.FindProperty("gameStartButton").objectReferenceValue;
        var hud = (RectTransform)ui.transform.Find("StatusHUD");
        var shader = Shader.Find("UI/NoZTest");
        if (!shader || !Shader.Find("TextMeshPro/Mobile/Distance Field Overlay"))
            throw new Exception("UI overlay shaders not found.");
        imageMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Neon UI Overlay.mat");
        if (!imageMaterial) {
            imageMaterial = new Material(shader) { name = "Neon UI Overlay", renderQueue = 4000 };
            AssetDatabase.CreateAsset(imageMaterial, Folder + "Neon UI Overlay.mat");
        }
        Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Neon UI design");
        var canvas = ui.GetComponent<Canvas>();
        if (canvas) { canvas.sortingOrder = 100; canvas.worldCamera = Camera.main; }
        Layout(hud, new Vector2(0, 500), new Vector2(880, 112));
        var bg = hud.GetComponent<Image>();
        bg.color = new Color(.008f, .022f, .055f, .92f);
        bg.material = imageMaterial;
        bg.raycastTarget = false;
        Box("TopRail", hud, new Vector2(0, 56), new Vector2(880, 2), Cyan);
        Box("BottomRail", hud, new Vector2(0, -56), new Vector2(880, 1), new Color(.15f,.55f,.8f,.6f));
        Box("LeftAccent", hud, new Vector2(-440,0), new Vector2(4,112), Cyan);
        Box("RightAccent", hud, new Vector2(440,0), new Vector2(4,112), Cyan);
        Box("DividerLeft", hud, new Vector2(-145,0), new Vector2(1,68), new Color(.13f,.48f,.65f,.7f));
        Box("DividerRight", hud, new Vector2(145,0), new Vector2(1,68), new Color(.13f,.48f,.65f,.7f));
        Label("NeonHeader", hud, new Vector2(0,83), new Vector2(880,30), announce.font, "N E O N   A R C A D E", 19, Cyan);
        Label("ScoreCaption", hud, new Vector2(-290,32), new Vector2(260,25), announce.font, "S C O R E", 16, new Color(.45f,.65f,.8f));
        Label("ComboCaption", hud, new Vector2(290,32), new Vector2(260,25), announce.font, "C O M B O", 16, new Color(.45f,.65f,.8f));
        Label("LifeCaption", hud, new Vector2(0,32), new Vector2(260,25), announce.font, "L I F E", 16, new Color(.45f,.65f,.8f));
        Layout(score.rectTransform, new Vector2(-290,-10), new Vector2(270,60));
        Layout(combo.rectTransform, new Vector2(290,-10), new Vector2(270,60));
        Style(score, 36, new Color(.9f,.97f,1));
        Style(combo, 36, Cyan);
        var hearts = data.FindProperty("heartImages");
        for (int i=0; i<hearts.arraySize; i++) {
            var heart=(Image)hearts.GetArrayElementAtIndex(i).objectReferenceValue;
            Layout(heart.rectTransform,new Vector2((i-(hearts.arraySize-1)*.5f)*62,-10),new Vector2(46,46));
            heart.material=imageMaterial;heart.raycastTarget=false;
        }
        Layout((RectTransform)button.transform, new Vector2(0,-180), new Vector2(300,88));
        var buttonImage=button.GetComponent<Image>();
        buttonImage.sprite=null;buttonImage.material=imageMaterial;buttonImage.color=Color.white;
        var colors=button.colors;
        colors.normalColor=new Color(.025f,.10f,.17f,.97f);
        colors.highlightedColor=new Color(.06f,.30f,.43f,1);
        colors.selectedColor=colors.highlightedColor;
        colors.pressedColor=new Color(.03f,.48f,.6f,1);
        colors.fadeDuration=.12f;button.colors=colors;
        Box("TopRail",button.transform,new Vector2(0,44),new Vector2(300,2),Cyan);
        Box("BottomRail",button.transform,new Vector2(0,-44),new Vector2(300,2),Cyan);
        Box("LeftRail",button.transform,new Vector2(-150,0),new Vector2(2,88),Cyan);
        Box("RightRail",button.transform,new Vector2(150,0),new Vector2(2,88),Cyan);
        var buttonText=button.GetComponentInChildren<TMP_Text>(true);
        buttonText.font=announce.font;buttonText.text="게임 시작";
        Layout(buttonText.rectTransform,new Vector2(0,8),new Vector2(280,50));
        Style(buttonText,32,new Color(.9f,.98f,1));
        Label("StartCaption",button.transform,new Vector2(0,-24),new Vector2(280,22),announce.font,"P L A Y",13,Cyan);
        Layout(announce.rectTransform,new Vector2(0,390),new Vector2(900,50));
        Style(announce,28,new Color(1,.8f,.35f));
        foreach (var text in ui.GetComponentsInChildren<TMP_Text>(true)) Overlay(text);
        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(ui.gameObject.scene);
        Debug.Log("NEON_UI_OK: HUD, start button and warning styled; overlay materials applied; gameplay references preserved.");
    }
    static void Layout(RectTransform rect,Vector2 position,Vector2 size) {
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition3D=new Vector3(position.x,position.y,0);
        rect.sizeDelta=size;rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;
    }
    static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size) {
        var child=parent.Find(name);
        var rect=child ? (RectTransform)child : (RectTransform)new GameObject(name,typeof(RectTransform)).transform;
        rect.SetParent(parent,false);rect.gameObject.layer=LayerMask.NameToLayer("UI");Layout(rect,position,size);return rect;
    }
    static void Box(string name,Transform parent,Vector2 position,Vector2 size,Color color) {
        var rect=Rect(name,parent,position,size);var img=rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
        img.color=color;img.material=imageMaterial;img.raycastTarget=false;
    }
    static void Label(string name,Transform parent,Vector2 position,Vector2 size,TMP_FontAsset font,string value,float fontSize,Color color) {
        var rect=Rect(name,parent,position,size);var label=rect.GetComponent<TextMeshProUGUI>() ?? rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font=font;label.text=value;Style(label,fontSize,color);
    }
    static void Style(TMP_Text text,float size,Color color) {
        text.fontSize=size;text.enableAutoSizing=false;text.alignment=TextAlignmentOptions.Center;
        text.color=color;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.NoWrap;
    }
    static void Overlay(TMP_Text text) {
        // 원본 폰트 재질은 보존하고 해당 폰트의 아틀라스를 쓰는 UI 전용 재질을 생성.
        string path=Folder+"Neon Text "+AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(text.font))+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material) {
            material=new Material(text.font.material) { name="Neon Text Overlay",shader=Shader.Find("TextMeshPro/Mobile/Distance Field Overlay"),renderQueue=4000 };
            material.SetColor("_OutlineColor",new Color(.005f,.015f,.035f,1));
            material.SetFloat("_OutlineWidth",.12f);
            AssetDatabase.CreateAsset(material,path);
        }
        text.fontSharedMaterial=material;
    }
}
