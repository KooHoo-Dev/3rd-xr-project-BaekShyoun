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
        private CancellationToken destroyToken;
        private bool isShowing;
        

        private void Awake()
        {
            Instance = this;
            destroyToken = this.GetCancellationTokenOnDestroy();
            ShowAnnounceText(false);
            gameStartButton.gameObject.SetActive(true);
        }
        
        # region Text Methods

        // 게임 시작, 타격 성공, 놓침 때 현재 값을 화면에 반영
        public void RefreshHUD(int score, int combo, int hearts)
        {
            if (scoreText != null) scoreText.SetText("점수 {0}", score);
            if (comboText != null) comboText.SetText("콤보 {0}", combo);

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
            if (isShowing) return;

            isShowing = true;
            ShowAnnounceText(true, "올바른 방향의 망치를 잡으세요.");

            await UniTask.WaitForSeconds(announceDuration, cancellationToken: destroyToken);
            ShowAnnounceText(false);
            isShowing = false;
        }
        # endregion
        
        # region Button Methods

        // 버튼 누르면 게임 시작하게 만드는 임시 로직 (수정할수도 안할수도?)
        public void PressGameStart()
        {
            Debug.Log("게임 시작");
            gameStartButton.gameObject.SetActive(false);
            GameManager.Instance.RepeatGame().Forget();
        }
        
        
        # endregion
    }
}
