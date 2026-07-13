namespace CatWorld.Cats
{
    /// <summary>
    /// Статус нахождения кота на ферме.
    /// OnFarm — на ферме (пустой статус в концепте), Lost — потеряшка (сбежал),
    /// InNewFamily — отдан/продан новым хозяевам.
    /// </summary>
    public enum FarmStatus
    {
        OnFarm,
        Lost,
        InNewFamily
    }
}
