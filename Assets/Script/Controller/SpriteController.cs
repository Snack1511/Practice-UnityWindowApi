using System;
using Framework.Extension.Component;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null;

    public void SetupComponent()
    {
        SpriteRenderer renderer = this.AddOrGetComponent<SpriteRenderer>();
        spriteRenderer = renderer;
    }

    public void LoadSprite(string spritePath)
    {
        //TODO [임시]: 리소스 매니저 만들면 바꿔버리자
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        spriteRenderer.sprite = sprite;
    }
}
