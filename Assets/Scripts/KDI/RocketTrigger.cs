using UnityEngine;
using Photon.Pun;

public class RocketTrigger : MonoBehaviour
{
    [SerializeField] GameObject rocketUI;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            //if (PlayManager.Instance.IsCanEscape)
            rocketUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            rocketUI.SetActive(false);
        }
    }
    public void OnClickTryEscape()
    {
        // 1. ��� �÷��̾� ������Ʈ Ž��
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();

            // 2. ���� �÷��̾��� ��츸 ����
            if (pv != null && pv.IsMine)
            {
                Debug.Log("Ż�� �õ�: " + pv.Owner.NickName);
                //PlayManager.Instance.TryEscape(pv.Owner); //Ż�� �� ����
                return;
            }
        }

        Debug.LogWarning("���� �÷��̾ ã�� �� ����");
    }
}
