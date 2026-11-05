using System;
using System.Collections.Generic;
using UnityEngine;

namespace PaintBlend.Painting
{
    [Serializable]
    public struct BrushSettings
    {
        [Range(0.002f, 0.25f)] public float Radius;
        [Range(0f, 1f)] public float Opacity;
        [Range(0f, 1f)] public float Hardness;
        public Color Colour;

        public static BrushSettings Default
        {
            get
            {
                return new BrushSettings
                {
                    Radius = 0.035f,
                    Opacity = 1f,
                    Hardness = 0.8f,
                    Colour = Color.magenta
                };
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class PaintSurface : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private int materialIndex;
        [SerializeField] private Shader stampShader;
        [SerializeField, Range(256, 2048)] private int textureResolution = 1024;

        private Material runtimeMaterial;
        private Material stampMaterial;
        private Texture sourceTexture;
        private RenderTexture front;
        private RenderTexture back;
        private readonly List<StampRecord> applied = new List<StampRecord>();
        private readonly List<StampRecord> redo = new List<StampRecord>();
        private int currentStrokeId;
        private bool initialized;

        public event Action PaintChanged;

        private struct StampRecord
        {
            public Vector2 Uv;
            public BrushSettings Brush;
            public int StrokeId;
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            ReleaseTextures();
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
            if (stampMaterial != null) Destroy(stampMaterial);
        }

        public bool Initialize()
        {
            if (initialized) return true;
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            if (stampShader == null) stampShader = Shader.Find("PaintBlend/UVPaintStamp");
            if (targetRenderer == null || stampShader == null)
            {
                Debug.LogWarning("PaintSurface needs a Renderer and the PaintBlend/UVPaintStamp shader.", this);
                return false;
            }

            Material[] materials = targetRenderer.materials;
            if (materialIndex < 0 || materialIndex >= materials.Length)
            {
                Debug.LogError("PaintSurface material index is out of range.", this);
                return false;
            }

            runtimeMaterial = materials[materialIndex];
            sourceTexture = runtimeMaterial.HasProperty("_BaseMap")
                ? runtimeMaterial.GetTexture("_BaseMap")
                : runtimeMaterial.mainTexture;
            if (sourceTexture == null) sourceTexture = Texture2D.whiteTexture;

            stampMaterial = new Material(stampShader);
            front = CreateTarget("PaintSurfaceFront");
            back = CreateTarget("PaintSurfaceBack");
            Graphics.Blit(sourceTexture, front);
            AssignPaintTexture(front);
            initialized = true;
            return true;
        }

        private RenderTexture CreateTarget(string targetName)
        {
            var target = new RenderTexture(textureResolution, textureResolution, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            target.name = targetName + "_" + GetInstanceID();
            target.useMipMap = true;
            target.autoGenerateMips = true;
            target.wrapMode = TextureWrapMode.Repeat;
            target.filterMode = FilterMode.Bilinear;
            target.Create();
            return target;
        }

        public void BeginStroke()
        {
            currentStrokeId++;
        }

        public bool Stamp(Vector2 uv, BrushSettings brush)
        {
            if (!Initialize()) return false;
            brush.Radius = Mathf.Clamp(brush.Radius, 0.002f, 0.25f);
            brush.Opacity = Mathf.Clamp01(brush.Opacity);
            brush.Hardness = Mathf.Clamp01(brush.Hardness);

            var record = new StampRecord { Uv = uv, Brush = brush, StrokeId = currentStrokeId };
            ApplyRecord(record);
            applied.Add(record);
            redo.Clear();
            PaintChanged?.Invoke();
            return true;
        }

        public bool Undo()
        {
            if (applied.Count == 0 || !Initialize()) return false;
            int stroke = applied[applied.Count - 1].StrokeId;
            int removed = 0;
            for (int i = applied.Count - 1; i >= 0 && applied[i].StrokeId == stroke; i--)
            {
                redo.Insert(0, applied[i]);
                applied.RemoveAt(i);
                removed++;
            }
            if (removed == 0) return false;
            RebuildFromHistory();
            PaintChanged?.Invoke();
            return true;
        }

        public bool Redo()
        {
            if (redo.Count == 0 || !Initialize()) return false;
            int stroke = redo[0].StrokeId;
            int moved = 0;
            while (moved < redo.Count && redo[moved].StrokeId == stroke)
            {
                applied.Add(redo[moved]);
                moved++;
            }
            redo.RemoveRange(0, moved);
            RebuildFromHistory();
            PaintChanged?.Invoke();
            return true;
        }

        public void ResetPaint()
        {
            if (!Initialize()) return;
            applied.Clear();
            redo.Clear();
            Graphics.Blit(sourceTexture, front);
            AssignPaintTexture(front);
            PaintChanged?.Invoke();
        }

        private void ApplyRecord(StampRecord record)
        {
            stampMaterial.SetVector("_BrushUV", record.Uv);
            stampMaterial.SetColor("_BrushColor", record.Brush.Colour);
            stampMaterial.SetFloat("_BrushRadius", record.Brush.Radius);
            stampMaterial.SetFloat("_BrushOpacity", record.Brush.Opacity);
            stampMaterial.SetFloat("_BrushHardness", record.Brush.Hardness);
            stampMaterial.SetFloat("_BrushAspect", 1f);
            Graphics.Blit(front, back, stampMaterial, 0);
            SwapTargets();
            AssignPaintTexture(front);
        }

        private void RebuildFromHistory()
        {
            Graphics.Blit(sourceTexture, front);
            for (int i = 0; i < applied.Count; i++)
            {
                StampRecord record = applied[i];
                stampMaterial.SetVector("_BrushUV", record.Uv);
                stampMaterial.SetColor("_BrushColor", record.Brush.Colour);
                stampMaterial.SetFloat("_BrushRadius", record.Brush.Radius);
                stampMaterial.SetFloat("_BrushOpacity", record.Brush.Opacity);
                stampMaterial.SetFloat("_BrushHardness", record.Brush.Hardness);
                stampMaterial.SetFloat("_BrushAspect", 1f);
                Graphics.Blit(front, back, stampMaterial, 0);
                SwapTargets();
            }
            AssignPaintTexture(front);
        }

        private void SwapTargets()
        {
            RenderTexture temp = front;
            front = back;
            back = temp;
        }

        private void AssignPaintTexture(Texture texture)
        {
            if (runtimeMaterial == null) return;
            if (runtimeMaterial.HasProperty("_BaseMap")) runtimeMaterial.SetTexture("_BaseMap", texture);
            if (runtimeMaterial.HasProperty("_MainTex")) runtimeMaterial.SetTexture("_MainTex", texture);
        }

        private void ReleaseTextures()
        {
            if (front != null) { front.Release(); Destroy(front); front = null; }
            if (back != null) { back.Release(); Destroy(back); back = null; }
            initialized = false;
        }
    }
}
