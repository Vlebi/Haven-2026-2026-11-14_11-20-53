using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Rigidbody2D rb;
    float horizontal;
    float vertical;
    public float speed;
    Vector2 movement;
    void Start()
    {
        Debug.Log("Movement script successfully started!");
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }
    void FixedUpdate()
    {
        movement = new Vector2(horizontal, vertical).normalized;
        rb.linearVelocity = movement * speed;
    }
}