using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whack_A_Mole;

[InitializeOnLoad]
public static class WhackMenuRaySetup
{
    static WhackMenuRaySetup(){EditorApplication.delayCall+=Apply;}
    static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)return;
        var current=Object.FindFirstObjectByType<SceneController>();
        if(!current || new SerializedObject(current).FindProperty("uiRayMaterial").objectReferenceValue)return;
        string original=EditorSceneManager.GetActiveScene().path;
        foreach(string name in new[]{"Title","Game","Ranking"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/00. Scenes/"+name+".unity");
            var data=new SerializedObject(Object.FindFirstObjectByType<SceneController>());
            data.FindProperty("uiRayMaterial").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Material>("Assets/02. UI/Ready Guide Neon.mat");
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene(original);
        Debug.Log("MENU_RAY_MATERIAL_OK");
    }
}
