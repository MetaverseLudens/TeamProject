using Photon.Pun;
using UnityEngine;

public class ButtonTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        Debug.Log("½Â¸®");
        GetComponent<Animator>().SetTrigger("Touch");
        string nickname = PhotonNetwork.NickName;
        PlayManager.Instance.photonView.RPC("ReportWinnerToMaster", RpcTarget.MasterClient, nickname);
        var props = PhotonNetwork.LocalPlayer.CustomProperties;
        PhotonView targetView = other.transform.parent.GetComponent<PhotonView>();
        if (targetView != null)
        {
            PlayManager.Instance.photonView.RPC("DisableCharacter", RpcTarget.All, targetView.ViewID, (int)props["charId"]);
        }
    }
}
