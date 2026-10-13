using System.Threading.Tasks;
using Soulbreak.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Main menu account button + login popup. Guests can create an account (keeping their progress)
    // or log in to an existing one; the popup opens once by itself for new players.
    public class AccountView : MonoBehaviour
    {
        private const string PromptedKey = "soulbreak.account.prompted";

        [SerializeField] private Button accountButton;
        [SerializeField] private Text accountLabel;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private InputField usernameField, passwordField;
        [SerializeField] private Button loginButton, createButton, logoutButton, closeButton;
        [SerializeField] private Text statusText, messageText;
        [SerializeField] private GameObject formRoot;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        public bool IsOpen { get; private set; }
        private bool busy;
        private float promptTimer = 1.6f;

        private void Awake()
        {
            accountButton.onClick.AddListener(Open);
            closeButton.onClick.AddListener(Close);
            loginButton.onClick.AddListener(() => _ = Run(() => AccountService.LogIn(usernameField.text, passwordField.text), S.accountLoginSuccess));
            createButton.onClick.AddListener(() => _ = Run(() => AccountService.CreateAccount(usernameField.text.Trim(), passwordField.text), S.accountCreateSuccess));
            logoutButton.onClick.AddListener(() => _ = LogOut());
        }

        private void OnEnable() => AccountService.OnAccountChanged += Refresh;
        private void OnDisable() => AccountService.OnAccountChanged -= Refresh;

        private async void Start()
        {
            Refresh();
            await AccountService.EnsureSignedIn();
            if (this != null) Refresh();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            messageText.text = "";
            passwordField.text = "";
            Refresh();
            Audio.UIAudio.Play(Audio.Sfx.UIOpen);
        }

        public void Close()
        {
            if (!IsOpen || busy) return;
            IsOpen = false;
            Audio.UIAudio.Play(Audio.Sfx.UIClose);
        }

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.blocksRaycasts = group.interactable = IsOpen;
            panel.localScale = Vector3.one * UIFactory.PopScale(group.alpha);

            // First visit: offer an account once.
            if (promptTimer > 0f && (promptTimer -= Time.unscaledDeltaTime) <= 0f
                && AccountService.IsGuest && PlayerPrefs.GetInt(PromptedKey, 0) == 0)
            {
                PlayerPrefs.SetInt(PromptedKey, 1);
                PlayerPrefs.Save();
                Open();
            }
        }

        private async Task Run(System.Func<Task<string>> action, string success)
        {
            if (busy) return;
            busy = true;
            SetInteractable(false);
            messageText.color = T.neutralText;
            messageText.text = S.accountWaiting;
            string error = await action();
            if (this == null) return;
            busy = false;
            SetInteractable(true);
            messageText.color = error == null ? T.good : T.bad;
            messageText.text = error ?? success;
            if (error == null) passwordField.text = "";
            Audio.UIAudio.Play(error == null ? Audio.Sfx.Buy : Audio.Sfx.Fail);
            Refresh();
        }

        private async Task LogOut()
        {
            if (busy) return;
            busy = true;
            SetInteractable(false);
            await AccountService.LogOut();
            if (this == null) return;
            busy = false;
            SetInteractable(true);
            messageText.color = T.neutralText;
            messageText.text = S.accountLoggedOut;
            Refresh();
        }

        private void SetInteractable(bool on)
        {
            loginButton.interactable = createButton.interactable = logoutButton.interactable = closeButton.interactable = on;
            usernameField.interactable = passwordField.interactable = on;
        }

        private void Refresh()
        {
            bool online = AccountService.Status == AccountService.State.Online;
            bool guest = AccountService.IsGuest;
            accountLabel.text = !online && AccountService.Status != AccountService.State.Connecting ? S.accountOffline
                : guest ? S.accountSignIn : AccountService.Username.ToUpper();

            statusText.text = guest
                ? S.accountGuestStatus
                : string.Format(S.accountLoggedInStatus, AccountService.Username);
            formRoot.SetActive(guest);
            logoutButton.gameObject.SetActive(!guest);
        }
    }
}
