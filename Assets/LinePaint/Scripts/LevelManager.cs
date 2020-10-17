using System.Collections.Generic;
using UnityEngine;

namespace LinePaint
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private Color[] colors;
        [SerializeField] private Material cubeMat;
        [SerializeField] private CameraZoom gameCamera;
        [SerializeField] private CameraZoom solutionCamera;
        [SerializeField] private float _cellSize;
        [SerializeField] private BrushController brushPefab;
        [SerializeField] private LinePaintScript linePaintPrefab;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private LevelDataScriptable[] leveldataArray;
        [SerializeField] private UIManager uIManager;

        private List<ConnectedLine> inProgressPattern = new List<ConnectedLine>();
        private List<LinePaintScript> connectedLinePaints = new List<LinePaintScript>();
        private Cell[,] cells;
        private Grid grid;
        private SwipeControl swipeControl;
        private BrushController currentBrush;

        // Start is called before the first frame update
        void Start()
        {
            GameManager.currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
            GameManager.totalDiamonds = PlayerPrefs.GetInt("TotalDiamonds", 0);
            
            uIManager.TotalDiamonds.text = "" + GameManager.totalDiamonds;
            uIManager.LevelText.text = "Level " + (GameManager.currentLevel + 1);
            GameManager.gameStatus = GameStatus.Playing;
            swipeControl = new SwipeControl();
            swipeControl.SetLevelManager(this);

            grid = new Grid();
            grid.Initialize(leveldataArray[GameManager.currentLevel].width, leveldataArray[GameManager.currentLevel].height, _cellSize);
            cells = new Cell[leveldataArray[GameManager.currentLevel].width, leveldataArray[GameManager.currentLevel].height];

            CreateGrid(Vector3.zero);

            currentBrush = Instantiate(brushPefab, Vector3.zero, Quaternion.identity);
            currentBrush.currentCoords = leveldataArray[GameManager.currentLevel].brushStartCoords;
            Vector3 brushStartPos = grid.GetCellWorldPosition(leveldataArray[GameManager.currentLevel].brushStartCoords.x, leveldataArray[GameManager.currentLevel].brushStartCoords.y);
            currentBrush.transform.position = brushStartPos;

            gameCamera.ZoomPerspectiveCamera(leveldataArray[GameManager.currentLevel].width, leveldataArray[GameManager.currentLevel].height);
            CompleteBoard();
        }

        private void Update()
        {
            if (swipeControl != null)
            {
                swipeControl.OnUpdate();
            }
        }

        private Cell CreateCells(int x, int y, Vector3 originPos)
        {
            Cell cell = Instantiate(cellPrefab);
            cell.CellCoords = new Vector2Int(x, y);
            cell.GetComponent<Renderer>().material = cubeMat;
            cell.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
            cell.transform.position = originPos + grid.GetCellWorldPosition(x, y);

            return cell;
        }

        public void MoveBrush(Swipe swipe)
        {
            Vector2Int newCoords = grid.GetCellXZBySwipe(currentBrush.currentCoords.x, currentBrush.currentCoords.y, swipe);

            if (newCoords != new Vector2Int(-1, -1))
            {
                SoundManager.Instance.PlayFx(FxType.BrushMove);
                Vector3 finalPos = grid.GetCellWorldPositionBySwipe(currentBrush.currentCoords.x, currentBrush.currentCoords.y, swipe);

                if (!ConnectionAlreadyDone(currentBrush.currentCoords, newCoords, true))
                {
                    inProgressPattern.Add(new ConnectedLine(currentBrush.currentCoords, newCoords));
                    cells[currentBrush.currentCoords.x, currentBrush.currentCoords.y].CellCenterPaint.gameObject.SetActive(true);
                    cells[currentBrush.currentCoords.x, currentBrush.currentCoords.y].CellCenterPaint.material.color = colors[GameManager.currentLevel % colors.Length];
                    LinePaintScript linePaint = Instantiate(linePaintPrefab, new Vector3(0, 0.2f, 0), Quaternion.identity);
                    linePaint.SetRendererPosition(currentBrush.transform.position + new Vector3(0, 0.2f, 0), 
                        finalPos + new Vector3(0, 0.2f, 0), colors[GameManager.currentLevel % colors.Length]);
                    linePaint.SetConnectedCoords(currentBrush.currentCoords, newCoords);
                    connectedLinePaints.Add(linePaint);
                }
                else
                {
                    RemoveConnectLinePaint(currentBrush.currentCoords, newCoords);
                }

                if (leveldataArray[GameManager.currentLevel].completePattern.Count <= inProgressPattern.Count)
                {
                    //Check for win
                    if (IsLevelComplete())
                    {
                        SoundManager.Instance.PlayFx(FxType.Victory);
                        GameManager.gameStatus = GameStatus.Complete;
                        GameManager.currentLevel++;
                        if (GameManager.currentLevel > leveldataArray.Length - 1)
                        {
                            GameManager.currentLevel = 0;
                        }
                        PlayerPrefs.SetInt("CurrentLevel", GameManager.currentLevel);
                        GameManager.totalDiamonds += 15;
                        PlayerPrefs.SetInt("TotalDiamonds", GameManager.totalDiamonds);

                        uIManager.LevelCompleted();
                    }

                }

                currentBrush.transform.position = finalPos;
                currentBrush.currentCoords = newCoords;
            }
        }

        private bool ConnectionAlreadyDone(Vector2Int startCoord, Vector2Int endCoord, bool removeItem)
        {
            bool connected = false;
            for (int i = 0; i < inProgressPattern.Count; i++)
            {
                if (inProgressPattern[i].startCoord == startCoord && inProgressPattern[i].endCoord == endCoord ||
                    inProgressPattern[i].endCoord == startCoord && inProgressPattern[i].startCoord == endCoord)
                {
                    if (removeItem)
                    {
                        inProgressPattern.RemoveAt(i);
                    }

                    connected = true;
                }
            }

            return connected;
        }

        private void RemoveConnectLinePaint(Vector2Int startCoord, Vector2Int endCoord)
        {
            for (int i = 0; i < connectedLinePaints.Count; i++)
            {
                if (connectedLinePaints[i].StartCoord == startCoord && connectedLinePaints[i].EndCoord == endCoord ||
                    connectedLinePaints[i].EndCoord == startCoord && connectedLinePaints[i].StartCoord == endCoord)
                {
                    LinePaintScript line = connectedLinePaints[i];
                    connectedLinePaints.RemoveAt(i);
                    Destroy(line.gameObject);

                    cells[endCoord.x, endCoord.y].CellCenterPaint.gameObject.SetActive(false);
                    return;
                }
            }
        }

        private bool IsLevelComplete()
        {
            //if player has done more connection than required we return false
            if (leveldataArray[GameManager.currentLevel].completePattern.Count != inProgressPattern.Count)
            {
                return false;
            }

            for (int i = 0; i < leveldataArray[GameManager.currentLevel].completePattern.Count; i++)
            {
                if (!ConnectionAlreadyDone(leveldataArray[GameManager.currentLevel].completePattern[i].startCoord, leveldataArray[GameManager.currentLevel].completePattern[i].endCoord, false))
                {
                    return false;
                }
            }

            return true;
        }


        private void CreateGrid(Vector3 originPos)
        {
            for (int x = 0; x < grid.GridArray.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GridArray.GetLength(1); y++)
                {
                    cells[x, y] = CreateCells(x, y, originPos);
                }
            }
        }

        private void CompleteBoard()
        {
            Vector3 offset = new Vector3((leveldataArray[GameManager.currentLevel].width - _cellSize) / 2, 5f, (leveldataArray[GameManager.currentLevel].height - _cellSize) / 2);
            Vector3 gridOriginPos = solutionCamera.transform.position - offset;

            solutionCamera.ZoomOrthographicSizeCamera(leveldataArray[GameManager.currentLevel].width, leveldataArray[GameManager.currentLevel].height);

            for (int i = 0; i < leveldataArray[GameManager.currentLevel].completePattern.Count; i++)
            {
                Vector3 startPos = gridOriginPos + grid.GetCellWorldPosition(leveldataArray[GameManager.currentLevel].completePattern[i].startCoord);
                Vector3 endPos = gridOriginPos + grid.GetCellWorldPosition(leveldataArray[GameManager.currentLevel].completePattern[i].endCoord);
                LinePaintScript linePaint = Instantiate(linePaintPrefab, new Vector3(0, 0.2f, 0), Quaternion.identity);
                linePaint.SetRendererPosition(startPos + new Vector3(0, 0.2f, 0), 
                    endPos + new Vector3(0, 0.2f, 0), colors[GameManager.currentLevel % colors.Length]);
            }
        }

    }
}