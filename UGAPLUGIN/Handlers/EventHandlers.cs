using System.Collections.Generic;
using Exiled.API.Enums;
using PlayerRoles;
using System.Data;
using Exiled.API.Extensions;
using Discord;
using Exiled.API.Features;
using Exiled.Events.Handlers;
using PluginAPI.Core;
using MEC;
using static MapGeneration.ImageGenerator;
using Achievements.Handlers;
using PluginAPI.Events;
using Exiled.Events.EventArgs.Player;
using Exiled.CustomRoles.API;
using System.ComponentModel;
using System.Linq;
using System;
using System.Security.Policy;
using System.Diagnostics.Eventing.Reader;
using PluginAPI.Helpers;
using PluginAPI.Roles;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Remoting.Messaging;
using Exiled.API.Features.Roles;
using Exiled.API.Features.DamageHandlers;
using Exiled.Events.EventArgs.Scp049;
using System.Threading;
using Utf8Json.Internal;
using HarmonyLib;
using System.Threading.Tasks;
using Exiled.Events.Features;
using PluginAPI.Core.Zones.Pocket;
using Exiled.Events.EventArgs.Interfaces;

// Right now I'm stuck on Steam ID 64 for KD Ratio. On the server it just gives everyone the same kill/death ratio.
namespace acoolplugin.Handlers
{
    /* 
add the player to a dictionary
string, string
first one is victim and the other one is the 106 that kidnapped
then in death event, if the death cause was decay, add kill for the 106  
but u have to save the value of which 106 player touched which player
and only increase if they actually die
if they escape then u delete that saved value
because within the event there is no way to know
so you have to save the value on the apply or hit event
and then effect k/d if the player dies
*/
    using Exiled.Events.EventArgs.Player;
    using System.Collections;

    public class EventHandlers
    {
        static Dictionary<string, string> Player106 = new Dictionary<string, string>();
        private static string WhereDafuqIsPlayer(string PlayerUserId, string SCP106UserId, EnteringPocketDimensionEventArgs ev)
        {
            var LarryKillingEventVar = new Dictionary<string, string>();
            PlayerUserId = ev.Player.UserId;
            SCP106UserId = ev.Scp106.UserId;
            LarryKillingEventVar.Add(PlayerUserId, SCP106UserId);
            return LarryKillingEventVar[PlayerUserId]; 
        }
        public void OnAttack106(Exiled.Events.EventArgs.Scp106.AttackingEventArgs ev)// what?
        {
            Player106.Add(ev.Target.UserId, ev.Player.UserId);


        }
        public void OnHurt(HurtingEventArgs ev)
        {
            if (!Player106.ContainsKey(ev.Player.UserId))
            {
                if (ev.DamageHandler.Type == DamageType.Scp106)
                {
                    Player106.Add(ev.Player.UserId, ev.Attacker.UserId);
                }
            }
            if (ev.DamageHandler.Type == DamageType.Scp207)
            {
                ev.Player.Heal(2f, overrideMaxHealth: false);
                return;
            }
        }
        public void OnDyingBy106Event(DyingEventArgs ev)
        {

        }
        public void OnDying(DyingEventArgs ev)
        {
            if (!DeathCount.ContainsKey(ev.Player.UserId))
            {
                return;
            }
            RoleCount.Remove(ev.Player.UserId);
            RoleCount.Add(ev.Player.UserId, Spectator);
            DeathCount[ev.Player.UserId]++;
            DeathCount.TryGetValue(ev.Player.UserId, out int Deaths);
            KillCount.TryGetValue(ev.Player.UserId, out int Kills);
            string KD = $"|KD|  {Kills} : {Deaths}  ";
            ev.Player.RankName = "Spectator                    " + KD;
            ev.Player.RankColor = null;
            ev.Player.RankColor.ToString();
            if (ev.DamageHandler.Type == DamageType.PocketDimension || ev.DamageHandler.Type == DamageType.Scp106)
            {
                if (!Player106.ContainsValue(ev.Player.UserId))
                {
                    Exiled.API.Features.Player.Get(ev.Player.UserId); 
                    Exiled.API.Features.Log.Info("WOW!");
                    return;
                }
                Player106.TryGetValue(ev.Player.UserId, out string Scp106);
                return;
            }
            try
            {
                if (ev.Player.UserId == ev.Attacker.UserId)
                {
                    KillCount[ev.Player.UserId]--;
                    DeathCount.TryGetValue(ev.Player.UserId, out int SuicideDeaths);
                    KillCount.TryGetValue(ev.Player.UserId, out int SuicideKills);
                    string KilledThemselvesKD = $"|KD|  {SuicideKills} : {SuicideDeaths}  ";
                    ev.Player.RankName = "Spectator                    " + KilledThemselvesKD;
                    ev.Player.RankColor = null;
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (!DeathCount.ContainsKey(ev.Attacker.UserId))
                {
                    return;
                }
                DyingNext(roleTypeId, ev);
            }
            catch (Exception ex)
            {
                Exiled.API.Features.Log.Error(string.Format("Just going to ignore this error?: {0}", (object)ex));
            }
        }
        internal Plugin plugin;
        internal EventHandlers(Plugin plugin) => this.plugin = plugin;
        public static bool AlreadyDied { get; set; }
        public static bool LeftTheServer { get; set; }
        public bool BecameSCP { get; set; }

        public void OnRoundStart()
        {
            Exiled.API.Features.Log.Info("Round has started!!!");
            Plugin.Instance.DuringRound = true;
            return;
        }
        public Dictionary<string, int> KillCount = new Dictionary<string, int>();
        public Dictionary<string, int> DeathCount = new Dictionary<string, int>();
        private static RoleTypeId roleTypeId;
        public Broadcast.BroadcastFlags Type { get; set; }
        public void OtherTeamsOnDying(RoleTypeId roleTypeId, DyingEventArgs ev)
        {
            RoleCount.TryGetValue(ev.Player.UserId, out bool Role);
            if (Role == Scientist)
            {
                if (ev.Attacker.IsNTF || ev.Attacker.Role == RoleTypeId.Scientist || ev.Attacker.Role == RoleTypeId.FacilityGuard)
                {
                    if (KillCount.ContainsKey(ev.Attacker.UserId))
                    {
                        KillCount[ev.Attacker.UserId]--;
                        DeathCount.TryGetValue(ev.Attacker.UserId, out int Deaths);
                        KillCount.TryGetValue(ev.Attacker.UserId, out int Kills);
                        string KD = $"|KD|  {Kills} : {Deaths}  ";
                        if (ev.Attacker.Role == RoleTypeId.Scientist)
                        {
                            ev.Attacker.RankName = "Scientist                    " + KD;
                            ev.Attacker.RankColor = "yellow";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                        if (ev.Attacker.IsNTF)
                        {
                            ev.Attacker.RankName = "Nine-Tailed-Fox              " + KD;
                            ev.Attacker.RankColor = "aqua";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                        if (ev.Attacker.Role == RoleTypeId.FacilityGuard)
                        {
                            ev.Attacker.RankName = "Facility Guard               " + KD;
                            ev.Attacker.RankColor = "nickel";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                    }
                }
                else
                {
                    KillCount[ev.Attacker.UserId]++;
                    NotTK(ev);
                    return;
                }
            }
            if (Role == ClassD)
            {
                // This method is just for facility guards, classd, scientist. There are two more for ntf and chaos.
                if (ev.Attacker.IsCHI || ev.Attacker.Role == RoleTypeId.ClassD)
                {
                    if (KillCount.ContainsKey(ev.Attacker.UserId))
                    {
                        KillCount[ev.Attacker.UserId]--;
                        DeathCount.TryGetValue(ev.Attacker.UserId, out int Deaths);
                        KillCount.TryGetValue(ev.Attacker.UserId, out int Kills);
                        string KD = $"|KD|  {Kills} : {Deaths}  ";
                        if (ev.Attacker.IsCHI)
                        {
                            ev.Attacker.RankName = "Chaos Insurgency             " + KD;
                            ev.Attacker.RankColor = "green";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                        if (ev.Attacker.Role == RoleTypeId.ClassD)
                        {
                            ev.Attacker.RankName = "Class-D                      " + KD;
                            ev.Attacker.RankColor = "pumpkin";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                    }
                }
                else
                {
                    KillCount[ev.Attacker.UserId]++;
                    NotTK(ev);
                    return;
                }
            }
            if (Role == FacilityGuard)
            {
                if (ev.Attacker.IsNTF || ev.Attacker.Role == RoleTypeId.Scientist || ev.Attacker.Role == RoleTypeId.FacilityGuard)
                {
                    if (KillCount.ContainsKey(ev.Attacker.UserId))
                    {
                        KillCount[ev.Attacker.UserId]--;
                        DeathCount.TryGetValue(ev.Attacker.UserId, out int Deaths);
                        KillCount.TryGetValue(ev.Attacker.UserId, out int Kills);
                        string KD = $"|KD|  {Kills} : {Deaths}  ";
                        if (ev.Attacker.Role == RoleTypeId.Scientist)
                        {
                            ev.Attacker.RankName = "Scientist                    " + KD;
                            ev.Attacker.RankColor = "yellow";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                        if (ev.Attacker.IsNTF)
                        {
                            ev.Attacker.RankName = "Nine-Tailed-Fox              " + KD;
                            ev.Attacker.RankColor = "aqua";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                        if (ev.Attacker.Role == RoleTypeId.FacilityGuard)
                        {
                            ev.Attacker.RankName = "Facility Guard               " + KD;
                            ev.Attacker.RankColor = "nickel";
                            ev.Attacker.RankColor.ToString();
                            return;
                        }
                    }
                    else
                    {
                        KillCount[ev.Attacker.UserId]++;
                        NotTK(ev);
                        return;
                    }
                }
            }
            else
            {
                KillCount[ev.Attacker.UserId]++;
                NotTK(ev);
                return;
            }

        }
        #region NTFTeamOnDying
        public void NTFTeamOnDying(RoleTypeId roleTypeId, DyingEventArgs ev)
        {
            // Come back to the OnDying Method when you're done.
            RoleCount.TryGetValue(ev.Player.UserId, out bool Role);

            if (ev.Attacker.IsNTF || ev.Attacker.Role == RoleTypeId.Scientist || ev.Attacker.Role == RoleTypeId.FacilityGuard)
            {
                if (KillCount.ContainsKey(ev.Attacker.UserId))
                {
                    KillCount[ev.Attacker.UserId]--;
                    DeathCount.TryGetValue(ev.Attacker.UserId, out int Deaths);
                    KillCount.TryGetValue(ev.Attacker.UserId, out int Kills);
                    string KD = $"|KD|  {Kills} : {Deaths}  ";
                    if (ev.Attacker.Role == RoleTypeId.Scientist)
                    {
                        ev.Attacker.RankName = "Scientist                    " + KD;
                        ev.Attacker.RankColor = "yellow";
                        ev.Attacker.RankColor.ToString();
                        return;
                    }
                    if (ev.Attacker.IsNTF)
                    {  
                        ev.Attacker.RankName = "Nine-Tailed-Fox              " + KD;
                        ev.Attacker.RankColor = "aqua";
                        ev.Attacker.RankColor.ToString();
                        return;
                    }
                    if (ev.Attacker.Role == RoleTypeId.FacilityGuard)
                    { 
                        ev.Attacker.RankName = "Facility Guard               " + KD;
                        ev.Attacker.RankColor = "nickel";
                        ev.Attacker.RankColor.ToString();
                        return;
                    }
                }
            }
            else
            {
                KillCount[ev.Attacker.UserId]++;
                NotTK(ev);
                return;
            }
        }
        #endregion
        #region ChaosTeamOnDying
        public void ChaosTeamOnDying(RoleTypeId roleTypeId, DyingEventArgs ev)
        {
            RoleCount.TryGetValue(ev.Player.UserId, out bool Role);
            if (ev.Attacker.IsCHI || ev.Attacker.Role == RoleTypeId.ClassD)
            {
                if (KillCount.ContainsKey(ev.Attacker.UserId))
                {
                    KillCount[ev.Attacker.UserId]--;
                    DeathCount.TryGetValue(ev.Attacker.UserId, out int Deaths);
                    KillCount.TryGetValue(ev.Attacker.UserId, out int Kills);
                    string KD = $"|KD|  {Kills} : {Deaths}  ";
                    ev.Attacker.RankName = "Chaos Insurgency             " + KD;
                    ev.Attacker.RankColor = "green";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                return;
            }
            else
            {
                // I'm goin under... drowning in these blues.
                KillCount[ev.Attacker.UserId]++;
                NotTK(ev);
                return;
            }
        }
        #endregion
        public void SCPTeamOnDying(RoleTypeId roleTypeId, DyingEventArgs ev)
        {
            KillCount[ev.Attacker.UserId]++;
            NotTK(ev);
            return;
        }
        public Dictionary<string, Exiled.API.Features.Player> UserIdsCache { get; set; }    
        //then in death event, if the death cause was decay, add kill for the 106  
        public void DyingNext(RoleTypeId roleTypeId, DyingEventArgs ev)
        {
            if (ev.Player.IsCHI)
            {
                ChaosTeamOnDying(roleTypeId, ev);
                return;
            }
            if (ev.Player.IsNTF)
            {
                NTFTeamOnDying(roleTypeId, ev);
                return;
            }
            if (ev.Player.IsScp)
            {
                SCPTeamOnDying(roleTypeId, ev);
                return;
            }
            else
            {
                OtherTeamsOnDying(roleTypeId, ev);
                return;
            }
        }
        public void NotTK(DyingEventArgs ev)
        {
            try
            {
                DeathCount.TryGetValue(ev.Attacker.UserId, out int Deaths);
                KillCount.TryGetValue(ev.Attacker.UserId, out int Kills);
                string KD = $"|KD|  {Kills} : {Deaths}  ";
                if (ev.Attacker.Role == RoleTypeId.Scp173)
                {
                    ev.Attacker.RankName = "SCP-173                      " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scp096)
                {
                    ev.Attacker.RankName = "SCP-096                      " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scp939)
                {
                    ev.Attacker.RankName = "SCP-939                      " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scp079)
                {
                    ev.Attacker.RankName = "SCP-079                      " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scp049)
                {
                    ev.Attacker.RankName = "SCP-049                      " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scp0492)
                {
                    ev.Attacker.RankName = "SCP-049-2                    " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scp3114)
                {
                    ev.Attacker.RankName = "SCP-3114                     " + KD;
                    ev.Attacker.RankColor = "crimson";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Scientist)
                {
                    ev.Attacker.RankName = "Scientist                    " + KD;
                    ev.Attacker.RankColor = "yellow";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.ClassD)
                {
                    ev.Attacker.RankName = "Class-D                      " + KD;
                    //ev.Attacker.RankName = "Chaos Insurgency                " + KD;
                    ev.Attacker.RankColor = "pumpkin";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.IsCHI)
                {
                    ev.Attacker.RankName = "Chaos Insurgency             " + KD;
                    ev.Attacker.RankColor = "green";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.FacilityGuard)
                {
                    ev.Attacker.RankName = "Facility Guard               " + KD;
                    ev.Attacker.RankColor = "nickel";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.IsNTF)
                {
                    ev.Attacker.RankName = "Nine-Tailed Fox              " + KD;
                    ev.Attacker.RankColor = "aqua";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
                if (ev.Attacker.Role == RoleTypeId.Tutorial)
                {
                    ev.Attacker.RankName = "Tutorial                     " + KD;
                    ev.Attacker.RankColor = "magenta";
                    ev.Attacker.RankColor.ToString();
                    return;
                }
            }
            catch
            {
                Exiled.API.Features.Log.Info("Hello!");
            }
        }

        Dictionary<string, bool> RoleCount = new Dictionary<string, bool>();
        public bool Scientist { get; set; }
        public bool ClassD { get; set; }
        public bool ChaosInsurgency { get; set; }
        public bool NineTailedFox { get; set; }
        public bool FacilityGuard { get; set; }
        public bool Tutorial { get; set; }
        public bool Overwatch { get; set; }
        public bool Spectator { get; set; }
        public bool Scp173 { get; set; }
        public bool Scp096 { get; set; }
        public bool AScp106 { get; set; }
        public bool Scp939 { get; set; }
        public bool Scp079 { get; set; }
        public bool Scp049 { get; set; }
        public bool Scp0492 { get; set; }
        public bool Scp3114 { get; set; }
        public bool DamagedBy106 { get; set; }

        public void OnSpawning(SpawningEventArgs ev)
        {
            #region NOT SCP
            if (DeathCount.ContainsKey(ev.Player.UserId))
            {
                AlreadyDied = true;
                DeathCount.TryGetValue(ev.Player.UserId, out int Deaths);
                KillCount.TryGetValue(ev.Player.UserId, out int Kills);
                string KD = $"|KD|  {Kills} : {Deaths}  ";
                if (ev.Player.Role == RoleTypeId.Spectator)
                {
                    ev.Player.RankName = "Spectator                    " + KD;
                    ev.Player.RankColor = null;
                    ev.Player.RankColor.ToString();
                }
                if (ev.Player.Role == RoleTypeId.Scientist)
                {
                    RoleCount[ev.Player.UserId] = Scientist;

                    ev.Player.RankName = "Scientist                    " + KD;
                    ev.Player.RankColor = "yellow";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.ClassD)
                {
                    RoleCount[ev.Player.UserId] = ClassD;
                    ev.Player.RankName = "Class-D                      " + KD;
                    ev.Player.RankColor = "pumpkin";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.IsCHI)
                {
                    RoleCount[ev.Player.UserId] = ChaosInsurgency;
                    ev.Player.RankName = "Chaos Insurgency             " + KD;
                    ev.Player.RankColor = "green";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.FacilityGuard)
                {
                    RoleCount[ev.Player.UserId] = FacilityGuard;
                    ev.Player.RankName = "Facility Guard               " + KD;
                    ev.Player.RankColor = "nickel";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.IsNTF)
                {
                    RoleCount[ev.Player.UserId] = NineTailedFox;
                    ev.Player.RankName = "Nine-Tailed Fox              " + KD;
                    ev.Player.RankColor = "aqua";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.Tutorial)
                {
                    RoleCount[ev.Player.UserId] = Tutorial;
                    ev.Player.RankName = "Tutorial                     " + KD;
                    ev.Player.RankColor = "magenta";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.Overwatch)
                {
                    RoleCount[ev.Player.UserId] = Overwatch;
                    ev.Player.RankName = "Overwatch                    " + KD;
                    ev.Player.RankColor = null; 
                    ev.Player.RankColor.ToString();
                    return;
                }
                #endregion
                if (ev.Player.Role == RoleTypeId.Scp173)
                {
                    RoleCount[ev.Player.UserId] = Scp173;
                    ev.Player.RankName = "SCP-173                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp096)
                {
                    RoleCount[ev.Player.UserId] = Scp096;
                    ev.Player.RankName = "SCP-096                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp106)
                {
                    RoleCount[ev.Player.UserId] = AScp106;
                    ev.Player.RankName = "SCP-106                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp939)
                {
                    RoleCount[ev.Player.UserId] = Scp939;
                    ev.Player.RankName = "SCP-939                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp079)
                {
                    RoleCount[ev.Player.UserId] = Scp079;
                    ev.Player.RankName = "SCP-079                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp049)
                {
                    RoleCount[ev.Player.UserId] = Scp049;
                    ev.Player.RankName = "SCP-049                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp0492)
                {
                    RoleCount[ev.Player.UserId] = Scp0492;
                    ev.Player.RankName = "SCP-049-2                    " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp3114)
                {
                    RoleCount[ev.Player.UserId] = Scp3114;
                    ev.Player.RankName = "SCP-3114                     " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                return;
            }
            else
            {
                AlreadyDied = false;
                DeathCount.Add(ev.Player.UserId, 0);
                KillCount.Add(ev.Player.UserId, 0);
                DeathCount.TryGetValue(ev.Player.UserId, out int Deaths);
                KillCount.TryGetValue(ev.Player.UserId, out int Kills);
                string KD = $"|KD|  {Kills} : {Deaths}  ";
                if (ev.Player.Role == RoleTypeId.Scientist)
                {
                    ev.Player.RankName = "Scientist                    " + KD;
                    ev.Player.RankColor = "yellow";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.ClassD)
                {
                    ev.Player.RankName = "Class-D                      " + KD;
                    //ev.Player.RankName = "Chaos Insurgency                " + KD;
                    ev.Player.RankColor = "pumpkin";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.IsCHI)
                {
                    ev.Player.RankName = "Chaos Insurgency             " + KD;
                    ev.Player.RankColor = "green";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.FacilityGuard)
                {
                    ev.Player.RankName = "Facility Guard               " + KD;
                    ev.Player.RankColor = "nickel";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.IsNTF)
                {
                    ev.Player.RankName = "Nine-Tailed Fox              " + KD;
                    ev.Player.RankColor = "aqua";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.Tutorial)
                {
                    ev.Player.RankName = "Tutorial                     " + KD;
                    ev.Player.RankColor = "magenta";
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.Overwatch)
                {
                    ev.Player.RankName = "Overwatch                    " + KD;
                    ev.Player.RankColor = null;
                    ev.Player.RankColor.ToString();
                    return;
                }
                if (ev.Player.Role == RoleTypeId.Scp173)
                { 
                    ev.Player.RankName = "SCP-173                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                    // I might use this bool later. Whatever?
                }
                if (ev.Player.Role == RoleTypeId.Scp096)
                {
                    ev.Player.RankName = "SCP-096                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp106)
                {
                    ev.Player.RankName = "SCP-106                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp939)
                {
                    ev.Player.RankName = "SCP-939                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp079)
                {
                    ev.Player.RankName = "SCP-079                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp049)
                {
                    ev.Player.RankName = "SCP-049                      " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp0492)
                {
                    ev.Player.RankName = "SCP-049-2                    " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
                if (ev.Player.Role == RoleTypeId.Scp3114)
                {
                    ev.Player.RankName = "SCP-3114                     " + KD;
                    ev.Player.RankColor = "crimson";
                    ev.Player.RankColor.ToString();
                    bool BecameSCP = true;
                }
            }
            //Dictionary COUNT!
        }
        public void OnLeaving(LeftEventArgs ev)
        {
            DeathCount.Remove(ev.Player.UserId);
            KillCount.Remove(ev.Player.UserId);
            RoleCount.Remove(ev.Player.UserId);
            Player106.Remove(ev.Player.UserId);
            AlreadyDied = false;
        }
    }
}