using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whack_A_Mole;


public static class WhackHeadHitSetup
{
    [MenuItem("Tools/Whack a Mole/Setup Head Hit VFX")]
    static void Apply(){
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)return;
        if(EditorSceneManager.GetActiveScene().name!="Game")return;
        var moles=UnityEngine.Object.FindObjectsByType<MoleController>(FindObjectsSortMode.None);
        var shader=Shader.Find("WhackEnvironment/HeadHitNeon");if(!shader || moles.Length==0)return;
        var ring=Material("Head Hit Ring",shader,1);var spark=Material("Head Hit Sparks",shader,0);
        bool changed=false;
        foreach(var mole in moles){
            if(mole.GetComponent<MoleHitVFX>())continue;
            Undo.RegisterFullObjectHierarchyUndo(mole.gameObject,"Add neon head hit VFX");
            var effect=mole.gameObject.AddComponent<MoleHitVFX>();
            var rings=Particles("Head Hit Ring",mole.transform,ring,true);
            var sparks=Particles("Head Hit Sparks",mole.transform,spark,false);
            var data=new SerializedObject(effect);
            data.FindProperty("hitRing").objectReferenceValue=rings;
            data.FindProperty("hitSparks").objectReferenceValue=sparks;
            data.FindProperty("moleRenderer").objectReferenceValue=mole.GetComponent<Renderer>();
            data.ApplyModifiedProperties();changed=true;
        }
        if(!changed)return;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(moles[0].gameObject.scene);EditorSceneManager.SaveScene(moles[0].gameObject.scene);
        Debug.Log("HEAD_HIT_SETUP_OK: twelve world-space neon ring/spark effects wired; no colliders, lights or physics changes.");
    }
    static Material Material(string name,Shader shader,float isRing){
        string path="Assets/03. Environment/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat)return mat;
        mat=new Material(shader);mat.SetFloat("_Ring",isRing);AssetDatabase.CreateAsset(mat,path);return mat;
    }
    static ParticleSystem Particles(string name,Transform parent,Material material,bool ring){
        var go=new GameObject(name,typeof(ParticleSystem));go.transform.SetParent(parent,false);
        var ps=go.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.playOnAwake=false;main.loop=false;main.duration=.5f;
        main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=ring?4:56;
        main.startLifetime=ring?new ParticleSystem.MinMaxCurve(.32f):new ParticleSystem.MinMaxCurve(.20f,.38f);
        main.startSpeed=ring?new ParticleSystem.MinMaxCurve(0):new ParticleSystem.MinMaxCurve(.5f,1.1f);
        main.startSize=ring?new ParticleSystem.MinMaxCurve(.12f):new ParticleSystem.MinMaxCurve(.025f,.045f);
        main.startColor=Color.white;main.gravityModifier=0;
        var emission=ps.emission;emission.enabled=false;
        var shape=ps.shape;shape.enabled=!ring;shape.shapeType=ParticleSystemShapeType.Cone;
        shape.angle=65;shape.radius=.015f;shape.rotation=new Vector3(-90,0,0);
        var size=ps.sizeOverLifetime;size.enabled=true;
        size.size=new ParticleSystem.MinMaxCurve(1,ring?new AnimationCurve(new Keyframe(0,.35f),new Keyframe(.25f,1.7f),new Keyframe(1,3.8f)):AnimationCurve.Linear(0,1,1,.1f));
        var color=ps.colorOverLifetime;color.enabled=true;
        var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.2f,.8f,1),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.8f,.2f),new GradientAlphaKey(0,1)});color.color=gradient;
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;
        renderer.renderMode=ring?ParticleSystemRenderMode.Billboard:ParticleSystemRenderMode.Stretch;
        renderer.lengthScale=2;renderer.velocityScale=.035f;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        return ps;
    }
    [MenuItem("Tools/Whack a Mole/Preview Head Hit (Play Mode)")]
    static void Preview(){
        if(!EditorApplication.isPlaying)return;
        var mole=UnityEngine.Object.FindObjectsByType<MoleController>(FindObjectsSortMode.None).OrderBy(m=>Vector3.Distance(Camera.main.transform.position,m.transform.position)).First();
        mole.PopUp();
        double until=EditorApplication.timeSinceStartup+.7;
        EditorApplication.CallbackFunction wait=null;
        wait=()=>{if(EditorApplication.timeSinceStartup<until)return;EditorApplication.update-=wait;
            if(!EditorApplication.isPlaying)return;
            mole.GetComponent<MoleHitVFX>().PlayHit();mole.HideInTheHole();
            double pauseAt=EditorApplication.timeSinceStartup+.10;
            EditorApplication.CallbackFunction pause=null;
            pause=()=>{if(EditorApplication.timeSinceStartup<pauseAt)return;EditorApplication.update-=pause;if(EditorApplication.isPlaying)EditorApplication.isPaused=true;};
            EditorApplication.update+=pause;
        };EditorApplication.update+=wait;
    }
}
