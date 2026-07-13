namespace CatWorld.Cats
{
    /// <summary>
    /// Текущая активность (состояние) кота. Одновременно активно только одно.
    /// Соответствует разделу «Состояния кота» концепта.
    /// </summary>
    public enum CatActivity
    {
        Idle,
        Walking,
        Eating,
        Sleeping,
        Dirty,
        Hungry
    }
}
