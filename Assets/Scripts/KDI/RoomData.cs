using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomData : MonoBehaviour
{
    TMP_Text RoomInfoText;
    public RoomInfo _roomInfo;

    public RoomInfo RoomInfo
    {
        get
        {
            return _roomInfo;
        }
        set
        {
            _roomInfo = value;
            RoomInfoText.text = $"{_roomInfo.Name} ({_roomInfo.PlayerCount}/{_roomInfo.MaxPlayers})";
        }
    }
    private void Awake()
    {
        RoomInfoText = transform.GetChild(0).GetComponent<TMP_Text>();
        SetText();
        GetComponent<Button>().onClick.AddListener(() => OnEnterRoom(_roomInfo.Name));
    }
    public void SetText()
    {
        if (_roomInfo != null)
        {
            transform.GetChild(0).GetComponent<TMP_Text>().text = $"{_roomInfo.Name}";
            transform.GetChild(1).GetComponent<TMP_Text>().text = $"{_roomInfo.PlayerCount} / {_roomInfo.MaxPlayers}";
        }
    }
    private void OnEnterRoom(string roomName)
    {
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = _roomInfo.MaxPlayers;

        PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
    }

}