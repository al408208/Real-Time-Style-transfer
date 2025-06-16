namespace NatureRendererDemo
{
    using UnityEngine;
    using UnityEngine.Rendering;

    [ExecuteInEditMode]
    public class RenderPipelineLighting : MonoBehaviour
    {

        [Header( "Standard" )]
        [SerializeField]
        private GameObject _standardLighting;

        [SerializeField]
        private Material _standardSky;

        [SerializeField]
        private Material _standardTerrain;

        private void OnValidate()
        {
            Awake();
        }

        private void Awake()
        {
#if UNITY_EDITOR


            var renderPipeline = QualitySettings.renderPipeline;
            if(renderPipeline == null)
                renderPipeline = GraphicsSettings.defaultRenderPipeline;

            var renderPipelineName = renderPipeline?.GetType().Name ?? "";

            if( _standardLighting != null )
                _standardLighting.SetActive( renderPipelineName == "" );

            switch( renderPipelineName )
            {
                case "":
                    RenderSettings.skybox = _standardSky;
                    SetTerrainMaterial( _standardTerrain );
                    break;
            }
#endif
        }

#if UNITY_EDITOR
        private void SetTerrainMaterial( Material material )
        {
            foreach( var terrain in FindObjectsByType<Terrain>(FindObjectsInactive.Include, FindObjectsSortMode.None) )
                terrain.materialTemplate = material;
        }

#endif
    }
}