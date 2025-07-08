using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    [SerializeField]
    private string[] _playerPrefabName;

    [SerializeField]
    private Vector3[] _spawnVec;

    private void Start()
    {
        _playerPrefabName = new string[4];
        _playerPrefabName[0] = MyString.PLAYER_PREFAB_0;
        _playerPrefabName[1] = MyString.PLAYER_PREFAB_1;
        _playerPrefabName[2] = MyString.PLAYER_PREFAB_2;
        _playerPrefabName[3] = MyString.PLAYER_PREFAB_3;
        _spawnVec = new Vector3[4];
        _spawnVec[0] = new Vector3(5, 0, 0);
        _spawnVec[1] = new Vector3(-5, 0, 0);
        _spawnVec[2] = new Vector3(0, 0, -5);
        _spawnVec[3] = new Vector3(0, 0, 5);
    }

    public void OnJoinedRoom()
    {
        int ran = Random.Range(0, 4);
        PhotonNetwork.Instantiate(_playerPrefabName[ran], _spawnVec[ran], Quaternion.identity);
    }
}
