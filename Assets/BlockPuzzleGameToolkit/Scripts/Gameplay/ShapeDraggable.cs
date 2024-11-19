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
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.System.Haptic;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ShapeDraggable : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private Vector2 touchOffset;
        private readonly float verticalOffset = 300;
        private Vector3 originalScale;
        private bool isDragging;
        private int activeTouchId = -1;
        private Canvas canvas;
        private Camera eventCamera;

        private Shape shape;
        private List<Item> _items = new();
        private HighlightManager highlightManager;
        private FieldManager field;
        private ItemFactory itemFactory;

        private void OnEnable()
        {
            itemFactory = FindObjectOfType<ItemFactory>();
            rectTransform = GetComponent<RectTransform>();
            shape = GetComponent<Shape>();
            shape.OnShapeUpdated += UpdateItems;
            UpdateItems();
            highlightManager = FindObjectOfType<HighlightManager>();
            field = FindObjectOfType<FieldManager>();

            // Get canvas and camera reference
            canvas = GetComponentInParent<Canvas>();
            eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        private void OnDisable()
        {
            shape.OnShapeUpdated -= UpdateItems;
            EndDrag();
        }

        private void Update()
        {
            if (EventManager.GameStatus != EGameState.Playing && EventManager.GameStatus != EGameState.Tutorial)
            {
                return;
            }

            if (isDragging && activeTouchId != -1)
            {
                var foundActiveTouch = false;
                foreach (var touch in Input.touches)
                {
                    if (touch.fingerId == activeTouchId)
                    {
                        HandleDrag(touch.position);
                        foundActiveTouch = true;

                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            EndDrag();
                        }

                        break;
                    }
                }

                if (!foundActiveTouch)
                {
                    EndDrag();
                }
            }
            else
            {
                foreach (var touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began && !isDragging)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touch.position, eventCamera))
                        {
                            activeTouchId = touch.fingerId;
                            BeginDrag(touch.position);
                            break;
                        }
                    }
                }
            }

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, eventCamera))
                {
                    BeginDrag(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButton(0) && isDragging && activeTouchId == -1)
            {
                HandleDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isDragging && activeTouchId == -1)
            {
                EndDrag();
            }
            #endif

            // Additional check to ensure EndDrag is called if dragging unexpectedly stops
            if (isDragging && activeTouchId == -1 && !Input.GetMouseButton(0) && Input.touchCount == 0)
            {
                EndDrag();
            }
        }

        private void UpdateItems()
        {
            _items = shape.GetActiveItems();
        }

        private void BeginDrag(Vector2 position)
        {
            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;
            originalScale = transform.localScale;

            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, position, eventCamera, out touchOffset);
        }

        private void HandleDrag(Vector2 position)
        {
            if (!isDragging)
            {
                return;
            }

            var cellSize = field.GetCellSize();
            var shapeOriginalWidth = 126f;
            var scaleFactor = cellSize / shapeOriginalWidth;

            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform, position, eventCamera, out var localPoint))
            {
                var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
                var normalizedX = localPoint.x / canvasWidth;
                var scaleFactorY = rectTransform.rect.height / canvas.GetComponent<RectTransform>().rect.height * 2.5f;

                rectTransform.anchoredPosition = new Vector2(
                    normalizedX * canvasWidth,
                    localPoint.y / scaleFactorY + verticalOffset + scaleFactorY
                );
            }

            if (AnyBusyCellsOrNoneCells())
            {
                if (IsDistancesToHighlightedCellsTooHigh())
                {
                    highlightManager.ClearAllHighlights();
                }

                return;
            }

            UpdateCellHighlights();
        }

        private void EndDrag()
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            activeTouchId = -1;

            if (highlightManager.GetHighlightedCells().Count == 0)
            {
                rectTransform.anchoredPosition = originalPosition;
                transform.localScale = originalScale;
                highlightManager.ClearAllHighlights();
                highlightManager.OnDragEndedWithoutPlacement();
                return;
            }

            HapticFeedback.TriggerHapticFeedback(HapticFeedback.HapticForce.Light);
            SoundBase.instance.PlaySound(SoundBase.instance.placeShape);

            foreach (var kvp in highlightManager.GetHighlightedCells())
            {
                kvp.Key.FillCell(kvp.Value.itemTemplate);
                kvp.Key.AnimateFill();
                if (kvp.Value.bonusItemTemplate != null)
                {
                    kvp.Key.SetBonus(kvp.Value.bonusItemTemplate);
                }
            }

            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(shape);
        }

        private bool IsDistancesToHighlightedCellsTooHigh()
        {
            var firstOrDefault = highlightManager.GetHighlightedCells().FirstOrDefault();
            return firstOrDefault.Key != null &&
                   Vector3.Distance(_items[0].transform.position, firstOrDefault.Key.transform.position) > 1f;
        }

        private bool AnyBusyCellsOrNoneCells()
        {
            return _items.Any(item =>
            {
                var cell = GetCellUnderShape(item);
                return cell == null || !cell.GetComponent<Cell>().IsEmpty();
            });
        }

        private void UpdateCellHighlights()
        {
            highlightManager.ClearAllHighlights();

            foreach (var item in _items)
            {
                var cell = GetCellUnderShape(item);
                if (cell != null)
                {
                    highlightManager.HighlightCell(cell, item);
                }
            }

            if (itemFactory._oneColorMode)
            {
                highlightManager.HighlightFill(field.GetFilledLines(true), itemFactory.GetColor());
            }
            else
            {
                highlightManager.HighlightFill(field.GetFilledLines(true), _items[0].itemTemplate);
            }
        }

        private Transform GetCellUnderShape(Item item)
        {
            var hit = Physics2D.Raycast(item.transform.position, Vector2.zero, 1);
            return hit.collider != null && hit.collider.CompareTag("Cell") ? hit.collider.transform : null;
        }
    }
}