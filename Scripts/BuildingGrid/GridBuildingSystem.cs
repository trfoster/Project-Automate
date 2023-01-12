using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.BuildingSOs;
using ProjectAutomate.GameAssets;
using ProjectAutomate.MyUtils;
using UnityEngine;
using TMPro;

namespace ProjectAutomate.BuildingGrid
{
	public sealed class GridBuildingSystem : MonoBehaviour
	{
		public static GridBuildingSystem Instance { get; private set; } //singleton stuff
		
		private BuildingSO selectedBuildingSO;
		public Grid grid;
		private float rotation;
		private float storedRotation;
		private bool hasStoredRotation;
		private int buildingIndex;
		private SpriteRenderer thisRenderer;
		[SerializeField] private TextMeshProUGUI buildingTypeText;
		//[SerializeField] private Transform debugObjectPrefab;
		[SerializeField] private bool advancedDebug;
		[SerializeField] private List<BuiltObject> buildingList;
		[SerializeField] private Camera mainCamera;

		private void Awake()
		{
			Instance = this;
			grid = new Grid(50, 50);
			thisRenderer = GetComponent<SpriteRenderer>();
			buildingList = new List<BuiltObject>();
		}

		private void Start()
		{
			selectedBuildingSO = References.Instance.buildingReferences.belt;
			thisRenderer.sprite = selectedBuildingSO.sprite;
			buildingTypeText.text = selectedBuildingSO.nameString;
		}

		//[Serializable]
		public sealed class GridObject
		{
			private readonly Grid grid;
			private readonly Vector2Int pivotPos;
			private BuiltObject builtObject;
			public GridObject(Grid grid, Vector2Int pivotPos)
			{
				this.grid = grid;
				this.pivotPos = pivotPos;
			}

			public void SetBuiltObject(BuiltObject settingBuiltObject)
			{
				builtObject = settingBuiltObject;
				grid.TriggerOnGridValueChanged(pivotPos, "construction");
				if (Instance.advancedDebug) Instance.buildingList.Add(builtObject);
			}

			public BuiltObject GetBuiltObject()
			{
				return builtObject;
			}

			public void ClearBuiltObject()
			{
				if (Instance.advancedDebug) Instance.buildingList.Remove(builtObject);
				builtObject = null;
				grid.TriggerOnGridValueChanged(pivotPos, "deletion");
			}

			public bool CanBuild()
			{
				return builtObject is null;
			}

			public override string ToString()
			{
				return $"Grid object at: {pivotPos.x.ToString()}, {pivotPos.y.ToString()}";
			}
		}

		private void Update()
		{
			if(PauseMenu.IsGamePaused) return;
			Vector3 pos = GetMousePos();
			Vector3Int mousePos = new (Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), -10);

			void StoreRotation(float rot)
			{
				switch (selectedBuildingSO.isRotatable)
				{
					case false when !hasStoredRotation:
						storedRotation = rotation;
						rotation = 0f;
						hasStoredRotation = true;
						break;
					case true:
						rotation = MyMathf.CorrectAngle(rotation + rot);
						if (hasStoredRotation)
						{
							hasStoredRotation = false;
							rotation = storedRotation;
						}
						break;
				}
				transform.rotation = Quaternion.Euler(0f, 0f, rotation);
			}
			
			if (Input.GetKeyDown(KeyCode.E))
			{
				StoreRotation(-90f);
			}
			else if (Input.GetKeyDown(KeyCode.Q))
			{
				StoreRotation(90f);
			}
			else if (Input.GetKeyDown(KeyCode.Tab))
			{
				selectedBuildingSO = References.Instance.buildingReferences.GetNextBuilding(selectedBuildingSO);
				StoreRotation(0f);
				thisRenderer.sprite = selectedBuildingSO.sprite;
				buildingTypeText.text = selectedBuildingSO.nameString;
			}

			Vector2Int buildingSize = selectedBuildingSO.GetSizeAtRot(rotation);
			int leftSide = buildingSize.x;
			int bottomSide = buildingSize.y;
			int rightSide = buildingSize.x;
			int topSide = buildingSize.y;
			if(buildingSize.x > 0) leftSide = 0;
			else rightSide = 0;
			if(buildingSize.y > 0) bottomSide = 0;
			else topSide = 0;
			if (mousePos.x + leftSide <= 0 || mousePos.x + rightSide > grid.GetWidth() - 2)
			{
				if (mousePos.y + bottomSide <= 0 || mousePos.y + topSide > grid.GetHeight() - 2) return;
				Transform thisTransform = transform;
				thisTransform.position = new Vector3(thisTransform.position.x, mousePos.y);
				return;
			}
			if (mousePos.y + bottomSide <= 0 || mousePos.y + topSide > grid.GetHeight() - 2)
			{
				if (mousePos.x + leftSide <= 0 || mousePos.x + rightSide > grid.GetWidth() - 2) return;
				Transform thisTransform = transform;
				thisTransform.position = new Vector3(mousePos.x, thisTransform.position.y);
				return;
			}
			
			transform.position = mousePos;
			if (Input.GetKeyDown(KeyCode.R))
			{
				if (grid.GetGridObject(mousePos).GetBuiltObject() is IChangeState building)
				{
					building.ChangeState();
				}
			}
			else if (Input.GetMouseButton(0))
			{
				
				Vector2Int mousePosXY = new Vector2Int(mousePos.x, mousePos.y);
				List<Vector2Int> gridPosList = selectedBuildingSO.GetGridPosList(mousePosXY, rotation);
				
				BuiltObject mousePosObject = grid.GetGridObject(mousePosXY).GetBuiltObject();
				
				Vector3 buildPos = new Vector3(mousePos.x, mousePos.y, selectedBuildingSO.layer);
				float buildRotation = 0f;
				if (selectedBuildingSO.isRotatable) buildRotation = rotation;
				do
				{
					do
					{
						if (mousePosObject is null) break;
						bool isSameType = selectedBuildingSO.replaceType == mousePosObject.buildingSO.replaceType;
						bool occupiesSameGridPositions = !mousePosObject.GetGridPosList().Except(selectedBuildingSO.GetGridPosList(mousePosXY, rotation)).Any();
						bool isOneRotatable = mousePosObject.buildingSO.isRotatable ^ selectedBuildingSO.isRotatable;
						bool hasDifferentRotation = Math.Abs(mousePosObject.rotation - transform.rotation.eulerAngles.z) > 0.001f;
						bool shouldReplace = isSameType && occupiesSameGridPositions && (isOneRotatable || hasDifferentRotation);
						/*Debug.Log("hasDifferentRotation: " + hasDifferentRotation);
						Debug.Log("building rotation: " + mousePosObject.Rotation.ToString("F40"));
						Debug.Log("mouse rotation: " + transform.rotation.z.ToString("F40"));*/
						if (shouldReplace)
						{
							mousePosObject.DestroySelf();
							
							BuiltObject builtObject = BuiltObject.Create(buildPos, mousePosXY, buildRotation, selectedBuildingSO);
							foreach (Vector2Int gridPos in gridPosList)
							{
								grid.GetGridObject(gridPos).SetBuiltObject(builtObject);
							}
							builtObject.SendBlockUpdate();
							break;
						}
						else
						{
							// TODO: some sort of effect like shake or particles to let the player know they cant build here
						}
					} while (false);
					
					bool canBuild = gridPosList.All(gridPos => grid.GetGridObject(gridPos).CanBuild());
					/*foreach (Vector2Int gridPos in gridPosList)
					{
						if (grid.GetGridObject(gridPos).CanBuild()) { /*Debug.Log("can build");#1# continue;}
						canBuild = false;
						break;
					}*/

					if (canBuild)
					{
						BuiltObject builtObject = BuiltObject.Create(buildPos, mousePosXY, buildRotation, selectedBuildingSO);
						foreach (Vector2Int gridPos in gridPosList)
						{
							grid.GetGridObject(gridPos).SetBuiltObject(builtObject);
						}
						builtObject.SendBlockUpdate();
					}
					else
					{
						// TODO: some sort of effect like shake or particles to let the player know they cant build here
						//StartCoroutine(Waiter());
					}
				} while (false);
            
			}
			else if (Input.GetMouseButton(1))
			{
				foreach (BuiltObject builtObject in selectedBuildingSO
					         .GetGridPosList(new Vector2Int(mousePos.x, mousePos.y), rotation)
					         .Select(gridPos => grid.GetGridObject(gridPos).GetBuiltObject())
					         .Where(builtObject => builtObject is not null))
				{
					builtObject.DestroySelf();
				}
			}
			else if (Input.GetMouseButtonDown(2)) //middle click
			{
				if (grid.GetGridObject(mousePos).GetBuiltObject() is not { } building) return;
				selectedBuildingSO = building.buildingSO;
				StoreRotation(0f);
				thisRenderer.sprite = selectedBuildingSO.sprite;
				buildingTypeText.text = selectedBuildingSO.nameString;
			}
		}

		private Vector3 GetMousePos()
		{
			return mainCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		/*private IEnumerator Waiter()
		{
			Debug.Log("waiter started");
			thisRenderer.color = new Color(230f, 57f, 70f, 255f);
			yield return new WaitForSeconds(0.5f);
			// ReSharper disable once Unity.InefficientPropertyAccess
			thisRenderer.color = new Color(255f, 255f, 255f, 160f);
		}*/
	}
}