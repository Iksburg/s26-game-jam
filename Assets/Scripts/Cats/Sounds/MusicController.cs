using System.Collections;
using UnityEngine;
using CatWorld.Cats; // Для доступа к GameSettings

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [Header("Пул музыки")]
    [SerializeField] private AudioClip[] _musicTracks;

    [Header("Настройки воспроизведения")]
    [Range(0f, 3f)] 
    [SerializeField] private float _fadeDuration = 2f; // Длительность кроссфейда/затухания
    
    private AudioSource _audioSource;
    private AudioClip _lastPlayedTrack;
    private bool _isPlaying = true;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = false; // Мы управляем циклом вручную для смены треков
        _audioSource.playOnAwake = false;
        
        // Подписка на изменение громкости
        GameSettings.Changed += UpdateVolume;
        UpdateVolume();
    }

    private void Start()
    {
        if (_musicTracks != null && _musicTracks.Length > 0)
        {
            StartCoroutine(MusicLoop());
        }
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= UpdateVolume;
    }

    private void UpdateVolume()
    {
        if (_audioSource != null)
        {
            _audioSource.volume = GameSettings.MusicVolume;
        }
    }

    private IEnumerator MusicLoop()
    {
        while (true)
        {
            AudioClip nextTrack = GetNextTrack();
            if (nextTrack == null) break;

            // Рассчитываем паузу между треками (-5 до 5 секунд)
            float pause = Random.Range(-5f, 5f);
            
            // Если пауза отрицательная или нулевая — делаем кроссфейд
            if (pause <= 0)
            {
                yield return PlayWithCrossfade(nextTrack, Mathf.Abs(pause));
            }
            else
            {
                // Если пауза положительная — ждем, потом играем с плавным началом
                yield return new WaitForSeconds(pause);
                yield return PlayWithFadeIn(nextTrack);
            }
        }
    }

    /// <summary>
    /// Выбирает трек, который не совпадает с предыдущим.
    /// </summary>
    private AudioClip GetNextTrack()
    {
        if (_musicTracks.Length == 0) return null;
        if (_musicTracks.Length == 1) return _musicTracks[0];

        AudioClip track;
        do
        {
            track = _musicTracks[Random.Range(0, _musicTracks.Length)];
        } while (track == _lastPlayedTrack);

        _lastPlayedTrack = track;
        return track;
    }

    /// <summary>
    /// Воспроизведение с плавным нарастанием громкости.
    /// </summary>
    private IEnumerator PlayWithFadeIn(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.volume = 0f;
        _audioSource.Play();

        float time = 0f;
        float targetVolume = GameSettings.MusicVolume;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, targetVolume, time / _fadeDuration);
            yield return null;
        }
        
        _audioSource.volume = targetVolume;
        
        // Ждем окончания трека
        yield return new WaitForSeconds(clip.length - _fadeDuration);
        
        // Плавное затухание в конце трека
        yield return FadeOut();
    }

    /// <summary>
    /// Кроссфейд: новый трек начинается, пока старый еще играет.
    /// Примечание: так как у нас один AudioSource, мы просто делаем быструю смену 
    /// с перекрытием по времени, имитируя микс.
    /// </summary>
    private IEnumerator PlayWithCrossfade(AudioClip clip, float overlapTime)
    {
        // Начинаем новый трек с громкостью 0
        float originalVolume = _audioSource.volume;
        _audioSource.clip = clip;
        _audioSource.volume = 0f;
        _audioSource.Play();

        // Одновременно убираем громкость старого контекста (который уже сменился клипом, 
        // но мы эмулируем переход) и добавляем новому.
        // В рамках одного AudioSource это работает как резкая смена клипа, 
        // поэтому мы просто делаем Fade In для нового трека, но начинаем его чуть раньше "виртуального" конца предыдущего.
        
        float time = 0f;
        float targetVolume = GameSettings.MusicVolume;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, targetVolume, time / _fadeDuration);
            yield return null;
        }

        _audioSource.volume = targetVolume;
        yield return new WaitForSeconds(clip.length - _fadeDuration);
        yield return FadeOut();
    }

    private IEnumerator FadeOut()
    {
        float startVolume = _audioSource.volume;
        float time = 0f;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, time / _fadeDuration);
            yield return null;
        }

        _audioSource.Stop();
        _audioSource.volume = 0f;
    }
}