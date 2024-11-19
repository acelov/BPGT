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

using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.System;
using DG.Tweening;
using TMPro;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class PreFailed : PopupWithCurrencyLabel
    {
        public TextMeshProUGUI continuePrice;
        public TextMeshProUGUI timerText;
        private int timer;

        public CustomButton continueButton;
        public CustomButton rewardButton;
        private int price;

        private void OnEnable()
        {
            price = GameManager.instance.GameSettings.continuePrice;
            continuePrice.text = price.ToString();
            continueButton.onClick.AddListener(Continue);
            timer = GameManager.instance.GameSettings.failedTimerStart;
            timerText.text = timer.ToString();
            SoundBase.instance.PlaySound(SoundBase.instance.warningTime);
            InvokeRepeating(nameof(UpdateTimer), 1, 1);
        }

        private void UpdateTimer()
        {
            if (MenuManager.instance.GetLastPopup() == this)
            {
                timer--;
            }
            else
            {
                timer = GameManager.instance.GameSettings.failedTimerStart;
            }

            timerText.text = timer.ToString();
            if (timer <= 0)
            {
                continueButton.interactable = false;
                rewardButton.interactable = false;

                CancelInvoke(nameof(UpdateTimer));
                EventManager.GameStatus = EGameState.Failed;
                Close();
            }
        }

        public void PauseTimer()
        {
            CancelInvoke(nameof(UpdateTimer));
        }

        private void Continue()
        {
            var coinsResource = ResourceManager.instance.GetResource("Coins");
            if (coinsResource.Consume(price))
            {
                CancelInvoke(nameof(UpdateTimer));
                ShowCoinsSpendFX(continueButton.transform.position);
                StopInteration();
                DOVirtual.DelayedCall(0.5f, ContinueGame);
            }
        }

        public void ContinueGame()
        {
            result = EPopupResult.Continue;
            EventManager.GameStatus = EGameState.Playing;
            Close();
        }
    }
}