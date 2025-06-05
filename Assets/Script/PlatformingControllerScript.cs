using System;
using UnityEngine;



public class PlatformingControllerScript : MonoBehaviour
{
    [SerializeField] private MoveController moveController = null;
    private PlatformingModel platformingModel;
    //private Rigidbody2D rigidbody2D;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        moveController.SetupComponent();
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
            moveController.Move(Vector3.up, platformingModel.JumpForce, Time.deltaTime);
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
    
    private void SetupPlatformingModel()
    {
        platformingModel ??= GetComponent<PlatformingModel>();
        platformingModel.SetupModel();
    }
}
