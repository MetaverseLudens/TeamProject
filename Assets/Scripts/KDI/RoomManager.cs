using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.Rendering;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    public Transform _slotParent;
    public GameObject _slotPrefab;
    public Button _actionBtn;
    public TMP_Text _actionBtnText;
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
        for(int i = 0; i < characterModelRoot.childCount; i++)
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
                PhotonNetwork.LoadLevel(MyString.SCENE_GAME);
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
            _actionBtnText.text = "Start";
            _actionBtn.interactable = CanStartGame();

        }
        else
        {
            bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") &&
                           (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
            _actionBtnText.text = isReady ? "Cancel" : "Ready";
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

    public void RequestCharacterSelection(int charId)
    {
        Debug.Log("RequestCharacterSelection " + charId);
        if (selectedCharacters.Contains(charId)) return;

        // 현재 선택한 캐릭터가 있다면 리스트에서 제거
        int? prevCharId = null;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("charId", out object prev))
        {
            prevCharId = (int)prev;
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "charId", charId } };

        if (prevCharId.HasValue)
            props["oldCharId"] = prevCharId.Value; // 이전 캐릭터도 함께 전송

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    void AutoAssignCharacter(Player player)
    {
        if (!PhotonNetwork.InRoom || player == null || !player.IsLocal)
            return;
        Debug.Log(player.CustomProperties);
        if (player.CustomProperties.ContainsKey("charId")) return;

        for (int i = 0; i < characterSprites.Length; i++)
        {
            if (!selectedCharacters.Contains(i))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "charId", i } });
                Debug.Log($"{player} Auto Assign Character");
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
        Debug.Log("OnPlayerEnteredRoom" + newPlayer.NickName);
        CreateSlot(newPlayer);
        AutoAssignCharacter(newPlayer);
        AutoAssignSeat(newPlayer);
        UpdateActionButton();
    }
    void AutoAssignSeat(Player player)
    {
        Debug.Log("AutoAssignSeat " + player.CustomProperties);
        if (player.CustomProperties.ContainsKey("seatIndex")) return;

        int seatIndex = PhotonNetwork.PlayerList.Length - 1; // 단순히 입장 순서대로
        Debug.Log("AutoAssignSeat for " + player.NickName + " → " + seatIndex);
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
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) //플레이어의 custom properties 값들이 바뀌면 실행
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

    public override void OnMasterClientSwitched(Player newMasterClient) //방장 나가서 새로 방장 설정
    {
        Debug.Log("방장이 바뀜 → " + newMasterClient.NickName);

        Hashtable props = new Hashtable { { "seatIndex", 0 } };
        newMasterClient.SetCustomProperties(props);

        UpdateActionButton();

        if (slots.TryGetValue(newMasterClient.ActorNumber, out var slot))
        {
            slot.SetReady(
                newMasterClient.CustomProperties.ContainsKey("isReady") && (bool)newMasterClient.CustomProperties["isReady"],
                true // 이제는 방장
            );
        }
    }   
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room → 내 custom props 초기화");

        // 내가 기존 방에서 쓰던 값 초기화
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
        Debug.Log("Reassigning seat indices...");

        // 모든 플레이어를 seatIndex 순으로 재배열
        int i = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Hashtable props = new Hashtable { { "seatIndex", i } };
            player.SetCustomProperties(props);
            i++;
        }
    }
}
