using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
  [Range(1, 20)] public float speed = 10;
  public float attackSpeed = 10;
  [Range(1, 20)] public float jumpVelocity;
  public float fallMultiplier = 2.5f;
  public float lowJumpMultiplier = 2f;
  [Range(1, 60)] public float idleTimeSetting = 3;
  public State state;
  [SerializeField, HideInInspector] public MovementState movState;

  private Rigidbody2D rb;
  private Animator _animator;
  private SpriteRenderer _renderer;
  private GameObject _initialPos;
  public Collider2D attackCollider;
  public GameObject rockPrefab;

  [SerializeField] private bool _isGrounded;
  [SerializeField] private bool _attack;
  [SerializeField] private bool _restartPlayer;
  [SerializeField] private float _lastIdleTime;

  [SerializeField] private float _startDashTime = 3f;
  [SerializeField] private float _currentDashTime;

  private void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();
    _renderer = GetComponent<SpriteRenderer>();

    _initialPos = GameObject.Find("InitialPos");
    state = State.Idle;
    attackCollider.enabled = false;
  }

  private void Update()
  {
    if (rb.velocity.y < 0)
      rb.velocity += (fallMultiplier - 1) * Time.deltaTime * Physics2D.gravity.y * Vector2.up;
    else if (rb.velocity.y > 0 && state != State.Jump)
      rb.velocity += (lowJumpMultiplier - 1) * Time.deltaTime * Physics2D.gravity.y * Vector2.up;
  }

  private void FixedUpdate()
  {
    if (Keyboard.current.anyKey.isPressed)
      _lastIdleTime = Time.time;

    _animator.SetBool(MovementState.IsSleeping.ToString(), IdleCheck());

    Restart();
  }

  private void Awake()
  {
    _lastIdleTime = Time.time;
  }

  private void OnValidate()
  {
    if (rb == null) rb = GetComponent<Rigidbody2D>();
  }

  private void OnCollisionEnter2D(Collision2D other)
  {
    _isGrounded = other.gameObject.layer == 8;
    if (_isGrounded)
    {
      state = State.Idle;
      _animator.SetBool(MovementState.IsJumping.ToString(), false);
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Lava"))
      _restartPlayer = true;

    if (other.CompareTag("CrackedBlock"))
      StartCoroutine(DestroyBlock(other));
  }

  private bool IdleCheck()
  {
    return Time.time - _lastIdleTime > idleTimeSetting;
  }

  public void OnMovePlayer(InputAction.CallbackContext context)
  {
    if (context.performed && !_attack)
    {
      state = State.Move;

      Vector2 direction = context.ReadValue<Vector2>();

      _animator.SetFloat(MovementState.Speed.ToString(), Mathf.Abs(direction.x * speed));
      rb.velocity += direction * speed;

      if (direction.x > 0) _renderer.flipX = false;
      if (direction.x < 0) _renderer.flipX = true;
    }

    if (context.canceled)
    {
      state = State.Idle;
      rb.velocity = Vector2.zero;
      _animator.SetFloat(MovementState.Speed.ToString(), 0f);
    }
  }

  public void OnJump(InputAction.CallbackContext context)
  {
    if (_isGrounded && context.performed)
    {
      _isGrounded = false;
      state = State.Jump;
      _animator.SetBool(MovementState.IsJumping.ToString(), true);

      rb.velocity += Vector2.up * jumpVelocity;
      rb.gravityScale = 1f;
    }

    if (context.canceled)
    {
      rb.gravityScale = 3f;
    }
  }

  public void OnAttack(InputAction.CallbackContext context)
  {
    if (context.performed)
      StartCoroutine(Attack());
  }

  private IEnumerator Attack()
  {
    _attack = true;
    state = State.Attack;
    attackCollider.enabled = true;

    _currentDashTime = _startDashTime;
    var direction = _renderer.flipX ? Vector2.left : Vector2.right;

    _animator.SetBool(MovementState.IsAttacking.ToString(), true);
    _animator.SetFloat(MovementState.Speed.ToString(), Mathf.Abs(direction.x * attackSpeed));

    yield return new WaitForSeconds(.5f);

    while (_currentDashTime > 0f)
    {
      _currentDashTime -= Time.deltaTime;
      rb.velocity = direction * attackSpeed;

      yield return null;
    }

    _animator.SetFloat(MovementState.Speed.ToString(), 0f);
    _animator.SetBool(MovementState.IsAttacking.ToString(), false);

    rb.velocity = Vector2.zero;
    _attack = false;
    state = State.Idle;
    attackCollider.enabled = false;

    yield return null;
  }

  private IEnumerator DestroyBlock(Collider2D other)
  {
    var direction = rb.velocity.x > 0 ? Vector2.left : Vector2.right;
    float bounce = 9.0f;
    print("direction" + direction.ToString());
    rb.AddForce(direction * bounce, ForceMode2D.Force);

    yield return new WaitForSeconds(.3f);

    Vector3 spawnRock = new Vector2(other.transform.position.x, other.transform.position.y);
    Vector3 spawnRock2 = new Vector2(other.transform.position.x - 1.0f, other.transform.position.y);

    Instantiate(rockPrefab, spawnRock, other.transform.rotation);
    GameObject rock2 = Instantiate(rockPrefab, spawnRock2, other.transform.rotation);
    rock2.GetComponent<SpriteRenderer>().flipX = true;

    Destroy(other.gameObject);

    yield return null;
  }

  private void Restart()
  {
    if (!_restartPlayer) return;

    rb.transform.position = new Vector3(
        _initialPos.transform.position.x,
        _initialPos.transform.position.y,
        rb.transform.position.z
    );
    _restartPlayer = false;
  }
}

public enum State
{
  Idle, Move, Jump, Attack
}

public enum MovementState
{
  Speed, IsAttacking, IsJumping, IsSleeping
}