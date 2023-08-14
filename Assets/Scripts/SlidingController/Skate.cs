using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skate : MonoBehaviour
{
    CharacterController cc;
    Vector3 velocity;
    public LayerMask groundLayerMask;
    public float gravity=-9.81f;
    public float slopeLimit = 0.01f;
    public Transform groundCheckPosition;
    public float groundCheckDistance = 1f;

    private Vector3 hitPointNormal;
    public bool isGrounded;

    private bool isSliding
    {
        get
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopehit, 2f))
            {
                hitPointNormal = slopehit.normal;
                print("bam");
                return Vector3.Angle(hitPointNormal, Vector3.up) > slopeLimit;
                
            }
            else
            {
                return false;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cc=GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = false;
        velocity.y += gravity * Time.deltaTime;
        if (isSliding)
        {
            velocity += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z);
        }
        cc.Move(velocity*Time.deltaTime);
        isGrounded = Physics.CheckSphere(groundCheckPosition.position, groundCheckDistance, groundLayerMask);
    }
}
