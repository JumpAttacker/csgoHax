using System;
using System.Numerics;
using Encoding = System.Text.Encoding;

namespace CsgoHaxOverlay.Entities
{
    public class Player
    {
        protected bool Equals(Player other)
        {
            return Entity == other.Entity;
        }

        public int Entity;
        public int Id;
        public WeaponHandler.ItemDefinitionIndex CurrentlyWeapon;
        public Team Team;
        public bool IsAlly;
        public string Name;

        public Player(int id)
        {
            Id = id;
            Entity = GetEntity(id);
            Health = GetHealth();
            //Armor = GetArmor();
            Team = GetTeam();
            Position = GetPosition();
            IsValid = CheckForDormant() && Team != Team.None && Team != Team.Spectator;
            Name = GetName();
            Name = System.Text.RegularExpressions.Regex.Replace(Name, @"\s+", "");
            if (LittleOverlay.LocalPlayer != null)
                IsAlly = Team == LittleOverlay.LocalPlayer.Team;
            Printer.PrintInfo($"NewPlayer ({Id}) [{Name}] {Entity} {Health} {Team} {MemUtils.ReadByte((IntPtr)(Entity + Signatures.m_bDormant))}");
            var counter = 0;
            foreach (var c in Name)
            {
                if (c.Equals(' '))
                {
                    Printer.PrintInfo($"Emplty#{counter}");
                }
                counter++;
            }
        }

        public virtual void Update()
        {
            var newEntity = GetEntity(Id);
            if (newEntity == 0)
            {
                IsValid = false;
                return;
            }
            Entity = newEntity;
            Team = GetTeam();
            IsValid = CheckForDormant() && Team != Team.None && Team != Team.Spectator;
            if (!IsValid) return;
            Position = GetPosition();
            IsAlive = CheckForAlive();
            IsAlly = Team == LittleOverlay.LocalPlayer.Team;
        }

        public bool IsAlive { get; set; }

        public bool IsEnemy => !IsAlly;
        public Vector3 Position;
        public float Health;
        //public float Armor;
        public bool IsValid;
        public Vector3 HeadPosition;

        public void ChangeColor(Vector3 clr)
        {
            MemUtils.Write((IntPtr) Entity + Netvars.m_clrRender,
                new[] {(byte) clr.X, (byte) clr.Y, (byte) clr.Z});
        }

        public bool CheckForDormant()
        {
            return MemUtils.ReadByte((IntPtr)(Entity + Signatures.m_bDormant)) == 0;
        }

        public static int GetEntity(int id)
        {
            return MemUtils.ReadInt32(LittleOverlay.Client + Signatures.dwEntityList + id * 0x10);
        }
        public int GetHealth()
        {
            return MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_iHealth));
        }
        public int GetArmor()
        {
            return MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_ArmorValue));
        }
        public Team GetTeam()
        {
            return (Team)MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_iTeamNum));
        }
        public Vector3 GetPosition()
        {
            return MemUtils.ReadVector3((IntPtr)(Entity + Netvars.m_vecOrigin));
        }

        public enum LifeState
        {
            LIFE_ALIVE = 0, // alive
            LIFE_DYING = 1, // playing death animation or still falling off of a ledge waiting to hit ground
            LIFE_DEAD = 2 // dead. lying still.
        }
        public bool CheckForAlive()
        {
            return (LifeState)MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_lifeState)) == LifeState.LIFE_ALIVE;
        }
        public LifeState GetLifeState()
        {
            return (LifeState)MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_lifeState));
        }

        public Vector3 GetEntityBonePosition(int boneid)
        {
            var boneMatrix = MemUtils.ReadInt32((IntPtr)(Entity + Netvars.m_dwBoneMatrix));

            var bonePos = new Vector3
            {
                X = MemUtils.ReadFloat((IntPtr) (boneMatrix + 0x30 * boneid + 0xC)),
                Y = MemUtils.ReadFloat((IntPtr) (boneMatrix + 0x30 * boneid + 0x1C)),
                Z = MemUtils.ReadFloat((IntPtr) (boneMatrix + 0x30 * boneid + 0x2C))
            };

            return bonePos;
        }

        public WeaponHandler.ItemDefinitionIndex GetCurrentWeapon()
        {
            return (WeaponHandler.ItemDefinitionIndex)GetWeaponId();
        }

        private int GetWeapon(int entity)
        {
            var a = MemUtils.ReadInt32((IntPtr)(entity + Netvars.m_hActiveWeapon)) & 0xFFF;
            var b = MemUtils.ReadInt32(LittleOverlay.Client + Signatures.dwEntityList + (a - 1) * 0x10);
            return b;
        }

        public int GetWeaponId()
        {
            return MemUtils.ReadInt32((IntPtr)(GetWeapon(Entity) + Netvars.m_iItemDefinitionIndex));
        }

        public int GetWeaponClip()
        {
            return MemUtils.ReadInt32((IntPtr)(GetWeapon(Entity) + Netvars.m_iClip1));
        }

        public int IsReloading(int entity)
        {
            return MemUtils.ReadInt32((IntPtr)(GetWeapon(entity) + Netvars.m_bInReload));
        }

        public string GetName()
        {
            var radarAddress = MemUtils.ReadInt32(LittleOverlay.Client + Signatures.dwRadarBase);
            radarAddress = MemUtils.ReadInt32((IntPtr)(radarAddress + 0x74));
            return MemUtils.ReadString((IntPtr)(radarAddress + 0x168 * (Id+1) + 0x180), 32,
                Encoding.ASCII);
        }
    }
}