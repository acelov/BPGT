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
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.GUI;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    [RequireComponent(typeof(Animator), typeof(CanvasGroup))]
    public class Popup : MonoBehaviour
    {
        public bool fade = true;
        private Animator animator;
        public CustomButton closeButton;
        private CanvasGroup canvasGroup;
        public Action OnShowAction;
        public Action<EPopupResult> OnCloseAction;
        protected EPopupResult result;

        public delegate void PopupEvents(Popup popup);

        public static event PopupEvents OnOpenPopup;
        public static event PopupEvents OnClosePopup;
        public static event PopupEvents OnBeforeCloseAction;


        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        public void Show<T>(Action onShow = null, Action<EPopupResult> onClose = null)
        {
            if (onShow != null)
            {
                OnShowAction = onShow;
            }

            if (onClose != null)
            {
                OnCloseAction = onClose;
            }

            OnOpenPopup?.Invoke(this);
            PlayShowAnimation();
        }

        private void PlayShowAnimation()
        {
            if (animator != null)
            {
                animator.Play("popup_show");
            }
        }

        public virtual void ShowAnimationSound()
        {
            SoundBase.instance.PlaySound(SoundBase.instance.swish[0]);
        }

        public virtual void AfterShowAnimation()
        {
            OnShowAction?.Invoke();
        }

        public virtual void CloseAnimationSound()
        {
            SoundBase.instance.PlayDelayed(SoundBase.instance.swish[1], .0f);
        }

        public virtual void Close()
        {
            if (closeButton)
            {
                closeButton.interactable = false;
            }

            canvasGroup.interactable = false;
            OnBeforeCloseAction?.Invoke(this);
            if (animator != null)
            {
                animator.Play("popup_hide");
            }
        }

        public virtual void AfterHideAnimation()
        {
            OnClosePopup?.Invoke(this);
            OnCloseAction?.Invoke(result);
            Destroy(gameObject, .5f);
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }

        public void Show()
        {
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1, 0.1f);
        }

        public virtual void Hide()
        {
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0, 0.5f);
        }

        public void CloseDelay()
        {
            Invoke(nameof(Close), 0.5f);
        }

        protected void StopInteration()
        {
            canvasGroup.interactable = false;
        }
    }
}