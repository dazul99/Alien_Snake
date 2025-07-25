using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private int turretRange = 3;
    private int turretDamage = 5;
    private int civilianRange = 2;
    private int civilianBuildingRange = 3;
    private int tankRange = 3;
    private int tankDamage = 1;
    private int truckRange = 3;

    private GameManager gameManager;

    private int typeOfNPC = 0;

    private bool stunned = false;

    private (int, int) position;

    private (int, int) snakePos;

    [SerializeField] private GameObject projectile;

    [SerializeField] private GameObject spawnUnit;

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
            return;
        }
        if (typeOfNPC == 8)
        {
            
            if (stunned)
            {
                stunned = !stunned;
                return;
            }
            if (SnakeIsClose(turretRange))
            {
                Shoot(turretDamage);
                stunned = true;
                return;
            }
        }

        if (typeOfNPC == 8)
        {
            if (SnakeIsClose(tankRange))
            {
                Shoot(tankDamage);
                return;
            }
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
            if (SnakeIsClose(civilianRange))
            {
                RunAway();
                stunned = true;
                return;
            }
            return;
        }

        if(typeOfNPC == 6)
        {
            if (stunned)
            {
                stunned = !stunned;
                return;
            }
            if (SnakeIsClose(civilianBuildingRange))
            {
                CreateCivilian();
                stunned = true;
                return;
            }
            return;
        }

        if (typeOfNPC == 7)
        {
            if (stunned)
            {
                stunned = !stunned;
                return;
            }
            if (SnakeIsClose(tankRange))
            {
                RunAwayRoads();
                stunned = true;
                return;
            }
            return;
        }

        if (typeOfNPC == 8) return;

        if (typeOfNPC == 9)
        {
            if (stunned)
            {
                stunned = !stunned;
                return;
            }
            if (SnakeIsClose(truckRange))
            {
                RunAwayRoads();
                stunned = true;
                return;
            }
            return;
        }
    }

    private void CreateCivilian()
    {
        (int, int) aux = position;
        if (snakePos.Item2 > position.Item2) aux.Item2--;
        else if (snakePos.Item2 < position.Item2) aux.Item2++;
        else if (snakePos.Item1 > position.Item1) aux.Item1--;
        else aux.Item1++;
        GameObject civ = Instantiate(spawnUnit);
        gameManager.AddedCivilian(civ, aux);
    }

    private void Shoot(int damage)
    {
        Vector3 dir = (gameManager.SnakeWorldPos() - transform.position).normalized;
        GameObject aux = Instantiate(projectile, transform.position, Quaternion.LookRotation(new Vector3(0,0,1), dir));
        aux.GetComponent<Projectile>().SetDamage(damage);
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

    private void RunAwayRoads()
    {
        GameManager.Direction dir = gameManager.GetRoads(new Vector2Int(position.Item1, position.Item2));
        gameManager.NPCMove(dir, typeOfNPC, position);
        if(dir == GameManager.Direction.Left) position.Item1--;
        else if(dir == GameManager.Direction.Right) position.Item1++;
        else if(dir == GameManager.Direction.Up) position.Item2--;
        else position.Item2++;
        transform.position = new Vector3(position.Item1 * 0.32f, position.Item2 * -0.32f, 0);
        return;
    }

    private bool SnakeIsClose(int x)
    {
        snakePos = gameManager.SnakePos();
        int dist = Mathf.Abs(snakePos.Item1 - position.Item1) + Mathf.Abs(snakePos.Item2 - position.Item2);
        if(dist<=x) return true;

        return false;
    }

    public void ChangeType(int type)
    {
        typeOfNPC = type;
        position.Item1 = (int)(transform.position.x / gameManager.lengthOfCell);
        position.Item2 = (int)(transform.position.y / -gameManager.lengthOfCell);
    }

}
