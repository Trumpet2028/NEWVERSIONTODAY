using Exiled.API.Features;

using Player = Exiled.Events.Handlers.Player;
using acoolplugin.Handlers;
using PluginAPI.Core.Attributes;
using Exiled.Permissions.Extensions;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using Exiled.API.Enums;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs;
using static PlayerRoles.PlayableScps.Scp079.GUI.Scp079ScannerGui;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using PlayerRoles.PlayableScps.Scp106;

namespace acoolplugin
{
    internal class Plugin : Plugin<Config>
    {
        public EventHandlers EventHandlers;
//        internal CoroutineHandle FFAHandle;

        internal bool DuringRound { get; set; }
        public static Plugin Instance { get; set; } = new Plugin();
        public static bool IsRestarting { get; set; } = false;
        public override string Prefix => "hmmmm interesting";

        public override string Author => "[UGA] Ranger";
        public EventHandlers _handlers;
        public override Exiled.API.Enums.PluginPriority Priority { get; } = Exiled.API.Enums.PluginPriority.Higher;

        public override void OnEnabled()
        {
            Instance = this;

            RegisterEvents();

            base.OnEnabled();

        }

        public override void OnDisabled()
        {
            Instance = null;

            UnRegisterEvents();

            base.OnDisabled();
        }
        // Friendly Fire Autoban Immunity! Don't work on this yet.
        internal bool IsImmune(Exiled.API.Features.Player player)
        {
            return player.CheckPermission("FFA.immune");
        }

        // ^ ^ ^ ^ ^ THIS!
        public void RegisterEvents()
        {
            try
            {
                this._handlers = new EventHandlers(this);
                Player.Hurting += _handlers.OnHurt;
                Player.Spawning += _handlers.OnSpawning;
                Player.Dying += _handlers.OnDying;
                Player.Left += _handlers.OnLeaving;
                Exiled.Events.Handlers.Scp106.Attacking += _handlers.OnAttack106;
                Exiled.Events.Handlers.Server.RoundStarted += _handlers.OnRoundStart;
            }
            catch(Exception ex)
            {
                Log.Error(string.Format("There was an error loading the plugin: {0}", (object)ex));
            }
        }
        public void UnRegisterEvents()
        {
            Player.Hurting -= _handlers.OnHurt;
            Player.Spawning -= _handlers.OnSpawning;
            Player.Dying -= _handlers.OnDying;
            Player.Left -= _handlers.OnLeaving;
            Exiled.Events.Handlers.Server.RoundStarted -= _handlers.OnRoundStart;
            this.EventHandlers = (EventHandlers)null;
        }
/*       internal Teamkiller AddAndGetTeamkiller(Exiled.API.Features.Player player) I'm just looking at this code. I found it off Pat's FFA (Friendly Fire Autoban.)
        {
            int id = player.Id;
            string nickname = player.Nickname;
            string userId = player.UserId;
            string ipAddress = player.IPAddress;
            if (!Plugin.Instance.Teamkillers.ContainsKey(userId))
            {
                Log.Info(string.Format("Adding Teamkiller entry for player #{0} {1} [{2}] [{3}]", (object)id, (object)nickname, (object)userId, (object)ipAddress));
                Plugin.Instance.Teamkillers[userId] = new Teamkiller(id, nickname, userId, ipAddress);
            }
            return Plugin.Instance.Teamkillers[userId];
        } */
    }
}
