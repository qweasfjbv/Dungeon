using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviorBase : MonoBehaviour
{
    private Rigidbody2D rigid;
    private Steering[] steerings;

    [SerializeField] public float maxVelocity = 0.1f;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        steerings = GetComponents<Steering>();
    }


    void Update()
    {
        Vector3 velocity = Vector3.zero;

        foreach(Steering steering in steerings)
        {
            var data = steering.GetSteering(this);
            velocity += data.velocity;
        }

        velocity = velocity.normalized * maxVelocity;

        rigid.MovePosition(velocity);

    }
}
