using System.Collections;
using UnityEngine;

namespace CatWorld.Cats
{
    /// <summary>
    /// После того как кот поел, через настраиваемое время создаёт какашку
    /// на том месте, где кот окажется в момент спавна. Работает для всех стадий
    /// (котёнок/взрослый/пожилой) — подписка на событие CatNeedsController.Ate.
    /// </summary>
    [RequireComponent(typeof(CatNeedsController))]
    public class CatPoopController : MonoBehaviour
    {
        [SerializeField] private Poop _poopPrefab;
        [Tooltip("Через сколько секунд после еды появляется какашка.")]
        [SerializeField, Min(0f)] private float _poopDelay = 5f;

        private CatNeedsController _needs;

        private void Awake()
        {
            _needs = GetComponent<CatNeedsController>();
        }

        private void OnEnable()
        {
            if (_needs != null)
                _needs.Ate += SchedulePoop;
        }

        private void OnDisable()
        {
            if (_needs != null)
                _needs.Ate -= SchedulePoop;
        }

        private void SchedulePoop()
        {
            // Отдельная корутина на каждый приём пищи — если кот поест несколько
            // раз, появится столько же какашек. При уничтожении кота корутины
            // останавливаются автоматически.
            StartCoroutine(PoopAfterDelay());
        }

        private IEnumerator PoopAfterDelay()
        {
            // Масштабированное время: на паузе (меню) отсчёт замирает.
            yield return new WaitForSeconds(_poopDelay);

            if (_poopPrefab == null)
            {
                Debug.LogWarning($"[CatPoop] У кота «{name}» не назначен префаб какашки.");
                yield break;
            }

            // Позиция берётся в момент спавна — какашка остаётся там, где кот сейчас.
            Instantiate(_poopPrefab, transform.position, Quaternion.identity);
        }
    }
}
