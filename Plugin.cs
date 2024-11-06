using System;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using System.Linq;
using System.Collections.Generic;
using TShockAPI;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using System.Drawing;

[ApiVersion(2, 1)]
namespace TShockPluginWhitelist
{
    public class WhitelistPlugin : TerrariaPlugin
    {
        public override string Name => "WhitelistPlugin";
        public override string Author => "ViiDuc";
        public override string Description => "A plugin to manage a whitelist for the server.";
        public override Version Version => new Version(1, 0, 0, 0);

        private const string WhitelistFile = "whitelist.json";

        private List<string> Whitelist { get; set; } = new List<string>();

        public WhitelistPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnPlayerLogin;
            Commands.ChatCommands.Add(new Command("whitelist.reload", ReloadWhitelist, "wl", "reload"));
            Commands.ChatCommands.Add(new Command("whitelist.add", AddToWhitelist, "wt", "add"));
        }

        private void OnPlayerLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs e)
        {
            if (!Whitelist.Contains(e.Player.Name.ToLower()))
            {
                e.Player.SendMessage("You are not whitelisted on this server.", Microsoft.Xna.Framework.Color.Red);
                e.Player.Kick("You are not whitelisted.");
            }
        }

        private void ReloadWhitelist(CommandArgs args)
        {
            try
            {
                var path = Path.Combine(TShockAPI.TShock.SavePath, WhitelistFile);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Whitelist = JsonConvert.DeserializeObject<List<string>>(json);
                    args.Player.SendMessage("Whitelist reloaded successfully.", Microsoft.Xna.Framework.Color.Green);
                }
                else
                {
                    args.Player.SendMessage("Whitelist file not found, creating new file.", Microsoft.Xna.Framework.Color.Yellow);
                    SaveWhitelist();
                }
            }
            catch (Exception ex)
            {
                args.Player.SendMessage($"Error reloading whitelist: {ex.Message}", Microsoft.Xna.Framework.Color.Red);
            }
        }

        private void AddToWhitelist(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendMessage("Usage: /wt add <username>", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            string username = args.Parameters[0].ToLower();
            if (Whitelist.Contains(username))
            {
                args.Player.SendMessage($"{username} is already in the whitelist.", Microsoft.Xna.Framework.Color.Yellow);
                return;
            }
            Whitelist.Add(username);
            SaveWhitelist();

            args.Player.SendMessage($"{username} has been added to the whitelist.", Microsoft.Xna.Framework.Color.Green);
        }

        private void SaveWhitelist()
        {
            try
            {
                var path = Path.Combine(TShockAPI.TShock.SavePath, WhitelistFile);
                string json = JsonConvert.SerializeObject(Whitelist, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                TShockAPI.TShock.Log.Error($"Error saving whitelist: {ex.Message}");
            }
        }
    }
}
