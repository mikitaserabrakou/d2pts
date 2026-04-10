using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dota2GSI;

namespace D2PTS
{
    internal class Program
    {
        private const string GSI_URI = "http://localhost:3003/";
        private const string API_PREFIX = "http://localhost:5005/"; // our own endpoint
        private static string _currentHero = null;
        private static readonly object _lock = new();
        private static string _lastConnectionLog = null;
        private static Dota2GSI.Nodes.DOTA_GameState _lastMapState = Dota2GSI.Nodes.DOTA_GameState.Undefined;

        private static async Task Main()
        {
            var gsi = new GameStateListener(GSI_URI);
            if (!gsi.Start())
            {
                Console.Error.WriteLine("Failed to start GSI listener");
                return;
            }
            gsi.NewGameState += OnNewGameState;
            Console.WriteLine("GSI listener started on " + GSI_URI);

            using var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:5005/hero/");
            listener.Prefixes.Add("http://[::1]:5005/hero/");
            listener.Start();
            Console.WriteLine("API listening on " + API_PREFIX + "hero");

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var ctx = await listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(ctx));
                }
            });

            Console.WriteLine("Press Ctrl-C to exit.");
            await Task.Delay(Timeout.Infinite);
        }

        private static void OnNewGameState(GameState gs)
        {
            if (gs == null) { Log("GSI push: null payload", ConsoleColor.Yellow); return; }

            if (gs.Provider != null)
            {
                var conn = $"name={gs.Provider.Name}  appid={gs.Provider.AppID}  version={gs.Provider.Version}";
                if (conn != _lastConnectionLog)
                {
                    Log($"GSI connected: {conn}", ConsoleColor.Cyan);
                    _lastConnectionLog = conn;
                }
            }

            var map = gs.Map;
            if (map != null && map.GameState != _lastMapState)
            {
                Log($"Map: {map.GameState}  (matchid={map.MatchID})",
                    map.GameState == Dota2GSI.Nodes.DOTA_GameState.DOTA_GAMERULES_STATE_GAME_IN_PROGRESS
                        ? ConsoleColor.Green
                        : ConsoleColor.DarkGray);
                _lastMapState = map.GameState;
            }

            var hero = PrettyHero(gs.Hero?.LocalPlayer?.Name ?? "");
            string prev;
            lock (_lock) prev = _currentHero;

            if (!string.IsNullOrWhiteSpace(hero) && hero != prev)
            {
                Log($"HERO PICKED: {hero}", ConsoleColor.Magenta);
                lock (_lock) _currentHero = hero;
            }
            else if (string.IsNullOrWhiteSpace(hero) && prev != null)
            {
                Log("Hero deselected / game ended", ConsoleColor.DarkYellow);
                lock (_lock) _currentHero = null;
            }
        }

        private static void HandleRequestAsync(HttpListenerContext ctx)
        {
            try
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (ctx.Request.HttpMethod == "OPTIONS")
                {
                    ctx.Response.StatusCode = 204;
                    ctx.Response.Close();
                    return;
                }

                string hero;
                lock (_lock) hero = _currentHero;

                Log($"HTTP {ctx.Request.HttpMethod} {ctx.Request.Url.LocalPath}  ->  {(hero ?? "no-hero")}",
                    ConsoleColor.DarkCyan);

                if (string.IsNullOrEmpty(hero))
                {
                    ctx.Response.StatusCode = 204;
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(hero);
                    ctx.Response.ContentType = "text/plain; charset=utf-8";
                    ctx.Response.StatusCode = 200;
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Log($"[HTTP ERROR] {ex.Message}", ConsoleColor.Red);
                ctx.Response.StatusCode = 500;
            }
            finally
            {
                ctx.Response.OutputStream.Close();
            }
        }
        static void Log(string msg, ConsoleColor c = ConsoleColor.Gray)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.ForegroundColor = c;
            Console.WriteLine($"[{time}] {msg}");
            Console.ResetColor();
        }
        private static readonly Dictionary<string, string> HeroNames =
            new(StringComparer.OrdinalIgnoreCase)
            {
        {"npc_dota_hero_abaddon", "Abaddon"},
        {"npc_dota_hero_alchemist", "Alchemist"},
        {"npc_dota_hero_antimage", "Anti-Mage"},
        {"npc_dota_hero_ancient_apparition", "Ancient Apparition"},
        {"npc_dota_hero_arc_warden", "Arc Warden"},
        {"npc_dota_hero_axe", "Axe"},
        {"npc_dota_hero_bane", "Bane"},
        {"npc_dota_hero_batrider", "Batrider"},
        {"npc_dota_hero_beastmaster", "Beastmaster"},
        {"npc_dota_hero_bloodseeker", "Bloodseeker"},
        {"npc_dota_hero_bounty_hunter", "Bounty Hunter"},
        {"npc_dota_hero_brewmaster", "Brewmaster"},
        {"npc_dota_hero_bristleback", "Bristleback"},
        {"npc_dota_hero_broodmother", "Broodmother"},
        {"npc_dota_hero_centaur", "Centaur Warrunner"},
        {"npc_dota_hero_chaos_knight", "Chaos Knight"},
        {"npc_dota_hero_chen", "Chen"},
        {"npc_dota_hero_clinkz", "Clinkz"},
        {"npc_dota_hero_rattletrap", "Clockwerk"},
        {"npc_dota_hero_crystal_maiden", "Crystal Maiden"},
        {"npc_dota_hero_dark_seer", "Dark Seer"},
        {"npc_dota_hero_dark_willow", "Dark Willow"},
        {"npc_dota_hero_dawnbreaker", "Dawnbreaker"},
        {"npc_dota_hero_dazzle", "Dazzle"},
        {"npc_dota_hero_death_prophet", "Death Prophet"},
        {"npc_dota_hero_disruptor", "Disruptor"},
        {"npc_dota_hero_doom_bringer", "Doom"},
        {"npc_dota_hero_dragon_knight", "Dragon Knight"},
        {"npc_dota_hero_drow_ranger", "Drow Ranger"},
        {"npc_dota_hero_earth_spirit", "Earth Spirit"},
        {"npc_dota_hero_earthshaker", "Earthshaker"},
        {"npc_dota_hero_elder_titan", "Elder Titan"},
        {"npc_dota_hero_ember_spirit", "Ember Spirit"},
        {"npc_dota_hero_enchantress", "Enchantress"},
        {"npc_dota_hero_enigma", "Enigma"},
        {"npc_dota_hero_faceless_void", "Faceless Void"},
        {"npc_dota_hero_grimstroke", "Grimstroke"},
        {"npc_dota_hero_gyrocopter", "Gyrocopter"},
        {"npc_dota_hero_hoodwink", "Hoodwink"},
        {"npc_dota_hero_huskar", "Huskar"},
        {"npc_dota_hero_invoker", "Invoker"},
        {"npc_dota_hero_wisp", "Io"},
        {"npc_dota_hero_jakiro", "Jakiro"},
        {"npc_dota_hero_juggernaut", "Juggernaut"},
        {"npc_dota_hero_keeper_of_the_light", "Keeper of the Light"},
        {"npc_dota_hero_kunkka", "Kunkka"},
        {"npc_dota_hero_kez", "Kez"},
        {"npc_dota_hero_legion_commander", "Legion Commander"},
        {"npc_dota_hero_leshrac", "Leshrac"},
        {"npc_dota_hero_lich", "Lich"},
        {"npc_dota_hero_life_stealer", "Lifestealer"},
        {"npc_dota_hero_lina", "Lina"},
        {"npc_dota_hero_lion", "Lion"},
        {"npc_dota_hero_lone_druid", "Lone Druid"},
        {"npc_dota_hero_luna", "Luna"},
        {"npc_dota_hero_lycan", "Lycan"},
        {"npc_dota_hero_magnataur", "Magnus"},
        {"npc_dota_hero_marci", "Marci"},
        {"npc_dota_hero_mars", "Mars"},
        {"npc_dota_hero_medusa", "Medusa"},
        {"npc_dota_hero_meepo", "Meepo"},
        {"npc_dota_hero_mirana", "Mirana"},
        {"npc_dota_hero_morphling", "Morphling"},
        {"npc_dota_hero_monkey_king", "Monkey King"},
        {"npc_dota_hero_naga_siren", "Naga Siren"},
        {"npc_dota_hero_furion", "Nature's Prophet"},
        {"npc_dota_hero_necrolyte", "Necrophos"},
        {"npc_dota_hero_night_stalker", "Night Stalker"},
        {"npc_dota_hero_nyx_assassin", "Nyx Assassin"},
        {"npc_dota_hero_ogre_magi", "Ogre Magi"},
        {"npc_dota_hero_omniknight", "Omniknight"},
        {"npc_dota_hero_oracle", "Oracle"},
        {"npc_dota_hero_obsidian_destroyer", "Outworld Destroyer"},
        {"npc_dota_hero_pangolier", "Pangolier"},
        {"npc_dota_hero_phantom_assassin", "Phantom Assassin"},
        {"npc_dota_hero_phantom_lancer", "Phantom Lancer"},
        {"npc_dota_hero_phoenix", "Phoenix"},
        {"npc_dota_hero_primal_beast", "Primal Beast"},
        {"npc_dota_hero_puck", "Puck"},
        {"npc_dota_hero_pudge", "Pudge"},
        {"npc_dota_hero_pugna", "Pugna"},
        {"npc_dota_hero_queenofpain", "Queen of Pain"},
        {"npc_dota_hero_razor", "Razor"},
        {"npc_dota_hero_riki", "Riki"},
        {"npc_dota_hero_rubick", "Rubick"},
        {"npc_dota_hero_sand_king", "Sand King"},
        {"npc_dota_hero_shadow_demon", "Shadow Demon"},
        {"npc_dota_hero_nevermore", "Shadow Fiend"},
        {"npc_dota_hero_shadow_shaman", "Shadow Shaman"},
        {"npc_dota_hero_silencer", "Silencer"},
        {"npc_dota_hero_skywrath_mage", "Skywrath Mage"},
        {"npc_dota_hero_slardar", "Slardar"},
        {"npc_dota_hero_slark", "Slark"},
        {"npc_dota_hero_snapfire", "Snapfire"},
        {"npc_dota_hero_sniper", "Sniper"},
        {"npc_dota_hero_spectre", "Spectre"},
        {"npc_dota_hero_spirit_breaker", "Spirit Breaker"},
        {"npc_dota_hero_storm_spirit", "Storm Spirit"},
        {"npc_dota_hero_sven", "Sven"},
        {"npc_dota_hero_techies", "Techies"},
        {"npc_dota_hero_templar_assassin", "Templar Assassin"},
        {"npc_dota_hero_terrorblade", "Terrorblade"},
        {"npc_dota_hero_tidehunter", "Tidehunter"},
        {"npc_dota_hero_shredder", "Timbersaw"},
        {"npc_dota_hero_tinker", "Tinker"},
        {"npc_dota_hero_tiny", "Tiny"},
        {"npc_dota_hero_treant", "Treant Protector"},
        {"npc_dota_hero_troll_warlord", "Troll Warlord"},
        {"npc_dota_hero_tusk", "Tusk"},
        {"npc_dota_hero_abyssal_underlord", "Underlord"},
        {"npc_dota_hero_undying", "Undying"},
        {"npc_dota_hero_ursa", "Ursa"},
        {"npc_dota_hero_vengefulspirit", "Vengeful Spirit"},
        {"npc_dota_hero_venomancer", "Venomancer"},
        {"npc_dota_hero_viper", "Viper"},
        {"npc_dota_hero_visage", "Visage"},
        {"npc_dota_hero_void_spirit", "Void Spirit"},
        {"npc_dota_hero_warlock", "Warlock"},
        {"npc_dota_hero_weaver", "Weaver"},
        {"npc_dota_hero_windrunner", "Windranger"},
        {"npc_dota_hero_winter_wyvern", "Winter Wyvern"},
        {"npc_dota_hero_witch_doctor", "Witch Doctor"},
        {"npc_dota_hero_skeleton_king", "Wraith King"},
        {"npc_dota_hero_zuus", "Zeus"}
            };

        private static string PrettyHero(string internalName)
        {
            return HeroNames.TryGetValue(internalName, out var pretty) ? pretty : internalName;
        }

    }
}
