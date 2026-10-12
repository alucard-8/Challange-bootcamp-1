using System.Collections.Generic;
using Soulbreak.Enemies;
using Soulbreak.Meta;
using Soulbreak.Perks;
using Soulbreak.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.Menu
{
    // The Index (main menu + pause menu): a paged grid of every enemy and perk, plus a side panel
    // showing the selected entry. Enemies are rendered live by a small preview camera; perks show
    // their shop card. Undiscovered entries stay "???". Reads MetaProgress, never writes it.
    public class IndexView : MonoBehaviour
    {
        [SerializeField] private CodexCatalog codex;

        private const int Columns = 3, Rows = 3, PerPage = Columns * Rows;
        private static readonly Vector3 PreviewSpot = new Vector3(5000f, 5000f, 0f);

        private static readonly Color[] RarityColors =
        {
            new Color(0.65f, 0.72f, 0.82f), new Color(0.3f, 0.65f, 1f), new Color(0.75f, 0.4f, 1f), new Color(1f, 0.75f, 0.2f),
        };

        public bool IsOpen { get; private set; }

        private bool enemyTab = true;
        private int page, selected;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Text pageText, countText;
        [SerializeField] private Button enemyTabBtn, perkTabBtn, prevButton, nextButton, backButton;
        [SerializeField] private List<Tile> tiles = new List<Tile>();

        // detail panel + the offscreen camera rig that renders the selected enemy
        [SerializeField] private Text dName, dTag, dDesc, dStats;
        [SerializeField] private RawImage dEnemyView;
        [SerializeField] private Image dCard, dLocked;
        [SerializeField] private Camera previewCam;
        [SerializeField] private SpriteRenderer previewSr;
        private Material dCardMat;
        private MaterialPropertyBlock block;
        private RenderTexture rt;

        [System.Serializable]
        private class Tile
        {
            public Image Body, Swatch;
            public Text Name, Tag;
            public CanvasGroup Group;
            public Button Button;
        }

        private void Awake()
        {
            enemyTabBtn.onClick.AddListener(() => SetTab(true));
            perkTabBtn.onClick.AddListener(() => SetTab(false));
            prevButton.onClick.AddListener(() => TurnPage(-1));
            nextButton.onClick.AddListener(() => TurnPage(1));
            backButton.onClick.AddListener(Close);
            for (int i = 0; i < tiles.Count; i++)
            {
                int index = i;
                tiles[i].Button.onClick.AddListener(() => Select(index));
            }

            previewCam.enabled = false;
            if (dCard.material != null) dCard.material = dCardMat = new Material(dCard.material);
            previewCam.transform.parent.position = PreviewSpot;
        }

        private void OnDestroy()
        {
            if (rt != null) rt.Release();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            page = 0;
            selected = 0;
            Refresh();
            Audio.UIAudio.Play(Audio.Sfx.UIOpen);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Audio.UIAudio.Play(Audio.Sfx.UIClose);
        }

        public void TurnPage(int dir)
        {
            int pages = Mathf.Max(1, Mathf.CeilToInt(Count() / (float)PerPage));
            int next = Mathf.Clamp(page + dir, 0, pages - 1);
            if (next == page) return;
            page = next;
            selected = page * PerPage;
            Audio.UIAudio.Play(Audio.Sfx.UIClick);
            Refresh();
        }

        private void SetTab(bool enemies)
        {
            if (enemyTab == enemies) return;
            enemyTab = enemies;
            page = 0;
            selected = 0;
            Audio.UIAudio.Play(Audio.Sfx.UIClick);
            Refresh();
        }

        private void Select(int tile)
        {
            int i = page * PerPage + tile;
            if (i >= Count() || i == selected) return;
            selected = i;
            Audio.UIAudio.Play(Audio.Sfx.UIClick);
            Refresh();
        }

        private int Count() => enemyTab ? codex.enemies.Length : codex.perks.Length;

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.blocksRaycasts = group.interactable = IsOpen;
            panel.localScale = Vector3.one * UIFactory.PopScale(group.alpha);

            // only render the preview while it's visible
            bool showEnemy = group.alpha > 0.01f && enemyTab && Discovered(selected);
            previewCam.enabled = showEnemy;
            previewSr.enabled = showEnemy;
            if (showEnemy)
            {
                float t = Time.unscaledTime;
                previewSr.transform.rotation = Quaternion.Euler(0, 0, t * 25f);
                previewSr.transform.position = PreviewSpot + Vector3.up * Mathf.Sin(t * 2f) * 0.05f;
            }
        }

        private void Refresh()
        {
            int total = Count(), found = 0;
            for (int i = 0; i < total; i++) if (Discovered(i)) found++;
            int pages = Mathf.Max(1, Mathf.CeilToInt(total / (float)PerPage));
            pageText.text = $"{page + 1} / {pages}";
            countText.text = $"DISCOVERED  {found} / {total}";
            enemyTabBtn.GetComponent<Image>().color = enemyTab ? UIFactory.Theme.accent * 0.6f : UIFactory.Theme.buttonColor;
            perkTabBtn.GetComponent<Image>().color = !enemyTab ? UIFactory.Theme.accent * 0.6f : UIFactory.Theme.buttonColor;

            for (int t = 0; t < tiles.Count; t++)
            {
                int i = page * PerPage + t;
                var tile = tiles[t];
                tile.Group.gameObject.SetActive(i < total);
                if (i >= total) continue;

                bool known = Discovered(i);
                tile.Group.alpha = known ? 1f : 0.5f;
                Color c = known ? ColorOf(i) : new Color(1, 1, 1, 0.15f);
                tile.Swatch.color = c;
                tile.Name.text = known ? NameOf(i) : "???";
                tile.Tag.text = known ? ShortTag(i) : (enemyTab ? "UNKNOWN ENEMY" : "UNKNOWN PERK");
                tile.Tag.color = known ? c : new Color(1, 1, 1, 0.4f);
                tile.Body.color = i == selected ? Color.Lerp(new Color(0.07f, 0.05f, 0.13f, 0.95f), c, 0.25f) : new Color(0.07f, 0.05f, 0.13f, 0.95f);
            }
            ShowDetail();
        }

        private void ShowDetail()
        {
            int i = Mathf.Clamp(selected, 0, Count() - 1);
            bool known = Count() > 0 && Discovered(i);
            dLocked.gameObject.SetActive(!known);
            dEnemyView.gameObject.SetActive(known && enemyTab);
            dCard.gameObject.SetActive(known && !enemyTab);

            if (!known)
            {
                dName.text = "???";
                dTag.text = enemyTab ? "UNKNOWN ENEMY" : "UNKNOWN PERK";
                dTag.color = new Color(1, 1, 1, 0.4f);
                dDesc.text = enemyTab ? "You haven't met this enemy yet. Survive longer to reveal it." : "This perk hasn't shown up in the shop yet.";
                dStats.text = "";
                return;
            }

            var c = ColorOf(i);
            dName.text = NameOf(i);
            dTag.color = c;
            if (enemyTab)
            {
                var e = codex.enemies[i];
                dTag.text = e.isBoss ? "BOSS" : "ENEMY";
                dDesc.text = CodexCatalog.Describe(e);
                dStats.text =
                    $"HEALTH      <b>{e.maxHealth}</b>\n" +
                    $"SPEED        <b>{e.speed:0.0}</b>\n" +
                    $"CORE DMG   <b>{e.damage}</b>\n" +
                    $"SOULS        <b>{e.soulReward}</b>";
                previewSr.sprite = e.sprite;
                if (e.material != null) previewSr.sharedMaterial = e.material;
                previewSr.GetPropertyBlock(block);
                block.SetColor("_Color", e.color);
                block.SetFloat("_Spikes", e.spikes);
                block.SetFloat("_Seed", 3.7f);
                block.SetFloat("_HealthFrac", 1f);
                block.SetFloat("_Flash", 0f);
                block.SetColor("_AuraColor", Color.clear);
                previewSr.SetPropertyBlock(block);
                previewSr.transform.localScale = Vector3.one * e.size;
                previewCam.orthographicSize = Mathf.Max(0.6f, e.size * 0.8f);
            }
            else
            {
                var p = codex.perks[i];
                dTag.text = $"{p.rarity.ToString().ToUpper()}  -  {(p.IsTechnique ? "TECHNIQUE" : p.category.ToString().ToUpper())}";
                dDesc.text = p.description;
                dStats.text =
                    $"COST          <b>{p.baseCost}</b>  (+{p.costGrowth} each)\n" +
                    $"MAX STACKS  <b>{(p.maxStacks > 0 ? p.maxStacks.ToString() : "no limit")}</b>";
                if (dCardMat != null)
                {
                    var tech = p.TechniqueEffect;
                    dCardMat.SetColor("_Accent", c);
                    dCardMat.SetFloat("_Style", tech != null ? 1f : 0f);
                    dCardMat.SetFloat("_Icon", tech != null && tech.module != null ? tech.module.icon : 0f);
                    dCardMat.SetFloat("_Rarity", (int)p.rarity);
                    dCardMat.SetFloat("_Category", (int)p.category);
                    dCardMat.SetFloat("_Dim", 0f);
                }
            }
        }

        private bool Discovered(int i)
        {
            if (i < 0 || i >= Count()) return false;
            return enemyTab
                ? codex.enemies[i] != null && MetaProgress.IsDiscovered(true, codex.enemies[i].name)
                : codex.perks[i] != null && MetaProgress.IsDiscovered(false, codex.perks[i].name);
        }

        private string NameOf(int i) => enemyTab ? CodexCatalog.Name(codex.enemies[i]) : codex.perks[i].displayName;

        private string ShortTag(int i)
        {
            if (enemyTab) { var e = codex.enemies[i]; return (e.isBoss ? "BOSS" : "ENEMY") + $"   HP {e.maxHealth}"; }
            var p = codex.perks[i];
            return $"{p.rarity.ToString().ToUpper()}   {(p.IsTechnique ? "TECHNIQUE" : p.category.ToString().ToUpper())}";
        }

        private Color ColorOf(int i)
        {
            if (!enemyTab) return RarityColors[Mathf.Clamp((int)codex.perks[i].rarity, 0, RarityColors.Length - 1)];
            var c = (Color)codex.enemies[i].color;
            float peak = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
            c /= peak; c.a = 1f;
            return c;
        }
    }
}
