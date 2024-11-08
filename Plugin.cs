using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace WhitelistPlugin
{
    [ApiVersion(2, 1)]
    public class WhitelistPlugin : TerrariaPlugin
    {
        private readonly string WhitelistFilePath = Path.Combine("tshock", "whitelist.json");
        private HashSet<string> whitelistedPlayers;

        public override string Name => "Whitelist Plugin";
        public override string Author => "ViiDuc";
        public override string Description => "A simple whitelist plugin for TShock 5.2";
        public override Version Version => new Version(1, 0);

        public WhitelistPlugin(Main game) : base(game) 
        {
            whitelistedPlayers = new HashSet<string>();
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            Commands.ChatCommands.Add(new Command("whitelist.reload", ReloadWhitelist, "wlreload"));
            Commands.ChatCommands.Add(new Command("whitelist.add", AddToWhitelist, "wladd"));

            try
            {
                LoadWhitelist();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[WhitelistPlugin] Error loading whitelist: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
            }
            base.Dispose(disposing);
        }

        private void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player == null)
            {
                return;
            }

            if (!whitelistedPlayers.Contains(player.Name.ToLower()))
            {
                player.Disconnect("You are not whitelisted on this server.");
                return;
            }
        }

        private void LoadWhitelist()
        {
            if (!File.Exists(WhitelistFilePath))
            {
                Directory.CreateDirectory("tshock");
                File.WriteAllText(WhitelistFilePath, JsonConvert.SerializeObject(new List<string>(), Formatting.Indented));
            }

            var whitelistData = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(WhitelistFilePath));
            whitelistedPlayers = new HashSet<string>(whitelistData.ConvertAll(p => p.ToLower()));
        }

        private void ReloadWhitelist(CommandArgs args)
        {
            try
            {
                LoadWhitelist();
                args.Player.SendSuccessMessage("Whitelist reloaded.");
            }
            catch (Exception ex)
            {
                args.Player.SendErrorMessage($"Error reloading whitelist: {ex.Message}");
            }
        }

        private void AddToWhitelist(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Usage: /wladd <playername>");
                return;
            }

            var playerName = args.Parameters[0].ToLower();
            if (!whitelistedPlayers.Contains(playerName))
            {
                whitelistedPlayers.Add(playerName);
                try
                {
                    SaveWhitelist();
                    args.Player.SendSuccessMessage($"{args.Parameters[0]} has been added to the whitelist.");
                }
                catch (Exception ex)
                {
                    args.Player.SendErrorMessage($"Error saving whitelist: {ex.Message}");
                }
            }
            else
            {
                args.Player.SendErrorMessage($"{args.Parameters[0]} is already whitelisted.");
            }
        }

        private void SaveWhitelist()
        {
            File.WriteAllText(WhitelistFilePath, JsonConvert.SerializeObject(whitelistedPlayers, Formatting.Indented));
        }
    }
}
