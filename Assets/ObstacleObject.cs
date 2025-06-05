using System;
using UnityEngine;

public class ObstacleObject : MonoBehaviour
{
    [SerializeField] private MoveController moveController = null;

    public float debugmovedist = 3;
    private void Awake()
    {
        moveController.SetupComponent();
    }

    private void Update()
    {
        moveController.Move(Vector3.left, debugmovedist, Time.deltaTime);
    }
}
