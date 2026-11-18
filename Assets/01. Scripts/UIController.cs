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

        [SerializeField] private MachineController[] machine;
        [SerializeField] private TMP_Text announceText;
        [SerializeField] private Button gameStartButton;
        [SerializeField] private float announceDuration = 3f;
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
            for (int i = 0; i < machine.Length; i++)
            {
                machine[i].RepeatGame().Forget();
            }
        }
        
        
        # endregion
    }
}