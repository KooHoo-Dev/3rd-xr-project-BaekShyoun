using System;
using UnityEngine;
using UnityPipeline.Microsoft.CodeAnalysis.FlowAnalysis;

namespace Whack_A_Mole
{
    public enum HandSide
    {
        Left,
        Right
    }

    public class MoleController : MonoBehaviour
    {
        // 1. 올라감
        // 2. 망치 든 상태에서 충돌시 내려감 (타격감 중요, 망치 컨트롤러도 필요)
        // ++ 현재 소속된 오락기가 왼쪽 오락기인지 오른쪽 오락기인지 Enum으로 설정 가능하게 한다.
        // 일단 오른쪽 뜨면 그 다음에는 왼쪽이 뜨게 하는 방식을 고려 중(번갈아 뜨게) - 추후 밸런스 잡으며 수정

        public HandSide moleSide; // 어느쪽 기계에 있는 두더지인지 분류
        [SerializeField] private float hideDepth = 0.15f; // 두더지가 내려갈 높이
        [SerializeField] private float speed = 0.3f; // 두더지가 오르내리는 스피드
        private MachineController machine; // isPopUp과 index 초기화용
        private Vector3 raisedPosition; // 두더지가 올라온 위치
        private Vector3 targetPosition; // 두더지가 이동할 목표 지점
        private bool isHide; // 두더지가 현재 숨은 상태인지 아닌지
        public bool isHit { get; private set; } // 두더지 맞았는지

        private void Awake()
        {
            machine = GetComponentInParent<MachineController>();
            // 두더지가 올라와야 할 위치 저장
            raisedPosition = transform.localPosition;
            HideInTheHole(); // 일단 숨고 시작
            transform.localPosition = targetPosition;
            isHit = false;
        }

        private void Update()
        {
            // 두더지 부드럽게 들어갔다 나오기
            transform.localPosition = Vector3.MoveTowards
                (transform.localPosition, targetPosition, speed * Time.deltaTime);
        }


        // 두더지가 숨음
        public void HideInTheHole()
        {
            targetPosition = raisedPosition + Vector3.down * hideDepth;
            isHide = true;
            machine.DoneHide();
        }

        // 두더지 나타남
        public void PopUp()
        {
            targetPosition = raisedPosition;
            isHide = false;
        }

        // 두더지에 망치 접촉시 두더지 숨기
        private void OnCollisionEnter(Collision collision)
        {
            // Tag가 Hammer가 아니면 반환
            if (collision.gameObject.CompareTag("Hammer") == false) return;
            // 두더지가 숨어 있으면 반환
            if (isHide) return;
            // Hammer와 Mole의 방향이 다르면 반환
            HammerController hammer = collision.gameObject.GetComponent<HammerController>();
            if (moleSide != hammer.hammerSide)
            {
                Debug.Log("잘못된 방향의 망치로 때렸습니다.");
                return;
            }
            Debug.Log($"{name}에 {collision.gameObject.name} 충돌함");
            isHit = true;
            HideInTheHole();
        }

}
}