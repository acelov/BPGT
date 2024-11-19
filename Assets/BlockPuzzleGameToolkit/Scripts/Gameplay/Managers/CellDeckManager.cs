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
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class CellDeckManager : MonoBehaviour
    {
        public CellDeck[] cellDecks;

        [SerializeField]
        private FieldManager field;

        [SerializeField]
        private ItemFactory itemFactory;

        [SerializeField]
        public Shape shapePrefab;

        private void OnEnable()
        {
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(FillCellDecks);
        }

        private void OnDisable()
        {
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(FillCellDecks);
        }

        public void FillCellDecks(Shape shape = null)
        {
            RemoveUsedShapes(shape);

            if (GameManager.instance.IsTutorialMode())
            {
                return;
            }

            if (cellDecks.Any(x => !x.IsEmpty))
            {
                return;
            }

            var usedShapeTemplates = new HashSet<ShapeTemplate>(GetShapes().Select(s => s.shapeTemplate));

            var haveFitShape = false;
            for (var index = 0; index < cellDecks.Length; index++)
            {
                var cellDeck = cellDecks[index];
                if (cellDeck.IsEmpty)
                {
                    var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    Shape randomShape = null;
                    if (!haveFitShape && index == cellDecks.Length - 1)
                    {
                        randomShape = itemFactory.CreateRandomShapeFits(shapeObject);
                    }
                    else
                    {
                        randomShape = itemFactory.CreateRandomShape(usedShapeTemplates, shapeObject);
                    }

                    if (field.CanPlaceShape(randomShape))
                    {
                        haveFitShape = true;
                    }

                    cellDeck.FillCell(randomShape);
                }
            }
        }

        public void FillCellDecksWithShapes(ShapeTemplate[] shapes)
        {
            if (shapes == null || shapes.Length == 0)
            {
                return;
            }

            // Clear existing shapes from the cell decks
            ClearCellDecks();

            for (var index = 0; index < cellDecks.Length && index < shapes.Length; index++)
            {
                var cellDeck = cellDecks[index];
                if (cellDeck.IsEmpty)
                {
                    var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    var shape = shapeObject.GetComponent<Shape>();

                    var shapeTemplate = shapes[index];
                    shape.UpdateShape(shapeTemplate);
                    shape.UpdateColor(itemFactory.GetColor());

                    cellDeck.FillCell(shape);
                }
            }
        }

        private void RemoveUsedShapes(Shape shape)
        {
            if (shape == null)
            {
                return;
            }

            foreach (var cellDeck in cellDecks)
            {
                if (cellDeck.shape == shape)
                {
                    cellDeck.FillCell(null);
                    PoolObject.Return(shape.gameObject);
                }
            }
        }

        public void ClearCellDecks()
        {
            foreach (var cellDeck in cellDecks)
            {
                cellDeck.ClearCell();
            }
        }

        public Shape[] GetShapes()
        {
            return cellDecks.Select(x => x.shape).Where(x => x != null).ToArray();
        }

        public void UpdateCellDeckAfterFail()
        {
            foreach (var cellDeck in cellDecks)
            {
                cellDeck.ClearCell();
                cellDeck.FillCell(itemFactory.CreateRandomShapeFits(PoolObject.GetObject(shapePrefab.gameObject)));
            }
        }

        public void OnSceneActivated(Level level)
        {
            if (!GameManager.instance.IsTutorialMode())
            {
                FillFitShapesOnly();
            }
        }

        private void FillFitShapesOnly()
        {
            for (var index = 0; index < cellDecks.Length; index++)
            {
                var cellDeck = cellDecks[index];
                cellDeck.ClearCell();
                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                var randomShape = itemFactory.CreateRandomShapeFits(shapeObject);
                cellDeck.FillCell(randomShape);
            }
        }

        public void AddShapeToFreeCell(ShapeTemplate shapeTemplate)
        {
            foreach (var cellDeck in cellDecks)
            {
                if (cellDeck.IsEmpty)
                {
                    var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    var shape = shapeObject.GetComponent<Shape>();
                    shape.UpdateShape(shapeTemplate);
                    shape.UpdateColor(itemFactory.GetColor());
                    cellDeck.FillCell(shape);
                    return;
                }
            }
        }
    }
}