using UnityEngine;

namespace PaintBlend.Painting
{
    public sealed class PaintRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private LayerMask paintableLayers = ~0;
        [SerializeField] private float maxPaintDistance = 8f;
        [SerializeField] private BrushSettings brush = new BrushSettings
        {
            Radius = 0.035f, Opacity = 1f, Hardness = 0.8f, Colour = Color.magenta
        };

        private PaintSurface activeStrokeSurface;
        private bool strokeActive;
        public BrushSettings CurrentBrush { get => brush; set => brush = value; }

        private void Awake()
        {
            if (sourceCamera == null) sourceCamera = Camera.main;
        }

        public void BeginStroke()
        {
            strokeActive = true;
            activeStrokeSurface = null;
        }

        public void EndStroke()
        {
            strokeActive = false;
            activeStrokeSurface = null;
        }

        public bool TryPaintScreenPoint(Vector2 screenPoint)
        {
            if (sourceCamera == null) return false;
            Ray ray = sourceCamera.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, maxPaintDistance, paintableLayers,
                QueryTriggerInteraction.Ignore))
                return false;

            PaintSurface surface = hit.collider.GetComponentInParent<PaintSurface>();
            if (surface == null) return false;
            return surface.Stamp(hit.textureCoord, brush);
        }

        public bool TrySampleBaseColour(Vector2 screenPoint, out Color sampledColour)
        {
            sampledColour = Color.white;
            if (sourceCamera == null) return false;
            Ray ray = sourceCamera.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, maxPaintDistance, ~0, QueryTriggerInteraction.Ignore))
                return false;

            Renderer renderer = hit.collider.GetComponentInParent<Renderer>();
            if (renderer == null) return false;
            Material material = renderer.sharedMaterial;
            if (material == null) return false;
            if (material.HasProperty("_BaseColor")) sampledColour = material.GetColor("_BaseColor");
            else if (material.HasProperty("_Color")) sampledColour = material.GetColor("_Color");
            return true;
        }
    }
}
