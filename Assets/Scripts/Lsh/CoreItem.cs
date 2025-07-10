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

        // viewID�� �Ѱܼ� �ٸ� Ŭ���̾�Ʈ������ �ش� Player�� ã����
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
