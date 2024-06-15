using EmpyrionNetAPIDefinitions;
using System;
using System.Collections.Generic;

namespace EmpyrionChatBotCommands
{
    public class ChatBotConfiguration
    {
        public class ChatBotCommandsEnabled {
            public bool Reset { get; set; } = true;
            public bool Home { get; set; } = true;
            public bool GetShipHere { get; set; } = true;
            public bool GetShipDown { get; set; } = true;
            public bool GotoShip { get; set; } = true;
        }
        public class PlayerData
        {
            public string SteamId { get; set; }
            public string Name { get; set; }
            public int HomeID { get; set; }
            public string HomeName { get; set; }
            public DateTime? LastGetShipCommand { get; set; }
            public DateTime? LastReset { get; set; }
        }

        public LogLevel LogLevel { get; set; }
        public string ChatCommandPrefix { get; set; } = "";
        public int MinutesUntilLastShipCommand { get; set; } = 3;
        public int MinutesUntilLastResetCommand { get; set; } = 3;

        public ChatBotCommandsEnabled Enabled { get; set; } = new ChatBotCommandsEnabled();

        public IList<PlayerData> Players { get; set; } = new List<PlayerData>();
    }
}