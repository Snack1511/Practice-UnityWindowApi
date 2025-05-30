using UnityEngine;

public class TestScript : MonoBehaviour
{
    public float debugSpeed = 1;

    Rigidbody2D rigidbody2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidbody2D ??= GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        //    MovePosition(Vector3.right, Time.deltaTime);
        //if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        //    MovePosition(Vector3.left, Time.deltaTime);
        //if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        //    MovePosition(Vector3.up, Time.deltaTime);        
        
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            AddForce(Vector3.right);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            AddForce(Vector3.left);
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            AddForce(Vector3.up);
    }

    // rigid붙어서 이동 transform 조작하면 안됨
    public void MovePosition(Vector3 direction, float deltaTime) 
    {
        transform.Translate(direction * debugSpeed * deltaTime);
    }

    public void AddForce(Vector3 forceDir) {
        rigidbody2D.AddForce(forceDir * debugSpeed, ForceMode2D.Impulse);
    }
}
