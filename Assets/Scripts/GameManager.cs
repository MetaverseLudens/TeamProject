using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void OnJoinedRoom()
    {
        var prefabName = "Player";
        PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
    }
}
