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
        // ��ġ ����
        transform.position = _initialPosition;

        // ���� �������� ȸ��
        transform.Rotate(Vector3.up * Time.deltaTime * _rotateSpeed, Space.World);
    }
}
