using UnityEngine;

public class RenderWindowWallpaper : MonoBehaviour
{
    [SerializeField] private Material m_Material;
    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        Graphics.Blit(from, to, m_Material);
    }
}
