using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class GameManager : MonoBehaviour
{
    private int lifes = 3;

    private int mutationBarCount = 0;
    private int mutationBarMax = 1;

    private int snakeLength = 0;
    private int partsLost = 0;

    private bool berserk;

    [SerializeField] private float durationOfTurn = 1.5f;

    [SerializeField] private GameObject head;
    [SerializeField] private GameObject tail;
    private List<GameObject> body;
    private (int, int) position;
    private (int, int) cellForward;

    [SerializeField] BoardElem[] inputsBoardElems;
    [SerializeField] Vector2Int[] inputsPos;

    private List<GameObject> elemsOnMap;

    private BoardElem[,] board;

    private bool gameOver;

    public float lengthOfCell = 0.319f;

    [SerializeField] private Direction direction;
    private Direction oldDirection;
    private bool hasToGrow = false;
    private int numberToGrow = 0;
    private bool grownThisTurn = false;
    private bool lostLife = false;

    private int damageTaken;
    [SerializeField] private int maxDamage = 10;

    private bool stunned = false;

    public enum BoardElem
    {
        Nothing = 0,
        Head = 1,
        PartOfBody = 2,
        IndestructibleStructure = 3,
        Mine = 4,
        Civilian = 5,
        CivilianBuilding = 6,
        Tank = 7,
        Turret = 8,
        PlutoniumTruck = 9,
    }

    [SerializeField] private GameObject[] gameObjects;

    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
    }

    [SerializeField] private int[] roads;
    [SerializeField] private bool[] horizontalRoads;

    [SerializeField] private int chanceOfTankSpawn;
    [SerializeField] private int chanceOfTruckSpawn;
    private bool tankCD;
    private bool truckCD;
    private List<Vector2Int> spawnPositions;

    private void Start()
    {
        elemsOnMap = new List<GameObject>();
        body = new List<GameObject>();
        spawnPositions = new List<Vector2Int>();
        board = new BoardElem[20,15];
        position.Item1 = (int) (head.transform.position.x / lengthOfCell);
        position.Item2 = (int)(head.transform.position.y / -lengthOfCell);
        board[position.Item1, position.Item2] = BoardElem.Head;
        oldDirection = direction;
        GameObject aux;
        for (int i = 0; i < inputsBoardElems.Length; i++)
        {
            if (inputsBoardElems[i] != BoardElem.Tank || inputsBoardElems[i] != BoardElem.PlutoniumTruck)
            {
                board[inputsPos[i].x, inputsPos[i].y] = inputsBoardElems[i];
                aux = Instantiate(gameObjects[(int)inputsBoardElems[i]]);
                aux.transform.position = new Vector3(inputsPos[i].x * lengthOfCell, inputsPos[i].y * -lengthOfCell, 0);
                elemsOnMap.Add(aux);
                aux.GetComponent<NPC>().ChangeType((int)inputsBoardElems[i]);
            }
        }
        
        for(int i = 0; i < roads.Length; i++)
        {
            if (horizontalRoads[i])
            {
                spawnPositions.Add(new Vector2Int(0, roads[i]));
                spawnPositions.Add(new Vector2Int(19, roads[i]));
            }
            else
            {
                spawnPositions.Add(new Vector2Int(roads[i], 0));
                spawnPositions.Add(new Vector2Int(roads[i], 14));
            }
        }

        StartCoroutine("Turn");
        
    }

    public void AddedCivilian(GameObject go, (int, int) pos)
    {
        go.transform.position = new Vector3(pos.Item1 * lengthOfCell,pos.Item2 * -lengthOfCell, 0);
        elemsOnMap.Add(go);
        go.GetComponent<NPC>().ChangeType(5);
    }

    public (int,int) SnakePos()
    {
        return position;
    }

    //REMOVE LATER
    public void Grow()
    {
        hasToGrow = true;
        numberToGrow++;
    }

    private IEnumerator Turn()
    {
        while (!gameOver)
        {
            for(int i = 0; i < elemsOnMap.Count; i++)
            {
                elemsOnMap[i].GetComponent<NPC>().Turn();
            }
            if(!stunned) Moveforward();
            if(gameOver)
            {

            }
            oldDirection = direction;
            if (hasToGrow)
            {
                IncreaseSize();
                numberToGrow--;
                if(numberToGrow == 0) hasToGrow = false;
            }
            grownThisTurn = false;
            lostLife = false;
            for (int i = 0; i < elemsOnMap.Count; i++)
            {
                elemsOnMap[i].GetComponent<NPC>().EndOfTurn();
            }
            CheckSpawn();
            yield return new WaitForSeconds(durationOfTurn);
        }
    }

    private void CheckSpawn()
    {
        if (!truckCD)
        {
            int rand = Random.Range(0, 101);
            if(chanceOfTruckSpawn > rand)
            {
                SpawnTruck();
            }
        }

        if (!tankCD)
        {
            int rand = Random.Range(0, 101);
            if (chanceOfTankSpawn > rand)
            {
                SpawnTank();
            }
        }
    }

    private void SpawnTruck()
    {
        List<Vector2Int> aux = new List<Vector2Int>();
        for (int i = 0; i < spawnPositions.Count; i++) if (board[spawnPositions[i].x, spawnPositions[i].y] == BoardElem.Nothing) aux.Add(spawnPositions[i]);
        int x = Random.Range(0, spawnPositions.Count);
        board[aux[x].x, aux[x].y] = BoardElem.PlutoniumTruck;

        GameObject truck = Instantiate(gameObjects[9]);
        truck.transform.position = new Vector3(aux[x].x * lengthOfCell, aux[x].y * -lengthOfCell, 0);
        elemsOnMap.Add(truck);
        truck.GetComponent<NPC>().ChangeType(9);
        StartCoroutine("TruckCooldown");
    }

    public Direction GetRoads(Vector2Int pos)
    {
        for (int i = 0;i < roads.Length; i++)
        {
            if (horizontalRoads[i] && roads[i] == pos.y)
            {
                if (position.Item1 > pos.x) return Direction.Left;
                else if (position.Item1 < pos.x) return Direction.Right;
                else
                {
                    if(Random.Range(0,2) == 0) return Direction.Left;
                    else return Direction.Right;
                }
            }
            else if (!horizontalRoads[i] && roads[i] == pos.x)
            {
                if (position.Item2 > pos.y) return Direction.Up;
                else if (position.Item1 < pos.x) return Direction.Down;
                else
                {
                    if (Random.Range(0, 2) == 0) return Direction.Up;
                    else return Direction.Down;
                }
            }
        }

        return Direction.Left;
    }

    private IEnumerator TruckCooldown()
    {
        truckCD = true;
        yield return new WaitForSeconds(2f);
        truckCD = false;
    }

    private void SpawnTank()
    {
        List<Vector2Int> aux = new List<Vector2Int>();
        for (int i = 0; i < spawnPositions.Count; i++) if (board[spawnPositions[i].x, spawnPositions[i].y] == BoardElem.Nothing) aux.Add(spawnPositions[i]);
        int x = Random.Range(0, spawnPositions.Count);
        board[aux[x].x, aux[x].y] = BoardElem.Tank;

        GameObject tank = Instantiate(gameObjects[7]);
        tank.transform.position = new Vector3(aux[x].x * lengthOfCell, aux[x].y * -lengthOfCell, 0);
        elemsOnMap.Add(tank);
        tank.GetComponent<NPC>().ChangeType(7);
        StartCoroutine("TankCooldown");
    }

    private IEnumerator TankCooldown()
    {
        tankCD = true;
        yield return new WaitForSeconds(2f);
        tankCD = false;
    }

    public void ChangeDirection(Direction dir)
    {
        if (Mathf.Abs(oldDirection - dir) == 2) return;
        direction = dir;
    }

    public void NPCDestroyed(GameObject x)
    {
        elemsOnMap.Remove(x);
    }

    private void Moveforward()
    {
        CheckForward();

        if (lostLife) return;

        if (gameOver) return;
        head.transform.position = new Vector3(cellForward.Item1 * 0.32f, cellForward.Item2 * -0.32f, 0);
        board[cellForward.Item1, cellForward.Item2] = BoardElem.Head;
        board[position.Item1,position.Item2] = BoardElem.Nothing;
        (int, int) pointer;
        for (int i = 0; i<body.Count; i++)
        {
            pointer.Item1 = (int)(body[i].transform.position.x / lengthOfCell);
            pointer.Item2 = (int)(body[i].transform.position.y / -lengthOfCell);
            body[i].transform.position = new Vector3(position.Item1 * 0.32f, position.Item2 * -0.32f, 0);
            board[position.Item1,position.Item2] = BoardElem.PartOfBody;
            board[pointer.Item1, pointer.Item2] = BoardElem.Nothing;
            position = pointer;
        }
        position = cellForward;
    }

    public bool SpaceFree( int x, int y)
    {
        if (x < 0 || y < 0 || x > 19 || y > 14) return false;
        if (board[x, y] == BoardElem.Nothing) return true;
        return false;
    }

    public void NPCMove(Direction dir, int type, (int,int) posNPC)
    {
        (int, int) pointer = posNPC;
        if (dir == Direction.Up) pointer.Item2--;
        else if (dir == Direction.Down) pointer.Item2++;
        else if (dir == Direction.Left) pointer.Item1--;
        else pointer.Item1++;

        board[pointer.Item1, pointer.Item2] = (BoardElem)type;
        board[posNPC.Item1, posNPC.Item2] = BoardElem.Nothing;
    }

    private void CheckForward()
    {
        cellForward = position;
        if (direction == Direction.Left) cellForward.Item1--;
        else if (direction == Direction.Right) cellForward.Item1++;
        else if (direction == Direction.Up) cellForward.Item2--;
        else cellForward.Item2++;

        if (cellForward.Item1 < 0 || cellForward.Item2 < 0 || cellForward.Item1 > 19 || cellForward.Item2 > 14)
        {
            
            LoseLife(true);
            return;
        }
        BoardElem aux = board[cellForward.Item1,cellForward.Item2];

        if (aux == BoardElem.Nothing) return;
        if ((aux == BoardElem.PartOfBody && !berserk)  || aux == BoardElem.IndestructibleStructure) LoseLife(true);
        else if (aux == BoardElem.Mine) MineCheck();
        else if (aux == BoardElem.Civilian || aux == BoardElem.Tank || aux == BoardElem.Turret) Consumed(aux);
        else if (aux == BoardElem.CivilianBuilding) EatingBuilding();
        else if (aux == BoardElem.PlutoniumTruck) BerserkMode();
    }

    private void LoseLife(bool x)
    {
        lifes--;
        lostLife = x;
        if(lifes <= 0)
        {
            GameOver();
        }
        Debug.Log("DAMAGE");
    }

    private void GameOver()
    {
        gameOver = true;
    }

    private void Consumed(BoardElem x)
    {
        Debug.Log("nom");
        board[cellForward.Item1, cellForward.Item2] = BoardElem.Nothing;
        //Checks mutation bar
        if (mutationBarCount < mutationBarMax)
        {
            if (x == BoardElem.Nothing) return;
            if (x == BoardElem.Civilian) mutationBarCount++;
            else if (x == BoardElem.Tank) mutationBarCount += 10;
            else mutationBarCount += 15;
            if(mutationBarCount > mutationBarMax) mutationBarCount = mutationBarMax;
        }

        if (mutationBarCount < mutationBarMax) return;

        //if mutation bar filled and is in berserk mode
        else if (berserk)
        {
            hasToGrow = true;
            numberToGrow += partsLost;
            return;
        }


        IncreaseSize();
        
    }

    private void IncreaseSize()
    {
        // checks if it has grown already(should never go here, but just in case)
        if (grownThisTurn)
            {
                hasToGrow = true;
                numberToGrow++;
                return;
            }
            snakeLength++;

        body.Add(Instantiate(tail));

        grownThisTurn = true;

        if(!hasToGrow) mutationBarCount = 0;
        else
        {
            numberToGrow--;
            if(numberToGrow <= 0) hasToGrow = false;
        }
    }

    private void MineCheck()
    {
        if (berserk)
        {
            IncreaseSize();
            berserk = false;
        }
        else if (snakeLength == 0) LoseLife(false);
        else DecreaseSize(snakeLength);
    }

    private void DecreaseSize(int x)
    {
        if(snakeLength == 0)
        {
            LoseLife(false);
            return;
        }
        if(snakeLength < x) x = snakeLength;
        (int, int) pointer;
        GameObject aux;
        int number = body.Count;
        for (int i = body.Count - 1; i > (number - x - 1); i--)
        { 
            pointer.Item1 = (int)(body[i].transform.position.x / lengthOfCell);
            pointer.Item2 = (int)(body[i].transform.position.y / -lengthOfCell);
            aux = body[i];
            body.Remove(aux);
            Destroy(aux);
            board[pointer.Item1, pointer.Item2] = BoardElem.Nothing;
        }
    }

    private void EatingBuilding()
    {
        StartCoroutine("EatingBuildingCO");
    }

    private IEnumerator EatingBuildingCO()
    {
        stunned = true;
        if(!berserk) yield return new WaitForSeconds(1.4f);
        else yield return new WaitForSeconds(0.5f);
        stunned = false;
        mutationBarCount += 30;
        if (mutationBarCount >= mutationBarMax) IncreaseSize();
    }

    private void BerserkMode()
    {
        if (berserk) StopCoroutine("Berserk");
        StartCoroutine("Berserk");
    }

    private IEnumerator Berserk()
    {
        berserk = true;
        yield return new WaitForSeconds(10);
        berserk = false;
    }

    public Vector3 SnakeWorldPos()
    {
        return head.transform.position;
    }

    public void DealDamage(int x)
    {
        damageTaken += x;

        if (damageTaken >= maxDamage)
        {
            DecreaseSize(1);
            damageTaken = 0;
        }
    }

}
