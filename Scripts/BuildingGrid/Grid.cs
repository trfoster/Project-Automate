using System;
using UnityEngine;
using static ProjectAutomate.BuildingGrid.GridBuildingSystem;

namespace ProjectAutomate.BuildingGrid
{
    [Serializable]
    public sealed class Grid
    {
        public class OnGridValueChangedEventArgs : EventArgs
        {
            public Vector2Int GridPos;
            public string Activator;
        }
        public static event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    
        private int width;
        private int height;
        private GridObject[,] gridObjectArray;
        //public bool showDebug;

        public Grid(int width, int height)
        {
            this.width = width + 2;
            this.height = height + 2;

            gridObjectArray = new GridObject[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridObjectArray[x, y] = new GridObject(this, new Vector2Int(x, y));
                }
            }


            //if (!showDebug) return;
            /*TextMesh[,] debugTextArray = new TextMesh[this.width, this.height];
            for (int x = 0; x < gridObjectArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridObjectArray.GetLength(1); y++)
                {
                    Vector2 pos = new Vector3(x, y, 0f);
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(gridObjectArray[x, y]?.ToString(), null, pos, 5, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(pos, new Vector2(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(pos, new Vector2(x + 1, y), Color.white, 100f);
                }
            }
            Vector2 endPoint = new Vector2(this.width, this.height);
            Debug.DrawLine(new Vector2(0, this.height), endPoint, Color.white, 100f);
            Debug.DrawLine(new Vector2(this.width, 0), endPoint, Color.white, 100f);

            OnGridValueChanged += (gridObject sender, OnGridValueChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.GridPos.x, eventArgs.GridPos.y].text = gridObjectArray[eventArgs.GridPos.x, eventArgs.GridPos.y].ToString();
            };*/
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public void SetGridObject(int x, int y, GridObject gridObject)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                gridObjectArray[x, y] = gridObject;
                OnGridValueChanged?.Invoke(this,
                    new OnGridValueChangedEventArgs { GridPos = new Vector2Int(x ,y), Activator = "construction" });
            }
            else
            {
                Debug.LogWarning($"Info has tried to be set to ({x.ToString()}, {y.ToString()}) which is outside the grid");
            }
        }

        public void SetGridObject(Vector2Int gridPos, GridObject gridObject)
        {
            SetGridObject(gridPos.x, gridPos.y, gridObject);
        }

        public void SetGridObject(Vector3 worldPos, GridObject gridObject)
        {
            SetGridObject(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y), gridObject);
        }

        public void TriggerOnGridValueChanged(Vector2Int gridPos, string sender)
        {
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { GridPos = gridPos, Activator = sender });
        }
        
        public GridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height) return gridObjectArray[x, y];
            return default;
        }
        
        public GridObject GetGridObject(float x, float y)
        {
            return GetGridObject( Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }
        
        public GridObject GetGridObject(Vector2Int gridPos)
        {
            return GetGridObject(gridPos.x, gridPos.y);
        }

        public GridObject GetGridObject(Vector3 worldPos)
        {
            return GetGridObject(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        }

        public GridObject GetGridObject(Vector2 worldPos)
        {
            return GetGridObject(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        }
    }
}
