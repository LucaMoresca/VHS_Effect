using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Tesi/VHSComponent", typeof(UniversalRenderPipeline))]
public class VHSComponent : VolumeComponent, IPostProcessComponent
{

    public ClampedFloatParameter intensity = new ClampedFloatParameter(value: 0, min: 0, max: 1, overrideState: true);
    public NoInterpColorParameter overlayColor = new NoInterpColorParameter(Color.cyan);
    public bool IsActive() => intensity.value > 0;
    public bool IsTileCompatible() => true;
}