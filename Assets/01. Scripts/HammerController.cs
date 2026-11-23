using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Whack_A_Mole
{
    public class HammerController : MonoBehaviour
    {
        public HandSide hammerSide;


        // 현재 망치 모델의 머리 중심과 크기 (로컬 좌표).
        [SerializeField] private Vector3 hitCenter = new Vector3(0f, .2286f, 0f);
        [SerializeField] private float hitRadius = .062f;
        [SerializeField] private float headHalfWidth = .105f;

        private XRGrabInteractable grab;
        private bool hasPreviousPosition;
        private readonly Vector3[] previousPositions = new Vector3[3];
        private readonly RaycastHit[] hitResults = new RaycastHit[128];
        private readonly Collider[] overlapResults = new Collider[128];

        public bool IsHeld => grab != null && grab.isSelected;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();

            // XRI가 잡는 위치를 계산할 때 팔 뻗기 보정도 함께 처리.
            HammerReachTransformer reach = GetComponent<HammerReachTransformer>();
            if (reach == null) reach = gameObject.AddComponent<HammerReachTransformer>();
            grab.AddSingleGrabTransformer(reach);
        }

        private void LateUpdate()
        {
            if (!IsHeld || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                hasPreviousPosition = false;
                return;
            }

            // Update에서 움직인 두더지와 망치의 위치를 물리 검사에도 반영.
            Physics.SyncTransforms();
            Vector3 scale = transform.lossyScale;
            float radius = hitRadius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

            for (int i = 0; i < 3; i++)
            {
                Vector3 position = transform.TransformPoint(hitCenter + Vector3.right * ((i - 1) * headHalfWidth));
                if (hasPreviousPosition)
                {
                    // 이전 위치부터 현재 위치까지 훑어서 빠르게 지나친 두더지도 확인.
                    Vector3 move = position - previousPositions[i];
                    float distance = move.magnitude;
                    if (distance > .0001f)
                    {
                        int count = Physics.SphereCastNonAlloc(previousPositions[i], radius, move / distance,
                            hitResults, distance, ~0, QueryTriggerInteraction.Ignore);
                        for (int j = 0; j < count; j++) CheckMole(hitResults[j].collider);
                    }

                    // SphereCast가 놓칠 수 있는 시작점 겹침과 현재 위치의 접촉도 확인.
                    CheckOverlap(previousPositions[i], radius);
                    CheckOverlap(position, radius);
                }
                previousPositions[i] = position;
            }
            // 처음 잡은 순간에는 망치가 손으로 이동한 경로를 타격으로 처리하지 않음.
            hasPreviousPosition = true;
        }

        private void CheckOverlap(Vector3 position, float radius)
        {
            int count = Physics.OverlapSphereNonAlloc(position, radius, overlapResults, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++) CheckMole(overlapResults[i]);
        }

        private void CheckMole(Collider target)
        {
            MoleController mole = target.GetComponentInParent<MoleController>();
            if (mole != null) mole.TryHit(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            for (int i = 0; i < 3; i++)
                Gizmos.DrawWireSphere(hitCenter + Vector3.right * ((i - 1) * headHalfWidth), hitRadius);
        }

        private XRBaseInteractor holdingHand;
        private bool previousKeepSelected;

        private void OnEnable()
        {
            GetComponent<XRGrabInteractable>().selectEntered.AddListener(OnGrab);
        }

        private void OnGrab(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
        {
            hasPreviousPosition = false;
            SoundManager.Instance?.PlaySFX(SFXType.HammerGrab);
        }

        public void LockGrab()
        {
            if (holdingHand != null) return;
            XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
            if (grab.isSelected == false) return;

            holdingHand = grab.interactorsSelecting[0] as XRBaseInteractor;
            if (holdingHand == null) return;

            // 그립을 놓거나 망치가 손의 탐색 범위를 벗어나도 잡은 상태 유지.
            previousKeepSelected = holdingHand.keepSelectedTargetValid;
            holdingHand.keepSelectedTargetValid = true;
            holdingHand.StartManualInteraction((IXRSelectInteractable)grab);
        }

        public void UnlockGrab()
        {
            if (holdingHand == null) return;

            // 게임 종료 시 유지 기능을 해제하고 기존 잡기 설정 복구.
            if (holdingHand.isPerformingManualInteraction)
                holdingHand.EndManualInteraction();
            holdingHand.keepSelectedTargetValid = previousKeepSelected;
            holdingHand = null;
        }

        private void OnDisable()
        {
            GetComponent<XRGrabInteractable>().selectEntered.RemoveListener(OnGrab);
            hasPreviousPosition = false;
            UnlockGrab();
        }
    }
}
