using Photon.Pun;
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
    }

    private void Rotate(float angle)
    {
        transform.Rotate(0, angle, 0);
    }


    [PunRPC]
    public void SpeedEffect(float speedOffset, float duration)
    {
        StartCoroutine(ApplySpeedRoutine(speedOffset, duration));
    }

    private IEnumerator ApplySpeedRoutine(float offset, float duration)
    {
        _moveSpeed += offset;
        yield return new WaitForSeconds(duration);
        _moveSpeed -= offset;
    }

    [PunRPC]
    public void PowerEffect(float speedOffset, float duration)
    {
        StartCoroutine(ApplyPowerRoutine(speedOffset, duration));
    }

    private IEnumerator ApplyPowerRoutine(float offset, float duration)
    {
        //_moveSpeed += offset;
        yield return new WaitForSeconds(duration);
        //_moveSpeed -= offset;
    }
    private int activeBuffCount = 0;
    private Vector3 originalScale;
    private Coroutine currentCoroutine;
    [PunRPC]
    public void ScaleEffect(float scaleOffset, float duration)
    {
        if (activeBuffCount == 0)
        {
            originalScale = transform.localScale;
            StartCoroutine(GrowToTarget(scaleOffset)); // 커지는 애니메이션 1번만 실행
        }

        activeBuffCount++;
        StartCoroutine(BuffTimer(duration)); // 지속 시간 관리만 따로 여러 개 가능
    }

    private IEnumerator GrowToTarget(float offset)
    {
        Vector3 targetScale = originalScale + new Vector3(offset, offset, offset);
        float halfDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private IEnumerator BuffTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        activeBuffCount--;

        if (activeBuffCount == 0)
        {
            StartCoroutine(ShrinkBack());
        }
    }

    private IEnumerator ShrinkBack()
    {
        float halfDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}