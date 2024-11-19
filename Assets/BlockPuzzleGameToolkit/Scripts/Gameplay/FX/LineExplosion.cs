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
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay.FX
{
    public class LineExplosion : MonoBehaviour
    {
        private ParticleSystem[] _particleSystem;
        public Image image;
        public RectTransform _rectTransform;
        private RectTransform[] _particleRectTransform;

        [SerializeField]
        private float AnimationDuration = 0.2f;

        private void Awake()
        {
            _particleSystem = GetComponentsInChildren<ParticleSystem>();
            _particleRectTransform = new RectTransform[_particleSystem.Length];
            for (var i = 0; i < _particleSystem.Length; i++)
            {
                _particleRectTransform[i] = _particleSystem[i].GetComponent<RectTransform>();
            }
        }

        private void Init(Vector3 center, Vector2 sizeInLocalSpace, Color color)
        {
            _rectTransform.anchoredPosition = center;
            _rectTransform.sizeDelta = Vector2.zero; // Start from size 0
            image.color = color;
            foreach (var particleSystem in _particleSystem)
            {
                var main = particleSystem.main;
                main.startColor = color;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(_rectTransform.DOSizeDelta(new Vector2(sizeInLocalSpace.x, 20), AnimationDuration));
            sequence.AppendCallback(() => _particleSystem[0].Play());
            sequence.Append(_rectTransform.DOSizeDelta(sizeInLocalSpace, AnimationDuration));
            sequence.Append(image.DOFade(0, AnimationDuration));
            sequence.AppendCallback(() =>
            {
                foreach (var particleSystem in _particleSystem)
                {
                    particleSystem.Stop();
                }

                PoolObject.Return(gameObject);
            });
        }

        public void Play(List<Cell> cells, Shape shape, (Vector3 min, Vector3 max, Vector2 size, Vector2 center) getMinMaxAndSizeForCanvas, Color itemTemplateTopColor)
        {
            if (cells == null || cells.Count == 0)
            {
                return;
            }

            var (min, max, sizeInLocalSpace, centerLocalPoint) = getMinMaxAndSizeForCanvas;
            var padding = 40f;
            sizeInLocalSpace += new Vector2(padding, padding);
            if (Mathf.Abs(min.y - max.y) > 1f)
            {
                _particleSystem[0].transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                _particleSystem[0].transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            Init(centerLocalPoint, sizeInLocalSpace, itemTemplateTopColor);
        }
    }
}