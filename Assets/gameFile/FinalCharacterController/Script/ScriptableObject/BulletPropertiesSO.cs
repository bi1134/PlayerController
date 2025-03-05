using UnityEngine;

[CreateAssetMenu(fileName = "BulletPropertiesSO", menuName = "Scriptable Objects/BulletPropertiesSO")]
public class BulletPropertiesSO : ScriptableObject
{
    public float bulletDrop = 9.81f;
    public int maxBounces = 2;
    public float maxLifeTime = 3f;
}
