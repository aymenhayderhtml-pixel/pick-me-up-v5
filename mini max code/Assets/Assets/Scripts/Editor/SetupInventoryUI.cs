using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.UI;

namespace PickMeUp.Game.Editor
{
    /// <summary>
    /// Editor script to procedurally build the Inventory UI.
    /// Attach to a GameObject in the scene to generate UI elements.
    /// </summary>
    public class SetupInventoryUI : MonoBehaviour
    {
        [Header("UI Configuration")]
        public Vector2 ScreenSize = new Vector2(1080, 2340);
        public float HeaderHeight = 120f;
        public float FilterHeight = 80f;
        public float FooterHeight = 100f;

        [Header("Colors")]
        public Color BackgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        public Color PanelColor = new Color(0.12f, 0.12f, 0.18f, 0.95f);
        public Color AccentColor = new Color(0.4f, 0.6f, 0.9f, 1f);
        public Color TextColor = Color.white;
        public Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Fonts")]
        public TMP_FontAsset MainFont;
        public Material MainFontMaterial;

        private Canvas _canvas;
        private RectTransform _contentArea;
        private RectTransform _headerArea;
        private RectTransform _filterArea;
        private RectTransform _itemGridArea;
        private RectTransform _footerArea;

        [ContextMenu("Generate Inventory UI")]
        public void GenerateUI()
        {
            ClearExistingUI();

            // Create Canvas
            CreateCanvas();

            // Create main container
            CreateMainContainer();

            // Create header
            CreateHeader();

            // Create filter area
            CreateFilterArea();

            // Create item grid area
            CreateItemGridArea();

            // Create footer
            CreateFooter();

            // Create inventory view component
            AddInventoryViewComponent();

            Debug.Log("Inventory UI generated successfully!");
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
            var canvasObj = new GameObject("InventoryCanvas");
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

            // Add background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(_contentArea, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = BackgroundColor;

            // Create header area
            var headerObj = new GameObject("HeaderArea", typeof(RectTransform));
            headerObj.transform.SetParent(_contentArea, false);
            _headerArea = headerObj.GetComponent<RectTransform>();
            _headerArea.anchorMin = new Vector2(0, 1 - HeaderHeight / ScreenSize.y);
            _headerArea.anchorMax = new Vector2(1, 1);
            _headerArea.offsetMin = Vector2.zero;
            _headerArea.offsetMax = Vector2.zero;

            // Create filter area
            var filterObj = new GameObject("FilterArea", typeof(RectTransform));
            filterObj.transform.SetParent(_contentArea, false);
            _filterArea = filterObj.GetComponent<RectTransform>();
            _filterArea.anchorMin = new Vector2(0, 1 - HeaderHeight / ScreenSize.y - FilterHeight / ScreenSize.y);
            _filterArea.anchorMax = new Vector2(1, 1 - HeaderHeight / ScreenSize.y);
            _filterArea.offsetMin = Vector2.zero;
            _filterArea.offsetMax = Vector2.zero;

            // Create item grid area
            var gridObj = new GameObject("ItemGridArea", typeof(RectTransform));
            gridObj.transform.SetParent(_contentArea, false);
            _itemGridArea = gridObj.GetComponent<RectTransform>();
            _itemGridArea.anchorMin = new Vector2(0, FooterHeight / ScreenSize.y);
            _itemGridArea.anchorMax = new Vector2(1, 1 - HeaderHeight / ScreenSize.y - FilterHeight / ScreenSize.y);
            _itemGridArea.offsetMin = Vector2.zero;
            _itemGridArea.offsetMax = Vector2.zero;

            // Create footer area
            var footerObj = new GameObject("FooterArea", typeof(RectTransform));
            footerObj.transform.SetParent(_contentArea, false);
            _footerArea = footerObj.GetComponent<RectTransform>();
            _footerArea.anchorMin = new Vector2(0, 0);
            _footerArea.anchorMax = new Vector2(1, FooterHeight / ScreenSize.y);
            _footerArea.offsetMin = Vector2.zero;
            _footerArea.offsetMax = Vector2.zero;
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
        }

        private void CreateFilterArea()
        {
            // Filter background
            var filterBgObj = new GameObject("FilterBg", typeof(RectTransform), typeof(Image));
            filterBgObj.transform.SetParent(_filterArea, false);
            var filterBgRT = filterBgObj.GetComponent<RectTransform>();
            filterBgRT.anchorMin = Vector2.zero;
            filterBgRT.anchorMax = Vector2.one;
            filterBgRT.offsetMin = Vector2.zero;
            filterBgRT.offsetMax = Vector2.zero;

            var filterBgImage = filterBgObj.GetComponent<Image>();
            filterBgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        }

        private void CreateItemGridArea()
        {
            // Grid background
            var gridBgObj = new GameObject("GridBg", typeof(RectTransform), typeof(Image));
            gridBgObj.transform.SetParent(_itemGridArea, false);
            var gridBgRT = gridBgObj.GetComponent<RectTransform>();
            gridBgRT.anchorMin = Vector2.zero;
            gridBgRT.anchorMax = Vector2.one;
            gridBgRT.offsetMin = Vector2.zero;
            gridBgRT.offsetMax = Vector2.zero;

            var gridBgImage = gridBgObj.GetComponent<Image>();
            gridBgImage.color = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, 0.3f);
        }

        private void CreateFooter()
        {
            // Footer background
            var footerBgObj = new GameObject("FooterBg", typeof(RectTransform), typeof(Image));
            footerBgObj.transform.SetParent(_footerArea, false);
            var footerBgRT = footerBgObj.GetComponent<RectTransform>();
            footerBgRT.anchorMin = Vector2.zero;
            footerBgRT.anchorMax = Vector2.one;
            footerBgRT.offsetMin = Vector2.zero;
            footerBgRT.offsetMax = Vector2.zero;

            var footerBgImage = footerBgObj.GetComponent<Image>();
            footerBgImage.color = PanelColor;
        }

        private void AddInventoryViewComponent()
        {
            var inventoryViewObj = new GameObject("InventoryView", typeof(RectTransform));
            inventoryViewObj.transform.SetParent(_canvas.transform, false);

            var inventoryView = inventoryViewObj.AddComponent<InventoryView>();

            // Link references
            inventoryView.Canvas = _canvas;
            inventoryView.ContentArea = _contentArea;
            inventoryView.HeaderArea = _headerArea;
            inventoryView.FilterArea = _filterArea;
            inventoryView.ItemGridArea = _itemGridArea;
            inventoryView.FooterArea = _footerArea;

            // Create prefab references for runtime
            CreatePrefabs();

            Debug.Log("InventoryView component added. Assign prefab references in Inspector.");
        }

        private void CreatePrefabs()
        {
            // Create ItemCard prefab
            CreateItemCardPrefab();

            Debug.Log("Inventory prefabs created.");
        }

        private void CreateItemCardPrefab()
        {
            var cardObj = new GameObject("ItemCard", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(150, 170);

            // Add components
            var image = cardObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var button = cardObj.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            var colors = button.colors;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            button.colors = colors;

            // Add card component
            var card = cardObj.AddComponent<InventoryItemCard>();

            // Save as prefab (in editor only)
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/InventoryItemCard.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            DestroyImmediate(cardObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        [ContextMenu("Clear Inventory UI")]
        public void ClearUI()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            Debug.Log("Inventory UI cleared.");
        }
    }
}