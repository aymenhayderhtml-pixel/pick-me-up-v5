using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.UI;

namespace PickMeUp.Game.Editor
{
    /// <summary>
    /// Editor script to procedurally build the Synthesis Lab UI.
    /// Attach to a GameObject in the scene to generate UI elements.
    /// </summary>
    public class SetupSynthUI : MonoBehaviour
    {
        [Header("UI Configuration")]
        public Vector2 ScreenSize = new Vector2(1080, 2340);
        public float HeaderHeight = 100f;
        public float BaseHeroAreaHeight = 300f;
        public float MaterialAreaHeight = 250f;
        public float PreviewAreaHeight = 200f;
        public float FooterHeight = 100f;

        [Header("Colors")]
        public Color BackgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        public Color PanelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        public Color AccentColor = new Color(0.4f, 0.7f, 0.9f, 1f);
        public Color TextColor = Color.white;

        private Canvas _canvas;
        private RectTransform _contentArea;
        private RectTransform _headerArea;
        private RectTransform _baseHeroArea;
        private RectTransform _materialArea;
        private RectTransform _previewArea;
        private RectTransform _animationArea;
        private RectTransform _footerArea;

        [ContextMenu("Generate Synthesis Lab UI")]
        public void GenerateUI()
        {
            ClearExistingUI();

            // Create Canvas
            CreateCanvas();

            // Create main container
            CreateMainContainer();

            // Create areas
            CreateHeader();
            CreateBaseHeroArea();
            CreateMaterialArea();
            CreatePreviewArea();
            CreateAnimationArea();
            CreateFooter();

            // Create SynthView component
            AddSynthViewComponent();

            Debug.Log("Synthesis Lab UI generated successfully!");
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
            var canvasObj = new GameObject("SynthCanvas");
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

            float runningY = 1f;

            // Header area
            var headerObj = new GameObject("HeaderArea", typeof(RectTransform));
            headerObj.transform.SetParent(_contentArea, false);
            _headerArea = headerObj.GetComponent<RectTransform>();
            _headerArea.anchorMin = new Vector2(0, runningY - HeaderHeight / ScreenSize.y);
            _headerArea.anchorMax = new Vector2(1, runningY);
            _headerArea.offsetMin = Vector2.zero;
            _headerArea.offsetMax = Vector2.zero;

            runningY -= HeaderHeight / ScreenSize.y;

            // Base hero area
            var baseHeroObj = new GameObject("BaseHeroArea", typeof(RectTransform));
            baseHeroObj.transform.SetParent(_contentArea, false);
            _baseHeroArea = baseHeroObj.GetComponent<RectTransform>();
            _baseHeroArea.anchorMin = new Vector2(0, runningY - BaseHeroAreaHeight / ScreenSize.y);
            _baseHeroArea.anchorMax = new Vector2(1, runningY);
            _baseHeroArea.offsetMin = Vector2.zero;
            _baseHeroArea.offsetMax = Vector2.zero;

            runningY -= BaseHeroAreaHeight / ScreenSize.y;

            // Material area
            var materialObj = new GameObject("MaterialArea", typeof(RectTransform));
            materialObj.transform.SetParent(_contentArea, false);
            _materialArea = materialObj.GetComponent<RectTransform>();
            _materialArea.anchorMin = new Vector2(0, runningY - MaterialAreaHeight / ScreenSize.y);
            _materialArea.anchorMax = new Vector2(1, runningY);
            _materialArea.offsetMin = Vector2.zero;
            _materialArea.offsetMax = Vector2.zero;

            runningY -= MaterialAreaHeight / ScreenSize.y;

            // Preview area
            var previewObj = new GameObject("PreviewArea", typeof(RectTransform));
            previewObj.transform.SetParent(_contentArea, false);
            _previewArea = previewObj.GetComponent<RectTransform>();
            _previewArea.anchorMin = new Vector2(0, runningY - PreviewAreaHeight / ScreenSize.y);
            _previewArea.anchorMax = new Vector2(1, runningY);
            _previewArea.offsetMin = Vector2.zero;
            _previewArea.offsetMax = Vector2.zero;

            runningY -= PreviewAreaHeight / ScreenSize.y;

            // Animation area
            var animationObj = new GameObject("AnimationArea", typeof(RectTransform), typeof(Image));
            animationObj.transform.SetParent(_contentArea, false);
            _animationArea = animationObj.GetComponent<RectTransform>();
            _animationArea.anchorMin = Vector2.zero;
            _animationArea.anchorMax = Vector2.one;
            _animationArea.offsetMin = Vector2.zero;
            _animationArea.offsetMax = Vector2.zero;

            var animationImage = animationObj.GetComponent<Image>();
            animationImage.color = new Color(1, 1, 1, 0);

            // Footer area
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

        private void CreateBaseHeroArea()
        {
            // Base hero area background
            var baseBgObj = new GameObject("BaseHeroBg", typeof(RectTransform), typeof(Image));
            baseBgObj.transform.SetParent(_baseHeroArea, false);
            var baseBgRT = baseBgObj.GetComponent<RectTransform>();
            baseBgRT.anchorMin = Vector2.zero;
            baseBgRT.anchorMax = Vector2.one;
            baseBgRT.offsetMin = Vector2.zero;
            baseBgRT.offsetMax = Vector2.zero;

            var baseBgImage = baseBgObj.GetComponent<Image>();
            baseBgImage.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
        }

        private void CreateMaterialArea()
        {
            // Material area background
            var materialBgObj = new GameObject("MaterialBg", typeof(RectTransform), typeof(Image));
            materialBgObj.transform.SetParent(_materialArea, false);
            var materialBgRT = materialBgObj.GetComponent<RectTransform>();
            materialBgRT.anchorMin = Vector2.zero;
            materialBgRT.anchorMax = Vector2.one;
            materialBgRT.offsetMin = Vector2.zero;
            materialBgRT.offsetMax = Vector2.zero;

            var materialBgImage = materialBgObj.GetComponent<Image>();
            materialBgImage.color = new Color(0.1f, 0.05f, 0.05f, 0.9f);
        }

        private void CreatePreviewArea()
        {
            // Preview area background
            var previewBgObj = new GameObject("PreviewBg", typeof(RectTransform), typeof(Image));
            previewBgObj.transform.SetParent(_previewArea, false);
            var previewBgRT = previewBgObj.GetComponent<RectTransform>();
            previewBgRT.anchorMin = Vector2.zero;
            previewBgRT.anchorMax = Vector2.one;
            previewBgRT.offsetMin = Vector2.zero;
            previewBgRT.offsetMax = Vector2.zero;

            var previewBgImage = previewBgObj.GetComponent<Image>();
            previewBgImage.color = PanelColor;
        }

        private void CreateAnimationArea()
        {
            // Animation area already created in CreateMainContainer
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

        private void AddSynthViewComponent()
        {
            var synthViewObj = new GameObject("SynthView", typeof(RectTransform));
            synthViewObj.transform.SetParent(_canvas.transform, false);

            var synthView = synthViewObj.AddComponent<SynthView>();

            // Link references
            synthView.Canvas = _canvas;
            synthView.ContentArea = _contentArea;
            synthView.HeaderArea = _headerArea;
            synthView.BaseHeroArea = _baseHeroArea;
            synthView.MaterialArea = _materialArea;
            synthView.PreviewArea = _previewArea;
            synthView.AnimationArea = _animationArea;
            synthView.FooterArea = _footerArea;

            // Create prefabs
            CreatePrefabs();

            Debug.Log("SynthView component added. Assign prefab references in Inspector.");
        }

        private void CreatePrefabs()
        {
            // Create HeroSelectCard prefab
            CreateHeroSelectCardPrefab();

            // Create MaterialCard prefab
            CreateMaterialCardPrefab();

            // Create ResultPreview prefab
            CreateResultPreviewPrefab();

            Debug.Log("Synthesis Lab prefabs created.");
        }

        private void CreateHeroSelectCardPrefab()
        {
            var cardObj = new GameObject("HeroSelectCard", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(200, 250);

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
            var prefabPath = "Assets/Prefabs/UI/HeroSelectCard.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            DestroyImmediate(cardObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        private void CreateMaterialCardPrefab()
        {
            var cardObj = new GameObject("MaterialCard", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(160, 200);

            var image = cardObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.1f, 0.1f, 0.8f);

            var button = cardObj.AddComponent<Button>();

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/MaterialCard.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            DestroyImmediate(cardObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        private void CreateResultPreviewPrefab()
        {
            var panelObj = new GameObject("ResultPreview", typeof(RectTransform));
            var panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2((int)ScreenSize.x - 100, (int)PreviewAreaHeight);
            panelRT.anchoredPosition = Vector2.zero;

            // Save as prefab
#if UNITY_EDITOR
            var prefabPath = "Assets/Prefabs/UI/ResultPreview.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(panelObj, prefabPath);
            DestroyImmediate(panelObj);
            Debug.Log($"Created prefab: {prefabPath}");
#endif
        }

        [ContextMenu("Clear Synthesis Lab UI")]
        public void ClearUI()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            Debug.Log("Synthesis Lab UI cleared.");
        }
    }
}