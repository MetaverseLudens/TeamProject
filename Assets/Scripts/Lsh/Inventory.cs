using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

public class Inventory : MonoBehaviourPun, IPunObservable
{
    [SerializeField]
    private PlayerCtrl _playerCtrl;

    [SerializeField]
    private GameObject _canvasObj, _coreItemSlotsPanel;

    [SerializeField]
    private CoreItem[] _coreItemObjs;

    [SerializeField]
    private Image[] _coreItemImgs;

    [SerializeField]
    private Sprite[] _deactivatedImgs;

    [SerializeField]
    private Sprite[] _activatedImgs;

    [SerializeField]
    private GameObject _alienObj;

    [SerializeField]
    private bool[] _haveCoreItemBools;


    private float _coreItemSlotsPanelCtrDelayTime = 0f;

    private void Start()
    {
        _haveCoreItemBools = new bool[3];
        _coreItemObjs = new CoreItem[3];
        _coreItemObjs[0] = GameObject.FindGameObjectWithTag(MyString.CORE_0_TAG).GetComponent<CoreItem>();
        _coreItemObjs[1] = GameObject.FindGameObjectWithTag(MyString.CORE_1_TAG).GetComponent<CoreItem>();
        _coreItemObjs[2] = GameObject.FindGameObjectWithTag(MyString.CORE_2_TAG).GetComponent<CoreItem>();

        StartCoroutine(nameof(CRT_AlienActivation));

        if (photonView.IsMine == false)
        {
            _canvasObj.SetActive(false);
            return;
        }
    }


    private void Update()
    {
        if (photonView.IsMine == false || _playerCtrl._leftHand.isValid == false)
            return;

        OnOffCoreSlots();
    }


    private IEnumerator CRT_AlienActivation()
    {
        WaitForSeconds waitTime = new WaitForSeconds(0.1f);
        while (true)
        {
            _alienObj.SetActive(_haveCoreItemBools[2]);
            yield return waitTime;
        }
    }


    //�ھ� ������ ����UI Ȱ��ȭ ��Ʈ�ѷ�
    private void OnOffCoreSlots()
    {
        _coreItemSlotsPanelCtrDelayTime = (_coreItemSlotsPanelCtrDelayTime > 0f) ? _coreItemSlotsPanelCtrDelayTime -= Time.deltaTime : 0f;

        if (_playerCtrl._leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool leftXbtnClick) && leftXbtnClick)
        {
            if (_coreItemSlotsPanelCtrDelayTime <= 0f)
            {
                _coreItemSlotsPanelCtrDelayTime = 0.5f;
                _coreItemSlotsPanel.SetActive(!_coreItemSlotsPanel.activeSelf);
            }
        }
    }

    #region GetCoreItemMethods
    [PunRPC]
    public void HaveOrNoneCoreItem(eCORE_ITEM_TYPE coreItemType)
    {
        int coreBoolIdx = (int)coreItemType;
        //ȹ�� bool�� trueó��
        _haveCoreItemBools[coreBoolIdx] = true;
        //UI�� ȹ��ǥ��
        _coreItemImgs[coreBoolIdx].sprite = _activatedImgs[coreBoolIdx];
    }
    #endregion


    #region LoseCoreItemMethods


    [PunRPC]
    public void LoseItem()
    {
        for (int i = 0; i < _haveCoreItemBools.Length; i++)
        {
            if (_haveCoreItemBools[i] == true)
            {
                _haveCoreItemBools[i] = false;
                _coreItemImgs[i].sprite = _deactivatedImgs[i];
                _coreItemObjs[i].photonView.RPC(nameof(CoreItem.DropItem), RpcTarget.All, transform.position);
                Debug.Log("LoseItem");
            }
        }
    }

    #endregion

/*    //�ٽɾ����� �� ������ ǥ���ϴ� UI SetActiveó��
    private IEnumerator CRT_CheckGatherAllCoreItems()
    {
        while (true)
        {
            bool isActiveAllGatherUI = false;
            for (int i = 0; i < _haveCoreItemBools.Length; i++)
            {
                if (_haveCoreItemBools[i] == true)
                {
                    isActiveAllGatherUI = true;
                }
            }
            yield return null;
        }
    }*/


    //ȹ�� ���� ���� ������Ʈ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            for (int i = 0; i < _haveCoreItemBools.Length; i++)
            {
                stream.SendNext(_haveCoreItemBools[i]);
            }
        }
        else
        {
            for (int i = 0; i < _haveCoreItemBools.Length; i++)
            {
                _haveCoreItemBools[i] = (bool)stream.ReceiveNext();
            }
        }
    }

}

public enum eCORE_ITEM_TYPE
{
    Core_0,
    Core_1,
    Core_2
}

public enum eCORE_ITEM_HAVE_OR_NONE
{
    Have,
    None
}
