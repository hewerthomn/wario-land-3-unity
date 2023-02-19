using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
  private Rigidbody2D rb;
  private float speed = 10.0f;
  private bool fall = false;

  public Vector2 direction;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  void FixedUpdate()
  {
    if (!fall)
    {
      StartCoroutine(Fall());
    }
  }

  private IEnumerator Fall()
  {
    transform.rotation = Quaternion.Euler(0, 0, -50);
    rb.AddForce(speed * Vector2.up, ForceMode2D.Force);
    yield return new WaitForSeconds(0.5f);

    fall = true;
    rb.gravityScale = 4.0f;

    yield return new WaitForSeconds(3.0f);

    Destroy(gameObject);
  }
}
