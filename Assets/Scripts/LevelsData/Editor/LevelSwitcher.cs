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

using System.IO;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData.Editor
{
    public class LevelSwitcher : VisualElement
    {
        private IntegerField levelNumberField;
        private readonly int num;
        private readonly LevelEditor levelEditor;
        private readonly Level level;

        public LevelSwitcher(SerializedObject levelSerializedObject, Level level, LevelEditor levelEditor)
        {
            this.level = level;
            num = level.Number;
            this.levelEditor = levelEditor;
            Draw(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIBuilder/LevelSwitcher.uxml").Instantiate());
        }

        private void Draw(TemplateContainer visualTree)
        {
            visualTree.Q<Button>("PlayButton").clickable.clicked += PlayLevel;
            visualTree.Q<Button>("PrevLevel").clickable.clicked += OpenPrevLevel;
            visualTree.Q<Button>("NextLevel").clickable.clicked += OpenNextLevel;
            visualTree.Q<Button>("NewLevel").clickable.clicked += NewLevel;
            visualTree.Q<Button>("DelLevel").clickable.clicked += DelLevel;
            visualTree.Q<Button>("Save").clickable.clicked += Save;
            levelNumberField = visualTree.Q<IntegerField>("LevelNum");
            levelNumberField.value = num;
            levelNumberField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    SwitchLevel(levelNumberField.value);
                }
            });

            Add(visualTree);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }

        private void PlayLevel()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                levelEditor.Save();
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                EditorApplication.isPlaying = true;
            }
        }

        private void DelLevel()
        {
            var levelsNum = GetFiles().Length;
            if (levelsNum > 1)
            {
                var scenePath = $"Assets/Resources/Levels/Level_{levelsNum}.asset";
                if (File.Exists(scenePath))
                {
                    File.Delete(scenePath);
                    SwitchLevel(levelsNum - 1);
                    AssetDatabase.Refresh();
                }
            }
        }

        private void OpenPrevLevel()
        {
            if (num > 1)
            {
                SwitchLevel(num - 1);
            }
        }

        private void OpenNextLevel()
        {
            SwitchLevel(num + 1);
        }

        private void SwitchLevel(int levelNumber)
        {
            var levels = GetFiles();
            if (levelNumber > 0 && levelNumber <= levels.Length)
            {
                var nextLevel = levels[levelNumber - 1];
                Selection.activeObject = nextLevel;
            }
        }

        private void Save()
        {
            levelEditor.Save();
        }

        private void NewLevel()
        {
            var levelsNum = GetFiles().Length + 1;
            var newLevel = ScriptableObject.CreateInstance<Level>();
            newLevel.name = $"Level_{levelsNum}";
            newLevel.levelType = level.levelType;
            newLevel.UpdateTargets();
            var path = $"Assets/BlockPuzzleGameToolkit/Resources/Levels/Level_{levelsNum}.asset";
            AssetDatabase.CreateAsset(newLevel, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SwitchLevel(levelsNum);
        }

        private Level[] GetFiles()
        {
            return Resources.LoadAll<Level>("Levels").OrderBy(l => l.Number).ToArray();
        }
    }
}