using Photon.Pun;
using UnityEngine;

public class CoreItem : MonoBehaviourPun
{
    [SerializeField]
    private GameObject _modelObj;
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private Collider _col;
    [SerializeField]
    private eCORE_ITEM_TYPE _coreItemType;

    [SerializeField]
    private float _dropMoveSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _dropMoveSpeed = 10f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(MyString.PLAYER_TAG) || photonView.IsMine == false)
            return;

        PhotonView targetView = collision.collider.GetComponent<PhotonView>();
        PlayerCtrl player = collision.collider.GetComponent<PlayerCtrl>();
        if (targetView == null || player._isGroggyState)
            return;

        _col.enabled = false;
        // viewID를 넘겨서 다른 클라이언트에서도 해당 Player를 찾도록
        photonView.RPC(nameof(SetCoreSlot), RpcTarget.All, targetView.ViewID);
        photonView.RPC(nameof(Off), RpcTarget.All);
    }

    [PunRPC]
    private void Off() { _modelObj.SetActive(false); }

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

    [PunRPC]
    public void DropItem(Vector3 dropPos)
    {
        transform.position = dropPos + Vector3.up * 2;
        _modelObj.SetActive(true);

        _rb.isKinematic = false;
        _col.enabled = true;
        _rb.angularVelocity = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
        var dir = Random.onUnitSphere;
        dir.y = Mathf.Abs(dir.y);
        _rb.AddForce(dir * _dropMoveSpeed, ForceMode.Impulse);
        Debug.Log("DropItem");
    }

}
