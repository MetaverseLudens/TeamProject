using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayManager : MonoBehaviour
{

    public Transform[] spawnPoints; // �ν����Ϳ��� 1P~4P ��ġ ������� ����

    void Start()
    {
        StartCoroutine(nameof(WaitAndSpawn));
        Invoke(nameof(EndGame), 4f);
    }
    IEnumerator WaitAndSpawn()
    {
        int waitFrame = 0;
        while (true)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("charId", out var cid) &&
                PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("seatIndex", out var sid) &&
                cid != null && sid != null)
            {
                Debug.Log($" Ready to Spawn. charId={cid}, seatIndex={sid}, after {waitFrame} frames");
                break;
            }

            if (waitFrame % 60 == 0)
                Debug.LogWarning($" Still waiting for charId / seatIndex... frame={waitFrame}");

            waitFrame++;
            yield return null;
        }
        SpawnMyPlayer();
    }

    void SpawnMyPlayer()
    {
        Debug.Log("Spawn my player");
        if (!PhotonNetwork.InRoom) return;

        ExitGames.Client.Photon.Hashtable props = PhotonNetwork.LocalPlayer.CustomProperties;

        if (!props.ContainsKey("charId") || !props.ContainsKey("seatIndex"))
        {
            Debug.LogError("charId �Ǵ� seatIndex�� ����");
            return;
        }

        int charId = (int)props["charId"];
        int seatIndex = (int)props["seatIndex"];

        Debug.Log("charId : " + charId);
        Debug.Log("seatindex : " + seatIndex);
        if (seatIndex >= spawnPoints.Length)
        {
            Debug.LogError("��ȿ���� ���� seatIndex");
            return;
        }

        string prefabName = $"Player_{charId}";
        PhotonNetwork.Instantiate(prefabName, spawnPoints[seatIndex].position, spawnPoints[seatIndex].rotation);
    }
    public void EndGame() //���� ������ ����
    {
        // ��� ȭ�� �� �����ְ�
        StartCoroutine(ReturnToLobbyAfterDelay());
    }

    IEnumerator ReturnToLobbyAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LeaveRoom(); // �ڵ����� PUNManager���� Lobby�� �̵�
    }
}
