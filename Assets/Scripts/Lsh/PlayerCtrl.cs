using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

public class PlayerCtrl : MonoBehaviourPun
{
    [SerializeField]
    private TrackedPoseDriver[] _handTrackings;
    [SerializeField]
    private Transform _viewCamTRs;      // XROrigin�� ������ ��ī�޶� HMD (Main Camera)
    [SerializeField]
    private Animator _anim;  //�÷��̾� ĳ������ �ִϸ�����
    [SerializeField]
    private Rigidbody _rb;

    public InputDevice _leftHand;
    public InputDevice _rightHand;

    [SerializeField]
    private float _moveSpeed = 5f;  //�̵� �ӵ�
    [SerializeField]
    private float _rotSpeed = 90f; // �ʴ� 90�� ȸ��

    [SerializeField]
    private float _jumpPower = 50f;
    [SerializeField]
    private bool _isGrounded = true;
    [SerializeField]
    public bool _isGroggyState = false;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private Transform _detectGroundTrs;

    private void Start()
    {
        if (photonView.IsMine == false)
        {
            foreach (var item in _handTrackings)
            {
                item.enabled = false;
            }
            _viewCamTRs.gameObject.SetActive(false);
            return;
        }

        InitializeLeftHand();
    }

    private void Update()
    {
        if (photonView.IsMine == false)
            return;

        if (!_leftHand.isValid)
        {
            InitializeLeftHand();
            return;
        }

        if (_leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input))
        {
            MovePlayer(input);
        }


        //�׷α���°� �ƴҶ� �����Ӱ���
        if(_isGroggyState == false)
        {
            //ȸ��
            if (!_leftHand.isValid || !_rightHand.isValid) { Start(); return; }

            // �޼� Ʈ����
            if (_leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftPressed) && leftPressed)
            {
                Rotate(-_rotSpeed * Time.deltaTime);
            }

            // ������ Ʈ����
            if (_rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightPressed) && rightPressed)
            {
                Rotate(+_rotSpeed * Time.deltaTime);
            }

            UpdateGrounded();

            // ���� ����ƽ Ŭ�� ����
            if (_leftHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool leftThumbstickClick) && leftThumbstickClick && _isGrounded)
            {
                _rb.AddForce(Vector3.up * _jumpPower);
            }
        }

        // XR ����̽����� ���� ������ ��ġ/ȸ�� ��������
        Vector3 headPosition = InputTracking.GetLocalPosition(XRNode.Head);
        Quaternion headRotation = InputTracking.GetLocalRotation(XRNode.Head);

        // ī�޶� ��ġ/ȸ�� ����
        //_viewCamTRs.localPosition = headPosition;
        _viewCamTRs.localRotation = headRotation;
    }

    private void InitializeLeftHand()
    {
        // �޼�, ������ ����̽� �ʱ�ȭ
        var leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftDevices);
        if (leftDevices.Count > 0) _leftHand = leftDevices[0];

        var rightDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightDevices);
        if (rightDevices.Count > 0) _rightHand = rightDevices[0];

        if (leftDevices.Count > 0)
        {
            _leftHand = leftDevices[0];
            Debug.Log("�޼� ��Ʈ�ѷ� �����: " + _leftHand.name);
        }
        else
        {
            Debug.LogWarning("�޼� ��Ʈ�ѷ��� ã�� �� �����ϴ�.");
        }

        if (rightDevices.Count > 0)
        {
            _rightHand = rightDevices[0];
            Debug.Log("������ ��Ʈ�ѷ� �����: " + _rightHand.name);
        }
        else
        {
            Debug.LogWarning("������ ��Ʈ�ѷ��� ã�� �� �����ϴ�.");
        }
    }

    private void UpdateGrounded()
    {
        var a = Physics.OverlapSphere(_detectGroundTrs.position, 0.1f, _groundMask);
        _isGrounded = (a.Length > 0) ? true : false;
    }

    private void MovePlayer(Vector2 input)
    {
        // HMD (Main Camera)�� ���� ���� �������� �̵� (���� ����)
        Vector3 forward = _viewCamTRs.forward;
        Vector3 right = _viewCamTRs.right;

        // ���� �̵��� �ݿ� (Y ����)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x;
        //_rb.MovePosition(transform.position + moveDirection * _moveSpeed * Time.deltaTime);
        _rb.position += moveDirection * _moveSpeed * Time.deltaTime;

        _anim.SetFloat("MoveX", moveDirection.x);
        _anim.SetFloat("MoveZ", moveDirection.z);
    }

    private void Rotate(float angle)
    {
        transform.Rotate(0, angle, 0);
    }

}
