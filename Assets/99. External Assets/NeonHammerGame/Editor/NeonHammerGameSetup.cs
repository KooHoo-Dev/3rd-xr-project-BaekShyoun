// NeonHammerGame - editor setup (URP).
// Builds the two prefabs the same way ArcadeHammerVol1 builds its prefabs:
//   FBX model prefab -> Prefab Variant with material overrides + MeshColliders (non-convex) from the Collision FBX.
// Runs automatically once when the folder is first imported (only if the prefabs are missing),
// and can be re-run from: Tools > Neon Hammer Game.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonHammerGame.EditorTools
{
    [InitializeOnLoad]
    public static class NeonHammerGameSetup
    {
        const string MachineFbx = "Meshes/NH_Arcade_Hammer_1_1.fbx";
        const string HammerFbx = "Meshes/NH_Arcade_Hammer_Hammer_1_1.fbx";
        const string MachineCollisionFbx = "Collision/NH_Arcade_Hammer_1_1_Collision.fbx";
        const string MoleCollisionFbx = "Collision/NH_Arcade_Hammer_Mole_Collision.fbx";
        const string HammerCollisionFbx = "Collision/NH_Arcade_Hammer_Hammer_1_1_Collision.fbx";
        const string PaletteMaterial = "Materials/NH_Neon_Palette.mat";
        const string HammerMaterial = "Materials/NH_BlueHammer.mat";
        public const string MachinePrefab = "Prefabs/NH_Arcade_Hammer_URP.prefab";
        public const string HammerPrefab = "Prefabs/NH_Arcade_Hammer_Hammer_URP.prefab";

        // Same object names as ArcadeHammerVol1 (Arcade_Hammer_1_1.fbx), so scripts that look children up by name keep working.
        const string BodyName = "Arcade_Hammer_1_1";
        const string MoleNamePrefix = "Arcade_Hammer_Mole_1_";
        const int MoleCount = 6;

        static NeonHammerGameSetup()
        {
            EditorApplication.delayCall += AutoBuildIfMissing;
        }

        static void AutoBuildIfMissing()
        {
            string root = Root();
            if (root == null) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(P(root, MachinePrefab)) != null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(P(root, HammerPrefab)) != null) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(P(root, MachineFbx)) == null) return; // not imported yet
            BuildPrefabs();
        }

        [MenuItem("Tools/Neon Hammer Game/Build Prefabs")]
        public static void BuildPrefabs()
        {
            string root = Root();
            if (root == null) { Debug.LogError("[NeonHammerGame] Could not locate the NeonHammerGame folder."); return; }
            EnsureFolder(root, "Prefabs");

            var palette = AssetDatabase.LoadAssetAtPath<Material>(P(root, PaletteMaterial));
            var hammerMat = AssetDatabase.LoadAssetAtPath<Material>(P(root, HammerMaterial));
            var machineCol = LoadMesh(P(root, MachineCollisionFbx));
            var moleCol = LoadMesh(P(root, MoleCollisionFbx));
            var hammerCol = LoadMesh(P(root, HammerCollisionFbx));

            // ---------- machine (body + 6 moles) ----------
            var machineModel = AssetDatabase.LoadAssetAtPath<GameObject>(P(root, MachineFbx));
            if (machineModel != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(machineModel);
                try
                {
                    SetMaterials(inst, palette);
                    var body = FindDeep(inst.transform, BodyName);
                    if (body != null) AddMeshCollider(body.gameObject, machineCol);
                    else Debug.LogWarning("[NeonHammerGame] '" + BodyName + "' not found in " + MachineFbx);
                    for (int i = 1; i <= MoleCount; i++)
                    {
                        var mole = FindDeep(inst.transform, MoleNamePrefix + i);
                        if (mole != null) AddMeshCollider(mole.gameObject, moleCol);
                        else Debug.LogWarning("[NeonHammerGame] '" + MoleNamePrefix + i + "' not found in " + MachineFbx);
                    }
                    PrefabUtility.SaveAsPrefabAsset(inst, P(root, MachinePrefab));
                }
                finally { UnityEngine.Object.DestroyImmediate(inst); }
            }
            else Debug.LogError("[NeonHammerGame] Missing " + P(root, MachineFbx));

            // ---------- hammer ----------
            var hammerModel = AssetDatabase.LoadAssetAtPath<GameObject>(P(root, HammerFbx));
            if (hammerModel != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(hammerModel);
                try
                {
                    SetMaterials(inst, hammerMat);
                    var mf = inst.GetComponentInChildren<MeshFilter>(true);
                    AddMeshCollider(mf != null ? mf.gameObject : inst, hammerCol);
                    PrefabUtility.SaveAsPrefabAsset(inst, P(root, HammerPrefab));
                }
                finally { UnityEngine.Object.DestroyImmediate(inst); }
            }
            else Debug.LogError("[NeonHammerGame] Missing " + P(root, HammerFbx));

            AssetDatabase.SaveAssets();
            Debug.Log("[NeonHammerGame] Prefabs built in " + P(root, "Prefabs"));
        }

        // Swaps every ArcadeHammerVol1 prefab instance in the open scenes for the NeonHammerGame prefab
        // (same position / rotation / scale / parent). On Unity 2022.2+ the built-in "Replace Prefab Asset"
        // is used, which keeps components you added to the instance (matched by object name).
        [MenuItem("Tools/Neon Hammer Game/Replace ArcadeHammerVol1 Prefabs In Open Scenes")]
        public static void ReplaceInOpenScenes()
        {
            string root = Root();
            if (root == null) return;
            var machine = AssetDatabase.LoadAssetAtPath<GameObject>(P(root, MachinePrefab));
            var hammer = AssetDatabase.LoadAssetAtPath<GameObject>(P(root, HammerPrefab));
            if (machine == null || hammer == null)
            {
                BuildPrefabs();
                machine = AssetDatabase.LoadAssetAtPath<GameObject>(P(root, MachinePrefab));
                hammer = AssetDatabase.LoadAssetAtPath<GameObject>(P(root, HammerPrefab));
            }
            if (machine == null || hammer == null) { Debug.LogError("[NeonHammerGame] Prefabs missing."); return; }

            var targets = new List<GameObject>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                foreach (var go in scene.GetRootGameObjects())
                    foreach (var t in go.GetComponentsInChildren<Transform>(true))
                    {
                        if (!PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject)) continue;
                        string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
                        if (!string.IsNullOrEmpty(path) && path.Replace('\\', '/').Contains("/ArcadeHammerVol1/"))
                            targets.Add(t.gameObject);
                    }
            }
            int n = 0;
            foreach (var go in targets)
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                bool isHammer = Path.GetFileName(path).Contains("Hammer_Hammer");
                Replace(go, isHammer ? hammer : machine);
                n++;
            }
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[NeonHammerGame] Replaced " + n + " ArcadeHammerVol1 prefab instance(s). Save the scene(s) to keep the change.");
        }

        // ------------------------------------------------------------------
        static void Replace(GameObject instance, GameObject prefab)
        {
            // Unity 2022.2+: PrefabUtility.ReplacePrefabAssetOfPrefabInstance (called via reflection so this
            // file also compiles on older versions).
            var method = typeof(PrefabUtility).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "ReplacePrefabAssetOfPrefabInstance" && m.GetParameters().Length == 4
                                     && m.GetParameters()[0].ParameterType == typeof(GameObject));
            if (method != null)
            {
                try
                {
                    var settings = Activator.CreateInstance(method.GetParameters()[2].ParameterType);
                    SetMember(settings, "logInfo", true);
                    SetMember(settings, "objectMatchMode", "ByName");
                    SetMember(settings, "prefabOverridesOptions", "KeepAllPossibleOverrides");
                    method.Invoke(null, new object[] { instance, prefab, settings, InteractionMode.UserAction });
                    // Old per-instance transform / material tweaks were made for the old model; drop them on children.
                    foreach (var t in instance.GetComponentsInChildren<Transform>(true))
                    {
                        if (t == instance.transform || PrefabUtility.IsAddedGameObjectOverride(t.gameObject)) continue;
                        try
                        {
                            PrefabUtility.RevertObjectOverride(t, InteractionMode.UserAction);
                            var r = t.GetComponent<Renderer>();
                            if (r != null) PrefabUtility.RevertObjectOverride(r, InteractionMode.UserAction);
                        }
                        catch { }
                    }
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[NeonHammerGame] Built-in replace failed (" + e.Message + "), using simple replace for " + instance.name);
                }
            }
            // Fallback: new instance with the same transform, old one removed (added components are NOT carried over).
            var parent = instance.transform.parent;
            var copy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, instance.scene);
            Undo.RegisterCreatedObjectUndo(copy, "Replace with NeonHammerGame");
            copy.transform.SetParent(parent, false);
            copy.transform.SetSiblingIndex(instance.transform.GetSiblingIndex());
            copy.transform.localPosition = instance.transform.localPosition;
            copy.transform.localRotation = instance.transform.localRotation;
            copy.transform.localScale = instance.transform.localScale;
            Undo.DestroyObjectImmediate(instance);
        }

        static void SetMember(object target, string name, object value)
        {
            var type = target.GetType();
            var f = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Type memberType = f != null ? f.FieldType : (p != null ? p.PropertyType : null);
            if (memberType == null) return;
            object v = value;
            if (memberType.IsEnum)
            {
                try { v = Enum.Parse(memberType, value.ToString()); } catch { return; }
            }
            try
            {
                if (f != null) f.SetValue(target, v);
                else if (p.CanWrite) p.SetValue(target, v);
            }
            catch { }
        }

        static void SetMaterials(GameObject go, Material mat)
        {
            if (mat == null) return;
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
            }
        }

        static void AddMeshCollider(GameObject go, Mesh mesh)
        {
            if (mesh == null) { Debug.LogWarning("[NeonHammerGame] Collision mesh missing for " + go.name); return; }
            var mc = go.GetComponent<MeshCollider>();
            if (mc == null) mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = false;   // same as ArcadeHammerVol1
        }

        static Mesh LoadMesh(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().FirstOrDefault();
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }

        static string Root()
        {
            foreach (var guid in AssetDatabase.FindAssets("NeonHammerGameSetup t:MonoScript"))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (!p.EndsWith("/Editor/NeonHammerGameSetup.cs")) continue;
                return p.Substring(0, p.Length - "/Editor/NeonHammerGameSetup.cs".Length);
            }
            return null;
        }

        static string P(string root, string rel) { return root + "/" + rel; }

        static void EnsureFolder(string root, string name)
        {
            if (!AssetDatabase.IsValidFolder(P(root, name))) AssetDatabase.CreateFolder(root, name);
        }
    }
}
