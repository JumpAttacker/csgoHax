using System;
using System.Numerics;

namespace CsgoHaxOverlay.Entities
{
    public class LocalPlayer : Player
    {
        public Matrix ViewMatrix;
        public Vector3 ViewAngles;
        public Vector2 PunchAngle;
        public Player PlayerInCrosshair;
        public LocalPlayer() : base(0)
        {
            Entity = MemUtils.ReadInt32(LittleOverlay.Client + Signatures.dwLocalPlayer);
            ViewMatrix = GetViewMatrix();
            ViewAngles = GetViewAngles();
        }

        public override void Update()
        {
            Entity = MemUtils.ReadInt32(LittleOverlay.Client + Signatures.dwLocalPlayer);
            if (Entity == 0)
                return;

            Health = GetHealth();
            //Armor = GetArmor();
            Team = GetTeam();
            Position = GetPosition();
            ViewMatrix = GetViewMatrix();
            ViewAngles = GetViewAngles();
            IsAlive = CheckForAlive();
            if (!Program.IsTriggerBot) return;
            var id = GetEnemyInCross();
            var target = LittleOverlay.Players.Find(x => x.Entity.Equals(id));
            if (target != null && target.Team != Team)
            {
                PlayerInCrosshair = target;
            }
            else
            {
                PlayerInCrosshair = null;
            }
        }

        public Vector3 GetViewAngles()
        {
            var clientState = MemUtils.ReadInt32(LittleOverlay.Engine + Signatures.dwClientState);
            return MemUtils.ReadVector3((IntPtr)(clientState + Signatures.dwClientState_ViewAngles));
        }

        public Matrix GetViewMatrix()
        {
            return MemUtils.ReadMatrix(LittleOverlay.Client + Signatures.dwViewMatrix, 4, 4);
        }

        private int _GetCrosshair()
        {
            return MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_iCrosshairId));
        }
        public int GetEnemyInCross()
        {
            return MemUtils.ReadInt32(LittleOverlay.Client + Signatures.dwEntityList + (_GetCrosshair() - 1) * 0x10);
        }

        public bool IsScoping()
        {
            return MemUtils.ReadByte((IntPtr)(Entity + Netvars.m_bIsScoped)) == 1;
        }
    }
}