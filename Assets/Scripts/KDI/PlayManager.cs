using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayManager : MonoBehaviourPunCallbacks
{
    public static PlayManager Instance;
    public Transform[] spawnPoints;
    public float gameDuration = 180f;
    public GameObject gameOverUI;
    public GameObject explosionEffect;

    private TMP_Text _myTimerText;
    private float startTime;

    public Camera _rocketViewCam;
    [SerializeField] Transform _rocketParent;
    [SerializeField] Animator _rocketAnim;
    [SerializeField] GameObject[] _rocketCharacters;
    [SerializeField] Sprite[] _victorySprites;
    [SerializeField] Image _victoryImage;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(WaitAndSpawn());
        StartCoroutine(InitGameStart());
    }

    IEnumerator InitGameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터만 실행
            yield return new WaitForSeconds(1f); // 아주 약간의 대기 (로딩 마무리 시간 확보)
            startTime = (float)PhotonNetwork.Time;
            Hashtable hash = new Hashtable { { "startTime", startTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }

        // 나머지 클라이언트는 여기서 startTime을 기다림
        while (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("startTime"))
            yield return null;

        startTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["startTime"];

        StartCoroutine(StartTimer());
    }

    public void RegisterTimerText(TMP_Text t)
    {
        _myTimerText = t;
    }

    IEnumerator StartTimer()
    {
        while (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("startTime"))
            yield return null;

        startTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["startTime"];

        while (PhotonNetwork.Time - startTime < gameDuration)
        {
            float remaining = gameDuration - (float)(PhotonNetwork.Time - startTime);
            if (_myTimerText != null)
                _myTimerText.text = $"{Mathf.FloorToInt(remaining / 60):00}:{Mathf.FloorToInt(remaining % 60):00}";
            yield return null;
        }

        if (_myTimerText != null)
            _myTimerText.text = "00:00";

        gameOverUI.SetActive(true);
    }   
    private bool hasLeftRoom = false;

    IEnumerator WaitAndSpawn()
    {
        int waitFrame = 0;
        while (true)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("charId", out var cid) &&
                PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("seatIndex", out var sid) &&
                cid != null && sid != null)
                break;

            waitFrame++;
            yield return null;
        }
        SpawnMyPlayer();
    }

    void SpawnMyPlayer()
    {
        if (!PhotonNetwork.InRoom) return;

        var props = PhotonNetwork.LocalPlayer.CustomProperties;
        int charId = (int)props["charId"];
        int seatIndex = (int)props["seatIndex"];
        if (seatIndex >= spawnPoints.Length) return;

        string prefabName = $"Player_{charId}";
        PhotonNetwork.Instantiate(prefabName, spawnPoints[seatIndex].position, spawnPoints[seatIndex].rotation);
    }

    [PunRPC]
    public void EscapeSequence(string winnerName)
    {
        StartCoroutine(EscapeSequenceRoutine(winnerName));
    }

    private IEnumerator EscapeSequenceRoutine(string winnerName)
    {
        _rocketViewCam.gameObject.SetActive(true);
        _rocketViewCam.depth = 50;
        yield return new WaitForSecondsRealtime(1f);
        _rocketAnim.SetTrigger("Fly");
        yield return new WaitForSecondsRealtime(3f);

        gameOverUI.SetActive(true);
        TMP_Text resultText = gameOverUI.GetComponentInChildren<TMP_Text>();
        if (resultText != null)
            resultText.text = winnerName;
    }
    private bool winnerDeclared = false;
    [PunRPC]
    public void ReportWinnerToMaster(string nickname)
    {
        if (winnerDeclared) return;
        winnerDeclared = true;
        photonView.RPC("FreezeAllPlayers", RpcTarget.All);
        photonView.RPC("EscapeSequence", RpcTarget.All, nickname);
    }
    [PunRPC]
    public void FreezeAllPlayers()
    {
        if (_rocketParent.childCount > 1)
        {
            _rocketParent.GetChild(0).gameObject.SetActive(false);
            _rocketParent.GetChild(1).gameObject.SetActive(true);
        } 
        Time.timeScale = 0f; // 전체 시간 정지
        var players = FindObjectsByType<PlayerCtrl>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.photonView.IsMine)
            {
                player.transform.GetChild(0).gameObject.SetActive(false); //UI 끄기
                player.transform.GetChild(3).gameObject.SetActive(false); //손 연동 끄기
                player.transform.GetChild(4).gameObject.SetActive(false);
                player.enabled = false; // 움직임도 멈추기
            }
        }
        Debug.Log("Time scaled to 0. All gameplay halted.");
    }
    [PunRPC]
    public void DisableCharacter(int viewID, int id)
    {
        Debug.Log($"DisableCharacter called on viewID={viewID}, charId={id}");
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            GameObject targetObj = targetView.gameObject;
            targetObj.SetActive(false);
        }
        _rocketCharacters[id].SetActive(true);
        _victoryImage.sprite = _victorySprites[id];

        double leaveTime = PhotonNetwork.Time + 7.0;
        photonView.RPC("ScheduleReturnToLobby", RpcTarget.All, leaveTime);
    }
    [PunRPC]
    public void ScheduleReturnToLobby(double leaveTime)
    {
        StartCoroutine(ClientWaitUntilLeave(leaveTime));
    }

    private IEnumerator ClientWaitUntilLeave(double leaveTime)
    {
        while (PhotonNetwork.Time < leaveTime)
            yield return null;

        Time.timeScale = 1f;

        if (!hasLeftRoom && PhotonNetwork.IsConnectedAndReady)
        {
            hasLeftRoom = true;
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom()
    {
        Time.timeScale = 1f;
        Hashtable resetProps = new Hashtable
    {
        { "charId", null },
        { "seatIndex", null },
        { "isReady", false }
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);

        PhotonNetwork.LoadLevel(MyString.SCENE_LOBBY);
    }
}