using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Whack_A_Mole;


public static class WhackSceneSetup
{
    const string Folder = "Assets/00. Scenes/";
    const string SoundPath = "Assets/02. UI/SoundManager.prefab";
    static TMP_FontAsset font;
    static Material textMaterial, imageMaterial;
    static readonly Color Cyan = new Color(.22f,.87f,1);

    static void BuildOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Folder + "Title.unity")) return;
        if (EditorSceneManager.GetActiveScene().name != "Game") return;
        Build();
    }
    [MenuItem("Tools/Whack a Mole/Create Title and Ranking Scenes")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        // 기존 씬을 실수로 덮어쓰지 않도록 최초 생성에만 사용.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Folder + "Title.unity")) return;
        var game = EditorSceneManager.GetActiveScene();
        if (game.name != "Game") throw new Exception("Open Game scene first.");
        var ui = UnityEngine.Object.FindFirstObjectByType<UIController>();
        var source = (TMP_Text)new SerializedObject(ui).FindProperty("centerMessageText").objectReferenceValue;
        font = source.font; textMaterial = source.fontSharedMaterial;
        imageMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/02. UI/Neon UI Overlay.mat");
        CreateSoundPrefab();
        PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SoundPath));
        new GameObject("SceneController", typeof(SceneController));
        Options(ui.transform);
        EditorSceneManager.SaveScene(game);
        EditorSceneManager.SaveScene(game, Folder + "Title.unity", true);
        EditorSceneManager.SaveScene(game, Folder + "Ranking.unity", true);
        MakeMenu("Title");
        MakeMenu("Ranking");
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(Folder+"Title.unity",true),
            new EditorBuildSettingsScene(Folder+"Game.unity",true),
            new EditorBuildSettingsScene(Folder+"Ranking.unity",true)
        };
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Folder + "Title.unity");
        Debug.Log("SCENE_FLOW_SETUP_OK: Title/Game/Ranking, shared sound prefab, world-space options and ranking UI saved.");
    }
    static void CreateSoundPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(SoundPath)) return;
        var go = new GameObject("SoundManager");
        var manager = go.AddComponent<SoundManager>();
        var bgm = go.AddComponent<AudioSource>(); bgm.playOnAwake=false; bgm.loop=true; bgm.spatialBlend=0;
        var sfx = go.AddComponent<AudioSource>(); sfx.playOnAwake=false; sfx.spatialBlend=0;
        var data = new SerializedObject(manager);
        data.FindProperty("bgmSource").objectReferenceValue=bgm;
        data.FindProperty("sfxSource").objectReferenceValue=sfx;
        var a=data.FindProperty("bgmSounds");a.arraySize=Enum.GetValues(typeof(BGMType)).Length;
        for(int i=0;i<a.arraySize;i++)a.GetArrayElementAtIndex(i).FindPropertyRelative("type").enumValueIndex=i;
        a=data.FindProperty("sfxSounds");a.arraySize=Enum.GetValues(typeof(SFXType)).Length;
        for(int i=0;i<a.arraySize;i++)a.GetArrayElementAtIndex(i).FindPropertyRelative("type").enumValueIndex=i;
        data.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(go,SoundPath);UnityEngine.Object.DestroyImmediate(go);
    }
    static void MakeMenu(string name)
    {
        var scene=EditorSceneManager.OpenScene(Folder+name+".unity");
        var ui=UnityEngine.Object.FindFirstObjectByType<UIController>();
        var canvas=ui.transform;
        UnityEngine.Object.DestroyImmediate(ui);
        foreach(Transform child in canvas.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
        foreach(var root in scene.GetRootGameObjects())
            if(root.name=="GameSets" || root.name=="GameManager" || root.name=="ReadyGuide")UnityEngine.Object.DestroyImmediate(root);
        foreach(var ripple in UnityEngine.Object.FindObjectsByType<NeonHitRipple>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(ripple);
        var rig=UnityEngine.Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        var locomotion=rig.transform.Find("Locomotion");if(locomotion)locomotion.gameObject.SetActive(false);
        var controller=UnityEngine.Object.FindFirstObjectByType<SceneController>();
        var backdrop=Box("MenuPanel",canvas,new Vector2(0,100),new Vector2(1040,790),new Color(.008f,.022f,.055f,.96f));
        backdrop.raycastTarget=false;
        Box("TopRail",canvas,new Vector2(0,495),new Vector2(1040,3),Cyan).raycastTarget=false;
        Box("BottomRail",canvas,new Vector2(0,-295),new Vector2(1040,2),Cyan).raycastTarget=false;
        if(name=="Title")
        {
            Label("Title",canvas,new Vector2(0,330),new Vector2(960,100),"NEON HAMMER",76);
            Label("Subtitle",canvas,new Vector2(0,230),new Vector2(900,70),"양손으로 두드리는 네온 아케이드",30);
            Button("Play",canvas,new Vector2(0,65),new Vector2(400,95),"게임 시작",controller.GoGame);
            Button("Records",canvas,new Vector2(0,-65),new Vector2(400,80),"기록 보기",controller.GoRanking);
            Label("Guide",canvas,new Vector2(0,-220),new Vector2(960,70),"L / R 망치를 잡고 최고 기록에 도전하세요",25);
        }
        else
        {
            Label("Title",canvas,new Vector2(0,440),new Vector2(960,70),"RANKING  /  TOP 10",46);
            var result=Label("Result",canvas,new Vector2(0,360),new Vector2(960,65),"이번 기록",28);
            var headers=Label("Columns",canvas,new Vector2(0,290),new Vector2(900,44),"순위<pos=15%>점수<pos=48%>최고 콤보<pos=68%>날짜",24);
            headers.alignment=TextAlignmentOptions.MidlineLeft;
            var rows=Label("Records",canvas,new Vector2(0,65),new Vector2(900,400),"아직 기록이 없습니다.",25);
            rows.alignment=TextAlignmentOptions.TopLeft;rows.lineSpacing=8;
            var data=new SerializedObject(controller);data.FindProperty("resultText").objectReferenceValue=result;
            data.FindProperty("rankingText").objectReferenceValue=rows;data.ApplyModifiedPropertiesWithoutUndo();
            Button("Retry",canvas,new Vector2(-240,-230),new Vector2(400,80),"다시 도전하기",controller.GoGame);
            Button("BackTitle",canvas,new Vector2(240,-230),new Vector2(400,80),"타이틀로 돌아가기",controller.GoTitle);
        }
        Options(canvas);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static void Options(Transform canvas)
    {
        if(!canvas.GetComponent<TrackedDeviceGraphicRaycaster>())canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        var root=Rect("OptionsUI",canvas,Vector2.zero,new Vector2(1200,1200));
        var controller=root.gameObject.AddComponent<OptionsController>();
        var panel=Box("OptionsPanel",root,new Vector2(250,210),new Vector2(500,410),new Color(.012f,.045f,.09f,1));
        Label("Heading",panel.transform,new Vector2(0,155),new Vector2(440,60),"SOUND OPTIONS",32);
        Label("BGM",panel.transform,new Vector2(-140,75),new Vector2(140,50),"BGM",27);
        Label("SFX",panel.transform,new Vector2(-140,-40),new Vector2(140,50),"SFX",27);
        var bgmValue=Label("BGMValue",panel.transform,new Vector2(150,75),new Vector2(100,50),"50%",25);
        var sfxValue=Label("SFXValue",panel.transform,new Vector2(150,-40),new Vector2(100,50),"80%",25);
        var bgm=Slider("BGMVolume",panel.transform,new Vector2(0,25),controller.ChangeBGM);
        var sfx=Slider("SFXVolume",panel.transform,new Vector2(0,-90),controller.ChangeSFX);
        Button("Close",panel.transform,new Vector2(0,-155),new Vector2(200,60),"닫기",controller.ToggleOptions);
        // 마지막에 배치하여 패널이 열려도 옵션 버튼은 계속 누를 수 있음.
        Button("Options",root,new Vector2(530,565),new Vector2(130,64),"옵션",controller.ToggleOptions);
        var data=new SerializedObject(controller);
        data.FindProperty("panel").objectReferenceValue=panel.gameObject;
        data.FindProperty("bgmSlider").objectReferenceValue=bgm;data.FindProperty("sfxSlider").objectReferenceValue=sfx;
        data.FindProperty("bgmValue").objectReferenceValue=bgmValue;data.FindProperty("sfxValue").objectReferenceValue=sfxValue;
        data.ApplyModifiedPropertiesWithoutUndo();panel.gameObject.SetActive(false);
    }
    static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
        rect.SetParent(parent,false);rect.gameObject.layer=5;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=position;rect.sizeDelta=size;return rect;
    }
    static Image Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
    {
        var img=Rect(name,parent,pos,size).gameObject.AddComponent<Image>();img.color=color;img.material=imageMaterial;return img;
    }
    static TMP_Text Label(string name,Transform parent,Vector2 pos,Vector2 size,string value,float fontSize)
    {
        var text=Rect(name,parent,pos,size).gameObject.AddComponent<TextMeshProUGUI>();
        text.font=font;text.fontSharedMaterial=textMaterial;text.text=value;text.fontSize=fontSize;
        text.alignment=TextAlignmentOptions.Center;text.color=Cyan;text.raycastTarget=false;
        text.textWrappingMode=TextWrappingModes.NoWrap;return text;
    }
    static Button Button(string name,Transform parent,Vector2 pos,Vector2 size,string caption,UnityAction action)
    {
        var img=Box(name,parent,pos,size,new Color(.025f,.14f,.23f,1));var button=img.gameObject.AddComponent<Button>();button.targetGraphic=img;
        var colors=button.colors;colors.highlightedColor=new Color(.3f,.9f,1);colors.pressedColor=new Color(.1f,.55f,.8f);button.colors=colors;
        var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;
        Label("Label",img.transform,Vector2.zero,size,caption,28);
        Box("Accent",img.transform,new Vector2(0,size.y*.5f),new Vector2(size.x,2),Cyan).raycastTarget=false;
        UnityEventTools.AddPersistentListener(button.onClick,action);return button;
    }
    static Slider Slider(string name,Transform parent,Vector2 pos,UnityAction<float> action)
    {
        var root=Rect(name,parent,pos,new Vector2(400,40));var slider=root.gameObject.AddComponent<Slider>();
        var bg=Box("Track",root,Vector2.zero,new Vector2(400,10),new Color(.1f,.25f,.34f));bg.raycastTarget=false;
        var area=Rect("FillArea",root,Vector2.zero,new Vector2(380,10));
        var fill=Box("Fill",area,Vector2.zero,new Vector2(380,10),Cyan);fill.raycastTarget=false;
        var handleArea=Rect("HandleArea",root,Vector2.zero,new Vector2(380,40));
        var handle=Box("Handle",handleArea,Vector2.zero,new Vector2(26,40),Color.white);
        fill.rectTransform.sizeDelta=Vector2.zero;handle.rectTransform.sizeDelta=new Vector2(26,0);
        var hitArea=root.gameObject.AddComponent<Image>();hitArea.color=Color.clear;
        slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
        slider.minValue=0;slider.maxValue=1;slider.wholeNumbers=false;
        var nav=slider.navigation;nav.mode=Navigation.Mode.None;slider.navigation=nav;
        UnityEventTools.AddPersistentListener(slider.onValueChanged,action);return slider;
    }
}
