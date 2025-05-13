using UnityEngine;

public class TestScript : MonoBehaviour
{
    public float debugSpeed = 3.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        //    MovePosition(Vector3.right, Time.deltaTime);
        //if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        //    MovePosition(Vector3.left, Time.deltaTime);
        //if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        //    MovePosition(Vector3.left, Time.deltaTime);
    }

    // rigid붙어서 이동 transform 조작하면 안됨
    public void MovePosition(Vector3 direction, float deltaTime) 
    {
        transform.Translate(direction * debugSpeed * deltaTime);
    }
}
