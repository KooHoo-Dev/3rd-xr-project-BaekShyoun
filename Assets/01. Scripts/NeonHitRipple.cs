using UnityEngine;
using UnityEngine.Rendering;

namespace Whack_A_Mole
{
    // 발판 바깥으로 퍼지는 타격 충격파. 링을 재사용해 연타 중에도 생성/삭제하지 않음.
    public class NeonHitRipple : MonoBehaviour
    {
        [SerializeField] private Mesh ringMesh;
        [SerializeField] private Material ringMaterial;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private int poolSize = 16;
        [SerializeField, Min(0f)] private float riseHeight = 3f; // 파동이 올라갈 높이
        private MeshRenderer[] rings;
        private float[] ages;
        private int nextRing;
        private GameManager game;
        private Mesh waveMesh;
        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private MaterialPropertyBlock properties;
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Start()
        {
            rings = new MeshRenderer[Mathf.Max(1, poolSize)];
            ages = new float[rings.Length];
            properties = new MaterialPropertyBlock();
            waveMesh = Instantiate(ringMesh);
            // 셰이더에서 바깥으로 변형되는 범위까지 포함
            waveMesh.bounds = new Bounds(Vector3.zero, new Vector3(14, 2, 14));
            for (int i = 0; i < rings.Length; i++)
            {
                var ring = new GameObject("Hit Wave " + (i + 1), typeof(MeshFilter), typeof(MeshRenderer));
                ring.transform.SetParent(transform, false);
                ring.transform.localPosition = new Vector3(0, 0.04f + i * 0.0002f, 1.5f);
                ring.GetComponent<MeshFilter>().sharedMesh = waveMesh;
                rings[i] = ring.GetComponent<MeshRenderer>();
                rings[i].sharedMaterial = ringMaterial;
                rings[i].shadowCastingMode = ShadowCastingMode.Off;
                rings[i].receiveShadows = false;
                rings[i].enabled = false;
                ages[i] = duration;
            }
            game = GameManager.Instance;
            if (game != null) game.MoleHit += Emit;
        }

        public void Emit()
        {
            if (!isActiveAndEnabled || rings == null) return;
            ages[nextRing] = 0;
            Draw(nextRing);
            nextRing = (nextRing + 1) % rings.Length;
        }

        private void Update()
        {
            for (int i = 0; i < rings.Length; i++)
            {
                if (!rings[i].enabled) continue;
                ages[i] += Time.deltaTime;
                Draw(i);
            }
        }

        private void Draw(int i)
        {
            float t = Mathf.Clamp01(ages[i] / Mathf.Max(0.01f, duration));
            rings[i].enabled = t < 1;
            // 발판 외곽에서 빠르게 터져 나간 뒤 감속하며 사라짐.
            float spread = 1f - Mathf.Pow(1f - t, 3f);
            rings[i].transform.localScale = Vector3.one * Mathf.Lerp(1f, 2.5f, spread);
            // 바깥 확산과 같은 비율로 상승해서 대각선으로 퍼짐
            rings[i].transform.localPosition = new Vector3(
                0, 0.04f + i * 0.0002f + riseHeight * spread, 1.5f);
            properties.SetColor(ColorId, new Color(0.12f, 1.6f, 4f, Mathf.Pow(1f - t, 1.3f)));
            properties.SetFloat(ProgressId, t);
            properties.SetFloat(SeedId, i * 2.39996f);
            rings[i].SetPropertyBlock(properties);
        }

        private void OnDisable()
        {
            if (rings != null)
                foreach (var ring in rings) if (ring) ring.enabled = false;
        }

        private void OnDestroy()
        {
            if (game != null) game.MoleHit -= Emit;
            if (waveMesh) Destroy(waveMesh);
        }
    }
}
