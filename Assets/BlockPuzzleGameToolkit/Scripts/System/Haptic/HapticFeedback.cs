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

using System.Runtime.InteropServices;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.System.Haptic
{
    public class HapticFeedback : MonoBehaviour
    {
        public enum HapticForce
        {
            Light,
            Medium,
            Heavy
        }

        private const string VibrationPrefKey = "VibrationLevel";

        #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _TriggerHapticFeedback(int force);
        #endif

        public static void TriggerHapticFeedback(HapticForce force)
        {
            if (!IsVibrationEnabled())
            {
                return;
            }

            #if UNITY_EDITOR
            return;
            #endif

            #if UNITY_IOS
            _TriggerHapticFeedback((int)force);
            #elif UNITY_ANDROID
            long[] pattern;
            switch (force)
            {
                case HapticForce.Light:
                    pattern = new long[] { 0, 50 };
                    break;
                case HapticForce.Medium:
                    pattern = new long[] { 0, 100 };
                    break;
                case HapticForce.Heavy:
                    pattern = new long[] { 0, 200 };
                    break;
                default:
                    pattern = new long[] { 0, 50 };
                    break;
            }

            Vibration.Vibrate(pattern, -1);
            #endif
        }

        private static bool IsVibrationEnabled()
        {
            return PlayerPrefs.HasKey(VibrationPrefKey) && PlayerPrefs.GetFloat(VibrationPrefKey) > 0;
        }
    }
}