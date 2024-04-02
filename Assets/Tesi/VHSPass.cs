using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class VHSPass : ScriptableRenderPass
{
    // Utilizzato per renderizzare dalla camera ai post-processing
    // avanti e indietro, fino a che non renderizziamo l'immagine finale
    // alla camera
    RenderTargetIdentifier source;
    RenderTargetIdentifier destinationA;
    RenderTargetIdentifier destinationB;
    RenderTargetIdentifier latestDest;

    readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
    readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");

    public VHSPass()
    {
        // Imposta l'evento di Pass del render
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Prende il descrittore di destinazione della camera. 
        //Useremo questo quando creiamo una texture di render temporanea.
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTarget;

        // Crea una texture di render temporanea usando il descrittore sopra.
        cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
        destinationA = new RenderTargetIdentifier(temporaryRTIdA);
        cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
        destinationB = new RenderTargetIdentifier(temporaryRTIdB);
    }

    // L'esecuzione effettiva del \textit{Pass}. 
    // Qui avviene il rendering personalizzato.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Salta il rendering del post processing all'interno della Scene View
        if (renderingData.cameraData.isSceneViewCamera)
            return;

        // Qui ottieni i tuoi materiali dalla tua classe personalizzata
        var materials = VHSMaterials.Instance;
        if (materials == null)
        {
            Debug.LogError("L'istanza dei Materiali di Post Processing Personalizzati è null");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
        cmd.Clear();

        // Questo contiene tutte le informazioni dei Volumi attuali
        // di cui avremo bisogno più tardi
        var stack = VolumeManager.instance.stack;

        #region Metodi Locali

        // Scambia le destinazioni di render avanti e indietro, in modo che
        // possiamo avere passaggi multipli e simili con solo poche texture
        void BlitTo(Material mat, int pass = 0)
        {
            var first = latestDest;
            var last = first == destinationA ? destinationB : destinationA;
            Blit(cmd, first, last, mat, pass);

            latestDest = last;
        }

        #endregion

        // Inizia con la sorgente della camera
        latestDest = source;

        //---Effetto personalizzato qui---
        var customEffect = stack.GetComponent<VHSComponent>();
        // Processa solo se l'effetto è attivo
        if (customEffect.IsActive())
        {
            var material = materials.customEffect;
            // P.s. ottimizza memorizzando l'ID della proprietà altrove
            material.SetFloat(Shader.PropertyToID("_Intensity"), customEffect.intensity.value);
            material.SetColor(Shader.PropertyToID("_OverlayColor"), customEffect.overlayColor.value);

            BlitTo(material);
        }

        // Si può aggiungere qualsiasi altro effetto/componente personalizzato 
        // nell'ordine di preferenza

        // Possiamo applicare il risultato finale alla camera
        Blit(cmd, latestDest, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    // Pulisce le RT temporanee quando non ne abbiamo più bisogno
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(temporaryRTIdA);
        cmd.ReleaseTemporaryRT(temporaryRTIdB);
    }
}