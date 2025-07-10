using Photon.Pun;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class PlayerGrab : MonoBehaviourPun
{
    [SerializeField]
    private PlayerCtrl _playerCtrl;

    [SerializeField]
    private Transform _rayPoint;

    //[HideInInspector]
    public GameObject _curGrabbedItem;

    [SerializeField]
    private Vector3 _throwStartVec;

    private bool _isGrabbing = false;

    [SerializeField]
    private float _rayLength = 3f;
    [SerializeField]
    private float _throwPower = 300f;

    [SerializeField]
    private LineRenderer _lineRend;

    private void Start()
    {
        if (photonView.IsMine == false)
            return;

        //던지기 스윙 시작점 업데이트
        StartCoroutine(nameof(CRT_UpdateThrowStartPoint));
    }

    private void Update()
    {
        if (photonView.IsMine == false)
            return;

        GrabItem();
    }

    private void GrabItem()
    {
        if (_playerCtrl._rightHand.TryGetFeatureValue(CommonUsages.gripButton, out _isGrabbing))
        {
            if (_isGrabbing)
            {
                if (_curGrabbedItem != null)
                    return;

                _lineRend.gameObject.SetActive(true);
                Grab();
                //photonView.RPC(nameof(GrabOthers), RpcTarget.Others);
            }
            //그랩 해제
            else
            {
                _lineRend.gameObject.SetActive(false);
                if (_curGrabbedItem == null)
                    return;

                Throwing();
            }
        }
    }

    private void Grab()
    {
        Ray ray = new Ray(_rayPoint.position, _rayPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _rayLength))
        {
            if (hit.collider == null)
                return;

            if (hit.collider.TryGetComponent(out IGrabable grabbedItem))
            {
                var itemView = hit.collider.GetComponent<PhotonView>();
                itemView.TransferOwnership(PhotonNetwork.LocalPlayer);

                _curGrabbedItem = hit.collider.gameObject;

                // RPC로 쥐기 요청 (grab point의 PhotonView ID 전달)
                itemView.RPC(nameof(Item_Rock.GetItem), RpcTarget.All, _rayPoint.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    private void GrabOthers()
    {
        Grab();
    }

    //나중에 입장한 플레이어가 하면 이상함
    private void Throwing()
    {
        PhotonView itemView = _curGrabbedItem.GetComponent<PhotonView>();

        // 상태 정리
        Rigidbody rb = _curGrabbedItem.GetComponent<Rigidbody>();
        Collider col = rb.GetComponent<Collider>();

        rb.isKinematic = false;
        rb.transform.SetParent(null);
        StartCoroutine(CRT_DelayThrowItemColEnable(col));

        Vector3 dir = (_curGrabbedItem.transform.position - _throwStartVec).normalized;
        float dist = Vector3.Distance(_curGrabbedItem.transform.position, _throwStartVec);
        float lastThrowPower = dist * _throwPower;

        // 직접 적용
        rb.AddForce(dir * lastThrowPower, ForceMode.Impulse);

        // 🔁 다른 클라이언트에 던지기 명령
        itemView.RPC(nameof(Item_Rock.RPC_ApplyThrowForce), RpcTarget.Others, dir, lastThrowPower);

        _curGrabbedItem = null;
        _throwStartVec = Vector3.zero;
    }


    private IEnumerator CRT_DelayThrowItemColEnable(Collider col)
    {
        col.enabled = false;
        yield return new WaitForSeconds(0.15f);
        col.enabled = true;
    }

    //waitTime간격으로 현재 쥐고 있는 아이템의 위치 저장(waitTime초만큼 딜레이)
    private IEnumerator CRT_UpdateThrowStartPoint()
    {
        WaitForSeconds waitTime = new WaitForSeconds(0.3f);
        while (true)
        {
            if (_curGrabbedItem != null)
            {
                _throwStartVec = _curGrabbedItem.transform.position;
            }
            yield return waitTime;
        }
    }

    private void OnDrawGizmos()
    {
        if (photonView.IsMine == false)
            return;

        Gizmos.color = Color.red;
        var a = _rayPoint.position + _rayPoint.forward * _rayLength;
        Gizmos.DrawLine(_rayPoint.position, a);
    }
}