using UnityEngine;

public class AlwaysRotating : MonoBehaviour
{
    public float _rotateSpeed = 2f;
    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * _rotateSpeed);
    }
}
