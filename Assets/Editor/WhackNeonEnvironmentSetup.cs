using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// 환경 전용 편집기 도구. 게임 로직 및 구매한 에셋은 수정하지 않습니다.
public static class WhackNeonEnvironmentSetup
{
    const string Folder = "Assets/03. Environment/";
    [MenuItem("Tools/Whack a Mole/Build Neon Environment")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Game") throw new InvalidOperationException("Open Game scene first.");
        if (GameObject.Find("NeonEnvironment")) throw new InvalidOperationException("Environment already exists.");
        var skyShader = Shader.Find("WhackEnvironment/CosmicSky");
        var glowShader = Shader.Find("WhackEnvironment/SoftGlow");
        if (!skyShader || !glowShader) throw new InvalidOperationException("Wait for shader import.");
        var root = new GameObject("NeonEnvironment");
        Undo.RegisterCreatedObjectUndo(root, "Create neon environment");
        var floor = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floor.SetColor("_BaseColor", new Color(.018f,.024f,.035f));
        floor.SetFloat("_Smoothness", .28f);
        AssetDatabase.CreateAsset(floor, Folder+"Platform.mat");
        var disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disk.name = "Black Circular Platform";
        // 장식 발판은 물리 충돌하지 않음. 기존 Plane이 바닥 충돌 담당.
        disk.GetComponent<Collider>().enabled = false;
        disk.transform.SetParent(root.transform);
        disk.transform.position = new Vector3(0,-.08f,1.5f);
        disk.transform.localScale = new Vector3(8,.08f,8);
        disk.GetComponent<Renderer>().sharedMaterial = floor;
        var glow = new Material(glowShader);
        glow.SetColor("_Color", new Color(.12f,1.3f,2.8f,1));
        AssetDatabase.CreateAsset(glow, Folder+"Blue Neon Rim.mat");
        var rim = new GameObject("Blue Neon Rim", typeof(MeshFilter), typeof(MeshRenderer));
        rim.transform.SetParent(root.transform);
        rim.transform.position = new Vector3(0,.008f,1.5f);
        const int n=192;
        var verts=new Vector3[(n+1)*2]; var uv=new Vector2[verts.Length]; var colors=new Color[verts.Length]; var triangles=new int[n*6];
        for(int j=0;j<=n;j++) {
            float a=j*Mathf.PI*2/n; var dir=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
            verts[j*2]=dir*3.83f; verts[j*2+1]=dir*4.04f;
            uv[j*2]=new Vector2(0,(float)j/n); uv[j*2+1]=new Vector2(1,(float)j/n);
            colors[j*2]=colors[j*2+1]=Color.white;
            if(j<n) { int t=j*6,k=j*2; triangles[t]=k;triangles[t+1]=k+2;triangles[t+2]=k+1;triangles[t+3]=k+1;triangles[t+4]=k+2;triangles[t+5]=k+3; }
        }
        var mesh=new Mesh {name="Neon Ring",vertices=verts,uv=uv,colors=colors,triangles=triangles}; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, Folder+"Neon Ring.asset");
        rim.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=rim.GetComponent<MeshRenderer>(); renderer.sharedMaterial=glow; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
        var sky = new Material(skyShader); AssetDatabase.CreateAsset(sky,Folder+"Cosmic Sky.mat");
        RenderSettings.skybox=sky;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.18f,.24f,.34f);
        RenderSettings.ambientEquatorColor=new Color(.09f,.12f,.19f);
        RenderSettings.ambientGroundColor=new Color(.035f,.045f,.07f);
        var plane=GameObject.Find("Plane"); if(plane && plane.TryGetComponent<Renderer>(out var oldFloor)) { Undo.RecordObject(oldFloor,"Hide original floor surface");oldFloor.enabled=false; }
        if(Camera.main) { Undo.RecordObject(Camera.main,"Set cosmic background"); Camera.main.clearFlags=CameraClearFlags.Skybox; }
        var dustMat=new Material(glowShader); dustMat.SetColor("_Color",new Color(.25f,.6f,1,.45f));dustMat.SetFloat("_Round",1);AssetDatabase.CreateAsset(dustMat,Folder+"Space Dust.mat");
        var dust=new GameObject("Slow Space Dust");dust.transform.SetParent(root.transform);dust.transform.position=new Vector3(0,3,9);
        var ps=dust.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.startLifetime=20;main.startSpeed=.025f;main.startSize=new ParticleSystem.MinMaxCurve(.015f,.035f);main.maxParticles=100;main.simulationSpace=ParticleSystemSimulationSpace.World;main.prewarm=true;
        var emission=ps.emission;emission.rateOverTime=5;
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(20,7,12);
        var pr=ps.GetComponent<ParticleSystemRenderer>();pr.sharedMaterial=dustMat;pr.shadowCastingMode=ShadowCastingMode.Off;pr.receiveShadows=false;
        ps.Play();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=root;
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0,0,1.5f),Quaternion.Euler(24,0,0),7);
        Debug.Log("NEON_ENVIRONMENT_OK: platform, sky and dust saved; purchased assets untouched.");
    }
}
