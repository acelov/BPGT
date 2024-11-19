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

using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    public class BackgroundChanger : MonoBehaviour, ILevelLoadable
    {
        public Sprite[] backgrounds;

        public void OnLevelLoaded(Level level)
        {
            var lastBackgroundIndex = GameManager.instance.GetLastBackgroundIndex();
            int newBackgroundIndex;
            do
            {
                newBackgroundIndex = Random.Range(0, backgrounds.Length);
            } while (newBackgroundIndex == lastBackgroundIndex);

            GameManager.instance.SetLastBackgroundIndex(newBackgroundIndex);
            GetComponent<Image>().sprite = backgrounds[newBackgroundIndex];
        }
    }
}