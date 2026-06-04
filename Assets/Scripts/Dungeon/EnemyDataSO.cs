using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "PickMeUp/Dungeon/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Earth,
        Light,
        Dark
    }

    [Header("Identity")]
    [SerializeField] private string id = "enemy_unnamed";
    [SerializeField] private string displayName = "Unnamed Enemy";
    [SerializeField, TextArea(2, 4)] private string description = string.Empty;

    [Header("Visuals")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite portrait;

    [Header("Stats")]
    [SerializeField, Min(1)] private int baseHp = 100;
    [SerializeField, Min(0)] private int baseAttack = 10;
    [SerializeField, Min(0)] private int baseDefense = 5;
    [SerializeField, Min(0.1f)] private float baseSpeed = 1f;
    [SerializeField] private ElementType element = ElementType.None;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public Sprite Portrait => portrait;
    public int BaseHp => baseHp;
    public int BaseAttack => baseAttack;
    public int BaseDefense => baseDefense;
    public float BaseSpeed => baseSpeed;
    public ElementType Element => element;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = string.IsNullOrWhiteSpace(displayName)
                ? "enemy_unnamed"
                : "enemy_" + displayName.Trim().ToLowerInvariant().Replace(" ", "_");
        }

        if (baseHp < 1) baseHp = 1;
        if (baseAttack < 0) baseAttack = 0;
        if (baseDefense < 0) baseDefense = 0;
        if (baseSpeed < 0.1f) baseSpeed = 0.1f;
    }
}
