using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whack_A_Mole;

public static class WhackNeonModelSwap
{
    const string Root="Assets/99. External Assets/NeonHammerGame/";
    static void ApplyOnce(){
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)return;
        if(EditorSceneManager.GetActiveScene().name!="Game")return;
        var machines=UnityEngine.Object.FindObjectsByType<MachineController>(FindObjectsSortMode.None);
        if(machines.Length!=2)return;
        if(machines.All(m=>m.GetComponentsInChildren<MeshFilter>().All(f=>AssetDatabase.GetAssetPath(f.sharedMesh).Contains("/NeonHammerGame/"))))return;
        Apply();
    }
    [MenuItem("Tools/Whack a Mole/Replace Models with NeonHammerGame")]
    public static void Apply(){
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var machines=UnityEngine.Object.FindObjectsByType<MachineController>(FindObjectsSortMode.None);
        var hammers=UnityEngine.Object.FindObjectsByType<HammerController>(FindObjectsSortMode.None);
        if(machines.Length!=2 || hammers.Length!=2)throw new Exception("Expected two machines and two hammers.");
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Meshes/NH_Arcade_Hammer_1_1.fbx");
        var hammerModel=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Meshes/NH_Arcade_Hammer_Hammer_1_1.fbx");
        var sources=model.GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.sharedMesh.name);
        var hammerSource=hammerModel.GetComponentInChildren<MeshFilter>(true);
        var palette=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/NH_Neon_Palette.mat");
        var hammerMat=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/NH_BlueHammer.mat");
        var bodyCollider=Mesh("Collision/NH_Arcade_Hammer_1_1_Collision.fbx");
        var moleCollider=Mesh("Collision/NH_Arcade_Hammer_Mole_Collision.fbx");
        var hammerCollider=Mesh("Collision/NH_Arcade_Hammer_Hammer_1_1_Collision.fbx");
        // 교체 전에 대응 메시를 확인하고 기존 오브젝트와 컴포넌트는 유지.
        foreach(var machine in machines)foreach(var f in machine.GetComponentsInChildren<MeshFilter>(true))
            if(!sources.ContainsKey(f.sharedMesh.name))throw new Exception("No matching mesh: "+f.name);
        var scripts=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Where(b=>b && b.GetType().Namespace=="Whack_A_Mole").ToArray();
        var snapshots=scripts.Select(s=>EditorJsonUtility.ToJson(s)).ToArray();
        foreach(var machine in machines){
            Undo.RegisterFullObjectHierarchyUndo(machine.gameObject,"Swap neon machine visuals");
            foreach(var f in machine.GetComponentsInChildren<MeshFilter>(true)){
                var source=sources[f.sharedMesh.name];bool mole=f.GetComponent<MoleController>();
                f.sharedMesh=source.sharedMesh;
                var renderer=f.GetComponent<MeshRenderer>();renderer.sharedMaterials=Enumerable.Repeat(palette,f.sharedMesh.subMeshCount).ToArray();
                if(mole){
                    // 새 모델의 구멍 위치에 맞춰 피벗만 이동. 스크립트/배열 참조는 그대로 유지.
                    f.transform.localPosition=source.transform.localPosition;
                    f.transform.localRotation=source.transform.localRotation;
                    f.transform.localScale=source.transform.localScale;
                }
                ReplaceCollider(f.gameObject,mole?moleCollider:bodyCollider);
            }
        }
        // 새 본체 폭이 더 넓으므로 두 게임기 사이에 작은 간격 확보.
        var left=machines.Single(m=>m.machineSide==HandSide.Left);
        var right=machines.Single(m=>m.machineSide==HandSide.Right);
        Vector3 middle=(left.transform.position+right.transform.position)*.5f;
        Vector3 axis=(right.transform.position-left.transform.position).normalized;
        left.transform.position=middle-axis*.60f;right.transform.position=middle+axis*.60f;
        foreach(var hammer in hammers){
            Undo.RegisterFullObjectHierarchyUndo(hammer.gameObject,"Swap neon hammer visuals");
            var filter=hammer.GetComponent<MeshFilter>();filter.sharedMesh=hammerSource.sharedMesh;
            filter.GetComponent<MeshRenderer>().sharedMaterials=Enumerable.Repeat(hammerMat,filter.sharedMesh.subMeshCount).ToArray();
            ReplaceCollider(hammer.gameObject,hammerCollider);
        }
        for(int i=0;i<scripts.Length;i++)
            if(EditorJsonUtility.ToJson(scripts[i])!=snapshots[i])throw new Exception("Gameplay component changed: "+scripts[i].name);
        foreach(var machine in machines){
            var data=new SerializedObject(machine);var moles=data.FindProperty("moles");
            if(moles.arraySize!=6)throw new Exception("Mole array count changed.");
            for(int i=0;i<6;i++)if(!moles.GetArrayElementAtIndex(i).objectReferenceValue)throw new Exception("Missing mole reference.");
        }
        EditorSceneManager.MarkSceneDirty(left.gameObject.scene);EditorSceneManager.SaveScene(left.gameObject.scene);
        Debug.Log("NEON_MODEL_SWAP_OK: 2 cabinets, 12 moles, 2 hammers replaced in place. Gameplay components and references unchanged; collider settings preserved; new hole positions and cabinet spacing applied.");
    }
    static Mesh Mesh(string path){return AssetDatabase.LoadAllAssetsAtPath(Root+path).OfType<Mesh>().First();}
    static void ReplaceCollider(GameObject go,Mesh mesh){
        foreach(var c in go.GetComponents<Collider>()){
            if(c is MeshCollider mc)mc.sharedMesh=mesh;
            else if(c is BoxCollider box){box.center=mesh.bounds.center;box.size=mesh.bounds.size;}
            else if(c is CapsuleCollider capsule){capsule.center=mesh.bounds.center;capsule.height=mesh.bounds.size.y;capsule.radius=Mathf.Max(mesh.bounds.extents.x,mesh.bounds.extents.z);}
            else throw new Exception("Unsupported collider: "+c.GetType().Name);
        }
    }
}


