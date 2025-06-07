using System;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    [SerializeField] private bool useRigid = false;

    private Transform targetTransform = null;
    private Rigidbody2D targetRigidBody2D = null;

    private bool isIgnoreDamping = false;
    
    public void SetupComponent()
    {
        if (useRigid)
        {
            SetupRigidbody();
        }
        else
        {
            SetupTransform();
        }
    }

    public void SetIgnoreDamping(bool isIgnoreDamping)
    {
        this.isIgnoreDamping = isIgnoreDamping;
    }

    private void SetupRigidbody()
    {
        targetRigidBody2D ??= GetComponent<Rigidbody2D>();
        targetRigidBody2D.gravityScale = 2;
        Debug.Assert(targetRigidBody2D is not null, "not found RigidBody2D");
    }

    private void SetupTransform()
    {
        targetTransform ??= GetComponent<Transform>();
    }

    public void Move(Vector3 moveDir, float moveDist, float deltaTime)
    {
        if (useRigid)
        {
            Force(moveDir, moveDist);
        }
        else
        {
            Translate(moveDir, moveDist, deltaTime);
        }
    }
    
    private void Force(Vector3 forceDir, float force) 
    {
        if (isIgnoreDamping)
        {
            targetRigidBody2D.linearVelocity = Vector2.zero;
            targetRigidBody2D.angularVelocity = 0.0f;
        }

        targetRigidBody2D.AddForce(forceDir * force, ForceMode2D.Impulse);
    }
    
    private void Translate(Vector3 moveDir, float moveDist, float deltaTime) 
    {
        targetTransform.Translate(moveDir * moveDist * deltaTime, Space.World);
    }
}
