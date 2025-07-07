using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    public Rigidbody myRigidbody;
    public float Right;
    public float Left;
    public float moveSpeed;
    public float MouseSensitivity;
    public float deadZone = 15;
    public bool isGrounded = false;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            transform.eulerAngles += MouseSensitivity * new Vector3(x: -Input.GetAxis("Mouse Y"), y: Input.GetAxis("Mouse X"), z: 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 100, ForceMode.Impulse);
        }

        if (transform.position.y > deadZone)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKey(KeyCode.W))
            myRigidbody.linearVelocity = Vector3.forward * 10;
        if (Input.GetKey(KeyCode.Q))
            myRigidbody.linearVelocity = Vector3.left * 7;
        if (Input.GetKey(KeyCode.E))
            myRigidbody.linearVelocity = Vector3.right * 7;
        if (Input.GetKey(KeyCode.S))
            myRigidbody.linearVelocity = Vector3.back * 9;
        if (Input.GetKey(KeyCode.D))
            transform.Rotate(transform.up, Input.GetAxis("Horizontal") * Right);
        if (Input.GetKey(KeyCode.A))
            transform.Rotate(transform.up, -Input.GetAxis("Horizontal") * Left);
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}