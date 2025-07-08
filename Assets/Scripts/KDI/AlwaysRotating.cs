using UnityEngine;

public class AlwaysRotating : MonoBehaviour
{
    public float _rotateSpeed = 30f;
    private Vector3 _initialPosition;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 위치 고정
        transform.position = _initialPosition;

        // 월드 기준으로 회전
        transform.Rotate(Vector3.up * Time.deltaTime * _rotateSpeed, Space.World);
    }
}
