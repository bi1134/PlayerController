using UnityEngine;

[CreateAssetMenu(fileName = "BulletPropertiesSO", menuName = "Scriptable Objects/BulletPropertiesSO")]
public class BulletPropertiesSO : ScriptableObject
{
    public BulletType bulletType;
    public int maxBounces = 2;
    public float maxLifeTime = 3f;
}

public enum BulletType
{
    Normal,
    Explosive,
    Electric,
    Fire,
    Ice
}
