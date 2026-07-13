using System.Text;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// Demo-проверка базовой сущности кота. Повесить на пустой GameObject в сцене
    /// и нажать Play — в Console появятся характеристики двух независимых котов.
    /// Критерий «Готово»: создаются два кота, у каждого свои характеристики и разный ID.
    /// </summary>
    public class CatSpawnDemo : MonoBehaviour
    {
        private void Start()
        {
            // Кот №1
            var barsik = Cat.Create("Барсик", Sex.Male, LifeStage.Kitten, new Color(1f, 0.6f, 0.2f));
            barsik.TryAddInnateTrait(CatTrait.Champion);
            barsik.TryAddInnateTrait(CatTrait.Clean);
            barsik.SetCleanliness(80f);

            // Кот №2
            var murka = Cat.Create("Мурка", Sex.Female, LifeStage.Adult, Color.gray);
            murka.TryAddInnateTrait(CatTrait.Sleepy);
            murka.TryAddAcquiredTrait(CatTrait.Quiet);
            murka.SetSatiety(65f);
            // Мурка — мать, Барсик — её ребёнок (демонстрация родословной).
            murka.AddChild(barsik.Id);

            // Проверка лимита врождённых черт (макс. 3): пытаемся добавить 4-ю.
            barsik.TryAddInnateTrait(CatTrait.Grumpy); // 3-я — ок
            bool fourth = barsik.TryAddInnateTrait(CatTrait.Meower); // 4-я — должна быть отклонена
            Debug.Log($"[CatSpawnDemo] Добавление 4-й врождённой черты Барсику: " +
                      (fourth ? "ПРИНЯТО (ошибка лимита!)" : "ОТКЛОНЕНО (лимит 3 работает)"));

            Debug.Log(Describe(barsik));
            Debug.Log(Describe(murka));

            Debug.Log(barsik.Id != murka.Id
                ? "[CatSpawnDemo] ID котов различны — характеристики независимы. Готово."
                : "[CatSpawnDemo] ОШИБКА: ID совпадают!");
        }

        private static string Describe(Cat cat)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Кот: {cat.Name} ===");
            sb.AppendLine($"ID: {cat.Id}");
            sb.AppendLine($"Пол: {cat.Sex}, Стадия: {cat.Stage}");
            sb.AppendLine($"Цвет шерсти: {cat.FurColor}");
            sb.AppendLine($"Врождённые черты: {string.Join(", ", cat.InnateTraits)}");
            sb.AppendLine($"Приобретённые черты: {string.Join(", ", cat.AcquiredTraits)}");
            sb.AppendLine($"Сытость: {cat.Satiety}%, Вода: {cat.Water}%, Чистота: {cat.Cleanliness}%");
            sb.AppendLine($"Родители: [{cat.ParentId1}] [{cat.ParentId2}]");
            sb.AppendLine($"Дети: {(cat.ChildrenIds.Count > 0 ? string.Join(", ", cat.ChildrenIds) : "нет")}");
            sb.AppendLine($"Активность: {cat.CurrentActivity}, Статус на ферме: {cat.FarmStatus}");
            return sb.ToString();
        }
    }
}
