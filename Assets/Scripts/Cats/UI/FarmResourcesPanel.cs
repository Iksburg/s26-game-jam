using UnityEngine;
using UnityEngine.UI;

namespace CatWorld.Cats
{
    /// <summary>
    /// Числа запасов корма и воды в правом верхнем углу UI.
    /// Обновляется по событию FarmResources.Changed.
    /// </summary>
    public class FarmResourcesPanel : MonoBehaviour
    {
        [SerializeField] private FarmResources _resources;
        [SerializeField] private Text _label;

        private void OnEnable()
        {
            if (_resources != null)
                _resources.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_resources != null)
                _resources.Changed -= Refresh;
        }

        /// <summary>Заполняется билдером сцены (editor wiring).</summary>
        public void Configure(FarmResources resources, Text label)
        {
            _resources = resources;
            _label = label;
        }

        private void Refresh()
        {
            if (_resources == null || _label == null)
                return;
            _label.text = $"Корм: {_resources.Food}   Вода: {_resources.Water}";
        }
    }
}
