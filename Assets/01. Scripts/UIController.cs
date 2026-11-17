using System.Threading;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Whack_A_Mole
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance;
        
        [SerializeField] private TMP_Text announceText;
        [SerializeField] private float announceDuration = 3f;
        private CancellationToken destroyToken;
        private bool isShowing;
        

        private void Awake()
        {
            Instance = this;
            destroyToken = this.GetCancellationTokenOnDestroy();
            ShowText(false);
        }
        
        public void ShowText(bool isShow, string message = "")
        {
            announceText.gameObject.SetActive(isShow);
            announceText.SetText(message);
        }

        public async UniTaskVoid ShowWrongSideAnnounce()
        {
            if (isShowing) return;

            isShowing = true;
            ShowText(true, "올바른 방향의 망치를 잡으세요.");

            await UniTask.WaitForSeconds(announceDuration, cancellationToken: destroyToken);
            ShowText(false);
            isShowing = false;
        }

    }
}