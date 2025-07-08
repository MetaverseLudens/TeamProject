using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

public class Inventory : MonoBehaviourPun
{
    [SerializeField]
    private PlayerCtrl _playerCtrl;

    [SerializeField]
    private GameObject _canvasObj, _coreItemSlotsPanel;

    [SerializeField]
    private Image[] _dontHaveCoreItemImgs;

    [SerializeField]
    private bool _haveCoreItem_0, _haveCoreItem_1, _haveCoreItem_2;

    [SerializeField]
    private float _coreItemSlotsPanelCtrDelayTime = 0f;

    private void Start()
    {
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



    //코어 아이템 슬롯 보기
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

    //코어아이템을 얻었을때 실행되는 함수
    [PunRPC]
    public void HaveOrNoneCoreItem(eCORE_ITEM_TYPE coreItemType)
    {
        switch (coreItemType)
        {
            case eCORE_ITEM_TYPE.Core_0:
                _haveCoreItem_0 = true;
                break;
            case eCORE_ITEM_TYPE.Core_1:
                _haveCoreItem_1 = true;
                break;
            case eCORE_ITEM_TYPE.Core_2:
                _haveCoreItem_2 = true;
                break;
            default:
                break;
        }
        _dontHaveCoreItemImgs[0].gameObject.SetActive(!_haveCoreItem_0);
        _dontHaveCoreItemImgs[1].gameObject.SetActive(!_haveCoreItem_1);
        _dontHaveCoreItemImgs[2].gameObject.SetActive(!_haveCoreItem_2);
    }

    [PunRPC]
    public void LoseAllCoreItem()
    {
        _haveCoreItem_0 = false;
        _haveCoreItem_1 = false;
        _haveCoreItem_2 = false;
        for (int i = 0; i < _dontHaveCoreItemImgs.Length; i++)
        {
            _dontHaveCoreItemImgs[i].gameObject.SetActive(true);
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
