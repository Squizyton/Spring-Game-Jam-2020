﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	
	public Camera cameraMain;
	public GameMode gameMode = GameMode.PlayMode;

	[Header("Edit Mode")]
	public GameObject grid;
	public GameObject prefabHoverPos;
	public GameObject prefabHoverNeg;
	public GameObject prefabArrow;

	[Header("Debug")]
	public GameObject selectedBuild;

	private Ray ray;
	private Vector3 hoverPoint;
	private GameObject selectedBuildInstance;

	private Dictionary<Vector2, GameObject> gridObjects;

	public bool IsBuildSelected {
		get { return (selectedBuild != null); }
	}

	public Vector2 EditHoverVector { get; private set; }

	public enum GameMode {
		PlayMode,
		EditMode
	}


	private void Awake() {
		DontDestroyOnLoad(gameObject);
		gridObjects = new Dictionary<Vector2, GameObject>();
	}

	private void Start() {
		if(prefabHoverPos)
			prefabHoverPos = Instantiate(prefabHoverPos);
		if(prefabHoverNeg)
			prefabHoverNeg = Instantiate(prefabHoverNeg);
		if(prefabArrow)
			prefabArrow = Instantiate(prefabArrow);
		
		//Todo: remove later
		if(selectedBuild != null)
			selectedBuildInstance = Instantiate(selectedBuild);
		
		
	}

	private void Update() {
		HandleInput();
		
		switch(gameMode) {
			case GameMode.PlayMode:
				OnPlayMode();
				break;
			case GameMode.EditMode:
				OnEditMode();
				break;
		}
	}

	private void HandleInput() {
		//Mouse input
		if(Input.GetMouseButtonDown(0)) {
			if(IsBuildSelected) {
				PlaceBuild();
			}
		}
		
		//Keyboard input
		if(Input.GetKeyDown(KeyCode.Tab)) {
			ToggleGameMode();
		} else if(Input.GetKeyDown(KeyCode.R)) {
			RotateBuild();
		}
	}

	private void OnPlayMode() {
		if(Input.GetMouseButtonDown(0)) {
			ray = cameraMain.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
					
			if(Physics.Raycast(ray, out hit, Mathf.Infinity)) {
				IInteractable interactable = hit.transform.gameObject.GetComponent<IInteractable>();

				//Is the hit object not interactable?
				if(interactable == null) return;

				//Interact with the interactable
				interactable.OnInteract();
			}
		}
		
		//Turn off edit mode gameObjects
		if(prefabHoverPos) prefabHoverPos.SetActive(false);
		if(prefabHoverNeg) prefabHoverNeg.SetActive(false);
		if(prefabArrow) prefabArrow.SetActive(false);
		if(selectedBuildInstance) selectedBuildInstance.SetActive(false);
	}

	private void OnEditMode() {
		ray = cameraMain.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray);
				
		//Interate through all of the hit points until the grid is found
		for(int i = 0; i < hits.Length; i++) {
			RaycastHit hit = hits[i];
					
			//If this object not the building grid try again.
			if(!hit.transform.gameObject.CompareTag("BuildGrid")) continue;
					
			hoverPoint = hit.point;

			//Convert the hit point to grid position, then convert back to world position for each axis
			int gridX = Mathf.RoundToInt(hoverPoint.x / 0.5f);
			hoverPoint.x = gridX * 0.5f;
			int gridY = Mathf.RoundToInt(hoverPoint.z / 0.5f);
			hoverPoint.z = gridY * 0.5f;
			
			//Store the hovered vector
			EditHoverVector = new Vector2(gridX, gridY);
			
			break;
		}
				
		//Show the edit mode game objects
		selectedBuildInstance.SetActive(true);
		prefabArrow.SetActive(true);
		
		//If the tile already has an object on it
		if(gridObjects.ContainsKey(EditHoverVector)) {
			prefabHoverPos.SetActive(false);
			prefabHoverNeg.SetActive(true);
		} else {
			prefabHoverPos.SetActive(true);
			prefabHoverNeg.SetActive(false);
		}
		
		//Move the hover objects to the hover point
		prefabHoverPos.transform.position = hoverPoint;
		prefabHoverNeg.transform.position = hoverPoint;
		prefabArrow.transform.position = hoverPoint;
		selectedBuildInstance.transform.position = hoverPoint;
	}

	[ContextMenu("Toggle Game Mode")]
	public void ToggleGameMode() {
		if(gameMode == GameMode.PlayMode) gameMode = GameMode.EditMode;
		else gameMode = GameMode.PlayMode;
	}

	/// <summary>
	/// Rotates the selected build
	/// </summary>
	public void RotateBuild() {
		selectedBuildInstance.transform.Rotate(Vector3.up, 90.0f);
		prefabArrow.transform.Rotate(Vector3.up, 90.0f);
	}

	/// <summary>
	/// Places the build onto the grid
	/// </summary>
	public void PlaceBuild() {
		//Create a copy of the hover clone
		GameObject createdBuild = Instantiate(selectedBuildInstance, grid.transform);

		if(gridObjects.ContainsKey(EditHoverVector)) {
			print("There is already a placed object here");
			return;
		}
		
		gridObjects.Add(EditHoverVector, createdBuild);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;

		if(gameMode == GameMode.EditMode) {
			if(hoverPoint != null) {
				Vector3 offset = new Vector3(0, 0.25f, 0);
				Gizmos.DrawWireCube(hoverPoint + offset, Vector3.one * 0.5f);
				//print("draw");
			}
		}
	}
}