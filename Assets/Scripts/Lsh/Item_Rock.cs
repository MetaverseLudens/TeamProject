using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Item_Rock : MonoBehaviourPun, IGrabable
{
    private Collider _col;
    private Rigidbody _rb;

    [SerializeField]
    private float _peakVelocity, _peakVelocityUpdateIntervalTime;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();

        float ran = Random.Range(2.0f, 4.0f);
        _rb.mass = ran;
        transform.localScale = new Vector3(ran, ran, ran);
    }

    private void Start()
    {
        StartCoroutine(nameof(CRT_UpdatePeakVelocity));
    }

    [PunRPC]
    public void GetItem(int grabPointViewID)
    {
        Transform grabPoint = PhotonView.Find(grabPointViewID).transform;

        // 부모 설정은 모든 클라이언트에서 실행
        transform.SetParent(grabPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;


        _rb.isKinematic = true;
        _col.enabled = false;
    }




    [PunRPC]
    public void RPC_ApplyThrowForce(Vector3 dir, float power)
    {
        transform.SetParent(null); // 모든 클라이언트에 적용

        if (!photonView.IsMine)
            return;

        StartCoroutine(ApplyForceAfterFixedUpdate(dir, power));
    }

    private IEnumerator ApplyForceAfterFixedUpdate(Vector3 dir, float power)
    {
        yield return new WaitForFixedUpdate(); // 💡 안정성 보장

        _col.enabled = false;
        transform.SetParent(null);
        _rb.isKinematic = false;

        yield return null; // 💡 isKinematic 반영 후

        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(dir * power, ForceMode.Impulse);

        Debug.Log("Force applied by RPC: " + dir * power);

        StartCoroutine(CRT_ReEnableCollider());
    }


    [ContextMenu(nameof(ChangeOwner))]
    public void ChangeOwner()
    {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    private IEnumerator CRT_UpdatePeakVelocity()
    {
        WaitForSeconds resetIntervalTime = new WaitForSeconds(_peakVelocityUpdateIntervalTime);
        while (true)
        {
            float curVelocity = _rb.linearVelocity.magnitude;
            //Debug.Log("CurVelocity: " + curVelocity);
            if (curVelocity > _peakVelocity)
            {
                _peakVelocity = curVelocity;
            }
            yield return resetIntervalTime;
            _peakVelocity = 0f;
        }
    }

    private IEnumerator CRT_ReEnableCollider()
    {
        yield return new WaitForSeconds(0.5f);
        _col.enabled = true;
    }

    
    public float GetVelocity()
    {
        return _peakVelocity;
    }
}