using Photon.Pun;
using UnityEngine;

public class ButtonTouch : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Hand"))
        {
            Debug.Log("�¸�");
            GetComponent<Animator>().SetTrigger("Touch");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.CompareTag("Hand"))
        {
            Debug.Log("�¸�");
            GetComponent<Animator>().SetTrigger("Touch");
            var winner = PhotonNetwork.NickName; // �Ǵ� Owner.NickName
            PlayManager.Instance.photonView.RPC("EscapeSequence", RpcTarget.All, winner);
        }
    }
}
