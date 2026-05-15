using System.Collections;
using UnityEngine;

namespace Audio
{
    //[RequireComponent(typeof())]
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [Header("Music Settings")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioClip[] _levelTracks;
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private float _fadeOutBeforeEnd = 5f; 

        [Header("Ambience Settings")]
        [SerializeField] private AudioSource _ambienceSource;
        [SerializeField] private AudioClip[] _ambienceClips;

        [Header("Sound Pool")]
        [SerializeField] private AudioSource _audioSourcePrefab;
        private AudioPool _soundPool;

        private Coroutine _musicFadeCoroutine;
        private Coroutine _trackEndCheckCoroutine;
        private int _currentTrackIndex = -1;

        private void Awake()
        {
            PlayNextTrack();
            PlayRandomAmbience();
            DontDestroyOnLoad(gameObject);
        }

        public void PlayNextTrack()
        {
            if (_levelTracks.Length == 0) return;

            _currentTrackIndex = (_currentTrackIndex + 1) % _levelTracks.Length;
            AudioClip nextClip = _levelTracks[_currentTrackIndex];

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);
            if (_trackEndCheckCoroutine != null)
                StopCoroutine(_trackEndCheckCoroutine);

            _musicFadeCoroutine = StartCoroutine(FadeMusic(nextClip));
            _trackEndCheckCoroutine = StartCoroutine(CheckTrackEnd(nextClip.length));
        }

        private IEnumerator FadeMusic(AudioClip nextClip)
        {
            _musicSource.clip = nextClip;
            _musicSource.Play();
            _musicSource.volume = 0f;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                _musicSource.volume = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _musicSource.volume = 1f;
        }

        private IEnumerator CheckTrackEnd(float trackLength)
        {
            // ∆дем, пока до конца трека останетс€ _fadeOutBeforeEnd секунд
            float waitTime = trackLength - _fadeOutBeforeEnd - _fadeDuration;
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            // ѕлавное затухание перед концом трека
            float elapsed = 0f;
            float startVolume = _musicSource.volume;
            while (elapsed < _fadeDuration)
            {
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / _fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _musicSource.volume = 0f;

            PlayNextTrack();
        }

        public void PlayRandomAmbience()
        {
            if (_ambienceClips.Length == 0) return;
            _ambienceSource.clip = _ambienceClips[Random.Range(0, _ambienceClips.Length)];
            _ambienceSource.Play();
        }

    }
}