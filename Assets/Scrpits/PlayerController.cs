using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            gameManager.ChangeDirection(GameManager.Direction.Left);
        } 
        else if(Input.GetKeyDown(KeyCode.D))
        {
            gameManager.ChangeDirection(GameManager.Direction.Right);
        }
        else if ( Input.GetKeyDown(KeyCode.W))
        {
            gameManager.ChangeDirection(GameManager.Direction.Up);
        }
        else if ( Input.GetKeyDown(KeyCode.S))
        {
            gameManager.ChangeDirection(GameManager.Direction.Down);
        }

    }
}
