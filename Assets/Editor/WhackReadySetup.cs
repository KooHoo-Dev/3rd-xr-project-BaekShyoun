using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Whack_A_Mole;

// 씬 참조와 안내 도형을 구성하는 편집기 도구. 게임 빌드에는 포함되지 않음.
[InitializeOnLoad]
public static class WhackReadySetup
{
    static WhackReadySetup() { EditorApplication.delayCall += Setup; }
    [MenuItem("Tools/Whack a Mole/Setup Ready Guide")]
    static void Setup()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)return;
        if(EditorSceneManager.GetActiveScene().name!="Game")return;
        var gm=UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if(!gm)return;
        var data=new SerializedObject(gm);
        if(data.FindProperty("gameReadyController").objectReferenceValue)return;
        var hammers=UnityEngine.Object.FindObjectsByType<HammerController>(FindObjectsSortMode.None);
        var left=hammers.Single(h=>h.hammerSide==HandSide.Left).GetComponent<XRGrabInteractable>();
        var right=hammers.Single(h=>h.hammerSide==HandSide.Right).GetComponent<XRGrabInteractable>();
        var camera=Camera.main;
        var origin=camera.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>();
        var locomotion=origin.transform.Find("Locomotion");
        if(!locomotion)throw new Exception("XR Origin Locomotion not found.");
        foreach(var provider in origin.GetComponentsInChildren<LocomotionProvider>(true))
            if(!provider.transform.IsChildOf(locomotion))throw new Exception("Provider outside Locomotion: "+provider.name);
        foreach(var body in origin.GetComponentsInChildren<XRBodyTransformer>(true))
            if(!body.transform.IsChildOf(locomotion))throw new Exception("Body transformer outside Locomotion: "+body.name);
        var root=new GameObject("ReadyGuide");Undo.RegisterCreatedObjectUndo(root,"Create ready guide");
        var ready=root.AddComponent<GameReadyController>();
        var leftMachine=(MachineController)data.FindProperty("leftMachine").objectReferenceValue;
        var rightMachine=(MachineController)data.FindProperty("rightMachine").objectReferenceValue;
        var point=new GameObject("ReadyCircle");point.transform.SetParent(root.transform,false);
        var position=(leftMachine.transform.position+rightMachine.transform.position)*.5f+leftMachine.transform.forward*1.15f;
        position.y=.025f;point.transform.position=position;
        const string path="Assets/02. UI/Ready Guide Neon.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material){material=new Material(Shader.Find("UI/NoZTest")){renderQueue=3500};AssetDatabase.CreateAsset(material,path);}
        for(int i=0;i<3;i++) {
            float width=i==0?.085f:i==1?.04f:.014f;
            var line=MakeLine("NeonRing"+i,point.transform,material,width,96,false);
            line.loop=true;line.startColor=line.endColor=new Color(.15f,.85f,1f,i==0?.12f:i==1?.35f:1f);
            for(int n=0;n<96;n++){float angle=n*Mathf.PI*2/96;line.SetPosition(n,new Vector3(Mathf.Cos(angle)*.3f,0,Mathf.Sin(angle)*.3f));}
        }
        var la=MakeLine("LeftHammerArrow",root.transform,material,.028f,5,true);
        var ra=MakeLine("RightHammerArrow",root.transform,material,.028f,5,true);
        var rd=new SerializedObject(ready);
        rd.FindProperty("leftHammer").objectReferenceValue=left;
        rd.FindProperty("rightHammer").objectReferenceValue=right;
        rd.FindProperty("playerCamera").objectReferenceValue=camera.transform;
        rd.FindProperty("locomotion").objectReferenceValue=locomotion.gameObject;
        rd.FindProperty("readyPoint").objectReferenceValue=point.transform;
        rd.FindProperty("readyCircle").objectReferenceValue=point;
        rd.FindProperty("leftArrow").objectReferenceValue=la;
        rd.FindProperty("rightArrow").objectReferenceValue=ra;
        rd.ApplyModifiedProperties();
        data.FindProperty("gameReadyController").objectReferenceValue=ready;data.ApplyModifiedProperties();
        point.SetActive(false);la.gameObject.SetActive(false);ra.gameObject.SetActive(false);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);EditorSceneManager.SaveScene(gm.gameObject.scene);
        Debug.Log("READY_SETUP_OK: hands, camera, locomotion and neon guide wired. Ready circle: "+position);
    }
    static LineRenderer MakeLine(string name,Transform parent,Material material,float width,int count,bool world)
    {
        var go=new GameObject(name,typeof(LineRenderer));go.transform.SetParent(parent,false);
        var line=go.GetComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=world;
        line.positionCount=count;line.startWidth=line.endWidth=width;
        line.startColor=line.endColor=new Color(.15f,.85f,1f);
        line.numCapVertices=6;line.numCornerVertices=6;
        line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
        return line;
    }
}
