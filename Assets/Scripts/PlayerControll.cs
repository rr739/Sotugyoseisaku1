using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerControll : NetworkBehaviour
{

    private Rigidbody2D _rigidbody2D;
    private Vector2 _velocity;
    private float _moveForce = 5f;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();


    }
    void Update()
    {

        if (this.IsOwner)
        {
            Vector2 velocity = new Vector2();
            if (Input.GetKey(KeyCode.W))
            {
                velocity.y = 1.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                velocity.y = -1.0f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                velocity.x = -1.0f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                velocity.x = 1.0f;
            }
            velocity.Normalize();
            SetMoveInputServerRpc(velocity);
        }

        if (this.IsServer)
        {
            _rigidbody2D.velocity = _velocity * _moveForce;
        }

        

    }

    [Unity.Netcode.ServerRpc]
    private void SetMoveInputServerRpc(Vector2 velocity)
    {
        _velocity = velocity;
    }
}
