using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Whack_A_Mole;

[InitializeOnLoad]
public static class WhackCenterMessageSetup
{
    static WhackCenterMessageSetup() { EditorApplication.delayCall += Setup; }
    static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (EditorSceneManager.GetActiveScene().name != "Game") return;
        var ui = Object.FindFirstObjectByType<UIController>();
        if (!ui) return;
        var data = new SerializedObject(ui);
        if (data.FindProperty("centerMessageText").objectReferenceValue) return;
        var source = (TMP_Text)data.FindProperty("scoreText").objectReferenceValue;
        var go = new GameObject("CenterMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create center message");
        go.layer = LayerMask.NameToLayer("UI");
        var rect = (RectTransform)go.transform;
        rect.SetParent(ui.transform, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
        rect.sizeDelta = new Vector2(1000,180);
        // Canvas가 카메라 아래로 내려간 만큼 보정하여 실제 시야 중앙에 배치.
        var camera = ui.GetComponent<Canvas>().worldCamera;
        if (!camera) camera = Camera.main;
        var local = camera.transform.InverseTransformPoint(rect.position);
        rect.position = camera.transform.TransformPoint(new Vector3(0,0,local.z));
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = source.font;
        text.fontSharedMaterial = source.fontSharedMaterial;
        text.fontSize = 112;
        text.enableAutoSizing = true;
        text.fontSizeMin = 64;
        text.fontSizeMax = 112;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(.22f,.87f,1f);
        text.raycastTarget = false;
        text.text = "3";
        // 짙은 배경 대신 얇은 네온 선으로 중앙 타격 영역을 덜 가림.
        for (int i=0;i<2;i++) {
            var rail = new GameObject(i==0?"UpperRail":"LowerRail",typeof(RectTransform),typeof(Image));
            rail.layer=go.layer;
            var rr=(RectTransform)rail.transform;rr.SetParent(rect,false);
            rr.anchorMin=rr.anchorMax=rr.pivot=new Vector2(.5f,.5f);
            rr.anchoredPosition=new Vector2(0,i==0?100:-100);rr.sizeDelta=new Vector2(540,2);
            var img=rail.GetComponent<Image>();img.color=new Color(.22f,.87f,1f,.65f);
            img.material=AssetDatabase.LoadAssetAtPath<Material>("Assets/02. UI/Neon UI Overlay.mat");
            img.raycastTarget=false;
        }
        data.FindProperty("centerMessageText").objectReferenceValue=text;
        data.ApplyModifiedProperties();go.SetActive(false);
        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        EditorSceneManager.SaveScene(ui.gameObject.scene);
        Debug.Log("CENTER_MESSAGE_SETUP_OK");

    }
}

// 새 시작/종료 흐름을 실제 시간 경과와 함께 검증. 편집기 전용.
[InitializeOnLoad]
public static class WhackCenterMessageVerification
{
    [MenuItem("Tools/Whack a Mole/Verify Countdown and Game Over")]
    static void Run() {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        WhackReadyVerification.Run();
    }
    static int stage;
    static float started;
    static TMP_Text text;
    static WhackCenterMessageVerification() { EditorApplication.playModeStateChanged += OnState; }
    static void OnState(PlayModeStateChange state) {
        if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("WhackCenterVerify",false)) {
            SessionState.SetBool("WhackCenterVerify",false);stage=0;
            EditorApplication.update += Verify;
        }
        if(state==PlayModeStateChange.ExitingPlayMode)EditorApplication.update -= Verify;
    }
    static void Check(bool value,string label) { if(!value)throw new System.Exception("CENTER_VERIFY_FAILED: "+label); }
    static void Verify() {
        if(!EditorApplication.isPlaying || !GameManager.Instance || !UIController.Instance)return;
        try {
            var gm=GameManager.Instance;var ui=UIController.Instance;
            if(stage==0) {
                text=(TMP_Text)new SerializedObject(ui).FindProperty("centerMessageText").objectReferenceValue;
                Check(!text.gameObject.activeSelf,"initially hidden");
                ui.PressGameStart();ui.PressGameStart();
                Check(text.text=="3","initial countdown");
                int hearts=gm.maxHearts;gm.MissMole();gm.HitMole();
                Check(gm.maxHearts==hearts && gm.score==0,"countdown ignores hits and misses");
                started=Time.time;stage=1;
            }
            float elapsed=Time.time-started;
            if(stage==1 && elapsed>1.15f){Check(text.text=="2","second count");stage=2;}
            if(stage==2 && elapsed>2.15f){Check(text.text=="1","third count");stage=3;}
            if(stage==3 && elapsed>3.15f){Check(text.text=="Game Start!","start message");stage=4;}
            if(stage==4 && elapsed>4.05f){
                Check(!text.gameObject.activeSelf,"start message clears");
                gm.HitMole();Check(gm.score==100,"game enabled");
                int remaining=gm.maxHearts; for(int i=0;i<remaining;i++)gm.MissMole();
                Check(text.text=="Game Over!" && text.gameObject.activeSelf,"shared text shows game over");
                gm.MissMole();gm.HitMole();
                Check(gm.maxHearts==0 && gm.score==100,"game over freezes counters");
                foreach(var mole in Object.FindObjectsByType<MoleController>(FindObjectsSortMode.None))Check(mole.IsHide,"game over hides targets");
                stage=5;
            }
            if(stage==5 && elapsed>7.5f){
                Check(text.text=="Game Over!" && text.gameObject.activeSelf,"game over persists");
                Check(gm.maxHearts==0 && gm.score==100,"old timers cannot change result");
                Debug.Log("CENTER_VERIFY_OK: 3/2/1/start, duplicate start guard, pregame gate, shared game-over text, stopped counters and targets verified.");
                EditorApplication.update-=Verify;EditorApplication.isPlaying=false;
            }
        } catch(System.Exception e){Debug.LogException(e);EditorApplication.update-=Verify;EditorApplication.isPlaying=false;}
    }
}



