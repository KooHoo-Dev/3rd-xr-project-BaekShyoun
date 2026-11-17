using UnityEngine;

namespace Whack_A_Mole
{
    public enum MachineSide
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
        
        // 어느쪽 기계에 있는 두더지인지 분류
        [SerializeField] private MachineSide machineSide;
    }
}