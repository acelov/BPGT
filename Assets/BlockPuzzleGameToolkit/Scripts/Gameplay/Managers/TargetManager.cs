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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Gameplay.FX;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;
using UnityEngine.Pool;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class TargetManager : MonoBehaviour
    {
        private Level level;
        private List<Target> _levelTargetInstance;
        private Dictionary<TargetScriptable, TargetGUIElement> _targetGuiElements;
        public BonusAnimation bonusAnimationPrefab;
        public TargetsUIHandler targetPanel;
        public Transform targetParent;

        private List<BonusAnimation> _bonusAnimations;
        private ObjectPool<BonusAnimation> bonusAnimationPool;
        public Transform fxPool;

        private void OnEnable()
        {
            bonusAnimationPool = new ObjectPool<BonusAnimation>(
                () => Instantiate(bonusAnimationPrefab, fxPool),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );
        }

        public bool IsLevelComplete()
        {
            return _levelTargetInstance.All(t => t.OnCompleted());
        }

        public void OnLevelLoaded(Level obj)
        {
            level = obj;
            if (level == null)
            {
                return;
            }

            _levelTargetInstance = new List<Target>(level.targetInstance.Where(t => t.amount > 0).Select(t => 
            {
                var target = t.Clone();
                target.totalAmount = t.amount;
                return target;
            }));
            _targetGuiElements = new Dictionary<TargetScriptable, TargetGUIElement>();
            var findObjectOfType = FindObjectOfType<TargetsUIHandler>();
            if (findObjectOfType != null)
            {
                Destroy(findObjectOfType.gameObject);
            }
            if (!GameManager.instance.IsTutorialMode())
            {
                var t = Instantiate(targetPanel, targetParent);
                t.OnLevelLoaded(level.levelType.elevelType);
            }
        }

        public void RegisterTargetGuiElement(TargetScriptable target, TargetGUIElement targetGuiElement)
        {
            _targetGuiElements[target] = targetGuiElement;
            var newCount = _levelTargetInstance.Find(t => t.targetScriptable == target).amount;
            targetGuiElement.UpdateCount(target.descending ? newCount : 0, false);
        }

        public void UpdateTargetCount(Target targetScriptable)
        {
            if (_targetGuiElements.TryGetValue(targetScriptable.targetScriptable, out var targetGuiElement))
            {
                targetGuiElement.UpdateCount(targetScriptable.amount, IsTargetCompleted(targetScriptable));
            }
        }

        private bool IsTargetCompleted(Target targetScriptable)
        {
            return targetScriptable.amount <= 0;
        }

        public List<Target> GetTargets()
        {
            return _levelTargetInstance;
        }

        public Dictionary<TargetScriptable, TargetGUIElement> GetTargetGuiElements()
        {
            return _targetGuiElements;
        }

        public IEnumerator AnimateTarget(List<List<Cell>> lines)
        {
            var bonusItems = new Dictionary<BonusItemTemplate, List<Vector3>>();
            foreach (var cells in lines)
            {
                foreach (var cell in cells)
                {
                    if (cell.HasBonusItem())
                    {
                        var bonusItem = cell.GetBonusItem();
                        if (!bonusItems.ContainsKey(bonusItem))
                        {
                            bonusItems[bonusItem] = new List<Vector3>();
                        }

                        bonusItems[bonusItem].Add(cell.transform.position);
                    }
                }
            }

            _bonusAnimations = new List<BonusAnimation>();
            foreach (var target in _levelTargetInstance)
            {
                if (target.targetScriptable.bonusItem == null)
                {
                    continue;
                }

                foreach (var bonusItem in bonusItems)
                {
                    if (bonusItem.Key == target.targetScriptable.bonusItem)
                    {
                        foreach (var position in bonusItem.Value)
                        {
                            var bonus = bonusAnimationPool.Get();
                            bonus.Fill(target.targetScriptable.bonusItem);
                            bonus.transform.position = position;
                            bonus.targetPos = _targetGuiElements[target.targetScriptable].transform.position;
                            target.amount--;
                            target.amount = Mathf.Max(0, target.amount);
                            bonus.OnFinish = _ =>
                            {
                                UpdateTargetCount(target);
                                _bonusAnimations.Remove(bonus);
                                bonusAnimationPool.Release(bonus);
                            };
                            _bonusAnimations.Add(bonus);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(.3f);
            var bonusAnimationsCopy = new List<BonusAnimation>(_bonusAnimations);
            foreach (var bonusAnimation in bonusAnimationsCopy)
            {
                bonusAnimation.MoveTo();
                yield return new WaitForSeconds(0.2f);
            }

            yield return null;
        }

        public void UpdateScoreTarget(int score)
        {
            var targetScriptable = _levelTargetInstance.Find(t => t.targetScriptable.GetType() == typeof(ScoreTargetScriptable));
            if (targetScriptable != null && _targetGuiElements.TryGetValue(targetScriptable.targetScriptable, out var targetGuiElement))
            {
                var target = _levelTargetInstance.Find(t => t.targetScriptable == targetScriptable.targetScriptable);
                target.amount -= score;
                targetGuiElement.UpdateCount(score, IsTargetCompleted(targetScriptable));
            }
        }

        public bool IsAnimationPlaying()
        {
            return _bonusAnimations is { Count: > 0 };
        }
    }
}