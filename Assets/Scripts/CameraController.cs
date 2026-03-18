using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    private Rigidbody playerRB;
    public Vector3 offset;
    public float speed;
    void Start()
    {
        playerRB = player.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        Vector3 playerForward = (playerRB.linearVelocity + player.transform.forward).normalized;
        transform.position=Vector3.Lerp(transform.position, 
        player.position + player.transform.TransformVector(offset) + playerForward * (-10f),
         speed*Time.deltaTime);
        transform.LookAt(player);
    }
}
