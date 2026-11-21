using UnityEngine;

namespace Whack_A_Mole
{
    public class MoleHitVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem hitRing;
        [SerializeField] private ParticleSystem hitSparks;
        [SerializeField] private Renderer moleRenderer;

        public void PlayHit()
        {
            // 머리가 내려가더라도 효과는 맞은 위치에 남아 재생.
            Bounds bounds = moleRenderer.bounds;
            Vector3 hitPosition = new Vector3(bounds.center.x, bounds.max.y + 0.02f, bounds.center.z);
            hitRing.transform.position = hitPosition;
            hitSparks.transform.position = hitPosition;
            hitRing.Emit(1);
            hitSparks.Emit(14);
        }
    }
}
