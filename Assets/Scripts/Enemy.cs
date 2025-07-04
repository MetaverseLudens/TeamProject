using Photon.Pun;
using System.Collections;
using UnityEngine;

public partial class Enemy : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _moveSpeed = 1f;

    private Rigidbody _rb;
    private Animator _anim;

    enum State
    {
        Nothing,
        Idle,
        Chase,
        Search,
        Damage,
        Die
    }

    [SerializeField]
    private State _state;
    [SerializeField]
    private State _nextState;
}

public partial class Enemy : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();

        if (photonView.IsMine)
        {
            ChangeState(State.Idle);
        }
    }

    private void Update()
    {
        if (_nextState != _state)
        {
            StopCoroutine($"Co{_state}");
            _state = _nextState;
            StartCoroutine($"Co{_state}");
        }
    }


    private void ChangeState(State state)
    {
        _nextState = state;
    }
}

public partial class Enemy : MonoBehaviourPunCallbacks
{
    [PunRPC]
    private void RPC_ChangeState(State state)
    {
        ChangeState(state);
    }
}

public partial class Enemy : MonoBehaviourPunCallbacks
{
    private IEnumerator CoIdle()
    {
        _anim.SetTrigger("Idle");
        yield return new WaitUntil(() => photonView.IsMine);

        _rb.linearVelocity = Vector3.zero;
        ChangeState(State.Search);
    }

    private IEnumerator CoSearch()
    {
        yield return new WaitUntil(() => photonView.IsMine);

        while(_target == null)
        {
            yield return null;
        }

        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, State.Chase);
    }
}

public partial class Enemy : MonoBehaviourPunCallbacks
{
    private IEnumerator CoChase()
    {
        _anim.SetTrigger("Move");
        yield return new WaitUntil(() => photonView.IsMine);

        while(_target != null)
        {
            var dir = _target.position - transform.position;
            dir.y = 0;

            var velocity = _moveSpeed * dir.normalized;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            transform.forward = dir.normalized;
            yield return null;
        }

        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, State.Idle);
    }
}

public partial class Enemy : MonoBehaviourPunCallbacks
{
    [PunRPC]
    private void RPC_SyncState(State state)
    {
        ChangeState(state);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SyncState), newPlayer, _state);
        }
    }
}