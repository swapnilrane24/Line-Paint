using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinePaint
{
    public enum Swipe { None, Up, Down, Left, TopLeft, BottomLeft, Right, TopRight, BottomRight };

    public class SwipeControl
    {
        private Vector2 startPos;
        private Vector2 endPos;
        private LevelManager _levelManager;

        public void SetLevelManager(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public void OnUpdate()
        {
            if (GameManager.gameStatus == GameStatus.Playing)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    startPos = Input.mousePosition;
                    endPos = startPos;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    endPos = Input.mousePosition;
                    if (Vector2.Distance(endPos, startPos) > 0.1f)
                    {
                        SwipeDirection();
                    }
                }
            }
        }

        private Swipe SwipeDirection()
        {
            Swipe direction = Swipe.None;
            Vector2 currentSwipe = endPos - startPos;
            float angle = (Mathf.Atan2(currentSwipe.y, currentSwipe.x) * (180 / Mathf.PI));

            if (angle > 67.5f && angle < 112.5f)
            {
                direction = Swipe.Up;
            }
            else if (angle < -67.5f && angle > -112.5f)
            {
                direction = Swipe.Down;
            }
            else if (angle < -157.5f || angle > 157.5f)
            {
                direction = Swipe.Left;
            }
            else if (angle > -22.5f && angle < 22.5f)
            {
                direction = Swipe.Right;
            }
            else if (angle > 22.5f && angle < 67.5f)
            {
                direction = Swipe.TopRight;
            }
            else if (angle > 112.5f && angle < 157.5f)
            {
                direction = Swipe.TopLeft;
            }
            else if (angle < -22.5f && angle > -67.5f)
            {
                direction = Swipe.BottomRight;
            }
            else if (angle < -112.5f && angle > -157.5f)
            {
                direction = Swipe.BottomLeft;
            }

            if (direction != Swipe.None)
            {
                _levelManager.MoveBrush(direction);
                direction = Swipe.None;
            }

            return direction;
        }
    }
}