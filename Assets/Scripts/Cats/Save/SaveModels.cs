using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Сохранённое состояние одного кота. Плоская структура ради JsonUtility:
    /// вложенные ссылки на родителей/детей хранятся как списки Id и
    /// восстанавливаются в два прохода.
    /// </summary>
    [Serializable]
    public class CatSaveData
    {
        public string catId;
        public string name;
        public LifeStage stage;
        public CatActivity activity;
        public FarmStatus farmStatus;

        // Потребности
        public float satiety;
        public float water;
        public float cleanliness;

        // Черты характера
        public List<CatTrait> innateTraits = new List<CatTrait>();
        public List<CatTrait> acquiredTraits = new List<CatTrait>();

        // Возраст: сколько секунд осталось до следующей стадии и до броска
        // шанса ухода у пожилого (отдельного поля возраста у кота нет —
        // прогресс взросления хранится именно в этих таймерах).
        public float stageTimer;
        public float leaveTimer;

        // Позиция на ферме, чтобы сцена восстановилась как была
        public float positionX;
        public float positionY;

        // Геном: Id обязателен для восстановления родословной
        public string genomeId;
        public Sex sex;
        public float colorR;
        public float colorG;
        public float colorB;
        public float colorA = 1f;
        public float sexStrength;

        // Родословная — только Id, объекты связываются вторым проходом
        public List<string> parentGenomeIds = new List<string>();
        public List<string> childGenomeIds = new List<string>();

        public Color GetColor() => new Color(colorR, colorG, colorB, colorA);

        public void SetColor(Color color)
        {
            colorR = color.r;
            colorG = color.g;
            colorB = color.b;
            colorA = color.a;
        }
    }

    /// <summary>Полное состояние игры для сохранения/загрузки.</summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>Версия формата — пригодится, если структура сейва изменится.</summary>
        public int version = 1;

        public List<CatSaveData> cats = new List<CatSaveData>();

        /// <summary>Мяукоины: механика экономики ещё не реализована, поле — задел.</summary>
        public int meowCoins;

        public int food;
        public int water;

        /// <summary>Когда сохранено (для будущего UI со списком сейвов).</summary>
        public string savedAt;
    }
}
