using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.UI;

namespace PickMeUp.Game.Editor
{
    /// <summary>
    /// Editor script to procedurally build the Dungeon UI.
    /// Attach to a GameObject in the scene to generate UI elements.
    /// </summary>
    public class SetupDungeonUI : MonoBehaviour
    {
        [Header("UI Configuration")]
        public Vector2 ScreenSize = new Vector2(1080, 2340);
        public float HeaderHeight = 120f;
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
        private RectTransform _bodyArea;
        private RectTransform _footerArea;

        [ContextMenu("Generate Dungeon UI")]
        public void GenerateUI()
        {
            ClearExistingUI();

            // Create Canvas
            CreateCanvas();

            // Create main container
            CreateMainContainer();

            // Create header
            CreateHeader();

            // Create body
            CreateBody();

            // Create footer
            CreateFooter();

            // Create dungeon view component
            AddDungeonViewComponent();

            Debug.Log("Dungeon UI generated successfully!");
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
            var canvasObj = new GameObject("DungeonCanvas");
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

            // Create body area
            var bodyObj = new GameObject("BodyArea", typeof(RectTransform));
            bodyObj.transform.SetParent(_contentArea, false);
            _bodyArea = bodyObj.GetComponent<RectTransform>();
            _bodyArea.anchorMin = new Vector2(0, FooterHeight / ScreenSize.y);
            _bodyArea.anchorMax = new Vector2(1, 1 - HeaderHeight / ScreenSize.y);
            _bodyArea.offsetMin = Vector2.zero;
            _bodyArea.offsetMax = Vector2.zero;

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

        private void CreateBody()
        {
            // Body background
            var bodyBgObj = new GameObject("BodyBg", typeof(RectTransform), typeof(Image));
            bodyBgObj.transform.SetParent(_bodyArea, false);
            var bodyBgRT = bodyBgObj.GetComponent<RectTransform>();
            bodyBgRT.anchorMin = Vector2.zero;
            bodyBgRT.anchorMax = Vector2.one;
            bodyBgRT.offsetMin = Vector2.zero;
            bodyBgRT.offsetMax = Vector2.zero;

            var bodyBgImage = bodyBgObj.GetComponent<Image>();
            bodyBgImage.color = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, 0.5f);
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

        private void AddDungeonViewComponent()
        {
            var dungeonViewObj = new GameObject("DungeonView", typeof(RectTransform));
            dungeonViewObj.transform.SetParent(_canvas.transform, false);

            var dungeonView = dungeonViewObj.AddComponent<DungeonView>();

            // Link references
            dungeonView.Canvas = _canvas;
            dungeonView.ContentArea = _contentArea;
            dungeonView.HeaderArea = _headerArea;
            dungeonView.BodyArea = _bodyArea;
            dungeonView.FooterArea = _footerArea;

            // Create prefab references for runtime
            CreatePrefabs();

            Debug.Log("DungeonView component added. Assign prefab references in Inspector.");
        }

        private void CreatePrefabs()
        {
            // Create DungeonCard prefab
            CreateDungeonCardPrefab();

            // Create TeamSlot prefab
            CreateTeamSlotPrefab();

            // Create ResultsPanel prefab
            CreateResultsPanelPrefab();
        }

        private void CreateDungeonCardPrefab()
        {
            var cardObj = new GameObject("DungeonCard", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(300, 180);

            // Add components
            var image = cardObj.AddComponent<Image>();
            image.color = PanelColor;

            var button = cardObj.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            var colors = button.colors;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            button.colors = colors;

            // Save as prefab (in editor only)
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/DungeonCard.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            DestroyImmediate(cardObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        private void CreateTeamSlotPrefab()
        {
            var slotObj = new GameObject("TeamSlot", typeof(RectTransform));
            var slotRT = slotObj.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(150, 180);

            // Add components
            var image = slotObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

            var button = slotObj.AddComponent<Button>();

            // Add TeamSlot component
            var teamSlot = slotObj.AddComponent<DungeonTeamSlot>();

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/DungeonTeamSlot.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath);
            DestroyImmediate(slotObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        private void CreateResultsPanelPrefab()
        {
            var panelObj = new GameObject("ResultsPanel", typeof(RectTransform));
            var panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.sizeDelta = ScreenSize;
            panelRT.anchoredPosition = Vector2.zero;

            // Add background overlay
            var overlayObj = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            overlayObj.transform.SetParent(panelObj.transform, false);
            var overlayRT = overlayObj.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            var overlayImage = overlayObj.GetComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.85f);

            // Add content panel
            var contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(panelObj.transform, false);
            var contentRT = contentObj.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.1f, 0.15f);
            contentRT.anchorMax = new Vector2(0.9f, 0.85f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            var contentImage = contentObj.AddComponent<Image>();
            contentImage.color = PanelColor;

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/DungeonResultsPanel.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(panelObj, prefabPath);
            DestroyImmediate(panelObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        [ContextMenu("Clear Dungeon UI")]
        public void ClearUI()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            Debug.Log("Dungeon UI cleared.");
        }
    }
}