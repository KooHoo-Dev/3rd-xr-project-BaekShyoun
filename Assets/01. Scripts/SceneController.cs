using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Whack_A_Mole
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text rankingText;
        [SerializeField] private Material uiRayMaterial;
        private bool isLoading;

        private void Start()
        {
            SetupUIRays();
            string sceneName = SceneManager.GetActiveScene().name;
            BGMType bgm = sceneName == "Title" ? BGMType.Title :
                sceneName == "Ranking" ? BGMType.Ranking : BGMType.Game;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBGM(bgm);
            if (sceneName == "Ranking") ShowRanking();
            if (sceneName == "Game") UIController.Instance.PressGameStart();
        }

        private void SetupUIRays()
        {
            // 기존 Near-Far 광선 하나로 망치와 UI를 함께 조작.
            foreach (var hand in FindObjectsByType<NearFarInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                hand.enableUIInteraction = true;
                hand.blockUIOnInteractableSelection = false;
                if (hand.GetComponent<HandPointerVisual>() == null)
                    hand.gameObject.AddComponent<HandPointerVisual>();
            }
        }

        private void ShowRanking()
        {
            GameRecord last = RecordManager.LastRecord;
            resultText.text = last == null ? "나의 최고 기록에 도전하세요" :
                $"이번 점수  {last.score:N0}    /    최고 콤보  {last.bestCombo}";
            var records = RecordManager.LoadRecords();
            rankingText.text = records.Count == 0 ? "아직 기록이 없습니다." : "";
            for (int i = 0; i < records.Count; i++)
            {
                GameRecord record = records[i];
                string row = $"{i + 1:00}     <pos=15%>{record.score:N0}<pos=48%>{record.bestCombo}<pos=68%>{record.date}";
                if (last != null && record.id == last.id) row = "<color=#38DEFF>" + row + "</color>";
                rankingText.text += row + "\n";
            }
        }

        public void GoGame() { LoadScene("Game"); }
        public void GoTitle() { LoadScene("Title"); }
        public void GoRanking() { LoadScene("Ranking"); }

        private void LoadScene(string sceneName)
        {
            if (isLoading) return;
            isLoading = true;
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
            Time.timeScale = 1;
            SceneManager.LoadScene(sceneName);
        }
    }
}
