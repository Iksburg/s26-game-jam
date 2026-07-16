using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>Тип потребности, которую удовлетворяет станция.</summary>
    public enum NeedType
    {
        Food,
        Water
    }

    /// <summary>
    /// Место миски с едой или поилки: пустой объект на сцене, к которому идут
    /// коты. Позицию задаёт дизайнер (внутри FarmBounds); визуал миски можно
    /// повесить на этот же объект позже.
    /// </summary>
    public class NeedStation : MonoBehaviour
    {
        [SerializeField] private NeedType _type;

        public NeedType Type => _type;
        public Vector2 Position => transform.position;

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void SetType(NeedType type)
        {
            _type = type;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _type == NeedType.Food ? new Color(1f, 0.6f, 0.1f) : new Color(0.2f, 0.6f, 1f);
            Gizmos.DrawWireSphere(transform.position, 0.35f);
        }
    }
}
