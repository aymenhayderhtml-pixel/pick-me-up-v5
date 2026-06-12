using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HubView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldLabel;
    [SerializeField] private TextMeshProUGUI gemsLabel;
    [SerializeField] private TextMeshProUGUI stonesLabel;
    [SerializeField] private TextMeshProUGUI staminaLabel;
    [SerializeField] private Button rosterBtn;
    [SerializeField] private Button synthBtn;
    [SerializeField] private Button trainBtn;
    [SerializeField] private Button towerBtn;
    [SerializeField] private Button summonBtn;
    [SerializeField] private Button dungeonBtn;
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button memorialBtn;

    private ICurrencyService _currencyService;

    private void Start()
    {
        // Setup background first so it renders even if services aren't ready
        SetupBackground();

        if (ServiceRegistry.Instance != null)
        {
            _currencyService = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        }

        WireButton(rosterBtn, "Roster");
        WireButton(synthBtn, "SynthesisLab");
        WireButton(trainBtn, "Facilities");
        WireButton(towerBtn, "Tower");
        WireButton(summonBtn, "Summon");
        WireButton(dungeonBtn, "Formation");
        WireButton(inventoryBtn, "Inventory");
        WireButton(memorialBtn, "MemorialHall");

        RefreshUI();
    }

    private void SetupBackground()
    {
        GameObject bgGo = null;
        foreach (Transform child in transform)
        {
            if (child.name == "Background")
            {
                bgGo = child.gameObject;
                break;
            }
        }
        if (bgGo == null)
        {
            bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(transform, false);
        }
        bgGo.transform.SetAsFirstSibling();
        var rt = bgGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = bgGo.GetComponent<Image>();
        img.raycastTarget = false;
        Sprite loadedSprite = Resources.Load<Sprite>("UI/hub_background");
        if (loadedSprite == null)
        {
            var tex = Resources.Load<Texture2D>("UI/hub_background");
            if (tex != null)
                loadedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        if (loadedSprite != null)
        {
            img.sprite = loadedSprite;
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.preserveAspect = false;
        }
        else
        {
            img.color = new Color(0.08f, 0.10f, 0.15f, 1f);
        }

        // Make CenterArea transparent so the background shows through
        foreach (Transform child in transform)
        {
            if (child.name == "CenterArea")
            {
                var centerImg = child.GetComponent<Image>();
                if (centerImg != null)
                {
                    centerImg.color = new Color(0f, 0f, 0f, 0f);
                }
                break;
            }
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (_currencyService == null)
        {
            return;
        }

        if (goldLabel != null)
        {
            goldLabel.text = "Gold: " + _currencyService.GetGold().ToString("N0");
        }

        if (gemsLabel != null)
        {
            gemsLabel.text = "Gems: " + _currencyService.GetGems().ToString("N0");
        }

        if (stonesLabel != null)
        {
            stonesLabel.text = "Stones: " + _currencyService.GetAttributeStones().ToString("N0");
        }

        if (staminaLabel != null) staminaLabel.text = "Stamina: --";
    }

    private void WireButton(Button button, string sceneName)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
    }
}
