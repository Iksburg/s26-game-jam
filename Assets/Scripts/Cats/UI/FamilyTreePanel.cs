using System.Collections.Generic;
using System.Linq;
using Cats.Genome.Abstract;
using CatWorld.Cats;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cats.UI
{
    /// <summary>
    /// Интерактивное окно бесконечного семейного древа.
    /// Позволяет перемещаться (Drag) и масштабировать (Zoom) холст родословной как в Figma.
    /// Рекурсивно строит полную генеалогическую карту кота с соединительными линиями.
    /// </summary>
    public class FamilyTreePanel : MonoBehaviour, IScrollHandler
    {
        [FormerlySerializedAs("_root")] [SerializeField] private GameObject root;
        [FormerlySerializedAs("_closeButton")] [SerializeField] private Button closeButton;

        [FormerlySerializedAs("_minZoom")]
        [Header("Настройки Figma-холста")]
        [SerializeField] private float minZoom = 0.3f;
        [FormerlySerializedAs("_maxZoom")] [SerializeField] private float maxZoom = 2f;
        [FormerlySerializedAs("_zoomSensitivity")] [SerializeField] private float zoomSensitivity = 0.1f;
        [FormerlySerializedAs("_nodeSpacing")] [SerializeField] private Vector2 nodeSpacing = new Vector2(200f, 150f);

        [FormerlySerializedAs("_lineWidth")]
        [Header("Настройки Соединительных Линий")]
        [SerializeField] private float lineWidth = 4f;
        [FormerlySerializedAs("_lineColor")] [SerializeField] private Color lineColor = new Color(0.45f, 0.4f, 0.35f, 0.7f);

        private RectTransform _viewport;
        private RectTransform _contentContainer;
        private ScrollRect _scrollRect;

        private Cat _cat;
        private readonly List<GameObject> _spawnedNodes = new List<GameObject>();
        private readonly HashSet<string> _processedGenomes = new HashSet<string>();

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            SetupFigmaCanvas();
        }

        private void SetupFigmaCanvas()
        {
            var windowTransform = root.transform.Find("Window");
            if (windowTransform == null) return;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(windowTransform, false);
            _viewport = viewportGo.GetComponent<RectTransform>();
            _viewport.anchorMin = Vector2.zero;
            _viewport.anchorMax = Vector2.one;
            _viewport.sizeDelta = new Vector2(-40f, -120f);
            _viewport.anchoredPosition = new Vector2(0f, -40f);
            
            viewportGo.GetComponent<Image>().color = new Color(0.93f, 0.9f, 0.82f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = true;

            var contentGo = new GameObject("ContentContainer", typeof(RectTransform));
            contentGo.transform.SetParent(_viewport, false);
            _contentContainer = contentGo.GetComponent<RectTransform>();
            _contentContainer.sizeDelta = new Vector2(5000f, 5000f);
            _contentContainer.anchoredPosition = Vector2.zero;

            _scrollRect = windowTransform.gameObject.AddComponent<ScrollRect>();
            _scrollRect.content = _contentContainer;
            _scrollRect.viewport = _viewport;
            _scrollRect.horizontal = true;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            _scrollRect.inertia = true;
            _scrollRect.scrollSensitivity = 0f;
        }

        public void Configure(GameObject root, Button closeButton)
        {
            this.root = root;
            this.closeButton = closeButton;
        }

        public void Open(Cat cat)
        {
            _cat = cat;
            if (root != null)
                root.SetActive(true);

            _contentContainer.anchoredPosition = Vector2.zero;
            _contentContainer.localScale = Vector3.one;

            BuildFullTree();
        }

        private void Close()
        {
            ClearTree();
            if (root != null)
                root.SetActive(false);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_contentContainer == null) return;

            var currentScale = _contentContainer.localScale;
            var zoomDelta = eventData.scrollDelta.y * zoomSensitivity;
            var newScaleX = Mathf.Clamp(currentScale.x + zoomDelta, minZoom, maxZoom);
            
            _contentContainer.localScale = new Vector3(newScaleX, newScaleX, 1f);
        }

        private void BuildFullTree()
        {
            ClearTree();
            if (_cat == null || _cat.Genome == null || _contentContainer == null) return;

            _processedGenomes.Clear();
            BuildTreeRecursive(_cat.Genome, Vector2.zero, 0);
        }

        private void BuildTreeRecursive(ICatGenome current, Vector2 pos, int depth)
        {
            if (current == null || _processedGenomes.Contains(current.Id)) return;
            _processedGenomes.Add(current.Id);

            var isTarget = (current.Id == _cat.Genome.Id);
            CreateNode(current.Name, current.Color, current.Sex, pos, isTarget);

            // --- КОРЕНЬ ВВЕРХ: Предки ---
            if (current.Parents != null && current.Parents.Count > 0)
            {
                var parentY = pos.y + nodeSpacing.y;
                var parentXOffset = nodeSpacing.x / Mathf.Max(1, depth); 

                // Достаем отца по индексу 0
                var father = current.Parents[0];
                var fatherPos = new Vector2(pos.x - parentXOffset, parentY);
        
                DrawLine(pos, fatherPos);
                BuildTreeRecursive(father, fatherPos, depth + 1);

                // Если в списке есть второй родитель, достаем мать по индексу 1
                if (current.Parents.Count > 1)
                {
                    var mother = current.Parents[1];
                    var motherPos = new Vector2(pos.x + parentXOffset, parentY);
            
                    DrawLine(pos, motherPos);
                    BuildTreeRecursive(mother, motherPos, depth + 1);
                }
            }

            // --- КОРЕНЬ ВНИЗ: Потомки ---
            if (current.Children is not { Count: > 0 }) return;
            
            var childY = pos.y - nodeSpacing.y;
            var childCount = current.Children.Count;
        
            var totalWidth = (childCount - 1) * nodeSpacing.x;
            var startX = pos.x - (totalWidth / 2f);

            for (var i = 0; i < childCount; i++)
            {
                var child = current.Children[i];
                var childX = startX + (i * nodeSpacing.x);
                var childPos = new Vector2(childX, childY);

                DrawLine(pos, childPos);
                BuildTreeRecursive(child, childPos, depth - 1);
            }
        }
        
        /// <summary>Динамически рисует прямоугольную UI-линию между двумя точками на холсте.</summary>
        private void DrawLine(Vector2 fromPos, Vector2 toPos)
        {
            var lineGo = new GameObject("TreeLine", typeof(RectTransform));
            // Задаем иерархию, чтобы линии рендерились ПОД карточками (были созданы первыми в списке дочерних)
            lineGo.transform.SetParent(_contentContainer, false);
            lineGo.transform.SetAsFirstSibling(); 
            _spawnedNodes.Add(lineGo);

            var rect = lineGo.GetComponent<RectTransform>();
            
            // Вычисляем вектор направления, расстояние и угол наклона между карточками
            var direction = toPos - fromPos;
            var distance = direction.magnitude;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Настраиваем анкоры в точку "fromPos" и вытягиваем прямоугольник на величину distance
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f); // Линия вращается вокруг своего начала
            rect.anchoredPosition = fromPos;
            rect.sizeDelta = new Vector2(distance, lineWidth);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            var image = lineGo.AddComponent<Image>();
            // Используем базовый белый пиксель, чтобы линия была сплошной и гладкой
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.color = lineColor;
        }

        /// <summary>Создает UI-ноду кота на холсте.</summary>
        private void CreateNode(string catName, Color furColor, Sex sex, Vector2 position, bool isTarget)
        {
            var nodeGo = new GameObject($"Node_{catName}", typeof(RectTransform));
            nodeGo.transform.SetParent(_contentContainer, false);
            _spawnedNodes.Add(nodeGo);

            var rect = nodeGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 110f);
            rect.anchoredPosition = position;

            var bgImage = nodeGo.AddComponent<Image>();
            bgImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = isTarget ? new Color(1f, 0.8f, 0.2f) : new Color(0.92f, 0.9f, 0.85f);

            var colorGo = new GameObject("Color", typeof(RectTransform));
            colorGo.transform.SetParent(nodeGo.transform, false);
            var colorRect = colorGo.GetComponent<RectTransform>();
            colorRect.anchorMin = colorRect.anchorMax = new Vector2(0f, 1f);
            colorRect.pivot = new Vector2(0f, 1f);
            colorRect.sizeDelta = new Vector2(35f, 35f);
            colorRect.anchoredPosition = new Vector2(12f, -12f);
            colorGo.AddComponent<Image>().color = furColor;

            var sexGo = new GameObject("Sex", typeof(RectTransform));
            sexGo.transform.SetParent(nodeGo.transform, false);
            var sexRect = sexGo.GetComponent<RectTransform>();
            sexRect.anchorMin = sexRect.anchorMax = new Vector2(0f, 1f);
            sexRect.pivot = new Vector2(0f, 1f);
            sexRect.sizeDelta = new Vector2(90f, 35f);
            sexRect.anchoredPosition = new Vector2(55f, -12f);
            var sexTxt = sexGo.AddComponent<Text>();
            sexTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sexTxt.fontSize = 20;
            sexTxt.color = sex == Sex.Male ? new Color(0.12f, 0.45f, 0.75f) : new Color(0.85f, 0.25f, 0.55f);
            sexTxt.text = sex == Sex.Male ? "Кот ♂" : "Кошка ♀";
            sexTxt.alignment = TextAnchor.MiddleLeft;

            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(nodeGo.transform, false);
            var nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.sizeDelta = new Vector2(-20f, 45f);
            nameRect.anchoredPosition = new Vector2(0f, 8f);
            var nameTxt = nameGo.AddComponent<Text>();
            nameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameTxt.fontSize = 18;
            nameTxt.color = new Color(0.2f, 0.15f, 0.1f);
            nameTxt.text = catName;
            nameTxt.alignment = TextAnchor.MiddleCenter;
            nameTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void ClearTree()
        {
            foreach (var node in _spawnedNodes.Where(node => node != null))
            {
                Destroy(node);
            }
            _spawnedNodes.Clear();
            _processedGenomes.Clear();
        }
    }
}
