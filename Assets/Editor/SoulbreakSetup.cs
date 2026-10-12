using System.IO;
using Soulbreak.Economy;
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

            var roster = Enemies(quad, enemyMat, orbMat, out var boss);
            var perks = Perks();
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

            var view = new GameObject("MainMenu").AddComponent<Soulbreak.Menu.MainMenuView>();
            Set(view, "ambience", amb);

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static VolumeProfile pauseProfile;

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
            Sys<EnemySplitter>("EnemySplitter");
            var dropper = Sys<OrbDropper>("OrbDropper");
            var combo = Sys<ComboSystem>("ComboSystem");
            var wallet = Sys<SoulWallet>("SoulWallet");
            var interest = Sys<SoulInterest>("SoulInterest");
            var healer = Sys<ReactorHealer>("ReactorHealer");
            var shop = Sys<PerkShop>("PerkShop");
            var gm = Sys<GameManager>("GameManager");
            var fx = Sys<FxSystem>("FxSystem");
            var hud = Sys<HUD>("HUD");
            var shopView = Sys<ShopView>("ShopView");
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
            Set(pauseFx, "pause", pause); Set(pauseFx, "volume", pauseVol);
            var pauseMenu = Sys<PauseMenuView>("PauseMenu");
            Set(pauseMenu, "pause", pause); Set(pauseMenu, "game", gm); Set(pauseMenu, "waves", waves); Set(pauseMenu, "wallet", wallet);

            var admin = Sys<AdminCheats>("AdminCheats");
            Set(admin, "shop", shop); Set(admin, "wallet", wallet);

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

            Set(hud, "reactor", reactor); Set(hud, "wallet", wallet); Set(hud, "ammo", ammo); Set(hud, "nova", nova);
            Set(hud, "combo", combo); Set(hud, "waves", waves); Set(hud, "gameManager", gm);

            Set(shopView, "shop", shop); Set(shopView, "wallet", wallet);

            Set(shake, "reactor", reactor); Set(shake, "nova", nova);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
