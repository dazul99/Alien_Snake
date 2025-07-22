using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private GameManager gameManager;

    private int typeOfNPC = 0;

    private bool stunned = false;

    private (int, int) position;

    private (int, int) snakePos;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    
    public void EndOfTurn()
    {
        if (gameManager.SnakePos() == position)
        {
            gameManager.NPCDestroyed(transform.gameObject);
            Destroy(gameObject);
        }
    }

    public void Turn()
    {
        if (typeOfNPC == 4) return;

        if (typeOfNPC == 5)
        {
            if (stunned)
            {
                stunned = !stunned;
                return;
            }
            if (SnakeIsClose())
            {
                RunAway();
                stunned = true;
            }
        }
    }

    private void RunAway()
    {
        if(snakePos.Item2 > position.Item2 && gameManager.SpaceFree(position.Item1, position.Item2-1))
        {
            gameManager.NPCMove(GameManager.Direction.Up, typeOfNPC, position);
            position.Item2--;
            transform.position = new Vector3(position.Item1 * 0.32f, position.Item2 * -0.32f, 0);
            return;
        }
        else if(snakePos.Item2 < position.Item2 && gameManager.SpaceFree(position.Item1, position.Item2 + 1))
        {
            gameManager.NPCMove(GameManager.Direction.Down, typeOfNPC, position);
            position.Item2++;
            transform.position = new Vector3(position.Item1 * 0.32f, position.Item2 * -0.32f, 0);
            return;
        }
        if (snakePos.Item1 > position.Item1 && gameManager.SpaceFree(position.Item1 - 1, position.Item2))
        {
            gameManager.NPCMove(GameManager.Direction.Left, typeOfNPC, position);
            position.Item1--;
            transform.position = new Vector3(position.Item1 * 0.32f, position.Item2 * -0.32f, 0);
            return;
        }
        else if (snakePos.Item1 < position.Item1 && gameManager.SpaceFree(position.Item1 + 1, position.Item2))
        {
            gameManager.NPCMove(GameManager.Direction.Right, typeOfNPC, position);
            position.Item1++;
            transform.position = new Vector3(position.Item1 * 0.32f, position.Item2 * -0.32f, 0);
            return;
        }
    }

    private bool SnakeIsClose()
    {
        snakePos = gameManager.SnakePos();
        int dist = Mathf.Abs(snakePos.Item1 - position.Item1) + Mathf.Abs(snakePos.Item2 - position.Item2);
        if(dist<=2 && typeOfNPC == 5) return true;

        return false;
    }

    public void ChangeType(int type)
    {
        typeOfNPC = type;
        position.Item1 = (int)(transform.position.x / gameManager.lengthOfCell);
        position.Item2 = (int)(transform.position.y / -gameManager.lengthOfCell);
    }

}
