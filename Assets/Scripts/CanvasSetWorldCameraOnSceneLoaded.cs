using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasSetWorldCameraOnSceneLoaded : MonoBehaviour {
	[SerializeField]
	Canvas _canvas;

	[SerializeField]
	[M8.SortingLayer]
	string _sortLayer;

	[SerializeField]
	int _sortOrder;

	[SerializeField]
	float _planeDistance;

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;

		_canvas.worldCamera = Camera.main;
	}

	void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void Awake() {
		if(!_canvas)
			_canvas = GetComponent<Canvas>();

		_canvas.sortingLayerName = _sortLayer;
		_canvas.sortingOrder = _sortOrder;
		_canvas.planeDistance = _planeDistance;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		_canvas.worldCamera = Camera.main;
	}
}
