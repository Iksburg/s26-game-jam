using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Заглушка системы сохранений для главного меню. Настоящая система
    /// (список котов, родословная, мяукоины — по концепту) появится в задаче
    /// сохранений и должна писать данные под этим же ключом.
    /// </summary>
    public static class SaveData
    {
        /// <summary>Ключ PlayerPrefs, под которым будет храниться сохранение.</summary>
        public const string SaveKey = "catworld_save";

        /// <summary>Есть ли сохранение (для кнопки «Продолжить»).</summary>
        public static bool HasSave => PlayerPrefs.HasKey(SaveKey);
    }
}
