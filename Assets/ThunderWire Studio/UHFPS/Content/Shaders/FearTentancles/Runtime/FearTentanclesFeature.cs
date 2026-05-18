using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UHFPS.Rendering
{
    [Serializable]
    public class FearTentanclesFeature : EffectFeature
    {
        public override string Name => "Fear Tentancles";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material EffectMaterial;

        public override void OnCreate()
        {
            RenderPass = new FearTentanclesRGPass(RenderPassEvent, EffectMaterial);
        }
    }
}