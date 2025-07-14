using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    public Transform _slotParent;
    public GameObject _slotPrefab;
    public Button _actionBtn;
    public Sprite[] _actionBtnSprites; //start > ready > cancel
    public Sprite[] characterSprites; // 캐릭터 ID별 스프라이트
    private Dictionary<int, PlayerSlotUI> slots = new Dictionary<int, PlayerSlotUI>();
    private HashSet<int> selectedCharacters = new HashSet<int>();

    public Transform characterModelRoot; // 캐릭터들 부모 오브젝트
    private GameObject[] allCharacterModels; // 모든 캐릭터 프리팹들

    [SerializeField] TextMeshProUGUI roomTxt;
    [SerializeField] TextMeshProUGUI countTxt;

    private void Awake()
    {
        Instance = this;
        allCharacterModels = new GameObject[characterModelRoot.childCount];
    }

    void Start()
    {
        for (int i = 0; i < characterModelRoot.childCount; i++)
        {
            allCharacterModels[i] = characterModelRoot.GetChild(i).gameObject;
            characterModelRoot.GetChild(i).gameObject.SetActive(false);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        StartCoroutine(WaitForJoinAndInit());
        _actionBtn.onClick.AddListener(OnActionButtonClicked);
        roomTxt.text = PhotonNetwork.CurrentRoom.Name;
        countTxt.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    void ShowMyCharacterModel()
    {
        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("charId", out object val))
            return;

        int charId = (int)val;

        for (int i = 0; i < allCharacterModels.Length; i++)
        {
            if (i == charId)
            {
                if (!allCharacterModels[i].activeSelf)
                    allCharacterModels[i].SetActive(true);
            }
            else
            {
                if (allCharacterModels[i].activeSelf)
                    allCharacterModels[i].SetActive(false);
            }
        }
    }

    // 캐릭터 선택 처리
    public void RequestCharacterSelection(int charId)
    {
        Debug.Log("RequestCharacterSelection " + charId);

        // 서버에서 캐릭터 선택을 처리하도록 요청
        photonView.RPC("RPC_RequestCharacterSelection", RpcTarget.MasterClient, charId);
    }

    // 서버에서 캐릭터 선택 처리 (Master Client에서만 실행)
    [PunRPC]
    void RPC_RequestCharacterSelection(int charId)
    {
        if (selectedCharacters.Contains(charId))
        {
            // 캐릭터가 이미 선택되었으므로 선택을 거부
            return;
        }

        selectedCharacters.Add(charId);

        // 선택된 캐릭터를 모든 클라이언트에 동기화
        photonView.RPC("RPC_UpdateCharacterSelection", RpcTarget.All, charId);
    }

    // 캐릭터 선택이 완료되었음을 모든 클라이언트에 알리는 RPC 메서드
    [PunRPC]
    void RPC_UpdateCharacterSelection(int charId)
    {
        // 선택된 캐릭터를 로컬 클라이언트에서 반영
        // 예: UI 업데이트 또는 캐릭터 모델 표시
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("charId"))
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "charId", charId } });
        }

        // 캐릭터 모델 보여주기
        ShowMyCharacterModel();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable
        {
            { "charId", null },
            { "seatIndex", null },
            { "isReady", false }
        };
        UpdateActionButton();
    }

    IEnumerator WaitForJoinAndInit()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            CreateSlot(player);

            //charId 처리
            if (player.CustomProperties.TryGetValue("charId", out object val))
            {
                int charId = (int)val;
                selectedCharacters.Add(charId);

                if (slots.TryGetValue(player.ActorNumber, out var slot))
                    slot.SetCharacterSprite(characterSprites[charId]);

                CharacterSelectPanel.Instance.DisableCharacterButton(charId);
            }

            //isReady 상태도 반영
            bool isReady = false;
            if (player.CustomProperties.TryGetValue("isReady", out object readyVal) && readyVal is bool b)
                isReady = b;

            if (slots.TryGetValue(player.ActorNumber, out var readySlot))
            {
                readySlot.SetReady(isReady, player.IsMasterClient);
            }
        }

        AutoAssignCharacter(PhotonNetwork.LocalPlayer);
        AutoAssignSeat(PhotonNetwork.LocalPlayer);

        UpdateActionButton();
        ShowMyCharacterModel();
    }

    void CreateSlot(Player player)
    {
        GameObject slotObj = Instantiate(_slotPrefab, _slotParent);
        PlayerSlotUI slotUI = slotObj.GetComponent<PlayerSlotUI>();
        slotUI.SetNickname(player.NickName);
        slots[player.ActorNumber] = slotUI;
        slotUI.SetReady(false, player.IsMasterClient);
    }

    void OnActionButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (CanStartGame())
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.LoadLevel(MyString.SCENE_GAME);
            }
        }
        else
        {
            bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") &&
                           (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", !isReady } });
        }

        UpdateActionButton();
    }

    void UpdateActionButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _actionBtn.GetComponent<Image>().sprite = _actionBtnSprites[0];
            _actionBtn.interactable = CanStartGame();
        }
        else
        {
            bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") &&
                           (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
            _actionBtn.GetComponent<Image>().sprite = isReady ? _actionBtnSprites[2] : _actionBtnSprites[1];
            _actionBtn.interactable = true;
        }
        countTxt.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    bool CanStartGame()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount >= 2 && AllReady();
    }

    bool AllReady()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.IsMasterClient) continue;
            if (!p.CustomProperties.ContainsKey("isReady") || !(bool)p.CustomProperties["isReady"])
                return false;
        }
        return true;
    }

    void AutoAssignCharacter(Player player)
    {
        if (!PhotonNetwork.InRoom || player == null || !player.IsLocal)
            return;

        if (player.CustomProperties.ContainsKey("charId")) return;

        for (int i = 0; i < characterSprites.Length; i++)
        {
            if (!selectedCharacters.Contains(i))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "charId", i } });
                return;
            }
        }
    }

    public bool IsCharacterTaken(int charId)
    {
        return selectedCharacters.Contains(charId);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreateSlot(newPlayer);
        AutoAssignCharacter(newPlayer);
        AutoAssignSeat(newPlayer);
        UpdateActionButton();
    }

    void AutoAssignSeat(Player player)
    {
        if (player.CustomProperties.ContainsKey("seatIndex")) return;

        int seatIndex = PhotonNetwork.PlayerList.Length - 1;
        player.SetCustomProperties(new Hashtable { { "seatIndex", seatIndex } });
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int actorNum = otherPlayer.ActorNumber;

        if (slots.TryGetValue(actorNum, out var slot))
        {
            Destroy(slot.gameObject);
            slots.Remove(actorNum);
        }

        if (otherPlayer.CustomProperties.TryGetValue("charId", out object val))
        {
            int charId = (int)val;
            selectedCharacters.Remove(charId);
            CharacterSelectPanel.Instance.EnableCharacterButton(charId);
        }

        UpdateActionButton();
        ReassignSeatIndices();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        int actorNum = targetPlayer.ActorNumber;

        if (changedProps.ContainsKey("isReady") && slots.TryGetValue(actorNum, out var targetSlot))
        {
            targetSlot.SetReady((bool)changedProps["isReady"], targetPlayer.IsMasterClient);
            UpdateActionButton();
        }

        if (changedProps.ContainsKey("charId"))
        {
            int newCharId = (int)changedProps["charId"];
            selectedCharacters.Add(newCharId);
            if (slots.TryGetValue(actorNum, out var s))
                s.SetCharacterSprite(characterSprites[newCharId]);

            CharacterSelectPanel.Instance.DisableCharacterButton(newCharId);

            if (changedProps.ContainsKey("oldCharId"))
            {
                int oldCharId = (int)changedProps["oldCharId"];
                selectedCharacters.Remove(oldCharId);
                CharacterSelectPanel.Instance.EnableCharacterButton(oldCharId);
            }
        }
        if (targetPlayer.IsLocal)
        {
            ShowMyCharacterModel();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Hashtable props = new Hashtable { { "seatIndex", 0 } };
        newMasterClient.SetCustomProperties(props);

        UpdateActionButton();

        if (slots.TryGetValue(newMasterClient.ActorNumber, out var slot))
        {
            slot.SetReady(
                newMasterClient.CustomProperties.ContainsKey("isReady") && (bool)newMasterClient.CustomProperties["isReady"],
                true
            );
        }
    }

    public override void OnLeftRoom()
    {
        ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable
        {
            { "charId", null },
            { "seatIndex", null },
            { "isReady", false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);
    }

    void ReassignSeatIndices()
    {
        int i = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Hashtable props = new Hashtable { { "seatIndex", i } };
            player.SetCustomProperties(props);
            i++;
        }
    }
}
