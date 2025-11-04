using UnityEngine;

[CreateAssetMenu(fileName = "New Collectible Data", menuName = "Scriptable Objects/State/New Collectible Data")]
public class CollectibleData : ScriptableObject
{
    #region Collectible Data
    [Header("Identity")]
    [SerializeField] private string collectibleID;
    [SerializeField] private string collectibleName;
    [TextArea(2, 4)]
    [SerializeField] private string description;
    
    [Header("Visuals")]
    [SerializeField] private GameObject worldModel;
    [SerializeField] private Sprite iconSprite;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("VFX (Optional)")]
    [SerializeField] private ParticleSystem pickupParticles;
    [SerializeField] private Color highlightColor = Color.yellow;
    #endregion
    
    #region Accessors
    public string CollectibleID => collectibleID;
    public string CollectibleName => collectibleName;
    public string Description => description;
    public GameObject WorldModel => worldModel;
    public Sprite IconSprite => iconSprite;
    public AudioClip PickupSound => pickupSound;
    public ParticleSystem PickupParticles => pickupParticles;
    public Color HighlightColor => highlightColor;
    #endregion
}
