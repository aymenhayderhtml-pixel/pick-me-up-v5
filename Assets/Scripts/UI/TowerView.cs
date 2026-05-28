using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerView : MonoBehaviour
{
    [SerializeField] public Button backBtn;
    [SerializeField] public TextMeshProUGUI floorLabel;
    [SerializeField] public TextMeshProUGUI currentFloorTitle;
    [SerializeField] public TextMeshProUGUI currentFloorNumber;
    [SerializeField] public TextMeshProUGUI highestFloorLabel;
    [SerializeField] public Button startRunBtn;
    [SerializeField] public TextMeshProUGUI startRunBtnText;
    
    private ITowerService _tower;
    
    void Start()
    {
        _tower = ServiceRegistry.Instance.Resolve<ITowerService>();
        _tower.LoadProgress();
        backBtn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("Hub"));
        startRunBtn.onClick.AddListener(OnStartRun);
        RefreshUI();
    }
    
    void OnStartRun()
    {
        if (_tower.CurrentFloor == 0)
            _tower.StartRun();
        // TODO: Load combat scene with floor data
        RefreshUI();
    }
    
    public void RefreshUI()
    {
        string floor = _tower.CurrentFloor > 0 ? _tower.CurrentFloor.ToString() : "—";
        floorLabel.text = $"Floor {floor}";
        currentFloorNumber.text = floor;
        highestFloorLabel.text = $"Highest: Floor {_tower.HighestFloor}";
        
        if (_tower.CurrentFloor > 0)
        {
            startRunBtnText.text = $"CONTINUE FLOOR {_tower.CurrentFloor}";
            currentFloorTitle.text = "CURRENT FLOOR";
        }
        else
        {
            startRunBtnText.text = "ENTER TOWER";
            currentFloorTitle.text = "TOWER STATUS";
        }
    }
}