using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum EnemyType
{
    EnemyNormal = 0,
    Watcher,
    Tentacle,
    _EnemyBase,
}

public class EnemyFactory : Singleton<EnemyFactory>
{
    EnemyNormalPool enemyNormalPool;
    EnemyWatcherPool watcherPool;
    TentaclePool tentaclePool;
    EnemyBasePool enemyBasePool;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        enemyNormalPool = GetComponentInChildren<EnemyNormalPool>();
        if (enemyNormalPool != null) enemyNormalPool.Initialize();
        watcherPool = GetComponentInChildren<EnemyWatcherPool>();
        if (watcherPool != null) watcherPool.Initialize();
        tentaclePool = GetComponentInChildren<TentaclePool>();
        if (tentaclePool != null) tentaclePool.Initialize();
        enemyBasePool = GetComponentInChildren<EnemyBasePool>();
        if (enemyBasePool != null) enemyBasePool.Initialize();
    }
 
    /// <summary>
    /// 풀에 있는 게임 오브젝트 하나 가져오기
    /// </summary>
    /// <param name="type">가져올 오브젝트의 종류</param>
    /// <param name="position">오브젝트가 배치될 위치</param>
    /// <param name="angle">오브젝트의 초기 각도</param>
    /// <returns>활성화된 오브젝트</returns>
    public GameObject GetObject(EnemyType type, Vector3? position = null, Vector3? euler = null)
    {
        GameObject result = null;
        switch (type)
        {
            case EnemyType.EnemyNormal:
                result = enemyNormalPool.GetObject(position, euler).gameObject;
                break;
            case EnemyType.Watcher:
                result = watcherPool.GetObject(position, euler).gameObject;
                break;
            case EnemyType.Tentacle:
                result = tentaclePool.GetObject(position, euler).gameObject;
                break;
            case EnemyType._EnemyBase:
                result = enemyBasePool.GetObject(position, euler).gameObject;
                break;
        }

        return result;
    }
    /// <summary>
    /// 보통 적 유닛을 가져오는 함수
    /// </summary>
    /// <param name="position">배치될 위치</param>
    /// <returns>활성화된 보통 적 유닛</returns>
    public EnemyNormal GetEnemyNormal(Vector3 position, float angle = 0.0f)
    {
        return enemyNormalPool.GetObject(position, angle * Vector3.forward);
    }
    /// <summary>
    /// 보통 적 유닛을 가져오는 함수
    /// </summary>
    /// <param name="position">배치될 위치</param>
    /// <returns>활성화된 보통 적 유닛</returns>
    public EnemyWatcher EnemyWatcherNormal(Vector3 position, float angle = 0.0f)
    {
        return watcherPool.GetObject(position, angle * Vector3.forward);
    }
    /// <summary>
    /// 보통 적 유닛을 가져오는 함수
    /// </summary>
    /// <param name="position">배치될 위치</param>
    /// <returns>활성화된 보통 적 유닛</returns>
    public Tentacle GetTentacle(Vector3 position, float angle = 0.0f)
    {
        return tentaclePool.GetObject(position, angle * Vector3.forward);
    }

}