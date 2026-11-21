using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
public static class WhackCosmicFloorSetup
{
    static WhackCosmicFloorSetup(){EditorApplication.delayCall+=Apply;}
    static void Apply(){
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var scene=EditorSceneManager.GetActiveScene();if(scene.name!="Game")return;
        var root=GameObject.Find("NeonEnvironment");if(!root)return;
        var floor=root.transform.Find("Black Circular Platform");var rim=root.transform.Find("Blue Neon Rim");
        var fs=Shader.Find("WhackEnvironment/CosmicFloor");var es=Shader.Find("WhackEnvironment/LivingNeonEdge");
        if(!floor || !rim || !fs || !es)return;
        const string path="Assets/03. Environment/";
        if(AssetDatabase.LoadAssetAtPath<Material>(path+"Cosmic Floor.mat"))return;
        var fm=new Material(fs);var em=new Material(es);
        AssetDatabase.CreateAsset(fm,path+"Cosmic Floor.mat");AssetDatabase.CreateAsset(em,path+"Living Neon Edge.mat");
        Undo.RecordObject(floor.GetComponent<Renderer>(),"Cosmic floor material");
        Undo.RecordObject(rim.GetComponent<Renderer>(),"Animated neon edge material");
        floor.GetComponent<Renderer>().sharedMaterial=fm;rim.GetComponent<Renderer>().sharedMaterial=em;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("COSMIC_FLOOR_OK: navy nebula floor and animated edge applied; collider and hit wave unchanged.");
    }
}
