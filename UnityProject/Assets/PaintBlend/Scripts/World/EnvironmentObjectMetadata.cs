using UnityEngine;

namespace PaintBlend.World
{
    public enum SurfaceFamily { Wood, Marble, Concrete, Fabric, Ceramic, Stone, PaintedWall, Metal, Foliage, Plastic, Glass }

    [DisallowMultipleComponent]
    public sealed class EnvironmentObjectMetadata : MonoBehaviour
    {
        [SerializeField] private string objectId;
        [SerializeField] private string displayName;
        [SerializeField] private SurfaceFamily surfaceFamily;
        [SerializeField] private Color referenceColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float naturalConcealment = 0.5f;
        [SerializeField] private bool isValidCamouflageReference = true;

        public string ObjectId => objectId;
        public string DisplayName => displayName;
        public SurfaceFamily Surface => surfaceFamily;
        public Color ReferenceColor => referenceColor;
        public float NaturalConcealment => naturalConcealment;
        public bool IsValidCamouflageReference => isValidCamouflageReference;

        public void Configure(string id, string title, SurfaceFamily family, Color color, float concealment)
        {
            objectId = id;
            displayName = title;
            surfaceFamily = family;
            referenceColor = color;
            naturalConcealment = Mathf.Clamp01(concealment);
        }
    }
}
