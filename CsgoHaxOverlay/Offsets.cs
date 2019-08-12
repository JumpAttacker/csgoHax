// ReSharper disable InconsistentNaming

using System;
using System.Runtime.Serialization;

namespace CsgoHaxOverlay
{
    // 2018-12-22 00:10:32.287010600 UTC

    public static class Netvars
    {
        public static int cs_gamerules_data { get; set; }
        public static int m_ArmorValue { get; set; }
        public static int m_Collision { get; set; }
        public static int m_CollisionGroup { get; set; }
        public static int m_Local { get; set; }
        public static int m_MoveType { get; set; }
        public static int m_OriginalOwnerXuidHigh { get; set; }
        public static int m_OriginalOwnerXuidLow { get; set; }
        public static int m_SurvivalGameRuleDecisionTypes { get; set; }
        public static int m_SurvivalRules { get; set; }
        public static int m_aimPunchAngle { get; set; }
        public static int m_aimPunchAngleVel { get; set; }
        public static int m_bBombPlanted { get; set; }
        public static int m_bFreezePeriod { get; set; }
        public static int m_bGunGameImmunity { get; set; }
        public static int m_bHasDefuser { get; set; }
        public static int m_bHasHelmet { get; set; }
        public static int m_bInReload { get; set; }
        public static int m_bIsDefusing { get; set; }
        public static int m_bIsQueuedMatchmaking { get; set; }
        public static int m_bIsScoped { get; set; }
        public static int m_bIsValveDS { get; set; }
        public static int m_bSpotted { get; set; }
        public static int m_bSpottedByMask { get; set; }
        public static int m_clrRender { get; set; }
        public static int m_dwBoneMatrix { get; set; }
        public static int m_fAccuracyPenalty { get; set; }
        public static int m_fFlags { get; set; }
        public static int m_flC4Blow { get; set; }
        public static int m_flDefuseCountDown { get; set; }
        public static int m_flDefuseLength { get; set; }
        public static int m_flFallbackWear { get; set; }
        public static int m_flFlashDuration { get; set; }
        public static int m_flFlashMaxAlpha { get; set; }
        public static int m_flNextPrimaryAttack { get; set; }
        public static int m_flTimerLength { get; set; }
        public static int m_hActiveWeapon { get; set; }
        public static int m_hMyWeapons { get; set; }
        public static int m_hObserverTarget { get; set; }
        public static int m_hOwner { get; set; }
        public static int m_hOwnerEntity { get; set; }
        public static int m_iAccountID { get; set; }
        public static int m_iClip1 { get; set; }
        public static int m_iCompetitiveRanking { get; set; }
        public static int m_iCompetitiveWins { get; set; }
        public static int m_iCrosshairId { get; set; }
        public static int m_iEntityQuality { get; set; }
        public static int m_iFOV { get; set; }
        public static int m_iFOVStart { get; set; }
        public static int m_iGlowIndex { get; set; }
        public static int m_iHealth { get; set; }
        public static int m_iItemDefinitionIndex { get; set; }
        public static int m_iItemIDHigh { get; set; }
        public static int m_iObserverMode { get; set; }
        public static int m_iShotsFired { get; set; }
        public static int m_iState { get; set; }
        public static int m_iTeamNum { get; set; }
        public static int m_lifeState { get; set; }
        public static int m_nFallbackPaintKit { get; set; }
        public static int m_nFallbackSeed { get; set; }
        public static int m_nFallbackStatTrak { get; set; }
        public static int m_nForceBone { get; set; }
        public static int m_nTickBase { get; set; }
        public static int m_rgflCoordinateFrame { get; set; }
        public static int m_szCustomName { get; set; }
        public static int m_szLastPlaceName { get; set; }
        public static int m_thirdPersonViewAngles { get; set; }
        public static int m_vecOrigin { get; set; }
        public static int m_vecVelocity { get; set; }
        public static int m_vecViewOffset { get; set; }
        public static int m_viewPunchAngle { get; set; }
    }
    public static class Signatures
    {
        public static int clientstate_choked_commands { get; set; }
        public static int clientstate_delta_ticks { get; set; }
        public static int clientstate_last_outgoing_command { get; set; }
        public static int clientstate_net_channel { get; set; }
        public static int convar_name_hash_table { get; set; }
        public static int dwClientState { get; set; }
        public static int dwClientState_GetLocalPlayer { get; set; }
        public static int dwClientState_IsHLTV { get; set; }
        public static int dwClientState_Map { get; set; }
        public static int dwClientState_MapDirectory { get; set; }
        public static int dwClientState_MaxPlayer { get; set; }
        public static int dwClientState_PlayerInfo { get; set; }
        public static int dwClientState_State { get; set; }
        public static int dwClientState_ViewAngles { get; set; }
        public static int dwEntityList { get; set; }
        public static int dwForceAttack { get; set; }
        public static int dwForceAttack2 { get; set; }
        public static int dwForceBackward { get; set; }
        public static int dwForceForward { get; set; }
        public static int dwForceJump { get; set; }
        public static int dwForceLeft { get; set; }
        public static int dwForceRight { get; set; }
        public static int dwGameDir { get; set; }
        public static int dwGameRulesProxy { get; set; }
        public static int dwGetAllClasses { get; set; }
        public static int dwGlobalVars { get; set; }
        public static int dwGlowObjectManager { get; set; }
        public static int dwInput { get; set; }
        public static int dwInterfaceLinkList { get; set; }
        public static int dwLocalPlayer { get; set; }
        public static int dwMouseEnable { get; set; }
        public static int dwMouseEnablePtr { get; set; }
        public static int dwPlayerResource { get; set; }
        public static int dwRadarBase { get; set; }
        public static int dwSensitivity { get; set; }
        public static int dwSensitivityPtr { get; set; }
        public static int dwSetClanTag { get; set; }
        public static int dwViewMatrix { get; set; }
        public static int dwWeaponTable { get; set; }
        public static int dwWeaponTableIndex { get; set; }
        public static int dwYawPtr { get; set; }
        public static int dwZoomSensitivityRatioPtr { get; set; }
        public static int dwbSendPackets { get; set; }
        public static int dwppDirect3DDevice9 { get; set; }
        public static int interface_engine_cvar { get; set; }
        public static int m_bDormant { get; set; }
        public static int m_pStudioHdr { get; set; }
        public static int m_pitchClassPtr { get; set; }
        public static int m_yawClassPtr { get; set; }
        public static int model_ambient_min { get; set; }
    }

}