using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayManager : MonoBehaviourPunCallbacks
{
    public static PlayManager Instance;

    public Transform[] spawnPoints;
    public float gameDuration = 180f;
    public GameObject cinematicCamera;
    public GameObject gameOverUI;
    public GameObject explosionEffect;

    private TMP_Text _myTimerText;
    private float startTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitGameStart());
    }

    IEnumerator InitGameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = (float)PhotonNetwork.Time;  // 이게 더 정확함
            Hashtable hash = new Hashtable { { "startTime", startTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }

        // 모든 유저가 startTime 셋팅 완료될 때까지 기다리기
        while (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("startTime"))
            yield return null;

        startTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["startTime"];

        // 이제 동기화된 startTime으로 타이머 시작
        StartCoroutine(StartTimer());
        StartCoroutine(WaitAndSpawn());
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

        EndGameSequence();
    }

    void EndGameSequence()
    {
        explosionEffect.SetActive(true);
        explosionEffect.GetComponent<ParticleSystem>().Play();
        gameOverUI.SetActive(true);
    }

    public void OnClickReturnToLobby()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnClickRestartRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(MyString.SCENE_ROOM);
    }

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

    public void EndGame()
    {
        StartCoroutine(ReturnToLobbyAfterDelay());
    }

    IEnumerator ReturnToLobbyAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LeaveRoom();
    }
}