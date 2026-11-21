using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Whack_A_Mole;

public static class WhackNeonUIVerification
{
    [MenuItem("Tools/Whack a Mole/Verify Neon UI (Play Mode)")]
    static void Verify()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first.");
        WhackHUDSetup.Verify();
        var ui=UIController.Instance;
        var data=new SerializedObject(ui);
        var button=(Button)data.FindProperty("gameStartButton").objectReferenceValue;
        bool linked=false;
        for(int i=0;i<button.onClick.GetPersistentEventCount();i++)
            linked |= button.onClick.GetPersistentTarget(i)==ui && button.onClick.GetPersistentMethodName(i)=="PressGameStart";
        if(!linked)throw new Exception("Start button listener missing.");
        foreach(var text in ui.GetComponentsInChildren<TMP_Text>(true))
            if(text.fontSharedMaterial.shader.name!="TextMeshPro/Mobile/Distance Field Overlay")throw new Exception("Text overlay missing: "+text.name);
        foreach(var img in ui.GetComponentsInChildren<Image>(true))
            if(img.material.shader.name!="UI/NoZTest")throw new Exception("Image overlay missing: "+img.name);
        ui.ShowAnnounceText(true,"올바른 방향의 망치를 잡으세요.");
        // 임시 물체를 UI보다 카메라에 가깝게 두어 가림 방지를 눈으로 확인. 플레이 종료 시 사라짐.
        if(!GameObject.Find("__UI_OcclusionCheck")) {
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.name="__UI_OcclusionCheck";
            UnityEngine.Object.Destroy(cube.GetComponent<Collider>());
            cube.transform.SetParent(Camera.main.transform,false);
            cube.transform.localPosition=new Vector3(0,.21f,.75f);
            cube.transform.localScale=new Vector3(1.05f,.18f,.025f);
            var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor",new Color(.6f,.1f,.32f));
            cube.GetComponent<Renderer>().material=mat;
        }
        Debug.Log("NEON_UI_VERIFY_OK: gameplay HUD checks passed, button event intact, all UI uses overlay. Magenta test object is closer than HUD; inspect readability then stop Play mode.");
    }
}
