using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whack_A_Mole
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        // 정상 타격 시 환경 효과에 알림
        public event System.Action MoleHit;

        public int score = 0;
        public int maxHearts = 3;
        
        [SerializeField] private MachineController leftMachine;
        [SerializeField] private MachineController rightMachine;
        [SerializeField] private GameReadyController gameReadyController;
        [SerializeField] private float startSpawnTime = 3f; // 처음 등장 시간
        [SerializeField] private float minSpawnTime = 0.5f; // 최소 등장 시간
        [SerializeField] private float decreaseTime = 0.05f; // 5콤보마다 점점 짧아지는 간격

        // 현재 간격
        private float currentSpawnTime;
        private int combo;
        private int bestCombo;
        
        private bool isLeft;
        private bool canPlay; // 카운트다운이 끝난 뒤에만 타격/실패 처리
        public bool IsPlaying => canPlay;
        private bool hasStarted;
        private bool hasEnded;

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
            // 중복 클릭으로 게임 루프가 여러 개 생기지 않도록 방지
            if (hasStarted || hasEnded) return;
            hasStarted = true;
            await gameReadyController.WaitUntilReady();
            if (hasEnded) return;
            await UIController.Instance.ShowCountdown();
            if (hasEnded) return;
            canPlay = true;
            
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
            MoleHit?.Invoke();
            combo++;
            bestCombo = Mathf.Max(bestCombo, combo);
            SoundManager.Instance?.PlaySFX(SFXType.MoleHit);
            if (combo % 10 == 0) SoundManager.Instance?.PlaySFX(SFXType.ComboStep);

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
            
            SoundManager.Instance?.PlaySFX(SFXType.MoleMiss);
            --maxHearts;
            RefreshHUD();
            Debug.Log($"현재 하트 개수 : {maxHearts}");

            if (maxHearts <= 0) EndGame();
        }

        public void EndGame()
        {
            if (hasEnded) return;
            hasEnded = true;
            gameReadyController.CancelReady();
            canPlay = false;
            // 남아 있는 두더지도 숨겨 이전 타이머의 실패 처리를 막음.
            foreach (var mole in FindObjectsByType<MoleController>(FindObjectsSortMode.None))
                mole.HideInTheHole();
            UIController.Instance.ShowGameOver();
            SoundManager.Instance?.PlaySFX(SFXType.GameOver);
            RecordManager.SaveRecord(score, bestCombo);
            GoRankingAfterDelay().Forget();
            Debug.Log("Game Over!");
        }
        // Game Over 문구를 잠깐 보여준 뒤 기록 씬으로 이동.
        private async UniTaskVoid GoRankingAfterDelay()
        {
            await UniTask.WaitForSeconds(2f, cancellationToken: destroyToken);
            SceneManager.LoadScene("Ranking");
        }
    }
}



