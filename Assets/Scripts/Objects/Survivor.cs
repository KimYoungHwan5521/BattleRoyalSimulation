using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Survivor : CustomObject
{
    [SerializeField] float moveSpeed;
    [SerializeField] float detectionRange;
    Vector2 moveDirection = Vector2.up;
    Vector2 lookRotation = Vector2.up;
    Vector2 rememberOriginalMoveDirection = Vector2.up;

    private void Update()
    {
        AI();
    }

    private void FixedUpdate()
    {
        Move(moveDirection, Time.fixedDeltaTime);
        Look(lookRotation);
    }

    void AI()
    {
        RaycastHit2D[] hits;
        hits = Physics2D.RaycastAll(transform.position, moveDirection.normalized, detectionRange);
        Debug.DrawRay(transform.position, moveDirection.normalized * detectionRange, Color.yellow);
        foreach (RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.gameObject.tag;
            if (hitTag == "Ground")
            {
                if (Random.Range(0, 3) < 2)
                {
                    moveDirection = moveDirection.Rotate(90);
                }
                else
                {
                    moveDirection = moveDirection.Rotate(135);
                }
                lookRotation = moveDirection;
                rememberOriginalMoveDirection = moveDirection;
                return;
            }
            else if(hitTag == "Wall")
            {
                if(Vector2.Angle(moveDirection.Rotate(-5), hit.normal) < Vector2.Angle(moveDirection.Rotate(5), hit.normal))
                {
                    moveDirection = moveDirection.Rotate(-5);
                }
                else
                {
                    moveDirection = moveDirection.Rotate(5);
                }
                lookRotation = moveDirection;
                return;
            }
        }
        hits = Physics2D.RaycastAll(transform.position, rememberOriginalMoveDirection.normalized, detectionRange);
        foreach (RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.gameObject.tag;
            if (hitTag == "Wall")
            {
                return;
            }
        }
        moveDirection = rememberOriginalMoveDirection;
    }

    void Move(Vector2 preferDirection, float deltaTime)
    {
        preferDirection.Normalize();
        transform.position += deltaTime * moveSpeed * (Vector3)preferDirection;
    }

    void Look(Vector2 preferDirection)
    {
        float angle = Mathf.Atan2(preferDirection.y, preferDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
}
