using UnityEngine;
using CatWorld.Cats; // Пространство имен из твоего GameSettings

public class CatSoundController : MonoBehaviour
{
    public static CatSoundController Instance { get; private set; }

    [Header("Пул звуков")]
    [SerializeField] private AudioClip[] _soundPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Возвращает случайный звук из общего пула.
    /// </summary>
    public AudioClip GetRandomSound()
    {
        if (_soundPool == null || _soundPool.Length == 0) return null;
        return _soundPool[Random.Range(0, _soundPool.Length)];
    }

    /// <summary>
    /// Возвращает текущую громкость звуковых эффектов из настроек.
    /// </summary>
    public float GetVolume()
    {
        return GameSettings.SoundVolume;
    }
}