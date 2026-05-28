using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.UI;

namespace PickMeUp.Game.Editor
{
    /// <summary>
    /// Editor script to procedurally build the Hub UI.
    /// Attach to a GameObject in the scene to generate UI elements.
    /// </summary>
    public class SetupHubUI : MonoBehaviour
    {
        [Header("UI Configuration")]
        public Vector2 ScreenSize = new Vector2(1080, 2340);
        public float TopBarHeight = 100f;
        public float BottomDockHeight = 180f;

        [Header("Colors")]
        public Color BackgroundColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        public Color PanelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        public Color AccentColor = new Color(0.4f, 0.6f, 0.9f, 1f);
        public Color TextColor = Color.white;

        private Canvas _canvas;
        private RectTransform _contentArea;
        private RectTransform _topBarArea;
        private RectTransform _centerArea;
        private RectTransform _bottomDockArea;

        [ContextMenu("Generate Hub UI")]
        public void GenerateUI()
        {
            ClearExistingUI();

            // Create Canvas
            CreateCanvas();

            // Create main container
            CreateMainContainer();

            // Create areas
            CreateTopBar();
            CreateCenterArea();
            CreateBottomDock();

            // Create HubView component
            AddHubViewComponent();

            Debug.Log("Hub UI generated successfully!");
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
            var canvasObj = new GameObject("HubCanvas");
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

            // Top bar area
            var topBarObj = new GameObject("TopBarArea", typeof(RectTransform));
            topBarObj.transform.SetParent(_contentArea, false);
            _topBarArea = topBarObj.GetComponent<RectTransform>();
            _topBarArea.anchorMin = new Vector2(0, 1 - TopBarHeight / ScreenSize.y);
            _topBarArea.anchorMax = new Vector2(1, 1);
            _topBarArea.offsetMin = Vector2.zero;
            _topBarArea.offsetMax = Vector2.zero;

            // Center area
            var centerObj = new GameObject("CenterArea", typeof(RectTransform));
            centerObj.transform.SetParent(_contentArea, false);
            _centerArea = centerObj.GetComponent<RectTransform>();
            _centerArea.anchorMin = new Vector2(0, BottomDockHeight / ScreenSize.y);
            _centerArea.anchorMax = new Vector2(1, 1 - TopBarHeight / ScreenSize.y);
            _centerArea.offsetMin = Vector2.zero;
            _centerArea.offsetMax = Vector2.zero;

            // Bottom dock area
            var bottomObj = new GameObject("BottomDockArea", typeof(RectTransform));
            bottomObj.transform.SetParent(_contentArea, false);
            _bottomDockArea = bottomObj.GetComponent<RectTransform>();
            _bottomDockArea.anchorMin = new Vector2(0, 0);
            _bottomDockArea.anchorMax = new Vector2(1, BottomDockHeight / ScreenSize.y);
            _bottomDockArea.offsetMin = Vector2.zero;
            _bottomDockArea.offsetMax = Vector2.zero;
        }

        private void CreateTopBar()
        {
            // Top bar background
            var topBgObj = new GameObject("TopBg", typeof(RectTransform), typeof(Image));
            topBgObj.transform.SetParent(_topBarArea, false);
            var topBgRT = topBgObj.GetComponent<RectTransform>();
            topBgRT.anchorMin = Vector2.zero;
            topBgRT.anchorMax = Vector2.one;
            topBgRT.offsetMin = Vector2.zero;
            topBgRT.offsetMax = Vector2.zero;

            var topBgImage = topBgObj.GetComponent<Image>();
            topBgImage.color = PanelColor;
        }

        private void CreateCenterArea()
        {
            // Center background
            var centerBgObj = new GameObject("CenterBg", typeof(RectTransform), typeof(Image));
            centerBgObj.transform.SetParent(_centerArea, false);
            var centerBgRT = centerBgObj.GetComponent<RectTransform>();
            centerBgRT.anchorMin = Vector2.zero;
            centerBgRT.anchorMax = Vector2.one;
            centerBgRT.offsetMin = Vector2.zero;
            centerBgRT.offsetMax = Vector2.zero;

            var centerBgImage = centerBgObj.GetComponent<Image>();
            centerBgImage.color = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, 0.5f);
        }

        private void CreateBottomDock()
        {
            // Dock background
            var dockBgObj = new GameObject("DockBg", typeof(RectTransform), typeof(Image));
            dockBgObj.transform.SetParent(_bottomDockArea, false);
            var dockBgRT = dockBgObj.GetComponent<RectTransform>();
            dockBgRT.anchorMin = Vector2.zero;
            dockBgRT.anchorMax = Vector2.one;
            dockBgRT.offsetMin = Vector2.zero;
            dockBgRT.offsetMax = Vector2.zero;

            var dockBgImage = dockBgObj.GetComponent<Image>();
            dockBgImage.color = PanelColor;
        }

        private void AddHubViewComponent()
        {
            var hubViewObj = new GameObject("HubView", typeof(RectTransform));
            hubViewObj.transform.SetParent(_canvas.transform, false);

            var hubView = hubViewObj.AddComponent<HubView>();

            // Link references
            hubView.Canvas = _canvas;
            hubView.ContentArea = _contentArea;
            hubView.TopBarArea = _topBarArea;
            hubView.CenterArea = _centerArea;
            hubView.BottomDockArea = _bottomDockArea;

            Debug.Log("HubView component added. Assign button references in Inspector.");
        }

        [ContextMenu("Clear Hub UI")]
        public void ClearUI()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            Debug.Log("Hub UI cleared.");
        }
    }
}