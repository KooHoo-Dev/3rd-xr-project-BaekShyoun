using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Whack_A_Mole
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
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
            while (canPlay)
            {
                MachineController targetMachine = isLeft ? leftMachine : rightMachine;
                isLeft = targetMachine.machineSide == HandSide.Left;
                await targetMachine.StartGame();
                isLeft = !isLeft;
            }
        }
    }
}