using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Damage { get; private set; }

    private Rigidbody _rb;

    public void Setup(Vector3 velocity, int damage)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = velocity;
        Damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Enemy>(out var enemy))
        {
            if (enemy.photonView.IsMine)
            {
                PhotonNetwork.Destroy(enemy.gameObject);
            }
        }

        Destroy(gameObject);
    }
}
