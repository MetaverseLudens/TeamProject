using UnityEngine;
using Photon.Pun;

public class RocketTrigger : MonoBehaviour
{
    [SerializeField] GameObject rocketUI;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            //if (PlayManager.Instance.IsCanEscape)
            rocketUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            rocketUI.SetActive(false);
        }
    }
    public void OnClickTryEscape()
    {
        // 1. 모든 플레이어 오브젝트 탐색
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();

            // 2. 로컬 플레이어인 경우만 실행
            if (pv != null && pv.IsMine)
            {
                Debug.Log("탈출 시도: " + pv.Owner.NickName);
                //PlayManager.Instance.TryEscape(pv.Owner); //탈출 및 성공
                return;
            }
        }

        Debug.LogWarning("로컬 플레이어를 찾을 수 없음");
    }
}
