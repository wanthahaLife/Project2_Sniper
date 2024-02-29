using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyWaypoint : MonoBehaviour
{
    public enum MovementType
    {
        Stand = 0,
        Loop,
        Patrol
    }

    public MovementType movementType = MovementType.Stand;
    public EnemyType enemyType;
    public int spawnCount = 1;
    public float spawnInterval = 1.0f;
    public float startSpawnTime = 0.0f;

    int waypointCount = 1;
    int[] index;
    Vector3[] spawnOffset;
    bool isForwardPatrol = true;
    Transform[] waypoints;
    public Transform[] currentWaypoint;

    /*Vector3[] spawnPositions;
    int usedSpawnIndex;
    int spawnPositionRange;
    int SpawnPositionRange
    {
        get => spawnPositionRange;
        set
        {
            if (spawnPositionRange != value)
            {
                spawnPositionRange = value;
                int spawnPositionSize = 1;
                for(int i = 0; i < spawnPositionRange; i++)
                {
                    spawnPositionRange += (i + 1) * 8;
                }
                spawnPositions = new Vector3[spawnPositionSize];
                if (spawnPositionRange == 1)
                {
                    spawnPositions[0] = new Vector3(0, 0, 0);
                }
                else
                {
                    int currX = 0;
                    int currY = 0;
                    for (int i = 0; i < spawnPositionSize; i++)
                    {
                        for (int x = -spawnPositionRange; x < spawnPositionRange + 1; x++)
                        {
                            for (int y = -spawnPositionRange; y < spawnPositionRange + 1; y++)
                            {
                                if (y > currY)
                                {
                                    spawnPositions[i] = 
                                }
                            }
                        }
                    }
                }
            }
        }
    }*/

    WaitForSeconds waitTime_Spawn;
    WaitForSeconds waitTime_SpawnStart;

    public int SpawnCount
    {
        get => spawnCount;
        set
        {
            if(spawnCount != value) {
                spawnCount = value;
                currentWaypoint = new Transform[spawnCount];
                for (int i = 0; i < spawnCount; i++)
                {
                    currentWaypoint[i] = waypoints[0];
                }
            }
        }
    }

    private void Awake()
    {
        waypointCount = transform.childCount;
        waypoints = new Transform[waypointCount];
        for(int i = 0; i < waypointCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
        currentWaypoint = new Transform[spawnCount];
        index = new int[spawnCount];
        spawnOffset = new Vector3[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            currentWaypoint[i] = waypoints[0];
            index[i] = 0;
            spawnOffset[i] = Vector3.zero;
        }
        waitTime_Spawn = new WaitForSeconds(spawnInterval);
        waitTime_SpawnStart = new WaitForSeconds(startSpawnTime);

    }

    private void Start()
    {
        StartCoroutine(Spawn());
    }

    public Vector3 GetWaypoint(int enemyIndex)
    {
        switch (movementType)
        {
            case MovementType.Stand:
                // 그냥 서있기
                break;
            case MovementType.Loop:
                NextPositionInLoop(enemyIndex);
                break;
            case MovementType.Patrol:
                NextPositionInPatrol(enemyIndex);
                break;
        }
        currentWaypoint[enemyIndex] = waypoints[index[enemyIndex]];
        return currentWaypoint[enemyIndex].position;
    }

    void NextPositionInLoop(int enemyIndex)
    {
        index[enemyIndex]++;
        index[enemyIndex] %= waypointCount;
    }

    void NextPositionInPatrol(int enemyIndex)
    {
        if (index[enemyIndex] > waypointCount-2)
        {
            isForwardPatrol = false;
        }
        else if (index[enemyIndex] < 1)
        {
            isForwardPatrol = true;
        }

        if (isForwardPatrol)
        {
            index[enemyIndex]++;
        }
        else
        {
            index[enemyIndex]--;
        }
    }

    IEnumerator Spawn()
    {
        yield return waitTime_SpawnStart;
        int count = 0;
        EnemyBase enemy;
        while (count < spawnCount || spawnCount == -1)
        {
            GameObject obj = EnemyFactory.Instance.GetObject(enemyType, waypoints[index[count]].position);
            enemy = obj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.SetWaypoint(transform.GetComponent<EnemyWaypoint>());
                SetSpawnPosition(enemy, count, waypoints[index[count]].position);
                enemy.EnemyIndex = count;
            }
            count++;
            yield return waitTime_Spawn;
        }
    }

    void SetSpawnPosition(EnemyBase enemy, int enemyIndex, Vector3 position)
    {
        int tryCount = spawnCount*30;
        spawnOffset[enemyIndex] = Vector3.zero;
        int line = 0;
        for(int i = 0; i < tryCount; i++)
        {
            Vector3 spawnPosition = position + spawnOffset[enemyIndex];
            Ray ray = new Ray(spawnOffset[enemyIndex], Vector3.up);
            if (!Physics.Raycast(ray, enemy.GetHeight()))
            {
                enemy.SetPosition(spawnPosition);
                return ;
            }
            spawnOffset[enemyIndex] += (enemy.GetWidth()*2) * Vector3.forward;
            line++;
        }
        Debug.LogWarning("몬스터를 생성할 공간이 없습니다.");
        enemy.gameObject.SetActive(false);
    }

}
