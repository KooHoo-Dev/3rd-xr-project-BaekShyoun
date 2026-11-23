using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace Whack_A_Mole
{
    // 별도의 레이캐스트 없이, 기존 광선이 판정한 위치에 점을 표시.
    public class HandPointerVisual : MonoBehaviour
    {
        private NearFarInteractor hand;
        private GameObject dot;
        private Material dotMaterial;

        private void Start()
        {
            hand = GetComponent<NearFarInteractor>();
            var shader = Shader.Find("UI/NoZTest");
            if (shader == null)
            {
                Debug.LogError("포인터용 UI/NoZTest 셰이더를 찾을 수 없습니다.");
                enabled = false;
                return;
            }

            dotMaterial = new Material(shader);
            // UI도 깊이 검사를 하지 않으므로, UI보다 나중에 그려 점이 가려지지 않게 함.
            dotMaterial.renderQueue = 5000;
            dotMaterial.color = new Color(.25f, .95f, 1f);
            dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "Aim Dot " + hand.handedness;
            dot.transform.SetParent(transform, false);
            dot.transform.localScale = Vector3.one * .018f;
            dot.GetComponent<Collider>().enabled = false;
            Destroy(dot.GetComponent<Collider>());
            dot.GetComponent<Renderer>().sharedMaterial = dotMaterial;
            dot.SetActive(false);
        }

        private void LateUpdate()
        {
            if (dot == null || hand == null) return;
            var hitType = hand.TryGetCurveEndPoint(out var position);
            bool show = hand.isActiveAndEnabled &&
                (hitType == EndPointType.UI ||
                 (hitType == EndPointType.ValidCastHit && !hand.hasSelection));
            dot.SetActive(show);
            if (!show) return;

            // 광선의 끝과 같은 위치. 표면에 묻히지 않도록 손 방향으로 조금 이동.
            var toHand = (hand.curveOrigin.position - position).normalized;
            dot.transform.position = position + toHand * .012f;
            dotMaterial.color = hitType == EndPointType.UI && hand.uiPressInput.ReadIsPerformed()
                ? Color.white : new Color(.25f, .95f, 1f);
        }

        private void OnDisable()
        {
            if (dot != null) dot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (dotMaterial != null) Destroy(dotMaterial);
        }
    }
}
