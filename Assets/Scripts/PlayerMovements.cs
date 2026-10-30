using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovements : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        movement = Vector2.zero;
        if (kb.wKey.isPressed) movement.y += 1;
        if (kb.sKey.isPressed) movement.y -= 1;
        if (kb.dKey.isPressed) movement.x += 1;
        if (kb.aKey.isPressed) movement.x -= 1;

        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                anim.SetFloat("MoveX", Mathf.Sign(movement.x));
            else
                anim.SetFloat("MoveX", 0);

            if (Mathf.Abs(movement.y) >= Mathf.Abs(movement.x))
                anim.SetFloat("MoveY", Mathf.Sign(movement.y));
            else
                anim.SetFloat("MoveY", 0);
        }

        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}