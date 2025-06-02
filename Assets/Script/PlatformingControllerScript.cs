using System;
using UnityEngine;



public class PlatformingControllerScript : MonoBehaviour
{
    private PlatformingModel platformingModel;
    private Rigidbody2D rigidbody2D;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetupRigidbody();
        SetupPlatformingModel();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (platformingModel.CanJump())
        {
            AddForce(Vector3.up, platformingModel.JumpForce);
            platformingModel.IncreaseCurJumpCount(-1);
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            platformingModel.ResetCurJumpCount();
        }
    }


    private void AddForce(Vector3 forceDir, float force) 
    {
        rigidbody2D.AddForce(forceDir * force, ForceMode2D.Impulse);
    }
    
    private void SetupRigidbody()
    {
        rigidbody2D ??= GetComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = 2;        
    }

    private void SetupPlatformingModel()
    {
        platformingModel ??= GetComponent<PlatformingModel>();
        platformingModel.SetupModel();
    }
}
