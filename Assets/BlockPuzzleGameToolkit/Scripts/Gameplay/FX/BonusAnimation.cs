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

using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay.FX
{
    public class BonusAnimation : MonoBehaviour
    {
        public AnimationCurve curveX;
        public AnimationCurve curveY;
        public AnimationCurve scaleCurveX;
        public AnimationCurve scaleCurveY;
        public Vector2 targetPos;
        private Vector3 originPos;
        private Bonus bonusItem;
        public GameObject sparklePrefab;

        public TweenCallback<BonusItemTemplate> OnFinish;

        private void Awake()
        {
            bonusItem = GetComponent<Bonus>();
        }

        private void OnEnable()
        {
            Appear();
        }

        private void Appear()
        {
            DOTween.To(() => 0f, value =>
            {
                var evaluatedScaleX = scaleCurveX.Evaluate(value);
                var evaluatedScaleY = scaleCurveY.Evaluate(value);
                transform.localScale = new Vector2(evaluatedScaleX, evaluatedScaleY);
            }, 1f, .3f).SetEase(Ease.Linear);
        }

        public void MoveTo()
        {
            originPos = transform.position;
            transform.DORotate(new Vector3(0, 0, 145), .6f, RotateMode.FastBeyond360).SetEase(Ease.Linear);

            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => 0f, value =>
            {
                var evaluatedX = curveX.Evaluate(value);
                var evaluatedY = curveY.Evaluate(value);
                transform.position = new Vector3(originPos.x + evaluatedX * (targetPos.x - originPos.x), originPos.y + evaluatedY * (targetPos.y - originPos.y), transform.position.z);
            }, 1f, .6f).SetEase(Ease.Linear).OnComplete(Finish));
            sequence.Join(transform.DOScale(Vector3.one * 2.8f, .6f).SetEase(Ease.Linear));
        }

        private void Finish()
        {
            PoolObject.GetObject(sparklePrefab).transform.position = transform.position;
            OnFinish(bonusItem.bonusItemTemplate);
            PoolObject.Return(gameObject);
        }

        public void Fill(BonusItemTemplate getBonusItem)
        {
            bonusItem.FillIcon(getBonusItem);
        }
    }
}