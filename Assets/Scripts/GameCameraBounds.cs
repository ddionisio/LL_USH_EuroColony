using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

public class GameCameraBounds : MonoBehaviour {
	[SerializeField]
	Rect _bounds;
	[SerializeField]
	bool _boundsIsBoxCollision;
	[SerializeField]
	bool _applyOnEnable = true;

	/// <summary>
	/// World space
	/// </summary>
	public Rect bounds {
		get {
			if(_boundsIsBoxCollision) {
				if(!mBoxColl)
					mBoxColl = GetComponent<BoxCollider2D>();

				if(mBoxColl)
					return new Rect(mBoxColl.bounds.min, mBoxColl.size);
			}

			Vector2 pos = transform.position;

			return new Rect(pos + _bounds.position, _bounds.size);
		}
	}

	private BoxCollider2D mBoxColl;	

	void OnEnable() {
		if(_applyOnEnable) {
			var gameCam = GameCamera.main;
			if(gameCam)
				gameCam.SetBounds(bounds, false);
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;

		var b = bounds;

		Gizmos.DrawWireCube(b.center, b.size);
	}
}
