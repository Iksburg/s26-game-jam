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
        private const string AutoSaveEnabledKey = "catworld_autosave_enabled";
        private const string AutoSaveIntervalKey = "catworld_autosave_interval";

        /// <summary>Допустимые интервалы автосохранения в минутах.</summary>
        public static readonly int[] AutoSaveIntervals = { 1, 5, 15 };

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

        /// <summary>Включено ли автосохранение. По умолчанию выключено.</summary>
        public static bool AutoSaveEnabled
        {
            get => PlayerPrefs.GetInt(AutoSaveEnabledKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(AutoSaveEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        /// <summary>Интервал автосохранения в минутах (1, 5 или 15).</summary>
        public static int AutoSaveIntervalMinutes
        {
            get
            {
                int stored = PlayerPrefs.GetInt(AutoSaveIntervalKey, AutoSaveIntervals[0]);
                // Защита от «мусора» в PlayerPrefs: интервал вне списка недопустим.
                return System.Array.IndexOf(AutoSaveIntervals, stored) >= 0 ? stored : AutoSaveIntervals[0];
            }
            set
            {
                if (System.Array.IndexOf(AutoSaveIntervals, value) < 0)
                    return;
                PlayerPrefs.SetInt(AutoSaveIntervalKey, value);
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
