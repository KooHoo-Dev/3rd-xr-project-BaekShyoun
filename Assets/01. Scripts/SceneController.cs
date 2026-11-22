using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

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
            // 망치를 잡은 손과 UI 선택을 분리하여 플레이 중에도 옵션을 누를 수 있게 함.
            foreach (var hand in FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None))
            {
                hand.enableUIInteraction = false;
                GameObject pointer = new GameObject("UI Ray " + hand.handedness);
                pointer.transform.SetParent(hand.transform.root, false);
                var ray = pointer.AddComponent<XRRayInteractor>();
                ray.rayOriginTransform = hand.farInteractionCaster.castOrigin;
                ray.interactionLayers = 0;
                ray.enableUIInteraction = true;
                ray.uiPressInput = hand.uiPressInput;
                var line = pointer.AddComponent<XRInteractorLineVisual>();
                line.lineWidth = .002f;
                line.lineLength = 3;
                pointer.GetComponent<LineRenderer>().sharedMaterial = uiRayMaterial;
                var color = new Gradient();
                color.SetKeys(new[] { new GradientColorKey(new Color(.22f, .87f, 1), 0) },
                    new[] { new GradientAlphaKey(.6f, 0), new GradientAlphaKey(.2f, 1) });
                line.validColorGradient = color;
                line.invalidColorGradient = color;
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
