using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YsoCorp {
    namespace GameUtils {

        [DefaultExecutionOrder(-10)]
        public class SettingManager : BaseManager {

            private static Color COLOR_ON = Color.white;
            private static Color COLOR_OFF = new Color(0.7f, 0.7f, 0.7f, 1f);

            public GameObject content;
            public Button bClose;
            public Button bCloseBlackBG;
            public Button bRestorePurchase;
            public Button bDataPrivacy;
            public GameObject panelBts;
            public Button bLang;
            public Transform langsContent;
            public List<LangSelector> langSelectors;
            public Text tVersion;
            public Button bVersion;

            public SettingsDebugWindow debugWindow;

            private float _versionLastClick;

            private void Awake() {
                this.UpdateCanvasScaler();
                this.ycManager.settingManager = this;
                this.bRestorePurchase.gameObject.SetActive(this.ycManager.ycConfig.HasInApps());
#if UNITY_ANDROID && !UNITY_EDITOR
                this.bRestorePurchase.gameObject.SetActive(false);
#endif
                this.bLang.gameObject.SetActive(false);
                this.tVersion.text = "v" + Application.version + "  sdk" + YCConfig.VERSION;
                if (this.ycManager.abTestingManager.GetPlayerSample() != "") {
                    this.tVersion.text += " (" + this.ycManager.abTestingManager.GetPlayerSample() + ")";
                }
                this.bVersion.onClick.AddListener(() => {
                    if (Time.unscaledTime - this._versionLastClick < 0.3f) {
                        this.debugWindow.gameObject.SetActive(true);
                    }
                    this._versionLastClick = Time.unscaledTime;
                });
            }

            private void Start() {
                this.InitLangs();
                this.panelBts.gameObject.SetActive(
                    this.bLang.gameObject.activeSelf
                );
                this.bClose.onClick.AddListener(() => {
                    this.content.SetActive(false);
                });
                this.bCloseBlackBG.onClick.AddListener(() => {
                    this.content.SetActive(false);
                });
                this.bRestorePurchase.onClick.AddListener(() => {
                    this.ycManager.inAppManager.RestorePurchases();
                });
                this.bDataPrivacy.onClick.AddListener(() => {
                    this.ycManager.adsManager.DisplayGDPR();
                });
                this.debugWindow.Init();
            }

            private void InitLangs() {
                int langCount = this.ycManager.i18nManager.i18NResourcesManager.i18ns.Count;
                if (langCount > 1) {
                    this.bLang.gameObject.SetActive(true);
                    this.bLang.image.sprite = this.ycManager.i18nManager.GetCurrentSprite();
                    foreach (KeyValuePair<string, Dictionary<string, string>> lang in this.ycManager.i18nManager.i18NResourcesManager.i18ns) {
                        LangSelector selector = this.langSelectors[0];
                        if (lang.Key != "EN") {
                            selector = Instantiate(this.langSelectors[0], langsContent);
                        }
                        selector.Init(lang.Key, () => {
                            this.bLang.image.sprite = this.ycManager.i18nManager.GetCurrentSprite();
                            this.langsContent.gameObject.SetActive(false);
                        });
                    }
                    this.bLang.onClick.AddListener(() => {
                        this.langsContent.gameObject.SetActive(true);
                    });
                }
            }

            public void UpdateCanvasScaler() {
                    this.GetComponent<CanvasScaler>().matchWidthOrHeight = (Screen.width > Screen.height) ? 0.69f : 0f;
            }

            /// <summary>
            /// Displays the settings window.
            /// </summary>
            public void Show() {
                this.content.SetActive(true);
                this.langsContent.gameObject.SetActive(false);
            }

        }
    }
}