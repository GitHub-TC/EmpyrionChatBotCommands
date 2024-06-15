using Eleon.Modding;
using EmpyrionNetAPIAccess;
using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
using EmpyrionNetAPITools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpyrionChatBotCommands
{
    public class ChatBotCommands : EmpyrionModBase
    {
        public enum Factions
        {
            Faction = 0,
            Private = 1,
        }

        public enum StructureTypes
        {
            Undef = 0, 
            BA = 2, 
            CV = 3, 
            SV = 4, 
            HV = 5, 
            AstVoxel = 7
        }

        public ChatBotCommands()
        {
            EmpyrionConfiguration.ModName = "EmpyrionChatBotCommands";
        }

        public ModGameAPI DediAPI { get; private set; }
        public ConfigurationManager<ChatBotConfiguration> Configuration { get; private set; }

        public override void Initialize(ModGameAPI dediAPI)
        {
            DediAPI = dediAPI;

            try
            {
                Log($"**EmpyrionChatBotCommands: loaded");

                LoadConfiguration();
                LogLevel = Configuration.Current.LogLevel;
                ChatCommandManager.CommandPrefix = Configuration.Current.ChatCommandPrefix;

                ChatCommands.Add(new ChatCommand(@"CB:help"                                                                 , (I, A) => DisplayHelp  (I.playerId),       "display help"));
                if (Configuration.Current.Enabled.Reset      ) ChatCommands.Add(new ChatCommand(@"CB:Reset"                 , (I, A) => Reset        (I.playerId),       "wipe player fo a resetart"));
                if (Configuration.Current.Enabled.Home       ) ChatCommands.Add(new ChatCommand(@"CB:SetHome:(?<ID>\d*)"    , (I, A) => SetHome      (I.playerId,A),     "set ID for home"));
                if (Configuration.Current.Enabled.Home       ) ChatCommands.Add(new ChatCommand(@"CB:GoHome"                , (I, A) => GotoHome     (I.playerId),       "go to home (if set)"));
                if (Configuration.Current.Enabled.GetShipDown) ChatCommands.Add(new ChatCommand(@"CB:GetShipDown:(?<ID>\d*)", (I, A) => GetShipDown  (I.playerId, A),    "get ship down to player"));
                if (Configuration.Current.Enabled.GetShipHere) ChatCommands.Add(new ChatCommand(@"CB:GetShipHere:(?<ID>\d*)", (I, A) => GetShipHere  (I.playerId, A),    "warp ship tom player"));
                if (Configuration.Current.Enabled.GotoShip   ) ChatCommands.Add(new ChatCommand(@"CB:GotoShip:(?<ID>\d*)"   , (I, A) => GotoShip     (I.playerId, A),    "teleport player to ship"));
            }
            catch (Exception Error)
            {
                Log($"**EmpyrionChatBotCommands Error: {Error} {string.Join(" ", Environment.GetCommandLineArgs())}", LogLevel.Error);
            }

        }

        private async Task GotoShip(int playerId, Dictionary<string, string> arguments)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            if (playerInfo.LastGetShipCommand.HasValue && (Configuration.Current.MinutesUntilLastShipCommand - (int)(DateTime.Now - playerInfo.LastGetShipCommand.Value).TotalMinutes) > 0)
            {
                InformPlayer(playerId, $"You have to wait {DisplayWaitDelay(Configuration.Current.MinutesUntilLastShipCommand, playerInfo.LastGetShipCommand)}");
                return;
            }

            var ID = int.TryParse(arguments["ID"]?.Trim(), out var id) ? id : 0;
            if (ID == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            var gsi = await Request_GlobalStructure_Info(ID.ToId());

            if (gsi.id == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            if ((gsi.factionGroup != (byte)Factions.Private || gsi.factionId != P.entityId) &&
                (gsi.factionGroup != (byte)Factions.Faction || gsi.factionId != P.factionId))
            {
                InformPlayer(playerId, $"ID not private or in your fraction");
                return;
            }

            if (gsi.type != (byte)StructureTypes.CV && gsi.type != (byte)StructureTypes.SV && gsi.type != (byte)StructureTypes.HV)
            {
                InformPlayer(playerId, $"no CV,SV,HV structure with ID {ID} found.");
                return;
            }

            playerInfo.LastGetShipCommand = DateTime.Now;
            Configuration.Save();

            if (gsi.PlayfieldName == P.playfield) await Request_Entity_Teleport         (new IdPositionRotation         (P.entityId,                    new PVector3(gsi.pos.x, gsi.pos.y + 200, gsi.pos.z), P.rot));
            else                                  await Request_Player_ChangePlayerfield(new IdPlayfieldPositionRotation(P.entityId, gsi.PlayfieldName, new PVector3(gsi.pos.x, gsi.pos.y + 200, gsi.pos.z), P.rot));
        }

        private async Task GetShipHere(int playerId, Dictionary<string, string> arguments)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            if (playerInfo.LastGetShipCommand.HasValue && (Configuration.Current.MinutesUntilLastShipCommand - (int)(DateTime.Now - playerInfo.LastGetShipCommand.Value).TotalMinutes) > 0)
            {
                InformPlayer(playerId, $"You have to wait {DisplayWaitDelay(Configuration.Current.MinutesUntilLastShipCommand, playerInfo.LastGetShipCommand.Value)}");
                return;
            }

            var ID = int.TryParse(arguments["ID"]?.Trim(), out var id) ? id : 0;
            if (ID == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            var gsi = await Request_GlobalStructure_Info(ID.ToId());

            if (gsi.id == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            if ((gsi.factionGroup != (byte)Factions.Private || gsi.factionId != P.entityId) &&
                (gsi.factionGroup != (byte)Factions.Faction || gsi.factionId != P.factionId))
            {
                InformPlayer(playerId, $"ID not private or in your fraction");
                return;
            }

            if (gsi.type != (byte)StructureTypes.CV && gsi.type != (byte)StructureTypes.SV && gsi.type != (byte)StructureTypes.HV)
            {
                InformPlayer(playerId, $"no CV,SV,HV structure with ID {playerInfo.HomeID} found.");
                return;
            }

            playerInfo.LastGetShipCommand = DateTime.Now;
            Configuration.Save();

            if (gsi.PlayfieldName == P.playfield) await Request_Entity_Teleport       (new IdPositionRotation         (gsi.id,                    new PVector3(P.pos.x, P.pos.y + 100, P.pos.z), gsi.rot));
            else                                  await Request_Entity_ChangePlayfield(new IdPlayfieldPositionRotation(gsi.id, gsi.PlayfieldName, new PVector3(P.pos.x, P.pos.y + 100, P.pos.z), gsi.rot));
        }

        private async Task GetShipDown(int playerId, Dictionary<string, string> arguments)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            if (playerInfo.LastGetShipCommand.HasValue && (Configuration.Current.MinutesUntilLastShipCommand - (int)(DateTime.Now - playerInfo.LastGetShipCommand.Value).TotalMinutes) > 0)
            {
                InformPlayer(playerId, $"You have to wait {DisplayWaitDelay(Configuration.Current.MinutesUntilLastShipCommand, playerInfo.LastGetShipCommand)}");
                return;
            }

            var ID = int.TryParse(arguments["ID"]?.Trim(), out var id) ? id : 0;
            if (ID == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            var gsi = await Request_GlobalStructure_Info(ID.ToId());

            if (gsi.id == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            if ((gsi.factionGroup != (byte)Factions.Private || gsi.factionId != P.entityId) &&
                (gsi.factionGroup != (byte)Factions.Faction || gsi.factionId != P.factionId))
            {
                InformPlayer(playerId, $"ID not private or in your fraction");
                return;
            }

            if (gsi.type != (byte)StructureTypes.CV && gsi.type != (byte)StructureTypes.SV && gsi.type != (byte)StructureTypes.HV)
            {
                InformPlayer(playerId, $"no CV,SV,HV structure with ID {ID} found.");
                return;
            }

            playerInfo.LastGetShipCommand = DateTime.Now;
            Configuration.Save();

            await Request_Entity_Teleport(new IdPositionRotation(gsi.id, new PVector3(P.pos.x, P.pos.y + 100, P.pos.z), P.rot));
        }

        private async Task GotoHome(int playerId)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            if (playerInfo.HomeID == 0)
            {
                InformPlayer(playerId, $"You have no home set with CB:SetHome:ID");
                return;
            }

            var gsi = await Request_GlobalStructure_Info(playerInfo.HomeID.ToId());

            if (gsi.id == 0)
            {
                InformPlayer(playerId, $"no structure with ID {playerInfo.HomeID} found.");
                return;
            }

            try{ await Request_Load_Playfield(new PlayfieldLoad(20, gsi.PlayfieldName, 0)); } catch { /* ignore errors */ }

            if (gsi.PlayfieldName == P.playfield) await Request_Entity_Teleport         (new IdPositionRotation         (P.entityId,                    new PVector3(gsi.pos.x, gsi.pos.y + 200, gsi.pos.z), gsi.rot));
            else                                  await Request_Player_ChangePlayerfield(new IdPlayfieldPositionRotation(P.entityId, gsi.PlayfieldName, new PVector3(gsi.pos.x, gsi.pos.y + 200, gsi.pos.z), gsi.rot));
        }

        private async Task SetHome(int playerId, Dictionary<string, string> arguments)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            var ID = int.TryParse(arguments["ID"]?.Trim(), out var id) ? id : 0;
            var gsi = await Request_GlobalStructure_Info(ID.ToId());

            if (gsi.id == 0)
            {
                InformPlayer(playerId, $"no structure with ID {ID} found.");
                return;
            }

            if (gsi.type != (byte)StructureTypes.CV && gsi.type != (byte)StructureTypes.BA)
            {
                InformPlayer(playerId, $"no CV,BA structure with ID {ID} found.");
                return;
            }

            if ((gsi.factionGroup == (byte)Factions.Private && gsi.factionId == P.entityId) ||
                (gsi.factionGroup == (byte)Factions.Faction && gsi.factionId == P.factionId))
            {
                playerInfo.HomeID = ID;
                playerInfo.HomeName = gsi.name;
                Configuration.Save();

                InformPlayer(playerId, $"Set home structure to ID [{playerInfo.HomeID}] {playerInfo.HomeName}");
            }
            else InformPlayer(playerId, $"ID not private or in your fraction");
        }

        private async Task Reset(int playerId)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            if (playerInfo.LastReset.HasValue && (Configuration.Current.MinutesUntilLastResetCommand - (int)(DateTime.Now - playerInfo.LastReset.Value).TotalMinutes) > 0)
            {
                InformPlayer(playerId, $"You have to wait {DisplayWaitDelay(Configuration.Current.MinutesUntilLastResetCommand, playerInfo.LastReset)}");
                return;
            }

            playerInfo.LastReset = DateTime.Now;
            Configuration.Save();

            await Request_ConsoleCommand(new PString($"kick {P.steamId} PlayerWipe"));
            TaskTools.Delay(10, () => File.Delete(Path.Combine(EmpyrionConfiguration.SaveGamePath, "Players", P.steamId + ".ply")));
        }

        private ChatBotConfiguration.PlayerData GetPlayerInfo(PlayerInfo P)
        {
            if(Configuration.Current.Players == null) Configuration.Current.Players = new List<ChatBotConfiguration.PlayerData>();

            var playerInfo = Configuration.Current.Players.SingleOrDefault(p => p.SteamId == P.steamId);
            if(playerInfo != null) return playerInfo;

            playerInfo = new ChatBotConfiguration.PlayerData() {
                SteamId = P.steamId,
                Name = P.playerName,
            };
            Configuration.Current.Players.Add(playerInfo);
            return playerInfo;
        }

        private void LoadConfiguration()
        {
            Configuration = new ConfigurationManager<ChatBotConfiguration>
            {
                ConfigFilename = Path.Combine(EmpyrionConfiguration.SaveGameModPath, @"ChatBotCommands.json")
            };

            Configuration.Load();
            Configuration.Save();
        }

        private async Task DisplayHelp(int playerId)
        {
            var P = await Request_Player_Info(playerId.ToId());
            var playerInfo = GetPlayerInfo(P);

            var help = new StringBuilder();
            if (Configuration.Current.Enabled.Home    ) help.AppendLine($"CB:GoHome goto [{playerInfo.HomeID}] {playerInfo.HomeName}");
            if (Configuration.Current.Enabled.Reset   ) help.AppendLine($"CB:Reset available {DisplayWaitDelay(Configuration.Current.MinutesUntilLastResetCommand, playerInfo.LastReset)}");
            if (playerInfo.LastGetShipCommand.HasValue) help.AppendLine($"Next ship commands available {DisplayWaitDelay(Configuration.Current.MinutesUntilLastShipCommand, playerInfo.LastGetShipCommand)}");

            await DisplayHelp(playerId, help.ToString());
        }

        private string DisplayWaitDelay(int minutesToWait, DateTime? lastCall)
        {
            if (!lastCall.HasValue) return "";

            var delta = minutesToWait - (int)(DateTime.Now - lastCall.Value).TotalMinutes;

            return delta > 0 ? $"in {delta} minute(s)" : "";
        }

    }
}
