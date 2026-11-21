using System.Linq;
using UnityEditor;
using UnityEngine;
using Whack_A_Mole;
public static class WhackModelInspect
{
    public static void Batch(){UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/00. Scenes/Game.unity");Run();}
    static void Run(){
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var report=new System.Text.StringBuilder("MODEL_INSPECT\n");
        foreach(var machine in Object.FindObjectsByType<MachineController>(FindObjectsSortMode.None)){
            report.AppendLine("MACHINE "+machine.name+" pos="+machine.transform.position);
            foreach(var mf in machine.GetComponentsInChildren<MeshFilter>(true))report.AppendLine(Describe(mf));
        }
        foreach(var hammer in Object.FindObjectsByType<HammerController>(FindObjectsSortMode.None)){
            report.AppendLine("HAMMER "+hammer.name);
            foreach(var mf in hammer.GetComponentsInChildren<MeshFilter>(true))report.AppendLine(Describe(mf));
            foreach(var c in hammer.GetComponents<Collider>())report.AppendLine("COLLIDER "+EditorJsonUtility.ToJson(c));
        }
        const string folder="Assets/99. External Assets/NeonHammerGame/";
        foreach(var file in new[]{"Meshes/NH_Arcade_Hammer_1_1.fbx","Meshes/NH_Arcade_Hammer_Hammer_1_1.fbx","Collision/NH_Arcade_Hammer_Mole_Collision.fbx"}){
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(folder+file);report.AppendLine("NEW "+file);
            if(model)foreach(var mf in model.GetComponentsInChildren<MeshFilter>(true))report.AppendLine(Describe(mf));
        }
        Debug.Log(report.ToString());
    }
    static string Describe(MeshFilter mf){return mf.name+" mesh="+mf.sharedMesh.name+" bounds="+mf.sharedMesh.bounds+" local="+mf.transform.localPosition+" rotation="+mf.transform.localEulerAngles+" scale="+mf.transform.localScale;}
}


