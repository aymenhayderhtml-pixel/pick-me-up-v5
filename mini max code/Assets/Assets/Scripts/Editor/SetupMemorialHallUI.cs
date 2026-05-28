using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.UI;

namespace PickMeUp.Game.Editor
{
    /// <summary>
    /// Editor script to procedurally build the Memorial Hall UI.
    /// Attach to a GameObject in the scene to generate UI elements.
    /// </summary>
    public class SetupMemorialHallUI : MonoBehaviour
    {
        [Header("UI Configuration")]
        public Vector2 ScreenSize = new Vector2(1080, 2340);
        public float HeaderHeight = 100f;
        public float FilterSortHeight = 80f;
        public float DetailPanelHeight = 600f;

        [Header("Colors")]
        public Color BackgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        public Color PanelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        public Color AccentColor = new Color(0.5f, 0.7f, 0.9f, 1f);
        public Color TextColor = Color.white;

        private Canvas _canvas;
        private RectTransform _contentArea;
        private RectTransform _headerArea;
        private RectTransform _filterSortArea;
        private RectTransform _gridArea;
        private RectTransform _detailPanelArea;

        [ContextMenu("Generate Memorial Hall UI")]
        public void GenerateUI()
        {
            ClearExistingUI();

            // Create Canvas
            CreateCanvas();

            // Create main container
            CreateMainContainer();

            // Create header
            CreateHeader();

            // Create filter/sort area
            CreateFilterSortArea();

            // Create grid area
            CreateGridArea();

            // Create detail panel area
            CreateDetailPanelArea();

            // Create MemorialHallView component
            AddMemorialHallViewComponent();

            Debug.Log("Memorial Hall UI generated successfully!");
        }

        private void ClearExistingUI()
        {
            var existingCanvas = GetComponent<Canvas>();
            if (existingCanvas != null)
            {
                DestroyImmediate(existingCanvas.gameObject);
            }
        }

        private void CreateCanvas()
        {
            var canvasObj = new GameObject("MemorialHallCanvas");
            canvasObj.transform.SetParent(transform, false);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 0;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = ScreenSize;
            canvasScaler.matchWidthOrHeight = 0.5f;

            var graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();

            // Set canvas as parent reference
            transform.SetParent(canvasObj.transform, true);
        }

        private void CreateMainContainer()
        {
            var containerObj = new GameObject("ContentArea", typeof(RectTransform));
            containerObj.transform.SetParent(_canvas.transform, false);

            _contentArea = containerObj.GetComponent<RectTransform>();
            _contentArea.anchorMin = Vector2.zero;
            _contentArea.anchorMax = Vector2.one;
            _contentArea.offsetMin = Vector2.zero;
            _contentArea.offsetMax = Vector2.zero;

            // Background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(_contentArea, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = BackgroundColor;

            // Header area
            var headerObj = new GameObject("HeaderArea", typeof(RectTransform));
            headerObj.transform.SetParent(_contentArea, false);
            _headerArea = headerObj.GetComponent<RectTransform>();
            _headerArea.anchorMin = new Vector2(0, 1 - HeaderHeight / ScreenSize.y);
            _headerArea.anchorMax = new Vector2(1, 1);
            _headerArea.offsetMin = Vector2.zero;
            _headerArea.offsetMax = Vector2.zero;

            // Filter/sort area
            var filterSortObj = new GameObject("FilterSortArea", typeof(RectTransform));
            filterSortObj.transform.SetParent(_contentArea, false);
            _filterSortArea = filterSortObj.GetComponent<RectTransform>();
            _filterSortArea.anchorMin = new Vector2(0, 1 - HeaderHeight / ScreenSize.y - FilterSortHeight / ScreenSize.y);
            _filterSortArea.anchorMax = new Vector2(1, 1 - HeaderHeight / ScreenSize.y);
            _filterSortArea.offsetMin = Vector2.zero;
            _filterSortArea.offsetMax = Vector2.zero;

            // Grid area
            var gridObj = new GameObject("GridArea", typeof(RectTransform));
            gridObj.transform.SetParent(_contentArea, false);
            _gridArea = gridObj.GetComponent<RectTransform>();
            _gridArea.anchorMin = new Vector2(0, DetailPanelHeight / ScreenSize.y);
            _gridArea.anchorMax = new Vector2(1, 1 - HeaderHeight / ScreenSize.y - FilterSortHeight / ScreenSize.y);
            _gridArea.offsetMin = Vector2.zero;
            _gridArea.offsetMax = Vector2.zero;

            // Detail panel area
            var detailObj = new GameObject("DetailPanelArea", typeof(RectTransform));
            detailObj.transform.SetParent(_contentArea, false);
            _detailPanelArea = detailObj.GetComponent<RectTransform>();
            _detailPanelArea.anchorMin = new Vector2(0, 0);
            _detailPanelArea.anchorMax = new Vector2(1, DetailPanelHeight / ScreenSize.y);
            _detailPanelArea.offsetMin = Vector2.zero;
            _detailPanelArea.offsetMax = Vector2.zero;
        }

        private void CreateHeader()
        {
            // Header background
            var headerBgObj = new GameObject("HeaderBg", typeof(RectTransform), typeof(Image));
            headerBgObj.transform.SetParent(_headerArea, false);
            var headerBgRT = headerBgObj.GetComponent<RectTransform>();
            headerBgRT.anchorMin = Vector2.zero;
            headerBgRT.anchorMax = Vector2.one;
            headerBgRT.offsetMin = Vector2.zero;
            headerBgRT.offsetMax = Vector2.zero;

            var headerBgImage = headerBgObj.GetComponent<Image>();
            headerBgImage.color = PanelColor;

            // Header content area
            var headerContentObj = new GameObject("HeaderContent", typeof(RectTransform));
            headerContentObj.transform.SetParent(_headerArea, false);
            var headerContentRT = headerContentObj.GetComponent<RectTransform>();
            headerContentRT.anchorMin = Vector2.zero;
            headerContentRT.anchorMax = Vector2.one;
            headerContentRT.offsetMin = new Vector2(20, 10);
            headerContentRT.offsetMax = new Vector2(-20, -10);
        }

        private void CreateFilterSortArea()
        {
            // Filter/sort background
            var filterBgObj = new GameObject("FilterSortBg", typeof(RectTransform), typeof(Image));
            filterBgObj.transform.SetParent(_filterSortArea, false);
            var filterBgRT = filterBgObj.GetComponent<RectTransform>();
            filterBgRT.anchorMin = Vector2.zero;
            filterBgRT.anchorMax = Vector2.one;
            filterBgRT.offsetMin = Vector2.zero;
            filterBgRT.offsetMax = Vector2.zero;

            var filterBgImage = filterBgObj.GetComponent<Image>();
            filterBgImage.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        }

        private void CreateGridArea()
        {
            // Grid background
            var gridBgObj = new GameObject("GridBg", typeof(RectTransform), typeof(Image));
            gridBgObj.transform.SetParent(_gridArea, false);
            var gridBgRT = gridBgObj.GetComponent<RectTransform>();
            gridBgRT.anchorMin = Vector2.zero;
            gridBgRT.anchorMax = Vector2.one;
            gridBgRT.offsetMin = Vector2.zero;
            gridBgRT.offsetMax = Vector2.zero;

            var gridBgImage = gridBgObj.GetComponent<Image>();
            gridBgImage.color = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, 0.3f);
        }

        private void CreateDetailPanelArea()
        {
            // Detail panel background (hidden by default)
            var detailBgObj = new GameObject("DetailBg", typeof(RectTransform), typeof(Image));
            detailBgObj.transform.SetParent(_detailPanelArea, false);
            var detailBgRT = detailBgObj.GetComponent<RectTransform>();
            detailBgRT.anchorMin = Vector2.zero;
            detailBgRT.anchorMax = Vector2.one;
            detailBgRT.offsetMin = Vector2.zero;
            detailBgRT.offsetMax = Vector2.zero;

            var detailBgImage = detailBgObj.GetComponent<Image>();
            detailBgImage.color = PanelColor;
        }

        private void AddMemorialHallViewComponent()
        {
            var memorialViewObj = new GameObject("MemorialHallView", typeof(RectTransform));
            memorialViewObj.transform.SetParent(_canvas.transform, false);

            var memorialView = memorialViewObj.AddComponent<MemorialHallView>();

            // Link references
            memorialView.Canvas = _canvas;
            memorialView.ContentArea = _contentArea;
            memorialView.HeaderArea = _headerArea;
            memorialView.FilterSortArea = _filterSortArea;
            memorialView.GridArea = _gridArea;
            memorialView.DetailPanelArea = _detailPanelArea;

            // Create prefab references
            CreatePrefabs();

            Debug.Log("MemorialHallView component added. Assign prefab references in Inspector.");
        }

        private void CreatePrefabs()
        {
            // Create HeroCard prefab
            CreateHeroCardPrefab();

            // Create DetailPanel prefab
            CreateDetailPanelPrefab();

            Debug.Log("Memorial Hall prefabs created.");
        }

        private void CreateHeroCardPrefab()
        {
            var cardObj = new GameObject("HeroCard", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(160, 220);

            // Add components
            var image = cardObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var button = cardObj.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            var colors = button.colors;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.2f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);
            button.colors = colors;

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/HeroCard.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            DestroyImmediate(cardObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        private void CreateDetailPanelPrefab()
        {
            var panelObj = new GameObject("HeroDetailPanel", typeof(RectTransform));
            var panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2((int)ScreenSize.x, (int)DetailPanelHeight);
            panelRT.anchoredPosition = Vector2.zero;

            // Add background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(panelObj.transform, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/HeroDetailPanel.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(panelObj, prefabPath);
            DestroyImmediate(panelObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        [ContextMenu("Clear Memorial Hall UI")]
        public void ClearUI()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            Debug.Log("Memorial Hall UI cleared.");
        }
    }
}