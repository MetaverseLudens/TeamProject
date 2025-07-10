using Photon.Pun;
using UnityEngine;

public class CoreItem : MonoBehaviourPun
{
    [SerializeField]
    private eCORE_ITEM_TYPE _coreItemType;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(MyString.PLAYER_TAG) || photonView.IsMine == false)
            return;

        PhotonView targetView = other.GetComponent<PhotonView>();
        if (targetView == null)
            return;

        // viewID를 넘겨서 다른 클라이언트에서도 해당 Player를 찾도록
        photonView.RPC(nameof(SetCoreSlot), RpcTarget.All, targetView.ViewID);
        photonView.RPC(nameof(Off), RpcTarget.All);
    }

    [PunRPC]
    private void Off() { gameObject.SetActive(false); }

    [PunRPC]
    private void SetCoreSlot(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            Inventory inven = targetView.GetComponent<Inventory>();
            if (inven != null)
            {
                inven.HaveOrNoneCoreItem(_coreItemType);
            }
        }
    }
}
