using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Whack_A_Mole;
public static class WhackRippleVerification
{
    [MenuItem("Tools/Whack a Mole/Verify Hit Waves (Play Mode)")]
    public static void Verify()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first.");
        var gm=GameManager.Instance;
        var ripple=UnityEngine.Object.FindFirstObjectByType<NeonHitRipple>();
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var rings=(MeshRenderer[])typeof(NeonHitRipple).GetField("rings",flags).GetValue(ripple);
        int Active()=>rings.Count(r=>r.enabled);
        void Check(bool condition,string message){if(!condition)throw new Exception("Ripple verification: "+message);}
        Check(Active()==0,"waves must initially be off");
        var fields=new[]{"combo","currentSpawnTime","canPlay"}.Select(n=>typeof(GameManager).GetField(n,flags)).ToArray();
        var original=fields.Select(f=>f.GetValue(gm)).ToArray();
        int score=gm.score,hearts=gm.maxHearts;
        try {
            gm.HitMole(); Check(Active()==1,"first scored hit must emit one wave");
            gm.HitMole(); Check(Active()==2,"second hit must overlap");
            gm.MissMole(); Check(Active()==2,"miss must not emit");
            fields[2].SetValue(gm,false);gm.HitMole();Check(Active()==2,"rejected hit must not emit");
        } finally {
            gm.score=score;gm.maxHearts=hearts;
            for(int i=0;i<fields.Length;i++)fields[i].SetValue(gm,original[i]);
            typeof(GameManager).GetMethod("RefreshHUD",flags).Invoke(gm,null);
        }
        double until=EditorApplication.timeSinceStartup+1.5;
        void Finish(){
            if(EditorApplication.timeSinceStartup<until)return;
            EditorApplication.update-=Finish;
            if(!ripple || !EditorApplication.isPlaying)return;
            Check(Active()==0,"waves must expire");
            Debug.Log("RIPPLE_VERIFY_OK: scored hits emit; overlap works; miss/rejected hit do not emit; waves expire; game state restored.");
        }
        EditorApplication.update+=Finish;
    }
}
