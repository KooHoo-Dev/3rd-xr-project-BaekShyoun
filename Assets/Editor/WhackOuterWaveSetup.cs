using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whack_A_Mole;
[InitializeOnLoad]
public static class WhackOuterWaveSetup
{
    static WhackOuterWaveSetup() { EditorApplication.delayCall += Apply; }
    private static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.name!="Game")return;
        var ripple=Object.FindFirstObjectByType<NeonHitRipple>();
        var shader=Shader.Find("WhackEnvironment/OuterShockwave");
        if(!ripple || !shader)return;
        const string path="Assets/03. Environment/";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path+"Outer Shockwave.mat");
        if(material)return;
        material=new Material(shader);
        AssetDatabase.CreateAsset(material,path+"Outer Shockwave.mat");
        var data=new SerializedObject(ripple);
        data.FindProperty("ringMaterial").objectReferenceValue=material;
        data.FindProperty("duration").floatValue=.65f;
        data.ApplyModifiedProperties();
        var floor=AssetDatabase.LoadAssetAtPath<Material>(path+"Platform.mat");
        floor.SetColor("_BaseColor",new Color(.003f,.004f,.007f,1));
        floor.SetColor("_Color",new Color(.003f,.004f,.007f,1));
        floor.SetFloat("_Smoothness",.08f);
        floor.SetFloat("_EnvironmentReflections",0);
        floor.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        EditorUtility.SetDirty(floor);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("OUTER_WAVE_READY: exterior jagged neon shockwave; dark floor; physics unchanged.");
    }
}
