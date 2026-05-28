using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.UI;

namespace PickMeUp.Game.Editor
{
    /// <summary>
    /// Editor script to procedurally build the Facilities UI.
    /// Attach to a GameObject in the scene to generate UI elements.
    /// </summary>
    public class SetupFacilityUI : MonoBehaviour
    {
        [Header("UI Configuration")]
        public Vector2 ScreenSize = new Vector2(1080, 2340);
        public float HeaderHeight = 120f;
        public float FacilityCardHeight = 140f;

        [Header("Colors")]
        public Color BackgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        public Color PanelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        public Color AccentColor = new Color(0.4f, 0.7f, 0.9f, 1f);
        public Color TextColor = Color.white;

        private Canvas _canvas;
        private RectTransform _contentArea;
        private RectTransform _headerArea;
        private RectTransform _facilityListArea;
        private RectTransform _detailArea;

        [ContextMenu("Generate Facility UI")]
        public void GenerateUI()
        {
            ClearExistingUI();

            // Create Canvas
            CreateCanvas();

            // Create main container
            CreateMainContainer();

            // Create header
            CreateHeader();

            // Create facility list area
            CreateFacilityListArea();

            // Create detail area
            CreateDetailArea();

            // Create FacilityView component
            AddFacilityViewComponent();

            Debug.Log("Facility UI generated successfully!");
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
            var canvasObj = new GameObject("FacilityCanvas");
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

            // Facility list area
            var listObj = new GameObject("FacilityListArea", typeof(RectTransform));
            listObj.transform.SetParent(_contentArea, false);
            _facilityListArea = listObj.GetComponent<RectTransform>();
            _facilityListArea.anchorMin = new Vector2(0, 0);
            _facilityListArea.anchorMax = new Vector2(1, 1 - HeaderHeight / ScreenSize.y);
            _facilityListArea.offsetMin = Vector2.zero;
            _facilityListArea.offsetMax = Vector2.zero;

            // Detail area
            var detailObj = new GameObject("DetailArea", typeof(RectTransform));
            detailObj.transform.SetParent(_contentArea, false);
            _detailArea = detailObj.GetComponent<RectTransform>();
            _detailArea.anchorMin = new Vector2(0.1f, 0.2f);
            _detailArea.anchorMax = new Vector2(0.9f, 0.8f);
            _detailArea.offsetMin = Vector2.zero;
            _detailArea.offsetMax = Vector2.zero;
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

            // Header content
            var headerContentObj = new GameObject("HeaderContent", typeof(RectTransform));
            headerContentObj.transform.SetParent(_headerArea, false);
            var headerContentRT = headerContentObj.GetComponent<RectTransform>();
            headerContentRT.anchorMin = Vector2.zero;
            headerContentRT.anchorMax = Vector2.one;
            headerContentRT.offsetMin = new Vector2(20, 10);
            headerContentRT.offsetMax = new Vector2(-20, -10);
        }

        private void CreateFacilityListArea()
        {
            // List background
            var listBgObj = new GameObject("ListBg", typeof(RectTransform), typeof(Image));
            listBgObj.transform.SetParent(_facilityListArea, false);
            var listBgRT = listBgObj.GetComponent<RectTransform>();
            listBgRT.anchorMin = Vector2.zero;
            listBgRT.anchorMax = Vector2.one;
            listBgRT.offsetMin = Vector2.zero;
            listBgRT.offsetMax = Vector2.zero;

            var listBgImage = listBgObj.GetComponent<Image>();
            listBgImage.color = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, 0.5f);

            // Scroll area
            var scrollObj = new GameObject("ScrollArea", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(_facilityListArea, false);
            var scrollRT = scrollObj.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            var scrollRect = scrollObj.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        private void CreateDetailArea()
        {
            // Detail background (hidden by default)
            var detailBgObj = new GameObject("DetailBg", typeof(RectTransform), typeof(Image));
            detailBgObj.transform.SetParent(_detailArea, false);
            var detailBgRT = detailBgObj.GetComponent<RectTransform>();
            detailBgRT.anchorMin = Vector2.zero;
            detailBgRT.anchorMax = Vector2.one;
            detailBgRT.offsetMin = Vector2.zero;
            detailBgRT.offsetMax = Vector2.zero;

            var detailBgImage = detailBgObj.GetComponent<Image>();
            detailBgImage.color = PanelColor;
        }

        private void AddFacilityViewComponent()
        {
            var facilityViewObj = new GameObject("FacilityView", typeof(RectTransform));
            facilityViewObj.transform.SetParent(_canvas.transform, false);

            var facilityView = facilityViewObj.AddComponent<FacilityView>();

            // Link references
            facilityView.Canvas = _canvas;
            facilityView.ContentArea = _contentArea;
            facilityView.HeaderArea = _headerArea;
            facilityView.FacilityListArea = _facilityListArea;
            facilityView.DetailArea = _detailArea;

            // Create prefabs
            CreatePrefabs();

            Debug.Log("FacilityView component added. Assign prefab references in Inspector.");
        }

        private void CreatePrefabs()
        {
            // Create FacilityCard prefab
            CreateFacilityCardPrefab();

            Debug.Log("Facility prefabs created.");
        }

        private void CreateFacilityCardPrefab()
        {
            var cardObj = new GameObject("FacilityCard", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(900, (int)FacilityCardHeight);

            // Add components
            var image = cardObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var button = cardObj.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            var colors = button.colors;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.15f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);
            button.colors = colors;

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/FacilityCard.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            DestroyImmediate(cardObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        [ContextMenu("Clear Facility UI")]
        public void ClearUI()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            Debug.Log("Facility UI cleared.");
        }
    }
}