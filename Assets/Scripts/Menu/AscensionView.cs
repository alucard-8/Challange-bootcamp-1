using System.Collections.Generic;
using Soulbreak.Meta;
using Soulbreak.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.Menu
{
    public class AscensionView : MonoBehaviour
    {
        private static readonly int CoreColorId = Shader.PropertyToID("_Color");
        private static readonly int ShieldColorId = Shader.PropertyToID("_ShieldColor");

        [SerializeField] private MetaCatalog catalog;
        [SerializeField] private SpriteRenderer menuReactor;

        public bool IsOpen { get; private set; }

        [System.Serializable]
        private class Entry
        {
            public string itemId;
            public Text title, body, buttonText;
            public Button button;
            public Image card, preview;
            [System.NonSerialized] public MetaItem item;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text essenceText;
        [SerializeField] private Button backButton;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            backButton.onClick.AddListener(Close);
            foreach (var e in entries)
            {
                var entry = e;
                entry.item = catalog.Find(entry.itemId);
                entry.button.onClick.AddListener(() => Click(entry));
                if (entry.preview.material != null) entry.preview.material = new Material(entry.preview.material);
                if (entry.item != null) SetupPreview(entry);
            }
            entries.RemoveAll(x => x.item == null);
        }

        private void OnEnable() => MetaProgress.OnChanged += Refresh;
        private void OnDisable() => MetaProgress.OnChanged -= Refresh;

        private void Start()
        {
            Refresh();
            PreviewSkin();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            Refresh();
            Audio.UIAudio.Play(Audio.Sfx.UIOpen);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Audio.UIAudio.Play(Audio.Sfx.UIClose);
        }

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 5f);
            group.blocksRaycasts = group.interactable = IsOpen;
            group.transform.localScale = Vector3.one * UIFactory.PopScale(group.alpha);
        }

        private void Click(Entry e)
        {
            if (!MetaProgress.IsUnlocked(e.item)) MetaProgress.TryUnlock(e.item);
            else MetaProgress.ToggleEquip(e.item);
            PreviewSkin();
        }

        private void PreviewSkin()
        {
            if (menuReactor == null) return;
            var skin = catalog.Find(MetaProgress.EquippedSkin);
            menuReactor.GetPropertyBlock(block);
            if (skin != null)
            {
                block.SetColor(CoreColorId, skin.coreColor);
                block.SetColor(ShieldColorId, skin.shieldColor);
            }
            menuReactor.SetPropertyBlock(block);
        }

        private void Refresh()
        {
            essenceText.text = string.Format(S.ascensionStatus, MetaProgress.Essence, MetaProgress.BestWave);
            foreach (var e in entries)
            {
                bool unlocked = MetaProgress.IsUnlocked(e.item);
                bool equipped = MetaProgress.IsEquipped(e.item);

                if (e.item.type == MetaItemType.GameMode) SetupPreview(e);
                if (e.item.type == MetaItemType.GameMode)
                {
                    e.title.text = unlocked ? e.item.displayName : S.ascensionUnknown;
                    e.body.text = unlocked ? e.item.description : e.item.unlockHint;
                    e.buttonText.text = !unlocked ? S.ascensionSecret : equipped ? S.ascensionActive : S.ascensionPlayMode;
                    e.button.interactable = unlocked;
                }
                else
                {
                    e.buttonText.text = !unlocked ? string.Format(S.ascensionUnlock, e.item.cost)
                        : equipped ? (e.item.type == MetaItemType.ReactorSkin ? S.ascensionEquipped : S.ascensionEquippedRemove) : S.ascensionEquip;
                    e.button.interactable = unlocked || MetaProgress.Essence >= e.item.cost;
                }
                e.card.color = equipped ? T.cardEquipped : T.cardBackgroundNear;
            }
        }

        private const float KindSkin = 0f, KindTechnique = 1f, KindStar = 2f, KindLocked = 3f;

        private static void SetupPreview(Entry e)
        {
            var m = e.preview.material;
            if (m == null || !m.HasProperty("_Kind")) return;
            var item = e.item;
            switch (item.type)
            {
                case MetaItemType.ReactorSkin:
                    m.SetFloat("_Kind", KindSkin);
                    m.SetColor("_Color", item.coreColor);
                    m.SetColor("_Color2", item.shieldColor);
                    break;
                case MetaItemType.StartingTechnique:
                    m.SetFloat("_Kind", KindTechnique);
                    m.SetFloat("_Icon", item.technique != null ? item.technique.icon : 0);
                    m.SetColor("_Color", item.technique != null ? item.technique.color : Color.white);
                    break;
                case MetaItemType.GameMode:
                    m.SetFloat("_Kind", MetaProgress.IsUnlocked(item) ? KindStar : KindLocked);
                    m.SetColor("_Color", new Color(3.2f, 1.8f, 0.6f));
                    m.SetColor("_Color2", new Color(0.6f, 2.2f, 3.4f));
                    break;
            }
        }
    }
}
