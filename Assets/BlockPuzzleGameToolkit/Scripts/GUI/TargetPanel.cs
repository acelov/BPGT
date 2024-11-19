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

using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.System;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class TargetPanel : MonoBehaviour
    {
        public GameObject targetPrefab;
        private readonly Dictionary<TargetScriptable, TargetBonusGUIElement> _list = new();

        private TargetManager targetManager;

        private void OnEnable()
        {
            _list.Clear();
            targetManager = FindObjectOfType<TargetManager>(true);
            if (GetComponentInParent<Popup>() == null)
            {
                OnLevelLoaded(FindObjectOfType<LevelManager>(true).GetCurrentLevel());
                RegisterTargets();
            }
            else
            {
                ShowTargets();
            }
        }

        private void ShowTargets()
        {
            var targets = targetManager.GetTargetGuiElements();
            foreach (var target in targets)
            {
                var targetElement = Instantiate(target.Value, transform);
                targetElement.transform.localScale = Vector3.one * 1.5f;
                targetElement.transform.DOScale(Vector3.zero, 0.5f).From().SetEase(Ease.OutBack);
                if (EventManager.GameStatus == EGameState.PreWin || EventManager.GameStatus == EGameState.Win)
                {
                    targetElement.GetComponent<TargetBonusGUIElement>().TargetCheck();
                }
            }
        }

        private void OnLevelLoaded(Level obj)
        {
            if (obj != null)
            {
                foreach (var target in obj.targetInstance)
                {
                    if (target.amount == 0)
                    {
                        continue;
                    }

                    var targetElement = Instantiate(targetPrefab, transform);
                    var targetBonusGUIElement = targetElement.GetComponent<TargetBonusGUIElement>();
                    targetBonusGUIElement.FillElement(target.targetScriptable.bonusItem, target.amount);
                    _list.Add(target.targetScriptable, targetBonusGUIElement);
                }
            }
        }

        private void RegisterTargets()
        {
            foreach (var target in _list)
            {
                targetManager.RegisterTargetGuiElement(target.Key, target.Value);
            }
        }
    }
}