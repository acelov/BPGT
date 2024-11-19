// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Globalization;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Popups.Daily;
using BlockPuzzleGameToolkit.Scripts.Services;
using BlockPuzzleGameToolkit.Scripts.Services.IAP;
using BlockPuzzleGameToolkit.Scripts.Settings;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.System
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public Action<string> purchaseSucceded;
        public DebugSettings debugSettings;
        public DailyBonusSettings dailyBonusSettings;
        public GameSettings GameSettings;
        public SpinSettings luckySpinSettings;
        public CoinsShopSettings coinsShopSettings;
        private (string id, ProductTypeWrapper.ProductType productType)[] products;
        private int lastBackgroundIndex = -1;
        private bool isTutorialMode;
        private MainMenu mainMenu;

        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            Application.targetFrameRate = 60;
            DOTween.SetTweensCapacity(1250, 512);

            mainMenu = FindObjectOfType<MainMenu>();
            if (mainMenu != null)
            {
                mainMenu.OnAnimationEnded += OnMainMenuAnimationEnded;
            }
        }

        private void OnEnable()
        {
            IAPManager.SubscribeToPurchaseEvent(PurchaseSucceeded);
            if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {
                if (CheckDailyBonusConditions())
                {
                    CustomButton.BlockInput(true);
                }
            }

            if (!IsTutorialShown() && !GameDataManager.isTestPlay)
            {
                SetTutorialMode(true);
            }
        }

        private void OnDisable()
        {
            IAPManager.UnsubscribeFromPurchaseEvent(PurchaseSucceeded);
            if (mainMenu != null)
            {
                mainMenu.OnAnimationEnded -= OnMainMenuAnimationEnded;
            }
            GameDataManager.isTestPlay = false; // Reset isTestPlay
        }

        private bool IsTutorialShown()
        {
            return PlayerPrefs.GetInt("tutorial", 0) == 1;
        }

        public void SetTutorialCompleted()
        {
            PlayerPrefs.SetInt("tutorial", 1);
            PlayerPrefs.Save();
        }

        private async void Start()
        {
            products = Resources.LoadAll<ProductID>("ProductIDs")
                .Select(p => (p.ID, p.productType))
                .ToArray();

            // Initialize gaming services
            await InitializeGamingServices.instance?.Initialize(
                OnInitializeSuccess,
                OnInitializeError
            );
            // Initialize IAP directly if InitializeGamingServices is not used
            await IAPManager.instance?.InitializePurchasing(products);
            if (IsNoAdsPurchased())
            {
                AdsManager.instance.RemoveAds();
            }

            if (GameDataManager.isTestPlay)
            {
                GameDataManager.SetLevel(GameDataManager.GetLevel());
            }
        }

        private void OnInitializeSuccess()
        {
            Debug.Log("Gaming services initialized successfully");
        }

        private void OnInitializeError(string errorMessage)
        {
            Debug.LogError($"Failed to initialize gaming services: {errorMessage}");
        }

        private void HandleDailyBonus()
        {
            if (StateManager.instance.CurrentState != EScreenStates.MainMenu)
            {
                return;
            }

            var shouldShowDailyBonus = CheckDailyBonusConditions();

            if (shouldShowDailyBonus)
            {
                var daily = MenuManager.instance.ShowPopup<DailyBonus>(()=>
                {
                    CustomButton.BlockInput(false);
                });
            }
        }

        private bool CheckDailyBonusConditions()
        {
            var today = DateTime.Today;
            var lastRewardDate = DateTime.Parse(PlayerPrefs.GetString("DailyBonusDay", today.Subtract(TimeSpan.FromDays(1)).ToString(CultureInfo.CurrentCulture)));
            return today.Date > lastRewardDate.Date;
        }

        public void RestartLevel()
        {
            DOTween.KillAll();
            MenuManager.instance.CloseAllPopups();
            EventManager.GetEvent(EGameEvent.RestartLevel).Invoke();
        }

        public void RemoveAds()
        {
            MenuManager.instance.ShowPopup<NoAds>();
        }

        public void MainMenu()
        {
            DOTween.KillAll();
            if (StateManager.instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.instance.GoMain();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Adventure)
            {
                SceneLoader.instance.StartMapScene();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.Map)
            {
                SceneLoader.instance.GoMain();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {
                MenuManager.instance.ShowPopup<Quit>();
            }
            else
            {
                SceneLoader.instance.GoMain();
            }
        }

        public void OpenMap()
        {
            if (GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.instance.StartGameSceneClassic();
            }
            else
            {
                SceneLoader.instance.StartMapScene();
            }
        }

        public void OpenGame()
        {
            SceneLoader.instance.StartGameScene();
        }

        public void PurchaseSucceeded(string id)
        {
            purchaseSucceded?.Invoke(id);
        }

        public bool IsNoAdsPurchased()
        {
            return IAPManager.instance?.IsProductPurchased("noAds") ?? false;
        }

        public void SetGameMode(EGameMode gameMode)
        {
            GameDataManager.SetGameMode(gameMode);
        }

        private EGameMode GetGameMode()
        {
            return GameDataManager.GetGameMode();
        }

        public int GetLastBackgroundIndex()
        {
            return lastBackgroundIndex;
        }

        public void SetLastBackgroundIndex(int index)
        {
            lastBackgroundIndex = index;
        }

        public void NextLevel()
        {
            GameDataManager.LevelNum++;
            OpenGame();
            RestartLevel();
        }

        public void SetTutorialMode(bool tutorial)
        {
            Debug.Log("Tutorial mode set to " + tutorial);
            isTutorialMode = tutorial;
        }

        public bool IsTutorialMode()
        {
            return isTutorialMode;
        }

        private void OnMainMenuAnimationEnded()
        {
            if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {

                HandleDailyBonus();
            }
        }
    }
}