using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ContentPlatform : MonoBehaviour
{
    [SerializeField] private float StartWaitingTime = 3.0f;

    [SerializeField] private ObjectSpawner characterSpawner;
    [SerializeField] private ObjectSpawner obstacleSpawner;
    
    private CancellationTokenSource contentStartTaskCTS = null;
    private void Awake()
    {
        StartTaskStartContent();
    }
    
    private void OnDestroy()
    {
        CancelTask();
    }
    
    private void StartTaskStartContent()
    {
        contentStartTaskCTS = new CancellationTokenSource();
        StartContentAsync(contentStartTaskCTS.Token).Forget();
    }

    private void CancelTask()
    {
        contentStartTaskCTS.Cancel();
        contentStartTaskCTS.Dispose();
        contentStartTaskCTS = null;
    }

    async private UniTask StartContentAsync(CancellationToken token)
    {
        float targetTimer = StartWaitingTime;

        //UI생성
        
        //캐릭터 스폰
        await characterSpawner.StartSpawnAsync(token);
        
        //timer대기
        await UniTask.Delay(TimeSpan.FromSeconds(targetTimer), cancellationToken: token);
        
        //장애물 생성
        await obstacleSpawner.StartSpawnAsync(token);
    }
}
