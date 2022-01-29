using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 nextMove;

    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMove(InputValue movementValue)
	{
        Vector2 newMovement = movementValue.Get<Vector2>();
        GetComponent<Animator>().SetFloat("velX", newMovement.x);
        GetComponent<Animator>().SetFloat("velY", newMovement.y);
	}
}
