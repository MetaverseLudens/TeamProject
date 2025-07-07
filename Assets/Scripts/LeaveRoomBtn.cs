using Photon.Pun;
using UnityEngine;

public class LeaveRoomBtn : MonoBehaviour
{
    public void LeftBtn()
    {
        PhotonNetwork.LeaveRoom();
    }
}
