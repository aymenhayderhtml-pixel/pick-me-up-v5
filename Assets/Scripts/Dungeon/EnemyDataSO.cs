using UnityEngine;

// ---------------------------------------------------------------------------
//  EnemyDataSO — extended to include an explicit integer SPD stat.
//  All existing fields preserved.  BaseSPD defaults to 40 so old assets
//  continue to compile and participate in the turn queue without reassignment.
// ---------------------------------------------------------------------------
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "PickMeUp/Dungeon/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public enum ElementType
    {
        None, Fire, Water, Earth, Light, Dark
    }

    [Header("Identity")]
    [SerializeField] private string id           = "enemy_unnamed";
    [SerializeField] private string displayName  = "Unnamed Enemy";
    [SerializeField, TextArea(2, 4)] private string description = string.Empty;

    [Header("Visuals")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite portrait;

    [Header("Stats")]
    [SerializeField, Min(1)]    private int   baseHp      = 100;
    [SerializeField, Min(0)]    private int   baseAttack  = 10;
    [SerializeField, Min(0)]    private int   baseDefense = 5;
    [SerializeField, Min(1)]    private int   baseSPD     = 40;   // NEW – integer speed for turn queue
    [SerializeField, Min(0.1f)] private float baseSpeed   = 1f;   // Legacy float kept for backwards compat
    [SerializeField]            private ElementType element = ElementType.None;

    [Header("Behavior")]
    [SerializeField] private EnemyBehavior behavior = EnemyBehavior.Standard;

    public EnemyBehavior Behavior => behavior;

    // Public accessors
    public string      Id          => id;
    public string      DisplayName => displayName;
    public string      Description => description;
    public Sprite      Icon        => icon;
    public Sprite      Portrait    => portrait;
    public int         BaseHp      => baseHp;
    public int         BaseAttack  => baseAttack;
    public int         BaseDefense => baseDefense;
    public int         BaseSPD     => baseSPD;
    public float       BaseSpeed   => baseSpeed;
    public ElementType Element     => element;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = string.IsNullOrWhiteSpace(displayName)
                ? "enemy_unnamed"
                : "enemy_" + displayName.Trim().ToLowerInvariant().Replace(" ", "_");
        }

        if (baseHp      < 1)    baseHp      = 1;
        if (baseAttack  < 0)    baseAttack  = 0;
        if (baseDefense < 0)    baseDefense = 0;
        if (baseSPD     < 1)    baseSPD     = 1;
        if (baseSpeed   < 0.1f) baseSpeed   = 0.1f;
    }
}


public enum EnemyBehavior
{
    Standard,
    FocusWeakest,
    FocusBackRow,
    FocusHealer,
    BossCleaves
}