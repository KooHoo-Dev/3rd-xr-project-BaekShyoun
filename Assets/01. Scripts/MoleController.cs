using System;
using UnityEngine;

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
        
        [SerializeField] private HandSide moleSide; // 어느쪽 기계에 있는 두더지인지 분류
        [SerializeField] private float hideDepth = 0.15f; // 두더지가 내려갈 높이
        [SerializeField] private float speed = 0.3f; // 두더지가 오르내리는 스피드
        private Vector3 raisedPosition; // 두더지가 올라온 위치
        private Vector3 targetPosition; // 두더지가 이동할 목표 지점

        private void Awake()
        {
            // 두더지가 올라와야 할 위치 저장
            raisedPosition = transform.localPosition;
            HideInTheHole(); // 일단 숨고 시작
            transform.localPosition = targetPosition;
        }

        private void Update()
        {
            transform.localPosition = Vector3.MoveTowards
                    (transform.localPosition, targetPosition, speed * Time.deltaTime);
        }
        
        
        // 두더지가 숨음
        public void HideInTheHole()
        {
            targetPosition = raisedPosition + Vector3.down * hideDepth;
        }

        public void PopUp()
        {
            targetPosition = raisedPosition;
        }
    }
}