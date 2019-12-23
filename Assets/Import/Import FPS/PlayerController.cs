using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody rb;

    float speed = 4.0f;
    float angularSpeed = 2.0f;
    float jumpSpeed = 4.0f;

    float rotationCamera = 0.0f;

    CapsuleCollider collider;

    bool grounded = false;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Shoot()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f));
        if(Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);
            Transform t = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            t.position = hit.point;
            t.localScale *= 0.1f;
        }
    }

    void Crouch()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            collider.height = 1.2f;
            if(grounded)
                transform.position += Vector3.down * 0.4f;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            collider.height = 2.0f;
            if (grounded)
                transform.position += Vector3.up * 0.4f;
        }
    }
	
	// Update is called once per frame
	void Update () {
        grounded = IsGrounded();

        if(Input.GetMouseButton(0))
        {
            Shoot();
        }

        Crouch();

        float verticalVelocity = rb.velocity.y;

        Vector3 velocity = transform.forward * Input.GetAxisRaw("Vertical");
        velocity += transform.right * Input.GetAxisRaw("Horizontal");

        velocity.Normalize();

        velocity *= speed;

        if(Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            velocity *= 0.3f;
        }

        velocity.y = verticalVelocity;
        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = jumpSpeed;
        }

        rb.velocity = velocity;
        

        rb.angularVelocity = Vector3.up * Input.GetAxis("Mouse X") * angularSpeed;

        rotationCamera += Input.GetAxis("Mouse Y") * angularSpeed;

        rotationCamera = Mathf.Clamp(rotationCamera, -80, 80);

        Camera.main.transform.localEulerAngles = Vector3.left * rotationCamera;
    }

    bool IsGrounded()
    {

        foreach(Collider c in Physics.OverlapSphere(transform.position - (transform.up * collider.height / 2.0f), 0.3f))
        {
            if(c.CompareTag("Ground"))
            {
                return true;
            }
        }
        return false;
    }
}
