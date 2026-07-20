using System;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Настройки игры: громкость музыки и звуков (0–1), полноэкранный режим.
    /// Хранятся в PlayerPrefs. Будущая аудиосистема подписывается на Changed
    /// и читает MusicVolume/SoundVolume — меню менять не придётся.
    /// </summary>
    public static class GameSettings
    {
        private const string MusicKey = "catworld_music_volume";
        private const string SoundKey = "catworld_sound_volume";

        /// <summary>Вызывается при изменении любой настройки.</summary>
        public static event Action Changed;

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, 1f);
            set
            {
                PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        public static float SoundVolume
        {
            get => PlayerPrefs.GetFloat(SoundKey, 1f);
            set
            {
                PlayerPrefs.SetFloat(SoundKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        public static bool Fullscreen
        {
            get => Screen.fullScreen;
            set
            {
                Screen.fullScreen = value;
                Changed?.Invoke();
            }
        }
    }
}
