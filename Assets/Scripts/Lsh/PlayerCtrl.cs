using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

public class PlayerCtrl : MonoBehaviourPun
{
    [SerializeField]
    private TrackedPoseDriver[] _handTrackings;
    [SerializeField]
    private Transform _viewCamTRs;      // XROrigin에 부착된 뷰카메라 HMD (Main Camera)
    [SerializeField]
    private Animator _anim;  //플레이어 캐릭터의 애니메이터
    [SerializeField]
    private Rigidbody _rb;

    public InputDevice _leftHand;
    public InputDevice _rightHand;

    [SerializeField]
    private float _moveSpeed = 5f;  //이동 속도
    [SerializeField]
    private float _rotSpeed = 90f; // 초당 90도 회전
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
    [SerializeField]
    TMP_Text _timerText;

    [SerializeField]
    private PlayerGrab _playerGrab;

    [SerializeField]
    public TMP_Text _nicknameTxt;

    //Sound
    [SerializeField]
    private AudioSource _playerAudioSrc;
    [SerializeField]
    private AudioClip _footStepClip;
    private float _stepInterval = 0.2f;
    private float _stepTime;
    [SerializeField] AudioClip _powerClip;
    [SerializeField] AudioClip _speedClip;
    [SerializeField] AudioClip _scaleUpClip;
    [SerializeField] AudioClip _scaleDownClip;


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

        var playManager = FindAnyObjectByType<PlayManager>();
        if (_timerText != null && playManager != null)
        {
            PlayManager.Instance.RegisterTimerText(_timerText);
        }
        InitializeLeftHand();
        GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, GetComponent<PhotonView>().Owner.NickName);
    }
    [PunRPC]
    public void SetNickname(string nick)
    {
        _nicknameTxt.text = nick;
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


        //그로기상태가 아닐때 움직임가능
        if (_isGroggyState == false)
        {
            //회전
            if (!_leftHand.isValid || !_rightHand.isValid) { Start(); return; }

            // 왼손 트리거
            if (_leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftPressed) && leftPressed)
            {
                Rotate(-_rotSpeed * Time.deltaTime);
            }

            // 오른손 트리거
            if (_rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightPressed) && rightPressed)
            {
                Rotate(+_rotSpeed * Time.deltaTime);
            }

            UpdateGrounded();

            // 왼쪽 섬스틱 클릭 점프
            if (_leftHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool leftThumbstickClick) && leftThumbstickClick && _isGrounded)
            {
                _rb.AddForce(Vector3.up * _jumpPower);
            }
        }

        // XR 디바이스에서 현재 헤드셋의 위치/회전 가져오기
        Vector3 headPosition = InputTracking.GetLocalPosition(XRNode.Head);
        Quaternion headRotation = InputTracking.GetLocalRotation(XRNode.Head);

        // 카메라 위치/회전 갱신
        //_viewCamTRs.localPosition = headPosition;
        _viewCamTRs.localRotation = headRotation;
    }

    private void InitializeLeftHand()
    {
        // 왼손, 오른손 디바이스 초기화
        var leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftDevices);
        if (leftDevices.Count > 0) _leftHand = leftDevices[0];

        var rightDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightDevices);
        if (rightDevices.Count > 0) _rightHand = rightDevices[0];

        if (leftDevices.Count > 0)
        {
            _leftHand = leftDevices[0];
            Debug.Log("왼손 컨트롤러 연결됨: " + _leftHand.name);
        }
        else
        {
            Debug.LogWarning("왼손 컨트롤러를 찾을 수 없습니다.");
        }

        if (rightDevices.Count > 0)
        {
            _rightHand = rightDevices[0];
            Debug.Log("오른손 컨트롤러 연결됨: " + _rightHand.name);
        }
        else
        {
            Debug.LogWarning("오른손 컨트롤러를 찾을 수 없습니다.");
        }
    }

    private void UpdateGrounded()
    {
        var a = Physics.OverlapSphere(_detectGroundTrs.position, 0.1f, _groundMask);
        _isGrounded = (a.Length > 0) ? true : false;
    }

    private void MovePlayer(Vector2 input)
    {
        // HMD (Main Camera)의 전방 방향 기준으로 이동 (수평만 적용)
        Vector3 forward = _viewCamTRs.forward;
        Vector3 right = _viewCamTRs.right;

        // 수평 이동만 반영 (Y 제거)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x;
        //_rb.MovePosition(transform.position + moveDirection * _moveSpeed * Time.deltaTime);
        _rb.position += moveDirection * _moveSpeed * Time.deltaTime;

        _anim.SetFloat("MoveX", moveDirection.x);
        _anim.SetFloat("MoveZ", moveDirection.z);
        //Debug.Log($"X: {moveDirection.x}\nZ: {moveDirection.z}");

        if (_isGrounded)
        {
            _stepTime -= Time.deltaTime;
            if (_stepTime <= 0 && moveDirection.magnitude != 0)
            {
                _playerAudioSrc.PlayOneShot(_footStepClip);

                photonView.RPC(nameof(PlayFootStepSound_RPC), RpcTarget.Others, transform.position);

                _stepTime = _stepInterval;
            }
        }

    }

    [PunRPC]
    public void PlayFootStepSound_RPC(Vector3 pos)
    {
        if (_footStepClip != null)
        {
            AudioSource.PlayClipAtPoint(_footStepClip, pos);
        }
    }
    private void Rotate(float angle)
    {
        transform.Rotate(0, angle, 0);
    }


    [PunRPC]
    public void FreezePlayer()
    {
        if (!photonView.IsMine) return;

        _rb.isKinematic = true;
        _leftHand = new InputDevice();
        _rightHand = new InputDevice();
        _moveSpeed = 0f;
        _rotSpeed = 0f;
    }

    #region Speed

    private float _baseSpeed = 5f;
    private float _speedOffset = 0f;
    Coroutine _speedCo = null;

    [PunRPC]
    public void SpeedEffect(float offset, float duration)
    {
        _speedOffset += offset;
        _moveSpeed = _baseSpeed + _speedOffset;

        if (_speedCo != null) StopCoroutine(_speedCo);
        _speedCo = StartCoroutine(RemoveSpeedAfter(offset, duration));
    }

    private IEnumerator RemoveSpeedAfter(float offset, float duration)
    {
        _playerAudioSrc.PlayOneShot(_speedClip);
        yield return new WaitForSeconds(duration);
        _speedOffset -= offset;
        _moveSpeed = _baseSpeed + _speedOffset;
        _speedCo = null;
    }

    #endregion

    #region Power

    [PunRPC]
    public void PowerEffect(float speedOffset, float duration)
    {
        StartCoroutine(ApplyPowerRoutine(speedOffset, duration));
    }

    private IEnumerator ApplyPowerRoutine(float offset, float duration)
    {
        _playerAudioSrc.PlayOneShot(_powerClip);
        _playerGrab._throwPower += offset;
        yield return new WaitForSeconds(duration);
        _playerGrab._throwPower -= offset;
    }
    private Vector3 _baseScale = Vector3.one;
    private Vector3 _scaleOffset = Vector3.zero;
    Coroutine _scaleCo = null;

    #endregion

    #region Scale
    [PunRPC]
    public void ScaleEffect(float offset, float duration)
    {
        Vector3 delta = Vector3.one * offset;
        _scaleOffset += delta;
        transform.localScale = _baseScale + _scaleOffset;
        if (offset > 0) _playerAudioSrc.PlayOneShot(_scaleUpClip);
        else _playerAudioSrc.PlayOneShot(_scaleDownClip);
        if (_scaleCo != null) StopCoroutine(_scaleCo);
        _scaleCo = StartCoroutine(RemoveScaleAfter(delta, duration));
    }

    private IEnumerator RemoveScaleAfter(Vector3 delta, float duration)
    {
        yield return new WaitForSeconds(duration);
        _scaleOffset -= delta;
        transform.localScale = _baseScale + _scaleOffset;
        _scaleCo = null;
    }

    #endregion
}