using System;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Хранилище сейва: сериализация GameSaveData в JSON и запись в PlayerPrefs
    /// под тем же ключом, который проверяет SaveData.HasSave (кнопка «Продолжить»).
    /// </summary>
    public static class SaveSystem
    {
        /// <summary>
        /// Запрошена ли загрузка сейва при переходе в игровую сцену.
        /// Ставится кнопкой «Продолжить», читается и сбрасывается GameSaveService.
        /// </summary>
        public static bool LoadRequested { get; set; }

        public static bool HasSave => PlayerPrefs.HasKey(SaveData.SaveKey);

        public static void Save(GameSaveData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveData.SaveKey, json);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] Игра сохранена: котов {data.cats.Count}, " +
                      $"корм {data.food}, вода {data.water}.");
        }

        /// <summary>Читает сейв. null — сохранения нет или оно повреждено.</summary>
        public static GameSaveData Load()
        {
            if (!HasSave)
                return null;

            string json = PlayerPrefs.GetString(SaveData.SaveKey);
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception e)
            {
                // Повреждённый сейв не должен ронять игру — стартуем как новая.
                Debug.LogError($"[SaveSystem] Не удалось прочитать сохранение: {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(SaveData.SaveKey);
            PlayerPrefs.Save();
        }
    }
}
