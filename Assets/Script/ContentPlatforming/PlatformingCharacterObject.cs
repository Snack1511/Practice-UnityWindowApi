using System;
using UnityEngine;



public class PlatformingCharacterObject : MonoBehaviour
{
    [SerializeField] private MoveController moveController = null;
    [SerializeField] private SPUM_Prefabs spumPrefabs = null; 
    private PlatformingModel platformingModel;
    //private Rigidbody2D rigidbody2D;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        moveController.SetupComponent();
        moveController.SetIgnoreDamping(true);
        SetupPlatformingModel();

        if (spumPrefabs.allListsHaveItemsExist())
        {
            spumPrefabs.PopulateAnimationLists();
        }

        spumPrefabs.OverrideControllerInit();
        Vector3 curLocalScale = spumPrefabs.transform.localScale;
        curLocalScale.x = -1;
        spumPrefabs.transform.localScale = curLocalScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            test();
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

    private const string COLLISIONTAG_GROUND = "Ground";
    private const string COLLISIONTAG_OBSTACLE = "Obstacle";
    
    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag(COLLISIONTAG_GROUND))
        {
            platformingModel.ResetCurJumpCount();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(COLLISIONTAG_OBSTACLE))
        {
            Debug.Log("Collision Obstacle");
        }
    }

    private void SetupPlatformingModel()
    {
        platformingModel ??= GetComponent<PlatformingModel>();
        platformingModel.SetupModel();
    }

    private void test()
    {
        spumPrefabs.PlayAnimation(PlayerState.OTHER, 1);
    }
}
