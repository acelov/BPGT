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

using BlockPuzzleGameToolkit.Scripts.Data;
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ClassicModeHandler : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI bestScoreText;

        [HideInInspector]
        public int bestScore;

        [HideInInspector]
        public int score;

        private LevelManager _levelManager;

        private void OnEnable()
        {
            bestScoreText.text = ResourceManager.instance.GetResource("Score").GetValue().ToString();
            _levelManager = FindObjectOfType<LevelManager>(true);
            _levelManager.OnLose += OnLose;
            _levelManager.OnScored += OnScored;
        }

        private void OnDisable()
        {
            _levelManager.OnLose -= OnLose;
            _levelManager.OnScored -= OnScored;
        }

        public void OnScored(int score)
        {
            this.score += score;
            scoreText.text = this.score.ToString();
        }

        public void OnLose()
        {
            bestScore = ResourceManager.instance.GetResource("Score").GetValue();
            if (score > bestScore)
            {
                ResourceManager.instance.GetResource("Score").Add(score);
            }
        }
    }
}