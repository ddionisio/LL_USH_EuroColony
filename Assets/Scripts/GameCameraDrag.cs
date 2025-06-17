using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using LoLExt;

public class GameCameraDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	public enum Axis {
		None,
		Horizontal,
		Vertical,
		Both
	}

	public Axis axis = Axis.Both;
	public float dragScale = 1f;

	private bool mIsDragging;
	private float mScreenDragScale;

	void OnApplicationFocus(bool focus) {
		if(!focus)
			mIsDragging = false;
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
		if(axis == Axis.None)
			return;

		mIsDragging = true;

		var gameCam = GameCamera.main;
		if(!gameCam)
			return;

		var pixelCam = gameCam.camPixel;
		if(!pixelCam)
			return;

		mScreenDragScale = dragScale / (pixelCam.pixelRatio * pixelCam.assetsPPU);
	}

	void IDragHandler.OnDrag(PointerEventData eventData) {
		if(!mIsDragging)
			return;

		var gameCam = GameCamera.main;
		if(!gameCam)
			return;

		Vector2 screenDelta = eventData.delta;
		Vector2 delta = Vector2.zero;

		switch(axis) {
			case Axis.Horizontal:
				delta.x = screenDelta.x * mScreenDragScale;
				break;
			case Axis.Vertical:
				delta.y = screenDelta.y * mScreenDragScale;
				break;
			case Axis.Both:
				delta = screenDelta * mScreenDragScale;
				break;
		}
				
		gameCam.SetPosition(gameCam.position - delta);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
		mIsDragging = false;
	}

	//deselect?
	//void IPointerClickHandler.OnPointerClick(PointerEventData eventData)	
}
