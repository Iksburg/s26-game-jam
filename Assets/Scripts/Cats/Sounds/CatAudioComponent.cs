using System.Collections.Generic;
using UnityEngine;
using CatWorld.Cats;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Cat))]
public class CatAudioComponent : MonoBehaviour
{
    private Cat _cat;
    private AudioSource _audioSource;
    
    // 5 случайных звуков для этого конкретного кота
    private List<AudioClip> _mySounds = new List<AudioClip>();

    private void Awake()
    {
        _cat = GetComponent<Cat>();
        _audioSource = GetComponent<AudioSource>();
        
        // Инициализация звуков при спавне/старте
        InitializeSounds();
    }

    private void Start()
    {
        StartCoroutine(MeowLoop());
    }

    private void InitializeSounds()
    {
        if (CatSoundController.Instance == null) return;

        _mySounds.Clear();
        var pool = CatSoundController.Instance;
        
        // Выбираем 5 уникальных звуков
        while (_mySounds.Count < 5)
        {
            var clip = pool.GetRandomSound();
            if (clip != null && !_mySounds.Contains(clip))
            {
                _mySounds.Add(clip);
            }
            
            // Защита от бесконечного цикла, если в пуле меньше 5 звуков
            if (pool.GetRandomSound() == null && _mySounds.Count == 0) break; 
        }
    }

    private System.Collections.IEnumerator MeowLoop()
    {
        while (true)
        {
            // Рассчитываем задержку на основе потребностей
            float delay = CalculateMeowDelay();
            
            yield return new WaitForSeconds(delay);
            
            PlayMeow();
        }
    }

    private float CalculateMeowDelay()
    {
        // Чем меньше параметры (ближе к 0), тем чаще мяуканье.
        // Средний уровень "бедствия" (0 - все хорошо, 100 - критично)
        // Инвертируем: 100 - значение = насколько плохо коту.
        float hungerStress = 100f - _cat.Satiety;
        float thirstStress = 100f - _cat.Water;
        float cleanStress = 100f - _cat.Cleanliness;

        // Суммарный стресс (максимум 300)
        float totalStress = hungerStress + thirstStress + cleanStress;
        
        // Нормализуем стресс от 0 до 1
        float stressFactor = Mathf.Clamp01(totalStress / 300f);

        // Маппинг: 
        // Stress 0 -> Delay 12 сек
        // Stress 1 -> Delay 1 сек
        // Используем Lerp для плавности
        float delay = Mathf.Lerp(12f, 1f, stressFactor);
        
        return delay;
    }

    private void PlayMeow()
    {
        Debug.Log("MEOW!!!");
        if (_mySounds.Count == 0 || CatSoundController.Instance == null) return;

        // Выбираем случайный звук из личных 5 звуков кота
        AudioClip clip = _mySounds[Random.Range(0, _mySounds.Count)];
        
        _audioSource.volume = CatSoundController.Instance.GetVolume();
        _audioSource.PlayOneShot(clip);
    }
}