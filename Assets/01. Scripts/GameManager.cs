using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Whack_A_Mole
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public int score = 0;
        public int maxHearts = 3;
        
        [SerializeField] private MachineController leftMachine;
        [SerializeField] private MachineController rightMachine;
        private bool isLeft;
        // 추후 반복문 탈출 조건(하트) 만들 거임
        private bool canPlay = true;

        private void Awake()
        {
            Instance = this;
        }
        
        public async UniTaskVoid RepeatGame()
        {
            await UniTask.WaitForSeconds(3f);
            while (canPlay)
            {
                MachineController targetMachine = isLeft ? leftMachine : rightMachine;
                isLeft = targetMachine.machineSide == HandSide.Left;
                await targetMachine.StartGame();
                isLeft = !isLeft;
            }
        }

        // 두더지 놓침 (하트 깎기)
        public void MissMole()
        {
            if (canPlay == false) return;
            --maxHearts;
            Debug.Log($"현재 하트 개수 : {maxHearts}");

            if (maxHearts <= 0) EndGame();
        }

        public void EndGame()
        {
            canPlay = false;
            Debug.Log("겜 오버ㅇㅇ");
        }
    }
}