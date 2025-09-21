using System;
using Script.Manager.SingletonManager;

namespace Script.Define
{
    namespace ModelDefine
    {
        //데이터가 필요하면, 얘를 상속받아 사용함
        // ModelManager를 통해 만들고 할당받음
        // ModelManager를 통해 실제 데이터에 적용한 뒤 Notify
        // ModelManager에 Notify 모듈 Model에 NotifyListener모듈 붙이면 되지 않을까
        // ModelManager는 단순 Manager클래스 -> Content나 Scene에 물려서 관리하면 되니까
        // 저장데이터와 런타임 데이터를 분리 --> SaveBaseData <-> ModelBaseData
        // IOManager에서 저장 -> SaveData.Notify -> ModelData.Listen
        public abstract class ModelBase
        {
        }

    }

    namespace SaveDefine
    {
        public static class SaveConstructor
        {
            public static SaveBase CreateSaveBase(ESaveType saveType)
            {
                SaveBase returnData = (saveType) switch
                {
                    ESaveType.Test=> new TestSaveData(),
                    _ => null,
                };
                return returnData;
            }
        }

        public abstract class SaveBase
        {
            

        }

        [Serializable]
        public class TestSaveData : SaveBase
        {
            public string test;
            public void SetTestString(string text)
            {
                test = text;
            }
        }
    }
}