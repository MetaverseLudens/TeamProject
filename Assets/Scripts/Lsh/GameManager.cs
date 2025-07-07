using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    [SerializeField]
    private Queue<string> _playerPrefabNameQueue = new Queue<string>();
    [SerializeField]
    private Queue<Vector3> _playerSpawnQueue = new Queue<Vector3>();

    private void Start()
    {
        _playerPrefabNameQueue.Enqueue(MyString.PLAYER_PREFAB_0);
        _playerPrefabNameQueue.Enqueue(MyString.PLAYER_PREFAB_1);
        _playerPrefabNameQueue.Enqueue(MyString.PLAYER_PREFAB_2);
        _playerPrefabNameQueue.Enqueue(MyString.PLAYER_PREFAB_3);
        _playerSpawnQueue.Enqueue(new Vector3(5, 0, 0));
        _playerSpawnQueue.Enqueue(new Vector3(-5, 0, 0));
        _playerSpawnQueue.Enqueue(new Vector3(0, 0, 5));
        _playerSpawnQueue.Enqueue(new Vector3(0, 0, -5));
    }

    public void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(_playerPrefabNameQueue.Dequeue(), _playerSpawnQueue.Dequeue(), Quaternion.identity);
    }
}
