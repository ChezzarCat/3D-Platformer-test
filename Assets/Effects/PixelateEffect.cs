using UnityEngine;

[ExecuteInEditMode]
public class PixelateEffect : MonoBehaviour
{
    public Material effectMaterial;
    [Range(1, 1024)]
    public float pixelationFactor = 100;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_PixelationFactor", pixelationFactor);
            Graphics.Blit(src, dest, effectMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
