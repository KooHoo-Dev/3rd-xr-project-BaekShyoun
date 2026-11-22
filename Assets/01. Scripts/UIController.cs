using System.Threading;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Whack_A_Mole
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance;

        [SerializeField] private TMP_Text announceText;
        [SerializeField] private Button gameStartButton;
        [SerializeField] private float announceDuration = 3f;
        [Header("게임 상태 표시")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private Image[] heartImages;
        [SerializeField] private TMP_Text centerMessageText;
        private bool isGameOver;
        private CancellationToken destroyToken;
        private bool isShowing;
        

        private void Awake()
        {
            Instance = this;
            destroyToken = this.GetCancellationTokenOnDestroy();
            ShowAnnounceText(false);
            centerMessageText.gameObject.SetActive(false);
            gameStartButton.gameObject.SetActive(true);
        }
        
        # region Text Methods

        // 게임 시작, 타격 성공, 놓침 때 현재 값을 화면에 반영
        public void RefreshHUD(int score, int combo, int hearts)
        {
            if (scoreText != null) scoreText.SetText("{0}", score);
            if (comboText != null) comboText.SetText("{0}", combo);

            if (heartImages == null) return;
            int remaining = Mathf.Clamp(hearts, 0, heartImages.Length);
            for (int i = 0; i < heartImages.Length; i++)
            {
                // 잃은 하트는 회색으로 표시해 남은 개수를 구분
                if (heartImages[i] != null)
                    heartImages[i].color = i < remaining
                        ? Color.white : new Color(0.22f, 0.22f, 0.22f, 0.65f);
            }
        }
        
        // 안내 텍스트 설정
        public void ShowAnnounceText(bool isShow, string message = "")
        {
            announceText.gameObject.SetActive(isShow);
            announceText.SetText(message);
        }
        
        // 망치의 방향이 잘못됐을 때 출력되는 안내 Text
        public async UniTaskVoid ShowWrongSideAnnounce()
        {
            if (isShowing || isGameOver) return;

            isShowing = true;
            ShowAnnounceText(true, "올바른 방향의 망치를 잡으세요.");

            await UniTask.WaitForSeconds(announceDuration, cancellationToken: destroyToken);
            ShowAnnounceText(false);
            isShowing = false;
        }
        // 준비 안내도 카운트다운과 같은 중앙 텍스트 사용.
        public void ShowCenterMessage(string message)
        {
            centerMessageText.color = new Color(0.22f, 0.87f, 1f);
            centerMessageText.SetText(message);
            centerMessageText.gameObject.SetActive(true);
        }

        // 카운트다운과 시작/종료 문구는 같은 TMP 텍스트를 재사용.
        public async UniTask ShowCountdown()
        {
            centerMessageText.gameObject.SetActive(true);
            centerMessageText.color = new Color(0.22f, 0.87f, 1f);
            for (int count = 3; count > 0; count--)
            {
                if (isGameOver) return;
                SoundManager.Instance?.PlaySFX(SFXType.Countdown);
                centerMessageText.SetText("{0}", count);
                await UniTask.WaitForSeconds(1f, cancellationToken: destroyToken);
            }
            if (isGameOver) return;
            SoundManager.Instance?.PlaySFX(SFXType.GameStart);
            centerMessageText.SetText("Game Start!");
            await UniTask.WaitForSeconds(0.8f, cancellationToken: destroyToken);
            if (!isGameOver) centerMessageText.gameObject.SetActive(false);
        }

        public void ShowGameOver()
        {
            isGameOver = true;
            ShowAnnounceText(false);
            centerMessageText.color = new Color(1f, 0.35f, 0.55f);
            centerMessageText.SetText("Game Over!");
            centerMessageText.gameObject.SetActive(true);
        }
        # endregion
        
        # region Button Methods

        // 버튼 누르면 게임 시작하게 만드는 임시 로직 (수정할수도 안할수도?)
        public void PressGameStart()
        {
            SoundManager.Instance?.PlaySFX(SFXType.ButtonClick);
            Debug.Log("게임 시작");
            gameStartButton.gameObject.SetActive(false);
            GameManager.Instance.RepeatGame().Forget();
        }
        
        
        # endregion
    }
}


