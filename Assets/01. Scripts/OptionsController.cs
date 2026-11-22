using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Whack_A_Mole
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text bgmValue;
        [SerializeField] private TMP_Text sfxValue;
        private bool isOpen;
        private float previousTimeScale;

        private void Start()
        {
            panel.SetActive(false);
            bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BGMVolume);
            sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SFXVolume);
            RefreshValues();
        }

        public void ToggleOptions()
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);
            SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
            if (isOpen)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = previousTimeScale;
                PlayerPrefs.Save();
            }
        }

        public void ChangeBGM(float value)
        {
            SoundManager.Instance.SetBGMVolume(value);
            RefreshValues();
        }

        public void ChangeSFX(float value)
        {
            SoundManager.Instance.SetSFXVolume(value);
            RefreshValues();
        }

        private void RefreshValues()
        {
            bgmValue.text = Mathf.RoundToInt(bgmSlider.value * 100) + "%";
            sfxValue.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
        }

        private void OnDisable()
        {
            if (isOpen) { Time.timeScale = previousTimeScale; isOpen = false; }
        }
    }
}
