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
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public partial class LevelManager
    {
        private void HandleGameStateChange(EGameState newState)
        {
            switch (newState)
            {
                case EGameState.PrepareGame:
                    Debug.Log("Game is preparing...");
                    var levelTypePrePlayPopup = GetCurrentLevel().levelType.prePlayPopup;
                    if (levelTypePrePlayPopup != null)
                    {
                        MenuManager.instance.ShowPopup(levelTypePrePlayPopup, null, _ => EventManager.GameStatus = EGameState.Playing);
                    }
                    else
                    {
                        EventManager.GameStatus = EGameState.Playing;
                    }

                    break;
                case EGameState.Playing:
                    Debug.Log("Game has started!");


                    break;
                case EGameState.PreFailed:
                    Debug.Log("Game is about to end...");
                    var preFailedPopup = GetCurrentLevel().levelType.preFailedPopup;
                    StartCoroutine(EndAnimations(() =>
                    {
                        if (preFailedPopup != null)
                        {
                            MenuManager.instance.ShowPopup(preFailedPopup, ClearEmptyCells, result =>
                            {
                                if (result == EPopupResult.Continue)
                                {
                                    cellDeck.UpdateCellDeckAfterFail();
                                }
                            });
                        }
                        else
                        {
                            EventManager.GameStatus = EGameState.Failed;
                        }
                    }));

                    break;
                case EGameState.Failed:
                    Debug.Log("Game has ended!");
                    var failedPopup = GetCurrentLevel().levelType.failedPopup;
                    if (failedPopup != null)
                    {
                        MenuManager.instance.ShowPopup(failedPopup);
                    }

                    break;
                case EGameState.PreWin:
                    Debug.Log("Game is about to end...");
                    var levelTypePreWinPopup = GetCurrentLevel().levelType.preWinPopup;
                    if (levelTypePreWinPopup != null)
                    {
                        MenuManager.instance.ShowPopup(levelTypePreWinPopup, null, _ => EventManager.GameStatus = EGameState.Win);
                    }
                    else
                    {
                        EventManager.GameStatus = EGameState.Win;
                    }

                    break;
                case EGameState.Win:
                    Debug.Log("Game has ended!");
                    MenuManager.instance.ShowPopup(GetCurrentLevel().levelType.winPopup);
                    break;
                default:
                    Debug.Log($"Game state changed to: {newState}");
                    break;
            }
        }
    }
}