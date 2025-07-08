using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ConsumePower : ConsumeItemBase
{
    public float effectDuration = 5f;
    public float originPower = 1.5f;
    public float powerBuff = 3f;
    public float powerDebuff = 0.7f;

    public override void OnConsumed(GameObject player)
    {
        if (!photonView.IsMine) return;

        var controller = player.GetComponent<PlayerCtrl>();
        if (controller != null)
        {
            float effect = Random.value < 0.5f ? powerBuff : powerDebuff;
            controller.GetComponent<PhotonView>().RPC("PowerEffect", RpcTarget.All, effect, effectDuration);
        }

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
