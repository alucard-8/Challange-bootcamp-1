using UnityEngine;

namespace Soulbreak.UI
{
    // Every player-facing UI string lives here so copy can be edited without touching code.
    // Views read them through a [SerializeField] UIStrings ref; UIFactory.Strings is the
    // Resources fallback when a ref is not wired. Placeholders use string.Format ({0}, {1}...).
    [CreateAssetMenu(menuName = "Soulbreak/UI Strings", fileName = "UIStrings")]
    public class UIStrings : ScriptableObject
    {
        [Header("Account")]
        public string accountOffline = "OFFLINE";
        public string accountSignIn = "GUEST  -  SIGN IN";
        public string accountGuestStatus =
            "You're playing as a <b>guest</b>: progress is only saved in this browser.\n" +
            "Create an account to save it online and put your name on the leaderboard.";
        [Tooltip("{0} = user name")]
        public string accountLoggedInStatus = "Logged in as <b>{0}</b>. Your progress is saved online.";
        public string accountWaiting = "Please wait...";
        public string accountLoggedOut = "Logged out. You're playing as a guest.";
        public string accountLoginSuccess = "Logged in. Progress loaded.";
        public string accountCreateSuccess = "Account created. Progress saved online.";

        [Header("Leaderboard")]
        public string leaderboardArrowOpen = ">";
        public string leaderboardArrowClosed = "<";
        public string leaderboardConnecting = "CONNECTING";
        public string leaderboardYou = "YOU";
        public string leaderboardGuestTag = "   (GUEST)";
        public string leaderboardNoScores = "NO SCORES YET - BE THE FIRST";
        public string leaderboardOffline = "OFFLINE - SCORES SAVED LOCALLY";
        public string leaderboardEmptyName = "---";
        public string leaderboardUnknownName = "???";

        [Header("Shop")]
        public string shopTechnique = "TECHNIQUE";
        [Tooltip("{0} = reroll cost")]
        public string shopReroll = "REROLL  ({0})";
        public string shopSold = "SOLD";
        [Tooltip("{0} = price")]
        public string shopBuy = "BUY  -  {0}";
        [Tooltip("{0} = price")]
        public string shopNeed = "NEED {0}";
        [Tooltip("{0} = stacks, {1} = max")]
        public string shopOwnedOf = "OWNED {0}/{1}";
        [Tooltip("{0} = stacks")]
        public string shopOwned = "OWNED {0}";
        [Tooltip("{0} = hotkey number")]
        public string shopHotkey = "[{0}]";

        [Header("How To Play")]
        public string howToTitle = "HOW TO PLAY";
        public string howToBodyLeft =
            "<color=#73D9FF><b>DEFEND THE CORE</b></color>\n\n" +
            "    Enemies march on the core in waves. Click them before they reach it. Survive as many waves as you can.\n\n\n" +
            "    <color=#FFC94C><b>AMMO</b></color>\n\n" +
            "    Your clip holds <b>6 shots</b> (+3 per <i>Extended Clip</i> perk). Empty clips reload by themselves; <b>R</b> / <b>RMB</b> reloads early. Enemies sometimes drop <b>gold ammo charges</b> that fly to the core and refill the clip instantly, more often when you're low.\n\n\n" +
            "    <color=#C780FF><b>SOULS & ORBS</b></color>\n\n" +
            "    Kills give souls. Shoot the orbs they drop: purple = souls, green = a shield charge.";
        public string howToBodyRight =
            "<color=#73D9FF><b>THE WEAPON RING</b></color>\n\n" +
            "    The ring around the core is cut into <b>slots</b>. Every shot goes through the slot your aim crosses, and that slot decides <b>what</b> you fire.\n\n\n" +
            "    - Plain slots fire your <b>basic shot</b>.\n\n" +
            "    - Each <b>technique</b> you buy adds one <b>special slot</b> (glowing, with an icon). It fires once, then empties and <b>reloads a random technique you own</b>.\n\n" +
            "    - Every wave the ring <b>re-rolls</b>: a new random slot count (5 to 12, never fewer than your techniques) and a new layout. That's why the number of slots changes.\n\n" +
            "    - The ring slowly turns, so aim where the special slot is.\n\n\n" +
            "    <color=#C780FF><b>SHOP</b></color>\n\n" +
            "    Between waves spend souls on buffs and techniques. Some techniques combine into <b>synergies</b>.";
        public string howToControls = "<b>LMB</b> shoot     <b>R / RMB</b> reload     <b>SPACE</b> nova     <b>Q</b> overdrive     <b>1-5</b> buy in shop     <b>ESC</b> pause";
        public string howToBack = "BACK";

        [Header("Pause Menu")]
        [Tooltip("{0}=wave, {1}=souls label, {2}=souls, {3}=seconds")]
        public string pauseStats = "WAVE        <b>{0}</b>\n{1,-12}<b>{2}</b>\nTIME          <b>{3:0}s</b>";

        [Header("Settings")]
        public string settingsOn = "ON";
        public string settingsOff = "OFF";
        [Tooltip("{0} = percent")]
        public string settingsPercent = "{0}%";

        [Header("Main Menu")]
        [Tooltip("{0}=gold hex, {1}=best, {2}=last, {3}=last wave, {4}=best wave, {5}=runs, {6}=diff hex, {7}=diff name")]
        public string menuRecords =
            "BEST SCORE   <b><color=#{0}>{1}</color></b>\n" +
            "LAST SCORE   <b>{2}</b>   <size=18>(wave {3})</size>\n" +
            "BEST WAVE     <b>{4}</b>      RUNS  <b>{5}</b>\n" +
            "DIFFICULTY    <b><color=#{6}>{7}</color></b>";

        [Header("Menu - toasts")]
        public string toastStarCoreUnlocked = "STAR CORE UNLOCKED\n<size=26>Switch modes any time in ASCENSION</size>";
        public string toastStarCoreMode = "STAR CORE MODE";
        public string toastSoulReactorMode = "SOUL REACTOR MODE";

        [Header("Ascension")]
        public string ascensionUnknown = "???";
        public string ascensionSecret = "SECRET";
        public string ascensionPlayMode = "PLAY THIS MODE";
        [Tooltip("{0} = cost")]
        public string ascensionUnlock = "UNLOCK  {0}";
        public string ascensionEquip = "EQUIP";
        public string ascensionEquipped = "EQUIPPED";
        public string ascensionActive = "ACTIVE  (click to leave)";
        public string ascensionEquippedRemove = "EQUIPPED  (click to remove)";
        [Tooltip("{0}=essence, {1}=best wave")]
        public string ascensionStatus = "{0} SOUL ESSENCE      BEST WAVE {1}";

        [Header("Index")]
        public string indexUnknown = "???";
        public string indexUnknownEnemy = "UNKNOWN ENEMY";
        public string indexUnknownPerk = "UNKNOWN PERK";
        public string indexUnknownEnemyBody = "You haven't met this enemy yet. Survive longer to reveal it.";
        public string indexUnknownPerkBody = "This perk hasn't shown up in the shop yet.";
        [Tooltip("{0} = found, {1} = total")]
        public string indexDiscovered = "DISCOVERED  {0} / {1}";
        [Tooltip("{0} = page, {1} = pages")]
        public string indexPage = "{0} / {1}";
        [Tooltip("{0}=isBoss tags")]
        public string indexEnemyTag = "ENEMY";
        public string indexBossTag = "BOSS";
        [Tooltip("{0}=hp, {1}=speed, {2}=dmg, {3}=souls")]
        public string indexEnemyStats =
            "HEALTH      <b>{0}</b>\nSPEED        <b>{1:0.0}</b>\nCORE DMG   <b>{2}</b>\nSOULS        <b>{3}</b>";
        [Tooltip("{0}=cost, {1}=growth, {2}=max stacks")]
        public string indexPerkStats =
            "COST          <b>{0}</b>  (+{1} each)\nMAX STACKS  <b>{2}</b>";
        public string indexNoLimit = "no limit";
        [Tooltip("{0} = rarity, {1} = category/technique")]
        public string indexPerkTag = "{0}  -  {1}";
        [Tooltip("{0}=tag (ENEMY/BOSS), {1}=health")]
        public string indexEnemyShortTag = "{0}   HP {1}";
        [Tooltip("{0}=rarity, {1}=category/technique")]
        public string indexPerkShortTag = "{0}   {1}";

        [Header("Game Over")]
        public string gameOverTitle = "REACTOR DESTROYED";
        public string gameOverBest = "BEST   {0}";
        [Tooltip("{0}=essence gained, {1}=essence label, {2}=essence total")]
        public string gameOverEssence = "+{0} {1}      TOTAL  {2}";
        [Tooltip("{0}=difficulty hex, {1}=difficulty name, {2}=wave")]
        public string gameOverSubtitle = "<color=#{0}>{1}</color>   -   WAVE {2}";

        [Header("HUD - generic")]
        public string hudReloading = "RELOADING";
        public string hudReloadingPercent = "RELOADING  {0:0}%";
        public string hudReloadHint = "R / RMB to reload";
        public string hudLowAmmo = "LOW AMMO";
        public string hudAmmoCharge = "AMMO CHARGE!";
        public string hudBlock = "BLOCK";
        public string hudKillSuffix = "!";
        [Tooltip("{0} = wave")]
        public string hudWave = "WAVE {0}";
        [Tooltip("{0} = wave")]
        public string hudWaveBoss = "WAVE {0}\n<size=40>THE BOSS APPROACHES</size>";
        public string hudWaveCleared = "WAVE CLEARED";
        public string hudNovaReady = "NOVA READY";
        public string hudEnraged = "  -  ENRAGED";
        [Tooltip("{0}=count, {1}=mult hex, {2}=mult")]
        public string hudCombo = "COMBO {0}   <color=#{1}>x{2:0.00}</color>";
        [Tooltip("{0}=wave")]
        public string hudBossWave = "<color=#FF5566>BOSS WAVE</color>";
        public string hudDefend = "Defend the reactor";
        public string hudWaveClearedShop = "Wave cleared - shopping";
        [Tooltip("{0} = seconds")]
        public string hudOverdriveTimer = "OVERDRIVE  {0:0.0}s";
        public string hudOverdriveReady = "OVERDRIVE  <b>[Q]</b>";
        [Tooltip("{0} = percent")]
        public string hudOverdrivePercent = "OVERDRIVE  {0:0}%";
        [Tooltip("{0}=current, {1}=max")]
        public string hudCoreHealth = "{0}  {1}/{2}";
        public string hudCollect = "+{0}";
        [Tooltip("{0} = nova label")]
        public string hudNovaReadyLabel = "{0}  <b>[SPACE]</b>";
        [Tooltip("{0} = nova label, {1} = seconds")]
        public string hudNovaCooldown = "{0}  {1:0.0}s";

        [Header("Tutorial - chrome")]
        [Tooltip("{0} = step, {1} = total")]
        public string tutorialCounter = "TUTORIAL  {0}/{1}";
        public string tutorialSkip = "[T] SKIP";
        public string tutorialDone = "DONE";
        public string tutorialNice = "NICE!";
        public string tutorialTarget = "TARGET";
        public string tutorialShoot = "SHOOT";
        public string tutorialAmmo = "AMMO";
        public string tutorialRing = "RING";

        [Header("Tutorial - steps (title / key / body)")]
        public string[] tutorialTitles =
        {
            "AIM & SHOOT", "PROTECT THE CORE", "THE WEAPON RING", "RELOAD", "AMMO CHARGES",
            "PICKUPS", "NOVA", "OVERDRIVE", "THE SHOP",
        };
        public string[] tutorialKeys =
        {
            "LEFT CLICK", "LEFT CLICK", "", "R  /  RIGHT CLICK", "", "SHOOT THE ORB", "SPACE", "Q", "",
        };
        public string[] tutorialBodies =
        {
            "Aim with the mouse and fire at the incoming enemies.",
            "Everything marches on the core in the middle. Destroy <b>3 enemies</b> before they reach it.",
            "Your shot goes through the <b>ring slot</b> you aim across. <b>Glowing slots</b> hold techniques: they fire once, then reload a random technique you own. The slot count re-rolls every wave (5 to 12).",
            "Empty clips reload on their own, but reloading early saves you in a pinch. <b>Reload now.</b>",
            "Enemies drop <color=#FFC94D>gold ammo charges</color>. They fly to the core and <b>instantly refill</b> your clip. Watch this one.",
            "<color=#C77DFF>Purple orbs</color> are souls, <color=#6CFF9A>green orbs</color> restore shield. <b>Shoot one</b> to collect it.",
            "Nova blasts every enemy <b>near the core</b>. Let them come close, then press SPACE.",
            "Kills fill the overdrive bar. It's full now: press <b>Q</b> to slow time and go berserk.",
            "Clear the wave to open the shop. Spend souls on perks and techniques and combine them for synergies. Good luck!",
        };
    }
}
