using System;

namespace PaintBlend.Gameplay
{
    public enum PoseType
    {
        Stand = 0, Sit = 1, Crouch = 2, Kneel = 3, Lean = 4,
        ArmsFolded = 5, ArmsRaised = 6, LieDown = 7
    }

    [Flags]
    public enum PoseMask
    {
        None = 0,
        Stand = 1 << 0,
        Sit = 1 << 1,
        Crouch = 1 << 2,
        Kneel = 1 << 3,
        Lean = 1 << 4,
        ArmsFolded = 1 << 5,
        ArmsRaised = 1 << 6,
        LieDown = 1 << 7,
        All = Stand | Sit | Crouch | Kneel | Lean | ArmsFolded | ArmsRaised | LieDown
    }

    public static class PoseMaskUtility
    {
        public static PoseMask ToMask(PoseType pose) { return (PoseMask)(1 << (int)pose); }
    }
}
