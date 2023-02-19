using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class Spearhead : MonoBehaviour
{
    public float walkSpeed;
    public bool mustPatrol;
    public bool mustFlip;

    public Vector2 direction;

    [SerializeField, HideInInspector] private Rigidbody2D rb;
    private Animator _animator;
    private SpriteRenderer _renderer;
    public Transform groundCheckPos;
    public LayerMask groundLayer;
    public Collider2D spearCollider;

    private readonly Vector2 positionMoveTo = new Vector2(1, 0);
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();

        mustPatrol = true;
        direction = Vector2.left;
        
        _animator.SetFloat("speed", 0f);
    }

    private void Update()
    {
        if (mustPatrol)
            Patrol();
    }

    private void FixedUpdate()
    {
        _animator.SetFloat("speed", Mathf.Abs(direction.x));

        if (mustPatrol)
        {
            mustFlip = !Physics2D.OverlapCircle(groundCheckPos.position, 0.1f, groundLayer);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.IsTouching(spearCollider))
            mustFlip = true;
    }

    void Patrol()
    {
        if (mustFlip)
            StartCoroutine(Flip());
        
        rb.velocity = walkSpeed * Time.fixedDeltaTime * direction;
    }

    IEnumerator Flip()
    {
        mustPatrol = false;
        var oldDirection = direction;
        direction = Vector2.zero;
        _animator.SetBool("isTurning", true);

        yield return new WaitForSeconds(1.0f);

        _animator.SetBool("isTurning", false);
        mustFlip = false;
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);

        yield return new WaitForSeconds(1.5f);
        
        mustPatrol = true;
        direction = new Vector2(oldDirection.x * -1, oldDirection.y);

        yield return new WaitForSeconds(1.0f);
    }
}
