using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerGroggy : MonoBehaviourPun
{
    [SerializeField]
    private PlayerCtrl _playerCtrl;
    [SerializeField]
    private Inventory _inventory;
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private Collider _col;
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private TrackedPoseDriver[] _trackPoseDrivers;
    [SerializeField]
    private ParticleSystem _groggyPtc;

    [SerializeField]
    private float _damageableRockVelocityValue;

    private void Awake()
    {
        _damageableRockVelocityValue = 5f;

        if (!photonView.IsMine)
            return;

        _playerCtrl = GetComponent<PlayerCtrl>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
/*        if (!photonView.IsMine)
            return;*/

        if (collision.collider.CompareTag(MyString.ROCK_TAG) && !_playerCtrl._isGroggyState)
        {
            Item_Rock rock = collision.collider.GetComponent<Item_Rock>();
            float rockVelocity = rock.GetVelocity();
            float rockMass = rock.GetComponent<Rigidbody>().mass;
            _col.enabled = false;
            _rb.isKinematic = true;
            Debug.Log("�浹");

            Debug.Log("RockVelocity: " + rockVelocity);
            
            //Temp
            _inventory.LoseItem();

            if (rockVelocity > _damageableRockVelocityValue)
            {
                // ��� Ŭ���̾�Ʈ�� groggy ���� ����
                photonView.RPC(nameof(RPC_StartGroggyState), RpcTarget.All);

                //_inventory.photonView.RPC(nameof(_inventory.LoseItem), RpcTarget.All);

                float groggyTime = 5f; // �Ǵ� rockVelocity * rockMass;
                StartCoroutine(CRT_StopGroggyState(groggyTime));
                Debug.Log("Velocity ��ȿ");
            }
        }
    }

    private IEnumerator CRT_StopGroggyState(float groggyTime)
    {
        yield return new WaitForSeconds(groggyTime);

        // ��� Ŭ���̾�Ʈ�� groggy ���� ����
        photonView.RPC(nameof(RPC_StopGroggyState), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartGroggyState()
    {
        _playerCtrl._isGroggyState = true;
        _groggyPtc.gameObject.SetActive(true);
        _anim.SetBool(MyString.GROGGY_ANIM, true);

        for (int i = 0; i < _trackPoseDrivers.Length; i++)
        {
            _trackPoseDrivers[i].enabled = false;
        }

        _playerCtrl.enabled = false;
        _inventory.photonView.RPC(nameof(Inventory.LoseItem), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StopGroggyState()
    {
        _groggyPtc.gameObject.SetActive(false);
        _anim.SetBool(MyString.GROGGY_ANIM, false);

        for (int i = 0; i < _trackPoseDrivers.Length; i++)
        {
            _trackPoseDrivers[i].enabled = true;
        }

        _playerCtrl.enabled = true;
        _playerCtrl._isGroggyState = false;
        _col.enabled = true;
        _rb.isKinematic = false;
    }
}
