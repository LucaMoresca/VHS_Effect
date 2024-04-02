using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "VHSMaterials", menuName = "Tesi/VHSMaterials")]
public class VHSMaterials : ScriptableObject
{
    public Material customEffect;

    static VHSMaterials _instance;

    public static VHSMaterials Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = UnityEngine.Resources.Load<VHSMaterials>("VHSMaterials");
            return _instance;
        }
    }
}