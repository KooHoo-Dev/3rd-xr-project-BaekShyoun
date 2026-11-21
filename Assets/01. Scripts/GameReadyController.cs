using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace Whack_A_Mole
{
    public class GameReadyController : MonoBehaviour
    {
        [Header("준비 상태 확인")]
        [SerializeField] private XRGrabInteractable leftHammer;
        [SerializeField] private XRGrabInteractable rightHammer;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private GameObject locomotion;
        [SerializeField] private Transform readyPoint;
        [SerializeField] private float readyRadius = 0.3f;

        [Header("준비 안내 표시")]
        [SerializeField] private LineRenderer leftArrow;
        [SerializeField] private LineRenderer rightArrow;
        [SerializeField] private GameObject readyCircle;

        private bool isPreparing;
        private bool isCancelled;
        private CancellationToken destroyToken;

        private void Awake()
        {
            destroyToken = this.GetCancellationTokenOnDestroy();
            // 평지 이동만 사용하므로 플레이어에게 낙하 중력을 적용하지 않음.
            foreach (var gravity in locomotion.GetComponentsInChildren<GravityProvider>(true))
                gravity.useGravity = false;
            ShowGuide(false);
        }

        private void Update()
        {
            if (isPreparing == false) return;

            // 잡은 망치의 화살표는 숨기고, 놓은 망치의 화살표만 표시.
            leftArrow.gameObject.SetActive(leftHammer.isSelected == false);
            rightArrow.gameObject.SetActive(rightHammer.isSelected == false);
            MoveArrow(leftArrow, leftHammer.transform);
            MoveArrow(rightArrow, rightHammer.transform);
        }

        public async UniTask WaitUntilReady()
        {
            isPreparing = true;
            // 준비 중에는 이동 가능, 원에 도착하면 아래에서 다시 잠금.
            locomotion.SetActive(true);
            while (isCancelled == false)
            {
                UIController.Instance.ShowCenterMessage("망치를 잡으세요");
                readyCircle.SetActive(false);

                // 양손 망치를 모두 잡을 때까지 대기.
                await UniTask.WaitUntil(() => IsHoldingBoth() || isCancelled,
                    cancellationToken: destroyToken);
                if (isCancelled) return;

                // 양손을 모두 잡은 뒤부터 게임 종료까지 망치를 유지.
                leftHammer.GetComponent<HammerController>().LockGrab();
                rightHammer.GetComponent<HammerController>().LockGrab();

                leftArrow.gameObject.SetActive(false);
                rightArrow.gameObject.SetActive(false);
                readyCircle.SetActive(true);
                UIController.Instance.ShowCenterMessage("게임기 앞으로 이동하세요");

                // 원 안에 들어오거나, 이동 중 망치를 놓으면 다음 조건 확인.
                await UniTask.WaitUntil(() => IsInsideCircle() || IsHoldingBoth() == false || isCancelled,
                    cancellationToken: destroyToken);
                if (isCancelled) return;
                if (IsHoldingBoth() == false) continue;

                isPreparing = false;
                ShowGuide(false);
                // XR 이동 기능만 잠금. 카메라와 손의 실제 추적은 계속 유지.
                locomotion.SetActive(false);
                return;
            }
        }

        private bool IsHoldingBoth()
        {
            return leftHammer.isSelected && rightHammer.isSelected;
        }

        private bool IsInsideCircle()
        {
            // 카메라 높이는 무시하고 발밑 위치가 원 안인지 확인.
            Vector3 playerPosition = playerCamera.position;
            Vector3 circlePosition = readyPoint.position;
            playerPosition.y = circlePosition.y;
            return Vector3.Distance(playerPosition, circlePosition) <= readyRadius;
        }

        private void MoveArrow(LineRenderer arrow, Transform hammer)
        {
            // 망치 위에서 살짝 위아래로 움직이는 아래쪽 화살표.
            Vector3 tip = hammer.position + Vector3.up * (0.45f + Mathf.Sin(Time.time * 3f) * 0.04f);
            // 화살표 위의 L/R 문구도 함께 이동하며 카메라를 향함.
            arrow.transform.position = tip + Vector3.up * 0.48f;
            arrow.transform.rotation = playerCamera.rotation;
            Vector3 right = playerCamera.right * 0.1f;
            arrow.SetPosition(0, tip + Vector3.up * 0.35f);
            arrow.SetPosition(1, tip);
            arrow.SetPosition(2, tip + Vector3.up * 0.12f - right);
            arrow.SetPosition(3, tip);
            arrow.SetPosition(4, tip + Vector3.up * 0.12f + right);
        }

        private void ShowGuide(bool isShow)
        {
            leftArrow.gameObject.SetActive(isShow);
            rightArrow.gameObject.SetActive(isShow);
            readyCircle.SetActive(isShow);
        }

        public void CancelReady()
        {
            leftHammer.GetComponent<HammerController>().UnlockGrab();
            rightHammer.GetComponent<HammerController>().UnlockGrab();
            isCancelled = true;
            isPreparing = false;
            ShowGuide(false);
        }
    }
}



