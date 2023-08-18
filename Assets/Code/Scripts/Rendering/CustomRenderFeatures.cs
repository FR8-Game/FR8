// using FR8.Rendering.Passes;
// using UnityEngine;
// using UnityEngine.Rendering.HighDefinition;
//
// namespace FR8.Rendering
// {
//     public sealed class CustomRenderFeatures : ScriptableRendererFeature
//     {
//         [SerializeField] private bool renderFog = true;
//         [SerializeField] private bool showFogInSceneView = false;
//         [SerializeField] private bool renderOutline = true;
//         [SerializeField] private bool volumetrics = true;
//
//         [Space]
//         [SerializeField] private bool renderFogOverSkybox;
//         
//         private FogPass fogPass;
//         private SelectionOutlinePass outlinePass;
//         private LightVolumetricPass volumetricPass;
//
//         public override void Create()
//         {
//             fogPass = new FogPass(renderFogOverSkybox, showFogInSceneView);
//             outlinePass = new SelectionOutlinePass();
//             volumetricPass = new LightVolumetricPass();
//         }
//
//         public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//         {
//             if (renderFog) renderer.EnqueuePass(fogPass);
//             if (renderOutline) renderer.EnqueuePass(outlinePass);
//             if (volumetrics) renderer.EnqueuePass(volumetricPass);
//         }
//     }
// }
