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

using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class Settings : PopupWithCurrencyLabel
    {
        [SerializeField]
        private CustomButton back;

        // privacypolicy button
        [SerializeField]
        private CustomButton privacypolicy;

        //shop button
        [SerializeField]
        private CustomButton shop;

        [SerializeField]
        private CustomButton retryButton;

        [SerializeField]
        private Slider vibrationSlider;

        private MenuManager menuManager;

        private const string VibrationPrefKey = "VibrationLevel";

        private void OnEnable()
        {
            back.onClick.AddListener(BackToMain);
            privacypolicy.onClick.AddListener(PrivacyPolicy);
            shop.onClick.AddListener(Shop);
            retryButton.onClick.AddListener(Retry);
            retryButton.gameObject.SetActive(StateManager.instance.CurrentState == EScreenStates.Game);

            // Load the saved vibration level
            LoadVibrationLevel();

            // Register the OnValueChanged event
            vibrationSlider.onValueChanged.AddListener(SaveVibrationLevel);
            menuManager = GetComponentInParent<MenuManager>();
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(BackToGame);
        }

        private void BackToGame()
        {
            DisablePause();
            Close();
        }

        private void OnDisable()
        {
            // Unregister the OnValueChanged event
            vibrationSlider.onValueChanged.RemoveListener(SaveVibrationLevel);
        }

        private void SaveVibrationLevel(float value)
        {
            PlayerPrefs.SetFloat(VibrationPrefKey, value);
            PlayerPrefs.Save();
        }

        private void LoadVibrationLevel()
        {
            if (PlayerPrefs.HasKey(VibrationPrefKey))
            {
                vibrationSlider.value = PlayerPrefs.GetFloat(VibrationPrefKey);
            }
            else
            {
                vibrationSlider.value = 1.0f;
                SaveVibrationLevel(1.0f);
            }
        }

        private void Retry()
        {
            GameManager.instance.RestartLevel();
            MenuManager.instance.FadeOut();
        }

        private void Shop()
        {
            StopInteration();

            DisablePause();
            MenuManager.instance.ShowPopup<CoinsShop>();
            Close();
        }

        private void PrivacyPolicy()
        {
            StopInteration();

            DisablePause();
            MenuManager.instance.ShowPopup<GDPR>();
            Close();
        }

        private void DisablePause()
        {
            if (StateManager.instance.CurrentState == EScreenStates.Game)
            {
                EventManager.GameStatus = EGameState.Playing;
            }
        }

        private void BackToMain()
        {
            StopInteration();

            Close();
            GameManager.instance.MainMenu();
        }
    }
}