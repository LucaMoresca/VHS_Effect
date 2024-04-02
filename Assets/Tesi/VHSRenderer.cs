using UnityEngine.Rendering.Universal;

[System.Serializable]
public class VHSRenderer : ScriptableRendererFeature
{
    VHSPass pass;

    public override void Create()
    {
        pass = new VHSPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}