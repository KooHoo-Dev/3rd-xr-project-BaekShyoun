using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whack_A_Mole
{
    public enum BGMType { Title, Game, Ranking }
    public enum SFXType { ButtonClick, HammerGrab, Countdown, GameStart, MolePopUp, MoleHit, MoleMiss, ComboStep, GameOver }

    [Serializable]
    public class BGMData
    {
        public BGMType type;
        public AudioClip clip;
    }

    [Serializable]
    public class SFXData
    {
        public SFXType type;
        public AudioClip clip;
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private BGMData[] bgmSounds;
        [SerializeField] private SFXData[] sfxSounds;
        [Header("게임 BGM 랜덤 재생 목록")]
        [SerializeField] private AudioClip[] gameBGMClips = new AudioClip[0];
        private List<AudioClip> nextBGMCandidates = new List<AudioClip>();
        private bool isGameBGM;
        private bool isAppPaused;

        public float BGMVolume { get; private set; }
        public float SFXVolume { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BGMVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("NeonHammer.BGM", .5f));
            SFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("NeonHammer.SFX", .8f));
            bgmSource.volume = BGMVolume;
            sfxSource.volume = SFXVolume;
        }

        private void Update()
        {
            // 게임 음악이 끝나면 다음 곡 재생. 앱이 일시정지 중이면 대기.
            if (isGameBGM == false || isAppPaused || AudioListener.pause) return;
            if (bgmSource.isPlaying == false) PlayRandomGameBGM();
        }

        public void PlayBGM(BGMType type)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            isGameBGM = false;
            if (type == BGMType.Game && PlayRandomGameBGM()) return;

            // 랜덤 목록이 비어 있으면 기존 Game 슬롯 사용. 다른 씬의 BGM도 반복 재생.
            bgmSource.loop = true;
            foreach (var sound in bgmSounds)
            {
                if (sound.type != type) continue;
                bgmSource.clip = sound.clip;
                if (sound.clip != null) bgmSource.Play();
                return;
            }
        }

        private bool PlayRandomGameBGM()
        {
            nextBGMCandidates.Clear();
            foreach (var clip in gameBGMClips)
            {
                // 빈 슬롯과 중복 등록된 곡 제외.
                if (clip != null && nextBGMCandidates.Contains(clip) == false)
                    nextBGMCandidates.Add(clip);
            }

            if (nextBGMCandidates.Count == 0)
            {
                isGameBGM = false;
                return false;
            }

            // 두 곡 이상이면 직전 곡 제외. 한 곡뿐이면 그 곡을 다시 재생.
            if (nextBGMCandidates.Count > 1) nextBGMCandidates.Remove(bgmSource.clip);
            int index = UnityEngine.Random.Range(0, nextBGMCandidates.Count);
            bgmSource.clip = nextBGMCandidates[index];
            bgmSource.loop = false;
            bgmSource.Play();
            isGameBGM = true;
            return true;
        }

        public void PlaySFX(SFXType type)
        {
            foreach (var sound in sfxSounds)
            {
                if (sound.type != type) continue;
                // 아직 사운드를 연결하지 않았으면 조용히 넘어감.
                if (sound.clip != null) sfxSource.PlayOneShot(sound.clip);
                return;
            }
        }

        public void SetBGMVolume(float volume)
        {
            BGMVolume = Mathf.Clamp01(volume);
            bgmSource.volume = BGMVolume;
            PlayerPrefs.SetFloat("NeonHammer.BGM", BGMVolume);
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            sfxSource.volume = SFXVolume;
            PlayerPrefs.SetFloat("NeonHammer.SFX", SFXVolume);
        }

        private void OnApplicationPause(bool isPaused) { isAppPaused = isPaused; if (isPaused) PlayerPrefs.Save(); }
        private void OnApplicationQuit() { PlayerPrefs.Save(); }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
