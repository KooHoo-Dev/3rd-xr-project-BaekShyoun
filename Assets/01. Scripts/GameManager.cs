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
        [SerializeField] private float startSpawnTime = 3f; // 처음 등장 시간
        [SerializeField] private float minSpawnTime = 0.5f; // 최소 등장 시간
        [SerializeField] private float decreaseTime = 0.05f; // 5콤보마다 점점 짧아지는 간격

        // 현재 간격
        private float currentSpawnTime;
        private int combo;
        
        private bool isLeft;
        private bool canPlay = true; // 반복문 탈출 조건

        private CancellationToken destroyToken;
        
        private void Awake()
        {
            destroyToken = this.GetCancellationTokenOnDestroy();
            Instance = this;
            currentSpawnTime = startSpawnTime;
        }
        
        private void Start()
        {
            // 모든 Awake가 끝난 뒤 초기 UI 표시
            RefreshHUD();
        }

        private void RefreshHUD()
        {
            if (UIController.Instance != null)
                UIController.Instance.RefreshHUD(score, combo, maxHearts);
        }

        public async UniTaskVoid RepeatGame()
        {
            // 준비시간 3초
            await UniTask.WaitForSeconds(3f, cancellationToken: destroyToken);
            
            while (canPlay)
            {
                // 두더지 등장 머신 정하기
                MachineController targetMachine = isLeft ? leftMachine : rightMachine;
                targetMachine.StartGame();
                
                // 오락기 교체
                isLeft = !isLeft;

                await UniTask.WaitForSeconds(currentSpawnTime, cancellationToken: destroyToken);
            }
        }

        public void HitMole()
        {
            if (canPlay == false) return;

            score += 100;
            combo++;

            // 5콤보마다 속도 증가
            int speedLevel = combo / 5;
            currentSpawnTime = Mathf.Max(minSpawnTime, startSpawnTime - speedLevel * decreaseTime);
            RefreshHUD();
            
            Debug.Log($"점수 : {score}, 콤보 : {combo}, 등장 간격 : {currentSpawnTime}");
        }
        
        // 두더지 놓침 (하트 깎기)
        public void MissMole()
        {
            if (canPlay == false) return;
            
            // 놓치면 콤보 및 시간 초기화
            combo = 0;
            currentSpawnTime = startSpawnTime;
            
            --maxHearts;
            RefreshHUD();
            Debug.Log($"현재 하트 개수 : {maxHearts}");

            // 잠시 꺼둠
            // if (maxHearts <= 0) EndGame();
        }

        public void EndGame()
        {
            canPlay = false;
            Debug.Log("겜 오버ㅇㅇ");
        }
    }
}
