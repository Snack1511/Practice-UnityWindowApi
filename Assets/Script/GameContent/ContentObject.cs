using Manager;
using UnityEngine;

namespace Script.GameContent
{
    public class ContentObject : MonoBehaviour
    {
        public EContentType ContentType { get; private set; }
        private ContentBase contentController;

        public void CreateContentController(EContentType eContentType, ContentInitInfo contentInitInfo)
        {
            ContentType = eContentType;
            contentController = (ContentType) switch
            {
                EContentType.EDungeon => new ContentDungeon(),
                EContentType.EVillage => new ContentVillage(),
                EContentType.EMine => new ContentMine(),
                _ => null,
            };

            contentController.InitalizeContent(contentInitInfo);
        }

        public void ActiveContent(bool active)
        {
            if (active)
            {
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