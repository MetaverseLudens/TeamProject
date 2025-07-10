using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ConsumeSpeed : ConsumeItemBase
{
    public float effectDuration = 5f;
    public float originSpeed = 1.5f;
    public float speedBuff = 0.5f;
    public float speedDebuff = -0.5f;

    public override void OnConsumed(GameObject player)
    {
        if (!photonView.IsMine) return;

        var controller = player.GetComponent<PlayerCtrl>();
        if (controller != null)
        {
            float effect = Random.value < 0.5f ? speedBuff : speedDebuff;
            controller.GetComponent<PhotonView>().RPC("SpeedEffect", RpcTarget.All, effect, effectDuration);
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
