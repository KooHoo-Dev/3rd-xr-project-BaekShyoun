using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whack_A_Mole;

[InitializeOnLoad]
public static class WhackFlowVerification
{
    static WhackFlowVerification() { EditorApplication.playModeStateChanged += OnState; }
    [MenuItem("Tools/Whack a Mole/Verify Scene Flow and Audio")]
    static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene("Assets/00. Scenes/Title.unity");
        SessionState.SetBool("VerifySceneFlow",true);EditorApplication.isPlaying=true;
    }
    static void OnState(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("VerifySceneFlow",false))return;
        SessionState.SetBool("VerifySceneFlow",false);Verify().Forget();
    }
    static void Check(bool value,string message) { if(!value)throw new Exception("FLOW_FAILED: "+message); }
    static T Find<T>() where T:UnityEngine.Object { return UnityEngine.Object.FindFirstObjectByType<T>(); }
    static Button Button(string name) { return UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name); }
    static async UniTask WaitScene(string name)
    {
        await UniTask.WaitUntil(()=>SceneManager.GetActiveScene().name==name);
        await UniTask.Delay(500,ignoreTimeScale:true);
        Check(Find<OptionsController>()!=null,"options in "+name);
        Check(UnityEngine.Object.FindObjectsByType<SoundManager>(FindObjectsSortMode.None).Length==1,"single sound manager in "+name);
    }
    static async UniTaskVoid Verify()
    {
        string[] keys={"NeonHammer.Records","NeonHammer.BGM","NeonHammer.SFX"};
        bool[] existed=keys.Select(PlayerPrefs.HasKey).ToArray();
        string records=PlayerPrefs.GetString(keys[0],"");float bgm=PlayerPrefs.GetFloat(keys[1],.5f),sfx=PlayerPrefs.GetFloat(keys[2],.8f);
        try
        {
            await WaitScene("Title");
            PlayerPrefs.DeleteKey(keys[0]);
            RecordManager.SaveRecord(500,3);RecordManager.SaveRecord(500,8);RecordManager.SaveRecord(1000,1);
            var list=RecordManager.LoadRecords();Check(list[0].score==1000 && list[1].bestCombo==8,"score then combo sorting");
            for(int i=0;i<12;i++)RecordManager.SaveRecord(i,1);
            Check(RecordManager.LoadRecords().Count==10,"top ten limit");
            PlayerPrefs.DeleteKey(keys[0]);
            Button("Options").onClick.Invoke();Check(Time.timeScale==0,"options pauses time");
            var sliders=UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None);
            sliders.Single(x=>x.name=="BGMVolume").value=.23f;sliders.Single(x=>x.name=="SFXVolume").value=.67f;
            Check(Mathf.Abs(SoundManager.Instance.BGMVolume-.23f)<.001f && Mathf.Abs(SoundManager.Instance.SFXVolume-.67f)<.001f,"independent volumes");
            var sounds=SoundManager.Instance.GetComponents<AudioSource>();
            Check(sounds.Any(x=>Mathf.Abs(x.volume-.23f)<.001f) && sounds.Any(x=>Mathf.Abs(x.volume-.67f)<.001f),"actual audio source volumes");
            Button("Close").onClick.Invoke();Check(Time.timeScale==1,"closing options resumes");
            Button("Records").onClick.Invoke();await WaitScene("Ranking");
            Check(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text=="아직 기록이 없습니다."),"empty ranking");
            Button("BackTitle").onClick.Invoke();await WaitScene("Title");
            Button("Play").onClick.Invoke();await WaitScene("Game");
            Check(Mathf.Abs(SoundManager.Instance.BGMVolume-.23f)<.001f,"volume survives scene load");
            var gm=GameManager.Instance;Check(gm.score==0 && gm.maxHearts==3,"new game reset");
            typeof(GameManager).GetField("canPlay",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(gm,true);
            for(int i=0;i<6;i++)gm.HitMole();gm.MissMole();gm.HitMole();gm.HitMole();
            gm.EndGame();gm.EndGame();
            await WaitScene("Ranking");
            list=RecordManager.LoadRecords();Check(list.Count==1 && list[0].score==800 && list[0].bestCombo==6,"single final record keeps best combo after miss");
            Check(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text.Contains("이번 점수") && t.text.Contains("800")),"result shown");
            Button("Retry").onClick.Invoke();await WaitScene("Game");
            Check(GameManager.Instance.score==0 && GameManager.Instance.maxHearts==3,"retry resets game");
            Find<SceneController>().GoTitle();await WaitScene("Title");
            Debug.Log("SCENE_FLOW_VERIFY_OK: Title/Game/Ranking/retry, top-10 sorting, best combo, duplicate-end guard, options pause, audio volume persistence and single SoundManager passed.");
        }
        catch(Exception e){Debug.LogException(e);}
        finally
        {
            Time.timeScale=1;
            if(existed[0])PlayerPrefs.SetString(keys[0],records);else PlayerPrefs.DeleteKey(keys[0]);
            if(existed[1])PlayerPrefs.SetFloat(keys[1],bgm);else PlayerPrefs.DeleteKey(keys[1]);
            if(existed[2])PlayerPrefs.SetFloat(keys[2],sfx);else PlayerPrefs.DeleteKey(keys[2]);
            PlayerPrefs.Save();EditorApplication.isPlaying=false;
        }
    }
}
