using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;

[InitializeOnLoad]
public static class WhackReadyGravityFix
{
    static WhackReadyGravityFix(){EditorApplication.delayCall+=Apply;}
    static void Apply(){
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)return;
        if(EditorSceneManager.GetActiveScene().name!="Game")return;
        var origin=Object.FindFirstObjectByType<XROrigin>();if(!origin)return;
        var root=origin.transform.Find("Locomotion");if(!root)return;
        bool changed=false;
        foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true)){
            if(!component)continue;
            var data=new SerializedObject(component);var gravity=data.FindProperty("m_UseGravity");
            if(gravity==null || gravity.propertyType!=SerializedPropertyType.Boolean || !gravity.boolValue)continue;
            Undo.RecordObject(component,"Disable player locomotion gravity");gravity.boolValue=false;data.ApplyModifiedProperties();changed=true;
        }
        if(!changed)return;
        EditorSceneManager.MarkSceneDirty(origin.gameObject.scene);EditorSceneManager.SaveScene(origin.gameObject.scene);
        Debug.Log("READY_GRAVITY_FIXED: player locomotion gravity disabled; hammer physics unchanged.");
    }
}
