using System;
using System.Numerics;

namespace CsgoHaxOverlay.Entities
{
    public class JustEntity
    {
        public int Entity;
        public Vector3 Position;
        public Vector2 W2SPosition;
        public int ClassId;

        public JustEntity(int entity)
        {
            Entity = entity;
            ClassId = GetClassId();
        }

        public void Update()
        {
            Position = GetPosition();
            W2SPosition = GetW2SPos();
            if (W2SPosition.X <= 0 || W2SPosition.Y <= 0 || W2SPosition.X >= Program.ScreenSize.X || W2SPosition.Y >= Program.ScreenSize.Y)
                return;
            ClassId = GetClassId();
        }

        public int GetClassId()
        {
            var one = MemUtils.ReadInt32((IntPtr)(Entity + 8));
            var two = MemUtils.ReadInt32((IntPtr)(one + 2 * 4));
            var three = MemUtils.ReadInt32((IntPtr)(two + 1));
            var classId = MemUtils.ReadInt32((IntPtr)(three + 20));
            return classId;
        }

        public Vector3 GetPosition()
        {
            return MemUtils.ReadVector3((IntPtr)(Entity + Netvars.m_vecOrigin));
        }

        public Vector2 GetW2SPos()
        {
            return MathUtils.WorldToScreen(LittleOverlay.LocalPlayer.ViewMatrix, Program.ScreenSize, Position);
        }
    }
}