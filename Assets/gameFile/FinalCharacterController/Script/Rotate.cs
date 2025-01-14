using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 50f;

   private void Update()
   {
        transform.Rotate(Vector3.up * Time.deltaTime * speed);
    }
}
