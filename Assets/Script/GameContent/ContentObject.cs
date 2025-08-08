using Manager;
using UnityEngine;

namespace Script.GameContent
{
    public class ContentObject : MonoBehaviour
    {
        public EContentType ContentType { get; private set; }
        private ContentBase contentController;

        public void CreateContentController(EContentType eContentType)
        {
            ContentType =  eContentType;
            contentController = (ContentType) switch
            {
                EContentType.EDungeon => new ContentDungeon(),
                EContentType.EVillage => new ContentVillage(),
                EContentType.EMine => new ContentMine(),
                _ => null,
            };
        }

        public void ActiveContent(bool active)
        {
            if (active)
            {
                contentController.InitalizeContent();
                contentController.SetupContentUI();
            }
            else
            {
                contentController.ReleaseContent();
            }
            
            gameObject.SetActive(active);
        }
    }
}