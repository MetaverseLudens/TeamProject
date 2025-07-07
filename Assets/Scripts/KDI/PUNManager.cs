using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PUNManager : MonoBehaviourPunCallbacks //나중에 서버 연결된 다음 버튼 활성화되는 안전장치 필요
{
    public static PUNManager Instance;
    private readonly string gameversion = "1";
    public ServerSettings setting = null; 
    private string userId = "shuttle";

    [SerializeField] Button ServerBtn;
    [SerializeField] int _roomMaxPlayers = 4;

    //public TMP_InputField _userIDInput;
    public TMP_InputField _roomNameInput;

    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();
    public GameObject roomPrefab;
    public Transform scrollContent;

    public GameObject playerPrefabs;
    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameversion;
        Connect();
    }

    private void Start()
    {
        //userId = PlayerPrefs.GetString("USER_ID", $"USER_{Random.Range(0, 100):00}");
        //_userIDInput.text = userId;
        //PhotonNetwork.NickName = userId;
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    #region 
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connect to Master Server");
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions ro = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = _roomMaxPlayers
        };

        string newRoomName = $"ROOM_{Random.Range(1000, 9999)}";

        _roomNameInput.text = newRoomName;
        PhotonNetwork.CreateRoom(newRoomName, ro);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect to Master Server");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Enter to Lobby");
        roomPrefab = Resources.Load<GameObject>("RoomBtnPrefab");
        //_userIDInput = GameObject.Find("NicknameInputfield")?.GetComponent<InputField>();
        //_roomNameInput = GameObject.Find("RoomNameInputfield")?.GetComponent<InputField>();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.NickName}OnJoinedRoom");
        var props = PhotonNetwork.LocalPlayer.CustomProperties;

        if (props.ContainsKey("charId"))
        {
            Debug.Log(">> 기존 charId 발견 → 삭제 시도");
            props.Remove("charId");
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        else
        {
            Debug.Log(">> charId 없음");
        }
        UpdatePlayer();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(MyString.SCENE_ROOM);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} Entered Room");
        UpdatePlayer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} Left Room");
        UpdatePlayer();
    }

    public void UpdatePlayer()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.Instantiate(playerPrefabs.name, Vector3.zero, Quaternion.identity);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        GameObject tempRoom = null;
        for (int i = 0; i < roomList.Count; i++)
        {
            var room = roomList[i];
            if (room.RemovedFromList == true)
            {
                roomDict.TryGetValue(room.Name, out tempRoom);
                Destroy(tempRoom);
                roomDict.Remove(room.Name);
            }
            else
            {
                if (roomDict.ContainsKey(room.Name) == false)
                {
                    GameObject _room = Instantiate(roomPrefab, scrollContent);
                    _room.GetComponent<RoomData>()._roomInfo = room;
                    _room.GetComponent<RoomData>().SetText();
                    roomDict.Add(room.Name, _room);
                }
                else //룸 이름으로 정보가 있다면
                {
                    if (roomDict.TryGetValue(room.Name, out tempRoom))
                    {
                        if (tempRoom == null)
                        {
                            roomDict.Remove(room.Name); // 이미 Destroy된 오브젝트 → 딕셔너리에서도 제거
                            continue;
                        }

                        tempRoom.GetComponent<RoomData>()._roomInfo = room;
                        tempRoom.GetComponent<RoomData>().SetText();
                    }
                }
            }
        }
    }

    public void BtnRandom()
    {
        /*
        if (string.IsNullOrEmpty(_userIDInput.text))
        {
            userId = $"USER_{Random.Range(0, 100):00}";
            _userIDInput.text = userId;
        }
        PlayerPrefs.SetString("USER_ID", _userIDInput.text);
        PhotonNetwork.NickName = _userIDInput.text;
        */
        PhotonNetwork.JoinRandomRoom();
    }

    public void BtnMakeRoom()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InLobby)
        {
            Debug.LogWarning("아직 로비에 입장하지 않았습니다.");
            return;
        }
        /*
        if (string.IsNullOrEmpty(_userIDInput.text))
        {
            userId = $"USER_{Random.Range(0, 100):00}";
            _userIDInput.text = userId;
        }
        //PlayerPrefs.SetString("USER_ID", _userIDInput.text);
        PhotonNetwork.NickName = _userIDInput.text;
        */
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = _roomMaxPlayers;
        if (string.IsNullOrEmpty(_roomNameInput.text))
        {
            _roomNameInput.text = $"ROOM_{Random.Range(1, 100):000}";
        }
        PhotonNetwork.CreateRoom(_roomNameInput.text, ro);
    }

    public override void OnLeftRoom()
    {
        if (!PhotonNetwork.InLobby) PhotonNetwork.JoinLobby();
        PhotonNetwork.LoadLevel(MyString.SCENE_LOBBY);
    }
    #endregion
}