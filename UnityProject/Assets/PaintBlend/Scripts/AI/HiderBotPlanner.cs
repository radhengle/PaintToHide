using System.Collections.Generic;
using UnityEngine;
using PaintBlend.Gameplay;
using PaintBlend.World;

namespace PaintBlend.AI
{
    public enum BotDifficulty { Easy, Normal, Hard }

    public sealed class HiderBotPlanner
    {
        public HidingSlot ChooseSlot(IList<HidingSlot> candidates, Transform bot,
            Color currentPaintColour, BotDifficulty difficulty, out float chosenScore)
        {
            chosenScore = float.NegativeInfinity;
            if (candidates == null || bot == null) return null;

            HidingSlot best = null;
            for (int i = 0; i < candidates.Count; i++)
            {
                HidingSlot slot = candidates[i];
                if (slot == null) continue;
                EnvironmentObjectMetadata metadata = slot.GetComponentInParent<EnvironmentObjectMetadata>();
                if (metadata == null || !metadata.IsValidCamouflageReference) continue;

                float distance = Vector3.Distance(bot.position, slot.PoseAnchor.position);
                float colourDistance = ColorDistance(currentPaintColour, metadata.ReferenceColor);
                float score = metadata.NaturalConcealment * 0.48f +
                              (1f - colourDistance) * 0.32f +
                              slot.BaseConcealment * 0.20f -
                              Mathf.Clamp01(distance / 45f) * 0.16f;

                // Lower difficulties intentionally make noisier choices.
                if (difficulty == BotDifficulty.Easy) score += Random.Range(-0.24f, 0.18f);
                else if (difficulty == BotDifficulty.Normal) score += Random.Range(-0.10f, 0.08f);
                else score += Random.Range(-0.025f, 0.025f);

                if (score > chosenScore)
                {
                    chosenScore = score;
                    best = slot;
                }
            }
            return best;
        }

        private static float ColorDistance(Color a, Color b)
        {
            Vector3 delta = new Vector3(a.r - b.r, a.g - b.g, a.b - b.b);
            return Mathf.Clamp01(delta.magnitude / 1.732f);
        }
    }
}
