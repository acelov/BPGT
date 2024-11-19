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
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class Cell : MonoBehaviour
    {
        public Item item;
        private CanvasGroup group;
        public bool busy;
        private ItemTemplate saveTemplate;
        private BoxCollider2D _boxCollider2D;

        public Image image;

        private bool isEmpty => !busy;
        private bool IsEmptyPreview => group.alpha == 0;

        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            group = item.GetComponent<CanvasGroup>();
        }

        public void FillCell(ItemTemplate itemTemplate)
        {
            item.FillIcon(itemTemplate);
            group.alpha = 1;
            busy = true;
        }

        public void FillCellFailed(ItemTemplate itemTemplate)
        {
            item.FillIcon(itemTemplate);
            group.alpha = 1;
        }

        public bool IsEmpty(bool preview = false)
        {
            return preview ? IsEmptyPreview : isEmpty;
        }

        public void ClearCell()
        {
            item.transform.localScale = Vector3.one;
            if (saveTemplate == null && !busy)
            {
                group.alpha = 0;
                busy = false;
            }
            else if (saveTemplate != null && busy)
            {
                FillCell(saveTemplate);
                saveTemplate = null;
            }
        }

        public void HighlightCell(ItemTemplate itemTemplate)
        {
            item.FillIcon(itemTemplate);
            group.alpha = 0.05f;
        }

        public void HighlightCellTutorial()
        {
            image.color = new Color(43f / 255f, 59f / 255f, 120f / 255f, 1f);
        }

        public void HighlightCellFill(ItemTemplate itemTemplate)
        {
            saveTemplate = item.itemTemplate;
            if (!item.HasBonusItem())
            {
                item.FillIcon(itemTemplate);
            }

            group.alpha = 1f;
        }

        public void DestroyCell()
        {
            saveTemplate = null;
            busy = false;
            item.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                ClearCell();
                item.ClearBonus();
            });
        }

        public Bounds GetBounds()
        {
            return _boxCollider2D.bounds;
        }

        public void InitItem()
        {
            _boxCollider2D.size = transform.GetComponent<RectTransform>().sizeDelta;
            item.name = "Item " + name;
            StartCoroutine(UpdateItem());
        }

        private IEnumerator UpdateItem()
        {
            yield return new WaitForSeconds(0.1f);
            // item.transform.SetParent(GameObject.Find("ItemsCanvas/Items").transform);
            item.transform.position = transform.position;
        }

        public void SetBonus(BonusItemTemplate bonusItemTemplate)
        {
            item.SetBonus(bonusItemTemplate);
        }

        public bool HasBonusItem()
        {
            return item.HasBonusItem();
        }

        public BonusItemTemplate GetBonusItem()
        {
            return item.bonusItemTemplate;
        }

        public void AnimateFill()
        {
            item.transform.DOScale(Vector3.one * 0.5f, 0.1f).OnComplete(() => { item.transform.DOScale(Vector3.one, 0.1f); });
        }

        public void DisableCell()
        {
            _boxCollider2D.enabled = false;
        }

        public bool IsDisabled()
        {
            return !_boxCollider2D.enabled;
        }

        public bool IsHighlighted()
        {
            return !IsDisabled();
        }
    }
}