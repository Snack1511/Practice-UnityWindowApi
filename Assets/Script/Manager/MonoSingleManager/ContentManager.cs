using System.Collections.Generic;
using pattern;
using Script.GameContent;
using UnityEngine;

namespace Script.Manager.MonoSingleManager
{
    public enum EContentType
    {
        EDungeon,
        EVillage,
        EMine,
    }

    public class ContentManager : MonoSingleton<ContentManager>
    {
        private Dictionary<EContentType, ContentObject> contents = new Dictionary<EContentType, ContentObject>();
        
        public void Initialize()
        {
            AddContent(EContentType.EDungeon, new ContentInitInfo());
            AddContent(EContentType.EVillage, new ContentInitInfo());
            AddContent(EContentType.EMine, new ContentInitInfo());
        }
        
        // Scene초기화 타이밍때 호출하면 될듯?
        /// <summary>
        /// 컨텐츠 활성화시 호출
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="active"></param>
        public void ActiveContent(EContentType contentType, bool active)
        {
            if (!contents.TryGetValue(contentType, out ContentObject contentObject))
            {
                Debug.LogError("Content not found: " + contentType);
                return;
            }
            
            contentObject.ActiveContent(active);
        }

        private void AddContent(EContentType contentType, ContentInitInfo contentInitInfo)
        {
            if (contents.ContainsKey(contentType))
            {
                Debug.LogError("Duplicate content type: " + contentType);
                return;
            }

            GameObject go = new GameObject(contentType.ToString());
            go.transform.SetParent(transform);
            go.SetActive(false);
            
            ContentObject targetComponent = go.AddComponent<ContentObject>();
            contents.Add(contentType, targetComponent);
            targetComponent.CreateContentController(contentType, contentInitInfo);
        }
    }
}