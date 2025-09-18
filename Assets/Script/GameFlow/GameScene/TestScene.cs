using System;
using Cysharp.Threading.Tasks;
using Manager;
using Script.Define;
using Script.Manager.SingletonManager;
using UnityEngine;



namespace Script.GameFlow.GameScene
{
    public class TestScene : SceneBase
    {
        // SceneModel model = new();

        public class TestTableData : TableDataBase
        {
            public string name;
            public int level;
            public float attackRange;
            public int[] skillIDs;
            public string[] skillTags;
            
        }

        public override async UniTask OnLoadResourceAsync(IProgress<LoadingProgressResult> progress)
        {
            await LoadTableAsync();
            await LoadSpriteAsync();
            await LoadPrefabAsync();
            await LoadSoundAsync();
            
            progress?.Report(new LoadingProgressResult()
            {
                amount = 1.0f
            });
        }
        
        private async UniTask LoadTableAsync()
        {
            //필요한 테이블 로딩 잘 되네ㅎ
            string path = "Table/testTable.csv";
            await TableManager.Instance.LoadTableData<TestTableData>(path);

            if (TableManager.TryGetData<TestTableData>(1, out TestTableData data1))
            {
                int a = 0;
            }
            if (TableManager.TryGetData<TestTableData>(2, out TestTableData data2))
            {
                int a = 0;
            }
            if (TableManager.TryGetData<TestTableData>(3, out TestTableData data3))
            {
                int a = 0;
            }
        }

        private async UniTask LoadSpriteAsync()
        {
            //필요한 스프라이트 리소스 로딩
            string path = "";
            //Sprite spt1 = await ResourcesManager.Instance.LoadAsync<Sprite>(path);
            
            path = "";
            //Sprite spt2 = await ResourcesManager.Instance.LoadAsync<Sprite>(path);
        }
        private async UniTask LoadPrefabAsync()
        {
            //필요한 프리팹 로딩
            string path = "";
            //GameObject obj = await ResourcesManager.Instance.LoadAsync<GameObject>(path);
            
            path = "";
            //GameObject obj2 = await  ResourcesManager.Instance.LoadAsync<GameObject>(path);
        }
        private async UniTask LoadSoundAsync()
        {
            //필요한 사운드 로딩
            string path = "";
            //AudioSource audioSource = await ResourcesManager.Instance.LoadAsync<AudioSource>(path);
            
            path = "";
            //AudioSource audioSource2 = await ResourcesManager.Instance.LoadAsync<AudioSource>(path);
        }

        public override void OnLoadComplete()
        {
            base.OnLoadComplete();
        }
        public TestScene(ESceneType SceneType) : base(SceneType)
        {
            
        }
        public override void EnterScene(ISceneInfoContext context)
        {
            base.EnterScene(context);
        }

        public override void UpdateScene()
        {
            base.UpdateScene();
            
            //세이브 테스트용 더미 하나 만들고
            // 저장 해보고
            // 로드해보고
            // Model이랑 연결해보고
            // Notify테스트 해보면 끝날 듯?
            
            //콘텐츠 매니저에서 콘텐츠 활성화
            if (Input.GetKeyDown(KeyCode.A))
            {
                
            }
        }
        
        
    }
}