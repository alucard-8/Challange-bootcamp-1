using System.Collections.Generic;
using System.IO;
using Soulbreak.Audio;
using Soulbreak.Core.Events;
using Soulbreak.Economy;
using Soulbreak.MapEvents;
using Soulbreak.Meta;
using Soulbreak.Enemies;
using Soulbreak.Fx;
using Soulbreak.GameFlow;
using Soulbreak.Orbs;
using Soulbreak.Perks;
using Soulbreak.Player;
using Soulbreak.Reactor;
using Soulbreak.UI;
using Soulbreak.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Soulbreak.EditorTools
{
    // Editor-only: builds every asset (sprite, materials, enemies, perks, post-processing)
    // and wires the scene. Safe to run again — it updates existing assets in place.
    public static class SoulbreakSetup
    {
        private const string Art = "Assets/Art";
        private const string Mats = "Assets/Art/Materials";
        private const string EnemyDir = "Assets/Data/Enemies";
        private const string PerkDir = "Assets/Data/Perks";

        private const string GameScenePath = "Assets/Scenes/SampleScene.unity";
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

        // Builds the game scene, then the main menu scene, then registers both in Build Settings.
        [MenuItem("Soulbreak/Setup Everything")]
        public static void RunAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(GameScenePath);
            Run();
            BuildMainMenu();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true),
            };
            Debug.Log("SOULBREAK: everything set up. Main menu is scene 0.");
        }

        [MenuItem("Soulbreak/Setup Scene")]
        public static void Run()
        {
            UIThemeAsset();
            pauseProfile = PauseProfileAsset();
            Folder("Assets", "Art"); Folder(Art, "Materials"); Folder("Assets", "Data");
            Folder("Assets/Data", "Enemies"); Folder("Assets/Data", "Perks");

            var quad = SquareSprite();
            var orbMat = Mat("Orb", "Soulbreak/Orb");
            var ringMat = Mat("Ring", "Soulbreak/Ring");
            var enemyMat = Mat("Enemy", "Soulbreak/Enemy");
            var reactorMat = Mat("Reactor", "Soulbreak/Reactor");
            var tracerMat = Mat("Tracer", "Soulbreak/Tracer");
            var bgMat = Mat("Background", "Soulbreak/Background");

            segmentMat = Mat("RingSegment", "Soulbreak/RingSegment");
            needleMat = tracerMat;
            Shots(quad, orbMat);
            SetReloadTimes();
            Techniques(quad, orbMat);
            Channels();

            var roster = Enemies(quad, enemyMat, orbMat, out var boss);
            var perks = Perks();
            BuildModes(quad, orbMat, roster, boss, perks, reactorMat, bgMat, enemyMat);
            var volumeProfile = VolumeProfileAsset();
            AssetDatabase.SaveAssets();

            BuildScene(quad, orbMat, ringMat, enemyMat, reactorMat, tracerMat, bgMat, roster, boss, perks, volumeProfile);
            Debug.Log("SOULBREAK: scene setup complete.");
        }

        // ---------------- Main menu scene ----------------
        [MenuItem("Soulbreak/Setup Main Menu")]
        public static void BuildMainMenu()
        {
            var quad = SquareSprite();
            var mats = new
            {
                orb = Mat("Orb", "Soulbreak/Orb"), ring = Mat("Ring", "Soulbreak/Ring"), enemy = Mat("Enemy", "Soulbreak/Enemy"),
                reactor = Mat("Reactor", "Soulbreak/Reactor"), tracer = Mat("Tracer", "Soulbreak/Tracer"),
                bg = Mat("Background", "Soulbreak/Background"), seg = Mat("RingSegment", "Soulbreak/RingSegment"),
            };
            UIThemeAsset();
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SoulbreakVolume.asset") ?? VolumeProfileAsset();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(UniversalAdditionalCameraData));
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.015f, 0.05f);
            camGo.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;

            var bg = new GameObject("Background").AddComponent<SpriteRenderer>();
            bg.sprite = quad; bg.sharedMaterial = mats.bg; bg.sortingOrder = -100;
            bg.transform.localScale = new Vector3(44f, 26f, 1f);

            var vol = new GameObject("Global Volume").AddComponent<Volume>();
            vol.isGlobal = true; vol.sharedProfile = profile;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var reactor = new GameObject("Reactor").AddComponent<SpriteRenderer>();
            reactor.transform.position = new Vector3(0, 0.1f, 0);
            reactor.transform.localScale = Vector3.one * 2.3f;
            reactor.sprite = quad; reactor.sharedMaterial = mats.reactor; reactor.sortingOrder = 1;

            var amb = new GameObject("MenuAmbience").AddComponent<Soulbreak.Menu.MenuAmbience>();
            Set(amb, "reactor", reactor.transform); Set(amb, "cam", cam); Set(amb, "quad", quad);
            Set(amb, "enemyMaterial", mats.enemy); Set(amb, "segmentMaterial", mats.seg); Set(amb, "ringMaterial", mats.ring);
            Set(amb, "orbMaterial", mats.orb); Set(amb, "tracerMaterial", mats.tracer);
            var ambSo = new SerializedObject(amb); ambSo.FindProperty("ringDiameter").floatValue = 4.1f; ambSo.ApplyModifiedPropertiesWithoutUndo();

            var view = UIPrefab<Soulbreak.Menu.MainMenuView>("MainMenu");
            Set(view, "ambience", amb);
            var menuInput = view.gameObject.AddComponent<Soulbreak.Menu.MainMenuInput>();
            Set(menuInput, "view", view); Set(menuInput, "ambience", amb); Set(menuInput, "catalog", MetaCatalogAsset());

            var ascension = UIPrefab<Soulbreak.Menu.AscensionView>("Ascension");
            Set(ascension, "menuReactor", reactor);
            Set(view, "ascension", ascension);

            camGo.AddComponent<AudioListener>();

            // Mode theming + the secret that unlocks Star Core mode
            var modeApplier = view.gameObject.AddComponent<Soulbreak.Menu.MenuModeApplier>();
            Set(modeApplier, "core", reactor); Set(modeApplier, "background", bg); Set(modeApplier, "ambience", amb);
            Set(modeApplier, "view", view);
            var egg = view.gameObject.AddComponent<Soulbreak.Menu.ReactorEasterEgg>();
            Set(egg, "core", reactor.transform); Set(egg, "view", view);
            Set(menuInput, "easterEgg", egg);

            var menuSettings = UIPrefab<SettingsView>("Settings");
            var difficultyView = UIPrefab<Soulbreak.Menu.DifficultyView>("Difficulty");
            Set(difficultyView, "menu", view); Set(view, "difficulty", difficultyView);
            Set(view, "settings", menuSettings);

            var index = UIPrefab<Soulbreak.Menu.IndexView>("Index");
            Set(view, "index", index); Set(menuInput, "index", index); Set(menuInput, "codex", CodexAsset());
            var menuHowTo = UIPrefab<HowToView>("HowTo");
            Set(view, "howToView", menuHowTo);
            UIPrefab<LeaderboardPanel>("Leaderboard");   // slide-out global leaderboard (right edge)
            Set(menuInput, "account", UIPrefab<AccountView>("Account"));   // account button + login popup

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        // ---------------- Game modes ----------------
        private static void BuildModes(Sprite quad, Material orbMat, WaveEnemy[] reactorRoster, EnemyConfig reactorBoss,
                                       PerkDefinition[] reactorPerks, Material reactorMat, Material bgMat, Material enemyMat)
        {
            Folder("Assets/Data", "Modes"); Folder("Assets/Data", "Movement"); Folder("Assets/Data", "EnemiesStar");
            Folder("Assets", "Resources");

            var starMat = Mat("Star", "Soulbreak/Star");
            var planetMat = Mat("Planet", "Soulbreak/Planet");
            var spaceMat = Mat("SpaceBackground", "Soulbreak/SpaceBackground");
            var vortexMat = Mat("Vortex", "Soulbreak/Vortex");

            T Asset<T>(string path) where T : ScriptableObject
            {
                var a = AssetDatabase.LoadAssetAtPath<T>(path);
                if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
                EditorUtility.SetDirty(a);
                return a;
            }

            // Movement strategies
            var spiral = Asset<SpiralMovement>("Assets/Data/Movement/Spiral.asset");
            spiral.tangential = 1.1f; spiral.radial = 0.5f;
            var orbiter = Asset<SpiralMovement>("Assets/Data/Movement/WideOrbit.asset");
            orbiter.tangential = 0.7f; orbiter.radial = 0.65f;
            var comet = Asset<CometMovement>("Assets/Data/Movement/Comet.asset");

            // Star-mode enemies (planet shader; _Spikes = surface type: 0-3 rocky, 4-7 gas, 8+ icy)
            EnemyConfig P(string name, string display, Color c, float size, int hp, float speed, int dmg, int souls,
                          float orbChance, int orbValue, float heal, float surface, MovementPattern move)
            {
                var e = Asset<EnemyConfig>($"Assets/Data/EnemiesStar/{name}.asset");
                e.displayName = display; e.sprite = quad; e.material = planetMat; e.color = c; e.size = size; e.spikes = surface;
                e.maxHealth = hp; e.speed = speed; e.damage = dmg; e.isBoss = false; e.movement = move; e.gravityPull = 0f;
                e.stopDistance = 0f; e.splitInto = null; e.splitCount = 0; e.projectileMaterial = orbMat; e.minion = null;
                e.soulReward = souls; e.soulOrbChance = orbChance; e.soulOrbValue = orbValue; e.healOrbChance = heal;
                return e;
            }

            var asteroid = P("Asteroid", "Asteroid", new Color(1.3f, 1.0f, 0.8f), 1.0f, 1, 1.5f, 1, 5, 0.2f, 12, 0.03f, 1, null);
            var rocky = P("RockyPlanet", "Rocky Planet", new Color(2.4f, 1.1f, 0.6f), 1.3f, 2, 1.2f, 1, 7, 0.25f, 14, 0.04f, 2, spiral);
            var cometCfg = P("Comet", "Comet", new Color(0.9f, 2.2f, 3.4f), 0.85f, 1, 2.2f, 1, 8, 0.3f, 15, 0.04f, 9, comet);
            var iceGiant = P("IceGiant", "Ice Giant", new Color(0.6f, 1.6f, 3f), 2.0f, 6, 0.75f, 2, 22, 0.6f, 30, 0.15f, 10, spiral);
            var moonlet = P("Moonlet", "Moonlet", new Color(1.6f, 1.5f, 1.4f), 0.65f, 1, 2.0f, 1, 2, 0.05f, 8, 0.01f, 3, spiral);
            var carrier = P("MoonCarrier", "Moon Carrier", new Color(2f, 1.6f, 1.1f), 1.6f, 3, 1.0f, 1, 10, 0.3f, 15, 0.05f, 0, spiral);
            carrier.splitInto = moonlet; carrier.splitCount = 3;
            var gasGiant = P("GasGiant", "Gas Giant", new Color(2.8f, 1.7f, 0.7f), 1.9f, 4, 0.9f, 1, 14, 0.4f, 20, 0.05f, 5, orbiter);
            gasGiant.stopDistance = 4.8f; gasGiant.fireInterval = 2.2f; gasGiant.projectileDamage = 1; gasGiant.projectileSpeed = 3f;
            var blackHole = P("BlackHole", "The Devourer", new Color(2.2f, 0.6f, 4.5f), 3.4f, 45, 0.35f, 6, 160, 1f, 40, 1f, 0, null);
            blackHole.isBoss = true; blackHole.material = vortexMat; blackHole.gravityPull = 1.6f; blackHole.minion = asteroid;
            var protostar = P("Protostar", "Protostar", new Color(0.6f, 3.2f, 1.6f), 2.1f, 25, 0.35f, 3, 40, 1f, 30, 0.5f, 9, spiral);

            var starRoster = new[]
            {
                new WaveEnemy { config = rocky, firstWave = 1, weight = 8 },
                new WaveEnemy { config = asteroid, firstWave = 1, weight = 6 },
                new WaveEnemy { config = cometCfg, firstWave = 2, weight = 5 },
                new WaveEnemy { config = iceGiant, firstWave = 3, weight = 3 },
                new WaveEnemy { config = carrier, firstWave = 4, weight = 3 },
                new WaveEnemy { config = gasGiant, firstWave = 5, weight = 3 },
            };

            // Star-mode perks: same effects/costs as the reactor perks, renamed for space.
            var starPerks = StarPerks(reactorPerks);

            LexiconEntry W(string k, string v) => new LexiconEntry { key = k, text = v };

            var reactorMode = Asset<ModeProfile>("Assets/Data/Modes/SoulReactor.asset");
            reactorMode.id = GameModeService.DefaultMode; reactorMode.displayName = "Soul Reactor";
            reactorMode.title = "SOULBREAK"; reactorMode.tagline = "THE  REACTOR  MUST  NOT  FALL";
            reactorMode.roster = reactorRoster; reactorMode.boss = reactorBoss; reactorMode.eliteSplitInto = gruntConfig;
            reactorMode.rejuvenator = rejuvenatorConfig; reactorMode.catalog = reactorPerks;
            reactorMode.coreMaterial = reactorMat; reactorMode.backgroundMaterial = bgMat; reactorMode.enemyMaterial = enemyMat;
            reactorMode.soulOrbColor = new Color(1.6f, 0.7f, 3f); reactorMode.healOrbColor = new Color(0.6f, 3f, 1.2f);
            reactorMode.musicVariant = 0;
            reactorMode.words = new LexiconEntry[0];

            var starMode = Asset<ModeProfile>("Assets/Data/Modes/StarCore.asset");
            starMode.id = GameModeService.StarMode; starMode.displayName = "Star Core";
            starMode.title = "STARBREAK"; starMode.tagline = "BURN  BRIGHTER  THAN  THEIR  GRAVITY";
            starMode.roster = starRoster; starMode.boss = blackHole; starMode.eliteSplitInto = moonlet;
            starMode.rejuvenator = protostar; starMode.catalog = starPerks;
            starMode.coreMaterial = starMat; starMode.backgroundMaterial = spaceMat; starMode.enemyMaterial = planetMat;
            starMode.soulOrbColor = new Color(3f, 2.4f, 1f); starMode.healOrbColor = new Color(0.6f, 2.6f, 3.2f);
            starMode.musicVariant = 1;
            starMode.words = new[]
            {
                W("core", "STAR"), W("shield", "FIELD"), W("souls", "STARDUST"), W("souls_banked", "Stardust gathered"),
                W("nova", "SUPERNOVA"), W("shop_title", "STAR FORGE"),
                W("shop_sub", "Forge stardust into power. Press 1-5 or click to buy."),
                W("gameover", "STAR COLLAPSED"), W("defend", "Hold back the planets"),
                W("pause_sub", "THE COSMOS HOLDS ITS BREATH"),
                W("Triple Shot", "Solar Wind"), W("Soul Cannon", "Plasma Bomb"), W("Seeker Swarm", "Meteor Swarm"),
                W("Chain Lightning", "Ion Arc"), W("Piercing Lance", "Solar Flare"), W("Orbital Blades", "Orbiting Moons"),
                W("Black Hole", "Gravity Well"),
            };

            var library = Asset<ModeLibrary>("Assets/Resources/ModeLibrary.asset");
            library.modes = new[] { reactorMode, starMode };
        }

        private static PerkDefinition[] StarPerks(PerkDefinition[] source)
        {
            const string dir = "Assets/Data/PerksStar";
            Folder("Assets/Data", "PerksStar");
            foreach (var guid in AssetDatabase.FindAssets("", new[] { dir }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var names = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Sharpened Souls", "Hot Plasma" }, { "Critical Eye", "Sunspot Focus" }, { "Soul Rend", "Fusion Burst" },
                { "Shrapnel", "Solar Shrapnel" }, { "Extended Clip", "Bigger Corona" }, { "Quick Hands", "Fast Fusion" },
                { "Overcharged Rounds", "Hypergiant Rounds" }, { "Soul Hunger", "Stellar Hunger" }, { "Rich Orbs", "Rich Stardust" },
                { "Lingering Orbs", "Lingering Nebula" }, { "Lucky Drops", "Lucky Comets" }, { "Momentum", "Orbital Momentum" },
                { "Soul Bank", "Stellar Nursery" }, { "Patch Up", "Fuse Hydrogen" }, { "Recharge", "Recharge Field" },
                { "Reinforced Core", "Dense Core" }, { "Capacitor", "Magnetic Pole" }, { "Healing Light", "Healing Aurora" },
                { "Nova Overcharge", "Supernova Overcharge" }, { "Wide Nova", "Wide Supernova" },
                { "Triple Shot Module", "Solar Wind" }, { "Soul Cannon Module", "Plasma Bomb" }, { "Seeker Swarm Module", "Meteor Swarm" },
                { "Chain Lightning", "Ion Arc" }, { "Piercing Lance", "Solar Flare" }, { "Orbital Blades", "Orbiting Moons" },
                { "Black Hole", "Gravity Well" }, { "Soul Reaper", "Big Bang" },
            };

            string Spacey(string s) => s.Replace("souls", "stardust").Replace("Soul ", "Star ").Replace("soul", "stardust")
                                        .Replace("reactor", "star").Replace("core health", "star mass").Replace("orbs", "stardust clouds")
                                        .Replace("Nova", "Supernova").Replace("blades", "moons").Replace("Black Hole", "Gravity Well");

            var result = new PerkDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                var src = source[i];
                var p = Object.Instantiate(src);
                p.displayName = names.TryGetValue(src.displayName, out var n) ? n : src.displayName;
                p.description = Spacey(src.description);
                p.effects = src.effects;   // share the same effect strategies
                AssetDatabase.CreateAsset(p, $"{dir}/{p.displayName.Replace(" ", "")}.asset");
                result[i] = p;
            }
            return result;
        }

        private static VolumeProfile pauseProfile;

        // Purple "Overdrive" look: tinted, saturated, extra bloom and chromatic split.
        private static VolumeProfile OverdriveProfileAsset()
        {
            const string path = "Assets/Settings/OverdriveVolume.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var color = profile.Add<ColorAdjustments>(true);
            color.colorFilter.Override(new Color(1f, 0.75f, 1.15f));
            color.saturation.Override(25f);
            color.contrast.Override(12f);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(2.8f);
            var ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0.55f);
            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.45f);
            vig.color.Override(new Color(0.35f, 0f, 0.5f));
            foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static SynergyDefinition[] Synergies()
        {
            Folder("Assets/Data", "Synergies");
            SynergyDefinition S(string name, string desc, ShotPattern a, ShotPattern b, ShotPattern trigger, ShotPattern bonus, float mult)
            {
                string path = $"Assets/Data/Synergies/{name.Replace(" ", "")}.asset";
                var s = AssetDatabase.LoadAssetAtPath<SynergyDefinition>(path);
                if (s == null) { s = ScriptableObject.CreateInstance<SynergyDefinition>(); AssetDatabase.CreateAsset(s, path); }
                s.displayName = name; s.description = desc; s.moduleA = a; s.moduleB = b; s.trigger = trigger; s.bonus = bonus;
                s.bonusDamageMultiplier = mult;
                EditorUtility.SetDirty(s);
                return s;
            }

            return new[]
            {
                S("Event Horizon", "Black Holes unleash chain lightning", vortexShot, chainShot, vortexShot, chainShot, 1.2f),
                S("Thunder Cannon", "Soul Cannon blasts also chain lightning", cannonShot, chainShot, cannonShot, chainShot, 1f),
                S("Swarm Volley", "Triple Shot also releases seekers", tripleShot, seekerShot, tripleShot, seekerShot, 0.8f),
                S("Blade Lance", "Piercing Lance also summons orbital blades", lanceShot, bladesShot, lanceShot, bladesShot, 0.8f),
                S("Seeking Storm", "Seekers come with a lance strike", seekerShot, lanceShot, seekerShot, lanceShot, 0.7f),
            };
        }

        // Index contents: every enemy and perk asset in the project (bosses last, perks by rarity).
        private static CodexCatalog CodexAsset()
        {
            const string path = "Assets/Data/CodexCatalog.asset";
            var codex = AssetDatabase.LoadAssetAtPath<CodexCatalog>(path);
            if (codex == null) { codex = ScriptableObject.CreateInstance<CodexCatalog>(); AssetDatabase.CreateAsset(codex, path); }

            List<T> All<T>(string filter) where T : Object
            {
                var list = new List<T>();
                foreach (var guid in AssetDatabase.FindAssets(filter, new[] { "Assets/Data" }))
                {
                    var a = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                    if (a != null) list.Add(a);
                }
                return list;
            }

            var enemies = All<EnemyConfig>("t:EnemyConfig");
            enemies.Sort((a, b) => a.isBoss != b.isBoss ? a.isBoss.CompareTo(b.isBoss) : a.maxHealth.CompareTo(b.maxHealth));
            var perks = All<PerkDefinition>("t:PerkDefinition");
            perks.Sort((a, b) => a.rarity != b.rarity ? a.rarity.CompareTo(b.rarity) : string.CompareOrdinal(a.displayName, b.displayName));
            codex.enemies = enemies.ToArray();
            codex.perks = perks.ToArray();
            EditorUtility.SetDirty(codex);
            return codex;
        }

        private static MetaCatalog MetaCatalogAsset()
        {
            Folder("Assets", "Resources");
            const string path = "Assets/Data/MetaCatalog.asset";
            var cat = AssetDatabase.LoadAssetAtPath<MetaCatalog>(path);
            if (tripleShot == null && cat != null) return cat;   // menu-only rebuild: keep the existing catalog
            if (cat == null) { cat = ScriptableObject.CreateInstance<MetaCatalog>(); AssetDatabase.CreateAsset(cat, path); }

            MetaItem T(string id, string name, string desc, int cost, ShotPattern p) =>
                new MetaItem { id = id, displayName = name, description = desc, cost = cost, type = MetaItemType.StartingTechnique, technique = p };
            MetaItem K(string id, string name, string desc, int cost, Color core, Color shield) =>
                new MetaItem { id = id, displayName = name, description = desc, cost = cost, type = MetaItemType.ReactorSkin, coreColor = core, shieldColor = shield };

            cat.items = new[]
            {
                T("start_triple", "Triple Shot", "Start every run with a Triple Shot module.", 60, tripleShot),
                T("start_seeker", "Seeker Swarm", "Start with homing seekers.", 100, seekerShot),
                T("start_cannon", "Soul Cannon", "Start with the explosive Soul Cannon.", 140, cannonShot),
                T("start_chain", "Chain Lightning", "Start with Chain Lightning.", 200, chainShot),
                T("start_lance", "Piercing Lance", "Start with the Piercing Lance.", 260, lanceShot),
                T("start_vortex", "Black Hole", "Start with a Black Hole.", 350, vortexShot),
                K("skin_azure", "Azure Core", "The original soul reactor.", 0, new Color(0.6f, 1.6f, 3f), new Color(0.5f, 2.5f, 3f)),
                K("skin_crimson", "Crimson Heart", "A furious red core.", 120, new Color(3.2f, 0.5f, 0.6f), new Color(3f, 1.2f, 0.5f)),
                K("skin_gold", "Gilded Sun", "Molten gold and white light.", 220, new Color(3.2f, 2.2f, 0.6f), new Color(3.4f, 3f, 1.6f)),
                K("skin_void", "Void Bloom", "Deep violet void energy.", 350, new Color(1.8f, 0.5f, 3.6f), new Color(0.9f, 2.8f, 2.4f)),
                K("skin_toxic", "Toxic Spring", "Radioactive green.", 300, new Color(0.7f, 3.4f, 0.9f), new Color(2.2f, 3.4f, 0.6f)),
                new MetaItem
                {
                    id = Soulbreak.Menu.ReactorEasterEgg.SecretId, displayName = "STAR CORE MODE", type = MetaItemType.GameMode,
                    modeId = GameModeService.StarMode, cost = 0,
                    description = "Become a newborn star. Planets spiral in, comets slingshot, a black hole hunts you.",
                    unlockHint = "Something stirs inside the core... click it. A lot. Fast.",
                },
            };
            EditorUtility.SetDirty(cat);
            return cat;
        }

        // Heavy desaturated, vignetted, warped look blended in while paused.
        private static VolumeProfile PauseProfileAsset()
        {
            const string path = "Assets/Settings/PauseVolume.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(-85f);
            color.postExposure.Override(-0.5f);
            color.contrast.Override(15f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.6f);
            vignette.color.Override(new Color(0.08f, 0f, 0.15f));
            var ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0.8f);
            var lens = profile.Add<LensDistortion>(true);
            lens.intensity.Override(-0.35f);
            lens.scale.Override(1.05f);

            foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void UIThemeAsset()
        {
            Folder("Assets", "Resources");
            var mat = Mat("UIPanel", "Soulbreak/UIPanel");
            const string path = "Assets/Resources/UITheme.asset";
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>(path);
            if (theme == null) { theme = ScriptableObject.CreateInstance<UITheme>(); AssetDatabase.CreateAsset(theme, path); }
            theme.panelMaterial = mat;
            theme.cardMaterial = Mat("UICard", "Soulbreak/UICard");
            theme.previewMaterial = Mat("UIPreview", "Soulbreak/UIPreview");
            EditorUtility.SetDirty(theme);
        }

        // ---------------- Assets ----------------
        private static Material segmentMat, needleMat;
        private static ShotPattern basicShot;
        private static ProjectileShotPattern tripleShot, cannonShot, seekerShot;
        private static ChainShotPattern chainShot;
        private static BeamShotPattern lanceShot;
        private static OrbitShotPattern bladesShot;
        private static VortexShotPattern vortexShot;

        private static void Shots(Sprite quad, Material orbMat)
        {
            Folder("Assets/Data", "Weapons");

            T Load<T>(string name) where T : ScriptableObject
            {
                string path = $"Assets/Data/Weapons/{name}.asset";
                var a = AssetDatabase.LoadAssetAtPath<T>(path);
                if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
                EditorUtility.SetDirty(a);
                return a;
            }

            var basic = Load<HitscanShotPattern>("BasicShot");
            basic.displayName = "Soul Laser"; basic.icon = 0; basic.drawsTracer = true;
            basic.color = new Color(1.2f, 2.2f, 4f);
            basicShot = basic;

            ProjectileShotPattern P(string name, string display, int icon, Color color, int count, float spread, float speed,
                                    float size, float dmg, float range, bool travel, float explode, bool homing)
            {
                var p = Load<ProjectileShotPattern>(name);
                p.displayName = display; p.icon = icon; p.color = color; p.drawsTracer = false;
                p.sprite = quad; p.material = orbMat;
                p.count = count; p.spreadAngle = spread; p.speed = speed; p.size = size; p.damageMultiplier = dmg;
                p.range = range; p.travelToTarget = travel; p.explosionRadius = explode; p.homing = homing; p.turnSpeed = 420f;
                return p;
            }

            tripleShot = P("TripleShot", "Triple Shot", 1, new Color(0.6f, 3f, 1.4f), 3, 16f, 18f, 0.45f, 1f, 18f, false, 0f, false);
            cannonShot = P("SoulCannon", "Soul Cannon", 2, new Color(2.4f, 0.9f, 4f), 1, 0f, 8f, 1.3f, 3f, 30f, true, 1.8f, false);
            seekerShot = P("SeekerSwarm", "Seeker Swarm", 3, new Color(3.2f, 2.2f, 0.5f), 3, 70f, 10f, 0.45f, 1f, 25f, false, 0f, true);
        }

        private static void Techniques(Sprite quad, Material orbMat)
        {
            T Load<T>(string name) where T : ShotPattern
            {
                string path = $"Assets/Data/Weapons/{name}.asset";
                var a = AssetDatabase.LoadAssetAtPath<T>(path);
                if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
                EditorUtility.SetDirty(a);
                return a;
            }

            chainShot = Load<ChainShotPattern>("ChainLightning");
            chainShot.displayName = "Chain Lightning"; chainShot.icon = 4; chainShot.color = new Color(1.2f, 2.4f, 4.5f);
            chainShot.reloadTime = 3.5f;

            lanceShot = Load<BeamShotPattern>("PiercingLance");
            lanceShot.displayName = "Piercing Lance"; lanceShot.icon = 5; lanceShot.color = new Color(4f, 1.2f, 1.6f);
            lanceShot.reloadTime = 4f;

            bladesShot = Load<OrbitShotPattern>("OrbitalBlades");
            bladesShot.displayName = "Orbital Blades"; bladesShot.icon = 6; bladesShot.color = new Color(0.8f, 4f, 3f);
            bladesShot.reloadTime = 7f; bladesShot.sprite = quad; bladesShot.material = orbMat;

            vortexShot = Load<VortexShotPattern>("BlackHole");
            vortexShot.displayName = "Black Hole"; vortexShot.icon = 7; vortexShot.color = new Color(2.2f, 0.6f, 4.5f);
            vortexShot.reloadTime = 8f; vortexShot.sprite = quad; vortexShot.material = Mat("Vortex", "Soulbreak/Vortex");
        }

        // ---------------- Event channels ----------------
        private class ChannelSet
        {
            public EnemyEventChannel enemySpawned, enemyKilled, enemyReached;
            public EnemyProjectileEventChannel projectileShotDown;
            public OrbEventChannel orbCollected;
            public ExplosionEventChannel explosions;
            public ChainEventChannel chains;
            public BeamEventChannel beams;
            public Vector2EventChannel headshots;
            public EnemyEventChannel bossEnraged;
            public AnnouncementEventChannel announcements;
            public EnemyDamageEventChannel enemyDamaged;
        }

        private static ChannelSet ch;

        private static T Channel<T>(string name, string description) where T : ScriptableObject
        {
            Folder("Assets/Data", "Events");
            string path = $"Assets/Data/Events/{name}.asset";
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
            var so = new SerializedObject(a);
            so.FindProperty("description").stringValue = description;
            so.ApplyModifiedPropertiesWithoutUndo();
            return a;
        }

        private static void Channels()
        {
            ch = new ChannelSet
            {
                enemySpawned = Channel<EnemyEventChannel>("EnemySpawned", "Raised by an enemy when it is created."),
                enemyKilled = Channel<EnemyEventChannel>("EnemyKilled", "Raised by an enemy when its health reaches 0."),
                enemyReached = Channel<EnemyEventChannel>("EnemyReachedReactor", "Raised by an enemy when it hits the reactor."),
                projectileShotDown = Channel<EnemyProjectileEventChannel>("EnemyProjectileShotDown", "Raised when the player shoots down an enemy bullet."),
                orbCollected = Channel<OrbEventChannel>("OrbCollected", "Raised when the player shoots an orb."),
                explosions = Channel<ExplosionEventChannel>("Explosions", "Raised by explosive weapons (cannon, black hole)."),
                chains = Channel<ChainEventChannel>("ChainLightning", "Raised by Chain Lightning with the points it jumped through."),
                beams = Channel<BeamEventChannel>("Beams", "Raised by the Piercing Lance."),
                headshots = Channel<Vector2EventChannel>("Headshots", "Raised when a shot hits an enemy's weak point (eye)."),
                bossEnraged = Channel<EnemyEventChannel>("BossEnraged", "Raised when a boss drops below 50% health and enters phase 2."),
                announcements = Channel<AnnouncementEventChannel>("Announcements", "Arena announcer messages (streaks, synergies, map events)."),
                enemyDamaged = Channel<EnemyDamageEventChannel>("EnemyDamaged", "Raised on every hit an enemy takes (damage numbers, sparks)."),
            };

            tripleShot.explodedChannel = ch.explosions;
            cannonShot.explodedChannel = ch.explosions;
            seekerShot.explodedChannel = ch.explosions;
            vortexShot.explodedChannel = ch.explosions;
            chainShot.chainChannel = ch.chains;
            ((HitscanShotPattern)basicShot).headshotChannel = ch.headshots;
            EditorUtility.SetDirty(basicShot);
            lanceShot.beamChannel = ch.beams;
            foreach (var s in new ShotPattern[] { tripleShot, cannonShot, seekerShot, vortexShot, chainShot, lanceShot })
                EditorUtility.SetDirty(s);
        }

        private static void SetReloadTimes()
        {
            tripleShot.reloadTime = 2.5f;
            cannonShot.reloadTime = 5f;
            seekerShot.reloadTime = 4f;
            EditorUtility.SetDirty(tripleShot); EditorUtility.SetDirty(cannonShot); EditorUtility.SetDirty(seekerShot);
        }

        private static void Folder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
        }

        private static Sprite SquareSprite()
        {
            string path = $"{Art}/Square.png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                var px = new Color32[64 * 64];
                for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Material Mat(string name, string shaderName)
        {
            string path = $"{Mats}/{name}.mat";
            var shader = Shader.Find(shaderName);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else mat.shader = shader;
            return mat;
        }

        private static EnemyConfig gruntConfig, rejuvenatorConfig;

        private static WaveEnemy[] Enemies(Sprite quad, Material enemyMat, Material orbMat, out EnemyConfig boss)
        {
            EnemyConfig E(string name, Color c, float size, int hp, float speed, int dmg, int souls,
                          float orbChance, int orbValue, float healChance, float spikes)
            {
                string path = $"{EnemyDir}/{name}.asset";
                var e = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
                if (e == null) { e = ScriptableObject.CreateInstance<EnemyConfig>(); AssetDatabase.CreateAsset(e, path); }
                e.sprite = quad; e.material = enemyMat; e.color = c; e.size = size; e.spikes = spikes;
                e.maxHealth = hp; e.speed = speed; e.damage = dmg; e.isBoss = false;
                e.stopDistance = 0f; e.splitInto = null; e.splitCount = 0; e.projectileMaterial = orbMat;
                e.soulReward = souls; e.soulOrbChance = orbChance; e.soulOrbValue = orbValue; e.healOrbChance = healChance;
                EditorUtility.SetDirty(e);
                return e;
            }

            var grunt = E("Grunt", new Color(2.6f, 0.55f, 0.6f), 1.25f, 1, 1.3f, 1, 5, 0.2f, 12, 0.03f, 6);
            var runner = E("Runner", new Color(3f, 2f, 0.4f), 1.0f, 1, 2.6f, 1, 7, 0.25f, 15, 0.04f, 4);
            var brute = E("Brute", new Color(2f, 0.5f, 2.6f), 2.0f, 5, 0.8f, 2, 20, 0.6f, 30, 0.15f, 9);
            var splitling = E("Splitling", new Color(0.6f, 2.6f, 1f), 0.8f, 1, 2.0f, 1, 2, 0.05f, 8, 0.01f, 5);
            var splitter = E("Splitter", new Color(0.6f, 2.6f, 1f), 1.6f, 3, 1.0f, 1, 10, 0.3f, 15, 0.05f, 5);
            splitter.splitInto = splitling; splitter.splitCount = 3;
            var spitter = E("Spitter", new Color(2.6f, 1.4f, 0.3f), 1.4f, 2, 1.1f, 1, 12, 0.4f, 20, 0.05f, 3);
            spitter.stopDistance = 4.5f; spitter.fireInterval = 2.2f; spitter.projectileDamage = 1; spitter.projectileSpeed = 3f;
            boss = E("SoulTyrant", new Color(3f, 0.4f, 1.5f), 3.6f, 40, 0.45f, 5, 150, 1f, 40, 1f, 12);
            boss.isBoss = true;
            boss.displayName = "Soul Tyrant";
            boss.minion = grunt;
            gruntConfig = grunt;

            rejuvenatorConfig = E("Rejuvenator", new Color(0.5f, 3.2f, 1.4f), 2.2f, 25, 0.35f, 3, 40, 1f, 30, 0.5f, 8);
            rejuvenatorConfig.displayName = "Rejuvenator";

            return new[]
            {
                new WaveEnemy { config = grunt, firstWave = 1, weight = 10 },
                new WaveEnemy { config = runner, firstWave = 2, weight = 6 },
                new WaveEnemy { config = brute, firstWave = 3, weight = 3 },
                new WaveEnemy { config = splitter, firstWave = 4, weight = 3 },
                new WaveEnemy { config = spitter, firstWave = 5, weight = 3 },
            };
        }

        private static PerkDefinition[] Perks()
        {
            // Remove old assets so the catalog is exactly what's listed below.
            foreach (var guid in AssetDatabase.FindAssets("", new[] { PerkDir }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            PerkDefinition P(string name, string desc, PerkCategory cat, PerkRarity rarity, int cost, int growth, int max,
                             params PerkEffect[] effects)
            {
                var p = ScriptableObject.CreateInstance<PerkDefinition>();
                p.displayName = name; p.description = desc; p.category = cat; p.rarity = rarity;
                p.baseCost = cost; p.costGrowth = growth; p.maxStacks = max; p.effects = effects;
                AssetDatabase.CreateAsset(p, $"{PerkDir}/{name.Replace(" ", "")}.asset");
                foreach (var e in effects)
                {
                    e.name = $"{name} - {e.GetType().Name}";
                    AssetDatabase.AddObjectToAsset(e, p);
                }
                return p;
            }

            StatPerkEffect S(StatType stat, float value, StatOperation op = StatOperation.Add)
            {
                var e = ScriptableObject.CreateInstance<StatPerkEffect>();
                e.stat = stat; e.value = value; e.operation = op;
                return e;
            }

            ReactorPerkEffect R(ReactorAction action, int amount)
            {
                var e = ScriptableObject.CreateInstance<ReactorPerkEffect>();
                e.action = action; e.amount = amount;
                return e;
            }

            WeaponModulePerkEffect W(ShotPattern module)
            {
                var e = ScriptableObject.CreateInstance<WeaponModulePerkEffect>();
                e.module = module; e.slots = 1;
                return e;
            }

            const PerkCategory Wp = PerkCategory.Weapon;
            const PerkCategory O = PerkCategory.Offense, Ec = PerkCategory.Economy, D = PerkCategory.Defense, A = PerkCategory.Ability;
            const PerkRarity C = PerkRarity.Common, Ra = PerkRarity.Rare, Ep = PerkRarity.Epic, L = PerkRarity.Legendary;

            return new[]
            {
                // Offense
                P("Sharpened Souls", "+1 damage per shot.", O, C, 60, 40, 0, S(StatType.Damage, 1)),
                P("Critical Eye", "+10% critical hit chance.", O, C, 50, 30, 5, S(StatType.CritChance, 0.1f)),
                P("Soul Rend", "Critical hits deal +75% more damage.", O, Ra, 80, 50, 4, S(StatType.CritMultiplier, 0.75f)),
                P("Shrapnel", "Shots explode and hit everything in a small radius.", O, Ra, 90, 60, 4, S(StatType.SplashRadius, 0.35f)),
                P("Extended Clip", "+3 shots per clip.", O, C, 45, 30, 5, S(StatType.ClipSize, 3)),
                P("Quick Hands", "Reload 20% faster.", O, C, 45, 30, 4, S(StatType.ReloadTime, 0.8f, StatOperation.Multiply)),
                P("Overcharged Rounds", "+2 damage and +10% critical chance.", O, Ep, 160, 120, 3, S(StatType.Damage, 2), S(StatType.CritChance, 0.1f)),

                // Economy
                P("Soul Hunger", "+25% souls from every source.", Ec, C, 60, 40, 0, S(StatType.SoulGain, 0.25f)),
                P("Rich Orbs", "Soul orbs are worth 40% more.", Ec, C, 40, 30, 5, S(StatType.OrbValue, 0.4f)),
                P("Lingering Orbs", "Orbs stay on the field 2 seconds longer.", Ec, C, 35, 25, 3, S(StatType.OrbLifetime, 2f)),
                P("Lucky Drops", "+10% chance for enemies to drop soul orbs.", Ec, Ra, 70, 50, 4, S(StatType.OrbDropChance, 0.1f)),
                P("Momentum", "Your kill combo lasts 1 second longer.", Ec, Ra, 60, 40, 3, S(StatType.ComboWindow, 1f)),
                P("Soul Bank", "Earn 10% interest on your souls after each wave.", Ec, Ep, 120, 100, 3, S(StatType.Interest, 0.1f)),

                // Defense
                P("Patch Up", "Repair 3 core health.", D, C, 40, 15, 0, R(ReactorAction.Heal, 3)),
                P("Recharge", "Restore 1 shield charge.", D, C, 35, 15, 0, R(ReactorAction.RestoreShield, 1)),
                P("Reinforced Core", "+3 max core health.", D, Ra, 90, 60, 0, R(ReactorAction.AddMaxHealth, 3)),
                P("Capacitor", "+1 max shield charge.", D, Ep, 140, 100, 3, R(ReactorAction.AddMaxShield, 1)),
                P("Healing Light", "+6% chance for enemies to drop heal orbs.", D, Ra, 70, 50, 3, S(StatType.HealOrbChance, 0.06f)),

                // Ability
                P("Nova Overcharge", "Nova recharges 25% faster.", A, Ra, 80, 60, 3, S(StatType.NovaCooldown, 0.75f, StatOperation.Multiply)),
                P("Wide Nova", "+1 Nova blast radius.", A, C, 50, 40, 4, S(StatType.NovaRadius, 1f)),

                // Weapon modules (installed into random slots of the weapon ring)
                P("Triple Shot Module", "Adds a Triple Shot slot to your weapon ring: 3 fast bolts in a spread.", Wp, C, 70, 50, 4, W(tripleShot)),
                P("Soul Cannon Module", "Adds a Soul Cannon slot: one giant orb that explodes where you aim.", Wp, Ra, 110, 70, 3, W(cannonShot)),
                P("Seeker Swarm Module", "Adds a Seeker slot: 3 homing missiles hunt enemies near your cursor.", Wp, Ra, 100, 70, 3, W(seekerShot)),

                P("Chain Lightning", "Technique: zap the target, then arc to 4 more enemies nearby.", Wp, Ra, 110, 70, 2, W(chainShot)),
                P("Piercing Lance", "Technique: a giant beam that pierces everything in a line for 2x damage.", Wp, Ra, 120, 80, 2, W(lanceShot)),
                P("Orbital Blades", "Technique: 3 blades orbit the reactor for 6s, slicing anything they touch.", Wp, Ep, 150, 100, 2, W(bladesShot)),
                P("Black Hole", "Technique: a vortex pulls enemies in, grinds them, then implodes for 3x damage.", Wp, Ep, 170, 110, 2, W(vortexShot)),

                // Legendary
                P("Soul Reaper", "+2 damage, +50% soul gain and +1 max shield.", O, L, 300, 200, 1,
                  S(StatType.Damage, 2), S(StatType.SoulGain, 0.5f), R(ReactorAction.AddMaxShield, 1)),
            };
        }

        private static VolumeProfile VolumeProfileAsset()
        {
            const string path = "Assets/Settings/SoulbreakVolume.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(1.6f);
            bloom.scatter.Override(0.7f);
            bloom.highQualityFiltering.Override(true);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.5f);

            var ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0.12f);

            foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        // ---------------- Scene ----------------
        private static void BuildScene(Sprite quad, Material orbMat, Material ringMat, Material enemyMat, Material reactorMat,
                                       Material tracerMat, Material bgMat, WaveEnemy[] roster, EnemyConfig boss,
                                       PerkDefinition[] perks, VolumeProfile profile)
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var n in new[] { "Systems", "Background", "Global Volume", "Pause Volume", "EventSystem" })
            {
                var old = GameObject.Find(n);
                if (old != null) Object.DestroyImmediate(old);
            }

            // Camera
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.02f, 0.015f, 0.05f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null) camData.renderPostProcessing = true;

            // Background
            var bg = new GameObject("Background");
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = quad; bgSr.sharedMaterial = bgMat; bgSr.sortingOrder = -100;
            bg.transform.localScale = new Vector3(44f, 26f, 1f);

            // Post-processing
            var vol = new GameObject("Global Volume").AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = profile;

            // Event system for the shop buttons (new Input System)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // Reactor
            var reactorGo = GameObject.Find("Reactor");
            reactorGo.transform.position = Vector3.zero;
            reactorGo.transform.localScale = Vector3.one * 2.6f;
            var rsr = reactorGo.GetComponent<SpriteRenderer>();
            rsr.sprite = quad; rsr.sharedMaterial = reactorMat; rsr.color = Color.white; rsr.sortingOrder = 1;
            reactorGo.GetComponent<CircleCollider2D>().radius = 0.21f;
            var reactor = reactorGo.GetComponent<Reactor.Reactor>();
            var reactorView = Get<ReactorView>(reactorGo);
            Set(reactorView, "reactor", reactor);

            // Systems
            var sys = new GameObject("Systems").transform;
            T Sys<T>(string name) where T : Component
            {
                var go = new GameObject(name);
                go.transform.SetParent(sys);
                return go.AddComponent<T>();
            }

            var stats = Sys<PlayerStats>("PlayerStats");
            var ammo = Sys<AmmoSystem>("AmmoSystem");
            var shooter = Sys<PlayerShooter>("PlayerShooter");
            var nova = Sys<NovaAbility>("NovaAbility");
            var waves = Sys<WaveManager>("WaveManager");
            var splitter = Sys<EnemySplitter>("EnemySplitter");
            var dropper = Sys<OrbDropper>("OrbDropper");
            var combo = Sys<ComboSystem>("ComboSystem");
            var wallet = Sys<SoulWallet>("SoulWallet");
            var interest = Sys<SoulInterest>("SoulInterest");
            var healer = Sys<ReactorHealer>("ReactorHealer");
            var shop = Sys<PerkShop>("PerkShop");
            var gm = Sys<GameManager>("GameManager");
            var fx = Sys<FxSystem>("FxSystem");
            var hud = UIPrefab<HUDCanvas>("HUD", sys);
            var hudGo = hud.gameObject;
            var shopView = UIPrefab<ShopView>("Shop", sys);
            var shake = Get<CameraShake>(cam.gameObject);
            var weapons = Sys<Weapons.WeaponRing>("WeaponRing");
            Set(weapons, "basicShot", basicShot);
            var wso = new SerializedObject(weapons);
            wso.FindProperty("maxSlots").intValue = 12;
            wso.ApplyModifiedPropertiesWithoutUndo();

            var ringView = new GameObject("WeaponRingView").AddComponent<Weapons.WeaponRingView>();
            ringView.transform.SetParent(sys);
            ringView.transform.position = reactorGo.transform.position;
            Set(ringView, "ring", weapons); Set(ringView, "shooter", shooter); Set(ringView, "quad", quad);
            Set(ringView, "segmentMaterial", segmentMat); Set(ringView, "needleMaterial", needleMat);

            Set(shooter, "weapons", weapons);
            Set(shop, "weapons", weapons);
            Set(gm, "weapons", weapons);

            // Pause: controller (time) + effect (post-processing) + menu (UI)
            var pauseVol = new GameObject("Pause Volume").AddComponent<Volume>();
            pauseVol.isGlobal = true; pauseVol.priority = 10f; pauseVol.weight = 0f; pauseVol.sharedProfile = pauseProfile;
            var pause = Sys<PauseController>("PauseController");
            Set(pause, "game", gm);
            var pauseFx = Sys<PauseEffect>("PauseEffect");
            Set(pauseFx, "pause", pause); Set(pauseFx, "volume", pauseVol); Set(pauseFx, "game", gm);
            var pauseMenu = UIPrefab<PauseMenuView>("PauseMenu", sys);
            Set(pauseMenu, "pause", pause); Set(pauseMenu, "game", gm); Set(pauseMenu, "waves", waves); Set(pauseMenu, "wallet", wallet);

            var admin = Sys<AdminCheats>("AdminCheats");
            Set(admin, "shop", shop); Set(admin, "wallet", wallet); Set(admin, "reactor", reactor);

            Set(ammo, "stats", stats);
            Set(shooter, "stats", stats); Set(shooter, "ammo", ammo); Set(shooter, "muzzle", reactorGo.transform); Set(shooter, "cam", cam);
            Set(nova, "center", reactorGo.transform); Set(nova, "stats", stats);

            Set(waves, "target", reactorGo.transform); Set(waves, "bossConfig", boss);
            var so = new SerializedObject(waves);
            var arr = so.FindProperty("roster");
            arr.arraySize = roster.Length;
            for (int i = 0; i < roster.Length; i++)
            {
                var el = arr.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("config").objectReferenceValue = roster[i].config;
                el.FindPropertyRelative("firstWave").intValue = roster[i].firstWave;
                el.FindPropertyRelative("weight").floatValue = roster[i].weight;
            }
            so.FindProperty("spawnRadius").floatValue = 12f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Set(dropper, "stats", stats); Set(dropper, "orbSprite", quad); Set(dropper, "orbMaterial", orbMat);
            Set(combo, "stats", stats);
            Set(wallet, "stats", stats); Set(wallet, "combo", combo);
            Set(interest, "waves", waves); Set(interest, "wallet", wallet); Set(interest, "stats", stats); Set(interest, "payoutPoint", reactorGo.transform);
            Set(healer, "reactor", reactor);

            Set(shop, "wallet", wallet); Set(shop, "stats", stats); Set(shop, "reactor", reactor);
            SetArray(shop, "catalog", perks);

            Set(gm, "reactor", reactor); Set(gm, "waves", waves); Set(gm, "shop", shop);

            Set(fx, "shooter", shooter); Set(fx, "nova", nova); Set(fx, "reactor", reactor);
            Set(fx, "quad", quad); Set(fx, "ringMaterial", ringMat); Set(fx, "tracerMaterial", tracerMat); Set(fx, "enemyMaterial", enemyMat);

            // HUD: one shared canvas + small single-purpose views
            Set(hud, "game", gm);
            var reactorPanel = hudGo.GetComponent<ReactorPanelView>();
            Set(reactorPanel, "reactor", reactor); Set(reactorPanel, "waves", waves);
            var soulView = hudGo.GetComponent<SoulCounterView>();
            Set(soulView, "wallet", wallet);
            var ammoView = hudGo.GetComponent<AmmoView>();
            Set(ammoView, "ammo", ammo);
            var novaView = hudGo.GetComponent<NovaView>();
            Set(novaView, "nova", nova);
            var comboView = hudGo.GetComponent<ComboView>();
            Set(comboView, "combo", combo);
            var banner = hudGo.GetComponent<BannerView>();
            Set(banner, "waves", waves); Set(banner, "nova", nova);
            var gameOver = hudGo.GetComponent<GameOverView>();
            Set(gameOver, "game", gm);
            Set(admin, "gameOverView", gameOver);

            Set(shopView, "shop", shop); Set(shopView, "wallet", wallet);

            Set(shake, "reactor", reactor); Set(shake, "nova", nova);

            // Input controllers (systems and views never read input themselves)
            var player = Sys<PlayerController>("PlayerController");
            Set(player, "shooter", shooter); Set(player, "ammo", ammo); Set(player, "nova", nova);
            var hotkeys = Sys<GameHotkeys>("GameHotkeys");
            Set(hotkeys, "game", gm); Set(hotkeys, "pause", pause);
            var shopKeys = Sys<ShopHotkeys>("ShopHotkeys");
            Set(shopKeys, "shop", shop);

            // Event channels (ScriptableObject assets shared by broadcasters and listeners)
            var so2 = new SerializedObject(waves);
            so2.FindProperty("channels.spawned").objectReferenceValue = ch.enemySpawned;
            so2.FindProperty("channels.killed").objectReferenceValue = ch.enemyKilled;
            so2.FindProperty("channels.reachedTarget").objectReferenceValue = ch.enemyReached;
            so2.FindProperty("channels.projectileShotDown").objectReferenceValue = ch.projectileShotDown;
            so2.ApplyModifiedPropertiesWithoutUndo();

            Set(splitter, "enemyKilled", ch.enemyKilled);
            Set(dropper, "enemyKilled", ch.enemyKilled); Set(dropper, "orbCollected", ch.orbCollected);
            Set(combo, "enemyKilled", ch.enemyKilled);
            Set(wallet, "enemyKilled", ch.enemyKilled); Set(wallet, "orbCollected", ch.orbCollected);
            Set(healer, "orbCollected", ch.orbCollected);
            Set(shake, "enemyKilled", ch.enemyKilled);
            Set(fx, "enemyKilled", ch.enemyKilled); Set(fx, "orbCollected", ch.orbCollected);
            Set(fx, "projectileShotDown", ch.projectileShotDown); Set(fx, "explosions", ch.explosions);
            Set(fx, "chains", ch.chains); Set(fx, "beams", ch.beams);

            WireFeatures(sys, cam, quad, orbMat, reactorGo, reactor, reactorView, stats, ammo, shooter, nova, waves, splitter,
                         combo, wallet, shop, gm, fx, hudGo, hud, shopView, weapons, pause, player, gameOver);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // Hit stop, overdrive, streaks, announcer, danger UI, boss bar, synergies, map events, audio, meta.
        private static void WireFeatures(Transform sys, Camera cam, Sprite quad, Material orbMat, GameObject reactorGo,
            Reactor.Reactor reactor, ReactorView reactorView, PlayerStats stats, AmmoSystem ammo, PlayerShooter shooter,
            NovaAbility nova, WaveManager waves, EnemySplitter splitter, ComboSystem combo, SoulWallet wallet, PerkShop shop,
            GameManager gm, FxSystem fx, GameObject hudGo, HUDCanvas hud, ShopView shopView, WeaponRing weapons,
            PauseController pause, PlayerController player, GameOverView gameOver)
        {
            T Sys<T>(string name) where T : Component
            {
                var go = new GameObject(name);
                go.transform.SetParent(sys);
                return go.AddComponent<T>();
            }

            // Time
            var time = Sys<TimeScaleService>("TimeScaleService");
            Set(pause, "time", time); Set(gm, "time", time);

            // Enemy registry + extra channels
            var registry = Sys<EnemyRegistry>("EnemyRegistry");
            Set(registry, "spawned", ch.enemySpawned); Set(registry, "killed", ch.enemyKilled); Set(registry, "reachedTarget", ch.enemyReached);
            Set(waves, "registry", registry);
            var wso = new SerializedObject(waves);
            wso.FindProperty("channels.bossEnraged").objectReferenceValue = ch.bossEnraged;
            wso.FindProperty("channels.damaged").objectReferenceValue = ch.enemyDamaged;
            wso.ApplyModifiedPropertiesWithoutUndo();
            Set(splitter, "eliteSplitInto", gruntConfig);
            Set(fx, "shop", shop); Set(fx, "bossEnraged", ch.bossEnraged);
            Set(fx, "enemyDamaged", ch.enemyDamaged); Set(fx, "sparkMaterial", orbMat);

            // Hit stop + impact flash
            var hitStop = Sys<HitStop>("HitStop");
            Set(hitStop, "time", time); Set(hitStop, "reactor", reactor); Set(hitStop, "headshots", ch.headshots);
            Set(hitStop, "enemyKilled", ch.enemyKilled); Set(hitStop, "bossEnraged", ch.bossEnraged);

            // Overdrive
            var overdrive = Sys<OverdriveSystem>("OverdriveSystem");
            Set(overdrive, "enemyKilled", ch.enemyKilled); Set(overdrive, "time", time);
            Set(shooter, "overdrive", overdrive); Set(player, "overdrive", overdrive);

            // Ammo charges (drop from enemies, fly to the core) + first-run tutorial
            Set(ammo, "orbCollected", ch.orbCollected);
            var ammoDropper = Object.FindAnyObjectByType<OrbDropper>();
            Set(ammoDropper, "ammo", ammo); Set(ammoDropper, "magnetTarget", reactorGo.transform);
            var tutorial = hudGo.GetComponent<TutorialView>();
            Set(tutorial, "game", gm); Set(tutorial, "shooter", shooter); Set(tutorial, "ammo", ammo);
            Set(tutorial, "nova", nova); Set(tutorial, "overdrive", overdrive);
            Set(tutorial, "enemyKilled", ch.enemyKilled); Set(tutorial, "orbCollected", ch.orbCollected);
            Set(tutorial, "dropper", ammoDropper); Set(tutorial, "registry", registry);

            // Index discoveries
            var codexRecorder = Sys<CodexRecorder>("CodexRecorder");
            Set(codexRecorder, "enemySpawned", ch.enemySpawned); Set(codexRecorder, "shop", shop);
            Set(codexRecorder, "announcements", ch.announcements);
            var odVol = new GameObject("Overdrive Volume").AddComponent<Volume>();
            odVol.isGlobal = true; odVol.priority = 5f; odVol.weight = 0f; odVol.sharedProfile = OverdriveProfileAsset();
            var odFx = Sys<OverdriveEffect>("OverdriveEffect");
            Set(odFx, "overdrive", overdrive); Set(odFx, "volume", odVol);

            // Kill streaks
            var streaks = Sys<StreakRewards>("StreakRewards");
            Set(streaks, "combo", combo); Set(streaks, "wallet", wallet); Set(streaks, "payoutPoint", reactorGo.transform);
            Set(streaks, "announcements", ch.announcements);

            // Synergies
            var synergies = Sys<SynergySystem>("SynergySystem");
            Set(synergies, "ring", weapons); Set(synergies, "shooter", shooter); Set(synergies, "stats", stats);
            Set(synergies, "announcements", ch.announcements);
            SetArray(synergies, "synergies", Synergies());
            Set(shopView, "synergies", synergies);

            // Map events
            var events = Sys<MapEventSystem>("MapEventSystem");
            Set(events, "waves", waves); Set(events, "reactor", reactor); Set(events, "wallet", wallet); Set(events, "stats", stats);
            Set(events, "enemyKilled", ch.enemyKilled); Set(events, "announcements", ch.announcements);
            Set(events, "urnSprite", quad); Set(events, "urnMaterial", orbMat); Set(events, "rejuvenatorConfig", rejuvenatorConfig);

            // HUD additions
            var flash = hudGo.GetComponent<ScreenFlashView>();
            Set(flash, "hitStop", hitStop);
            var odView = hudGo.GetComponent<OverdriveView>();
            Set(odView, "overdrive", overdrive);
            var announcer = hudGo.GetComponent<AnnouncerView>();
            Set(announcer, "announcements", ch.announcements);
            var headshotView = hudGo.GetComponent<HeadshotPopupView>();
            Set(headshotView, "headshots", ch.headshots);
            var danger = hudGo.GetComponent<DangerView>();
            Set(danger, "registry", registry); Set(danger, "reactor", reactor);
            var bossBar = hudGo.GetComponent<BossHealthView>();
            Set(bossBar, "spawned", ch.enemySpawned); Set(bossBar, "bossEnraged", ch.bossEnraged);

            // Audio
            var audio = Sys<AudioDirector>("AudioDirector");
            Set(audio, "shooter", shooter); Set(audio, "ammo", ammo); Set(audio, "nova", nova); Set(audio, "overdrive", overdrive);
            Set(audio, "reactor", reactor); Set(audio, "waves", waves); Set(audio, "shop", shop); Set(audio, "combo", combo);
            Set(audio, "enemyKilled", ch.enemyKilled); Set(audio, "bossEnraged", ch.bossEnraged); Set(audio, "orbCollected", ch.orbCollected);
            Set(audio, "projectileShotDown", ch.projectileShotDown); Set(audio, "explosions", ch.explosions);
            Set(audio, "headshots", ch.headshots); Set(audio, "announcements", ch.announcements);
            var aso = new SerializedObject(audio);
            var sounds = aso.FindProperty("patternSounds");
            var map = new (ShotPattern p, Sfx s)[]
            {
                (basicShot, Sfx.Laser), (tripleShot, Sfx.Bolt), (cannonShot, Sfx.Cannon), (seekerShot, Sfx.Missile),
                (chainShot, Sfx.Zap), (lanceShot, Sfx.Lance), (bladesShot, Sfx.Blade), (vortexShot, Sfx.Vortex),
            };
            sounds.arraySize = map.Length;
            for (int i = 0; i < map.Length; i++)
            {
                sounds.GetArrayElementAtIndex(i).FindPropertyRelative("pattern").objectReferenceValue = map[i].p;
                sounds.GetArrayElementAtIndex(i).FindPropertyRelative("sound").enumValueIndex = (int)map[i].s;
            }
            aso.ApplyModifiedPropertiesWithoutUndo();

            var musicDir = Sys<MusicDirector>("MusicDirector");
            Set(musicDir, "game", gm); Set(musicDir, "waves", waves); Set(musicDir, "registry", registry);
            Set(musicDir, "pause", pause); Set(musicDir, "overdrive", overdrive); Set(musicDir, "reactor", reactorGo.transform);
            if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();

            // Meta progression
            var catalog = MetaCatalogAsset();
            var loadout = Sys<StartingLoadout>("StartingLoadout");
            Set(loadout, "catalog", catalog); Set(loadout, "ring", weapons); Set(loadout, "reactorView", reactorView);
            var recorder = Sys<MetaRecorder>("MetaRecorder");
            var runStats = Sys<RunStats>("RunStats");
            Set(runStats, "enemyKilled", ch.enemyKilled); Set(runStats, "headshots", ch.headshots); Set(runStats, "combo", combo);
            Set(recorder, "game", gm); Set(recorder, "wallet", wallet); Set(recorder, "waves", waves); Set(recorder, "stats", runStats);
            var difficultyApplier = Sys<DifficultyApplier>("DifficultyApplier");
            Set(difficultyApplier, "waves", waves); Set(difficultyApplier, "reactor", reactor);
            Set(gameOver, "recorder", recorder);
            Set(Sys<LeaderboardSubmitter>("LeaderboardSubmitter"), "recorder", recorder);

            // Feedback: damage numbers, aim label, settings
            var dmgNumbers = hudGo.GetComponent<DamageNumbersView>();
            Set(dmgNumbers, "damaged", ch.enemyDamaged); Set(dmgNumbers, "stats", stats);
            var aimLabel = hudGo.GetComponent<AimLabelView>();
            Set(aimLabel, "shooter", shooter); Set(aimLabel, "ring", weapons);
            Set(aimLabel, "game", gm); Set(aimLabel, "core", reactorGo.transform);
            var settings = UIPrefab<SettingsView>("Settings", sys);
            var pauseMenu = Object.FindAnyObjectByType<PauseMenuView>();
            Set(pauseMenu, "settings", settings);
            Set(pauseMenu, "index", UIPrefab<Soulbreak.Menu.IndexView>("Index", sys));
            Set(pauseMenu, "howTo", UIPrefab<HowToView>("HowTo", sys));
            var pauseBoard = UIPrefab<LeaderboardPanel>("Leaderboard", sys);
            Set(pauseBoard, "pause", Object.FindAnyObjectByType<PauseController>());
            var boardSo = new SerializedObject(pauseBoard);
            boardSo.FindProperty("sortingOrder").intValue = 22;   // above the pause menu, below its popups
            boardSo.ApplyModifiedPropertiesWithoutUndo();
            Set(Object.FindAnyObjectByType<GameHotkeys>(), "pauseMenu", pauseMenu);

            // Core upgrades (visual) + game mode
            var upgrades = Sys<CoreUpgradeView>("CoreUpgradeView");
            Set(upgrades, "shop", shop); Set(upgrades, "ring", weapons); Set(upgrades, "reactorView", reactorView);
            Set(upgrades, "announcements", ch.announcements); Set(upgrades, "quad", quad); Set(upgrades, "satelliteMaterial", orbMat);

            var modeApplier = Sys<ModeApplier>("ModeApplier");
            Set(modeApplier, "waves", waves); Set(modeApplier, "splitter", splitter); Set(modeApplier, "mapEvents", events);
            Set(modeApplier, "shop", shop); Set(modeApplier, "dropper", Object.FindAnyObjectByType<OrbDropper>());
            Set(modeApplier, "fx", fx);
            Set(modeApplier, "core", reactorGo.GetComponent<SpriteRenderer>());
            Set(modeApplier, "background", GameObject.Find("Background").GetComponent<SpriteRenderer>());
        }

        // Places one of the UI prefabs (Assets/Prefabs/UI) in the open scene.
        private static T UIPrefab<T>(string name, Transform parent = null) where T : Component
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/UI/{name}.prefab");
            if (asset == null) throw new System.Exception($"Missing UI prefab '{name}' in Assets/Prefabs/UI.");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            if (parent != null) go.transform.SetParent(parent, false);
            return go.GetComponent<T>();
        }

        private static T Get<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogError($"Setup: no field '{field}' on {target.GetType().Name}"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
