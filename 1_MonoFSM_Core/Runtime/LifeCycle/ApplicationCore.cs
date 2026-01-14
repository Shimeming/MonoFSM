using UnityEngine;

[DefaultExecutionOrder(10000)]
public class ApplicationCore : SingletonBehaviour<ApplicationCore>
{
    private void Start()
    {
        //FIXME: MonoPoolObj 會初始化了

        PoolManager.HandleGameLevelAwakeReverse(gameObject);
        PoolManager.HandleGameLevelAwake(gameObject);
        PoolManager.HandleGameLevelStartReverse(gameObject);
        PoolManager.HandleGameLevelStart(gameObject);
        PoolManager.ResetReload(gameObject);
    }
    //FIXME: 寫個一鍵複製？
}
