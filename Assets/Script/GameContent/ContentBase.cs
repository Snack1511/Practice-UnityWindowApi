namespace Script.GameContent
{
    public class ContentInitInfo
    {
        
    }

    public abstract class ContentBase
    {
        /// <summary>
        /// 데이터 초기화 및 컨텐츠 초기 설정시 확장 필요
        /// </summary>
        public virtual void InitalizeContent(ContentInitInfo  info)
        {
            
        }
        
        /// <summary>
        /// 데이터 정리 및 컨텐츠 종료시 확장 필요
        /// </summary>
        public virtual void ReleaseContent()
        {
            
        }

        /// <summary>
        /// 컨텐츠 UI할당시 확장 필요
        /// </summary>
        public virtual void SetupContentUI()
        {
        }
    }
}