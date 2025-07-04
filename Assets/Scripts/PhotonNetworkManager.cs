using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private UnityEvent _joinedRoomEvent;
    private void Start()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ���� �����, �� ���� �õ�");
        TryJoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"�� ���� ����: {PhotonNetwork.CurrentRoom.Name}");
        _joinedRoomEvent?.Invoke();
    }

    private void TryJoinOrCreateRoom()
    {
        var roomName = "MyRoom";
        var options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, typedLobby: default);
        Debug.Log($"�� ���� �Ǵ� ���� �õ�: {roomName}");
    }
}
