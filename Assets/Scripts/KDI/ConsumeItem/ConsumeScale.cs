using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ConsumeScale : ConsumeItemBase
{
    public float effectDuration = 5f;
    public float originScale = 1.5f;
    public float scaleBuff = 0.3f;
    public float scaleDebuff = -0.3f;

    public override void OnConsumed(GameObject player)
    {
        if (!photonView.IsMine) return;

        var controller = player.GetComponent<PlayerCtrl>();
        if (controller != null)
        {
            float effect = Random.value < 0.5f ? scaleBuff : scaleDebuff;
            controller.GetComponent<PhotonView>().RPC("ScaleEffect", RpcTarget.All, effect, effectDuration);
        }
        Debug.Log("내가 아이템 먹음");
        ConsumeItemSpawner.Instance.DecreaseItemCount();

        PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.CompareTag("Player"))
        {
            OnConsumed(other.gameObject);
        }
    }
}
