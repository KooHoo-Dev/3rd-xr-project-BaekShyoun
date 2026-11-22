using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Whack_A_Mole;

[InitializeOnLoad]
public static class WhackReadyVerification
{
    static int stage;
    static float time;
    static float startHeight;
    static TMP_Text text;
    static XRGrabInteractable left,right;
    static WhackTestHand leftHand,rightHand;
    static Transform origin,point;
    static GameObject locomotion,circle,leftArrow,rightArrow;
    static WhackReadyVerification(){EditorApplication.playModeStateChanged+=OnState;}
    [MenuItem("Tools/Whack a Mole/Verify Ready Flow")]
    public static void Run(){
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        SessionState.SetBool("ReadyVerifyPending",true);EditorApplication.isPlaying=true;
    }
    static void OnState(PlayModeStateChange state){
        if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("ReadyVerifyPending",false)){
            SessionState.SetBool("ReadyVerifyPending",false);stage=0;EditorApplication.update+=Tick;
        }
        if(state==PlayModeStateChange.ExitingPlayMode)EditorApplication.update-=Tick;
    }
    static void Check(bool ok,string message){if(!ok)throw new Exception("READY_VERIFY_FAILED: "+message);}
    static WhackTestHand Hand(string name,InteractorHandedness side){
        var go=new GameObject(name);var collider=go.AddComponent<SphereCollider>();collider.isTrigger=true;collider.radius=.01f;
        var rb=go.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
        var hand=go.AddComponent<WhackTestHand>();hand.handedness=side;return hand;
    }
    static void MoveHead(Vector3 target){var delta=target-Camera.main.transform.position;delta.y=0;origin.position+=delta;}
    static void Next(int value){stage=value;time=Time.time;}
    static void Tick(){
        if (!EditorApplication.isPlaying) return;
        if (stage == 11 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Ranking")
        {
            Debug.Log("READY_VERIFY_OK: grab lock, countdown, game over and Ranking scene transition passed.");
            EditorApplication.update -= Tick; EditorApplication.isPlaying = false; return;
        }
        if (!UIController.Instance || !GameManager.Instance) return;
        try{
            var ui=UIController.Instance;var gm=GameManager.Instance;
            if(stage==0){
                var ready=UnityEngine.Object.FindFirstObjectByType<GameReadyController>();var d=new SerializedObject(ready);
                left=(XRGrabInteractable)d.FindProperty("leftHammer").objectReferenceValue;
                right=(XRGrabInteractable)d.FindProperty("rightHammer").objectReferenceValue;
                point=(Transform)d.FindProperty("readyPoint").objectReferenceValue;
                circle=(GameObject)d.FindProperty("readyCircle").objectReferenceValue;
                locomotion=(GameObject)d.FindProperty("locomotion").objectReferenceValue;
                leftArrow=((LineRenderer)d.FindProperty("leftArrow").objectReferenceValue).gameObject;
                rightArrow=((LineRenderer)d.FindProperty("rightArrow").objectReferenceValue).gameObject;
                text=(TMP_Text)new SerializedObject(ui).FindProperty("centerMessageText").objectReferenceValue;
                origin=Camera.main.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>().transform;
                MoveHead(point.position+Vector3.back*1.5f);
                leftHand=Hand("__LeftTestHand",InteractorHandedness.Left);rightHand=Hand("__RightTestHand",InteractorHandedness.Right);
                startHeight=origin.position.y;
                ui.PressGameStart();ui.PressGameStart();gm.HitMole();gm.MissMole();Next(1);return;
            }
            float elapsed=Time.time-time;
            if(elapsed<.25f)return;
            if(stage==1){
                if(elapsed<5f)return;
                Check(Mathf.Abs(origin.position.y-startHeight)<.05f,"player height stable for 5 seconds after start");
                Check(text.text=="망치를 잡으세요" && leftArrow.activeSelf && rightArrow.activeSelf && !circle.activeSelf,"initial pick-up guide");
                Check(gm.score==0 && gm.maxHearts==3,"preparation blocks score and miss");
                leftHand.pressed=true;leftHand.interactionManager.SelectEnter(leftHand,(IXRSelectInteractable)left);Next(2);return;
            }
            if(stage==2){
                Check(left.isSelected && !right.isSelected && !leftArrow.activeSelf && rightArrow.activeSelf && !circle.activeSelf,"one hammer waits");
                rightHand.pressed=true;rightHand.interactionManager.SelectEnter(rightHand,(IXRSelectInteractable)right);Next(3);return;
            }
            if(stage==3){
                Check(circle.activeSelf && text.text=="게임기 앞으로 이동하세요" && !leftArrow.activeSelf && !rightArrow.activeSelf,"both hands enable destination");
                leftHand.pressed=false;rightHand.pressed=false;Next(4);return;
            }
            if(stage==4){
                Check(left.isSelected && right.isSelected && circle.activeSelf && leftHand.isPerformingManualInteraction && rightHand.isPerformingManualInteraction,"released grips stay locked after both hammers grabbed");
                rightHand.pressed=true;rightHand.interactionManager.SelectEnter(rightHand,(IXRSelectInteractable)right);
                MoveHead(point.position+Vector3.back*.5f);Next(5);return;
            }
            if(stage==5){
                Check(circle.activeSelf && locomotion.activeSelf && text.text=="게임기 앞으로 이동하세요","outside circle stays unlocked");
                MoveHead(point.position);Next(6);return;
            }
            if(stage==6){
                Check(!locomotion.activeSelf && !circle.activeSelf && text.text=="3","entry locks movement and begins countdown");
                Check(Camera.main.enabled && left.isSelected && right.isSelected,"head and hands still active");
                Next(7);return;
            }
            if(stage==7 && elapsed>1f){Check(text.text=="2","count 2");stage=8;}
            if(stage==8 && elapsed>2f){Check(text.text=="1","count 1");stage=9;}
            if(stage==9 && elapsed>3f){Check(text.text=="Game Start!","start message");stage=10;}
            if(stage==10 && elapsed>4f){
                Check(!text.gameObject.activeSelf,"message hides at play");
                gm.HitMole();Check(gm.score==100,"play scores");
                int remaining=gm.maxHearts;for(int i=0;i<remaining;i++)gm.MissMole();
                Check(text.text=="Game Over!" && text.gameObject.activeSelf,"game over retained");
                Check(!left.isSelected && !right.isSelected && !leftHand.isPerformingManualInteraction && !rightHand.isPerformingManualInteraction,"game over releases both hammer locks");
                gm.MissMole();gm.HitMole();Check(gm.maxHearts==0 && gm.score==100,"game over freezes results");
                Next(11);return;
            }
            if(stage==11 && elapsed>3.5f){
                Check(gm.maxHearts==0 && gm.score==100,"old timers remain stopped");
                Debug.Log("READY_VERIFY_OK: real XRI grabs, one/both hand conditions, grip release held by lock, game over unlock, outside/inside circle, locomotion lock, countdown and game over passed.");
                EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
            }
        }catch(Exception e){Debug.LogException(e);EditorApplication.update-=Tick;EditorApplication.isPlaying=false;}
    }
}


public class WhackTestHand : XRDirectInteractor
{
    public bool pressed;
    public override bool isSelectActive => pressed || base.isSelectActive;
}
