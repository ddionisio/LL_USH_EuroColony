using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, M8.IPoolInit, M8.IPoolSpawn, M8.IPoolSpawnComplete, M8.IPoolDespawn {
	public const string parmData = "u_dat"; //UnitData
	public const string parmState = "u_state"; //Unit.State
	public const string parmSpawnPoint = "u_spawnPt"; //Vector2
	public const string parmFacing = "u_facing"; //Facing

	public enum State {
		None,

		Ghost,

		Idle, //determine decisions here
		Move, //move to destination
		Act, //perform action

		Hurt, //when damaged, also used as invul. delay

		Spawning,
		Despawning,

		Retreat, //run away from danger

		Dying, //can be revived
		Death, //animate and despawn

		Victory, //for allies
	}

	public enum Facing {
		Right,
		Left
	}

	[Header("Display")]
	public Transform root;

	[Header("Animations")]
	public M8.Animator.Animate animator;
	[M8.Animator.TakeSelector]
	public int takeIdle = -1;

	[Header("Signal Listen")]
	public M8.Signal signalListenPlay;
	public M8.Signal signalListenStop;

	public UnitData data { get; private set; }

	public virtual Vector2 position {
		get { return transform.position; }
		set { transform.position = value; }
	}

	public Vector2 up {
		get { return transform.up; }
		set {
			if(up != value)
				transform.up = value;
		}
	}
		
	public State state {
		get { return mState; }
		set {
			if(mState != state) {
				ClearState();

				mState = value;

				RestartStateTime();

				ApplyState();

				stateChangedCallback?.Invoke(this);
			}
		}
	}

	public Facing facing {
		get { return mFacing; }
		set {
			if(mFacing != value) {
				mFacing = value;
				ApplyFacing();
			}
		}
	}

	public int hitpoints {
		get { return mHitpoints; }
		set {
			if(mHitpoints != value) {
				mHitpoints = value;


			}
		}
	}

	public int hitpointsMax {
		get { return data ? data.hitpoints : 0; }
	}

	public BoxCollider2D boxCollider { get; private set; }

	public Bounds bounds { get { return boxCollider ? boxCollider.bounds : new Bounds(); } }

	public Rect boundsRect { 
		get {
			var b = bounds;
			return new Rect(b.min, b.size);
		}
	}

	public M8.PoolDataController poolCtrl { get; private set; }

	/// <summary>
	/// Elapsed time since this state
	/// </summary>
	public float stateTimeElapsed { get { return Time.time - stateTimeLastChanged; } }

	/// <summary>
	/// Last time since state change
	/// </summary>
	public float stateTimeLastChanged { get; private set; }

	/// <summary>
	/// Check if this unit has been marked (used for targeting by AI)
	/// </summary>
	public int markCount { get { return mMark; } }

	public bool isVisible {
		get {
			var cam = GameCamera.main;
			return cam.IsVisible(boundsRect);
		}
	}

	public Color colorTint {
		get { return mSpriteColor; }
		set {
			if(mSpriteColor != value)
				ColorApplyTint(value);
		}
	}

	public string sortingLayer {
		get {
			return mSprites != null && mSprites.Length > 0 && mSprites[0] ? mSprites[0].sortingLayerName : "";
		}

		set {
			if(mSprites != null) {
				for(int i = 0; i < mSprites.Length; i++) {
					var spr = mSprites[i];
					if(spr)
						spr.sortingLayerName = value;
				}
			}
		}
	}

	public event System.Action<Unit> stateChangedCallback;

	protected Coroutine mRout;

	private State mState;

	private Facing mFacing;

	private int mHitpoints;

	private int mMark;

	private SpriteRenderer[] mSprites;
	private Color[] mSpriteColors;
	private Color mSpriteColor = Color.white;
	private bool mSpriteIsColorApplied;

	public void ColorApplyTint(Color color) {
		mSpriteColor = color;
		mSpriteIsColorApplied = true;

		if(mSprites != null) {
			for(int i = 0; i < mSprites.Length; i++) {
				var spr = mSprites[i];
				if(spr)
					spr.color = mSpriteColors[i] * mSpriteColor;
			}
		}
	}

	public void ColorRevert() {
		if(mSpriteIsColorApplied) {
			if(mSprites != null) {
				for(int i = 0; i < mSprites.Length; i++) {
					var spr = mSprites[i];
					if(spr)
						spr.color = mSpriteColors[i];
				}
			}

			mSpriteIsColorApplied = false;
		}
	}

	public void AddMark() {
		mMark++;
	}

	public void RemoveMark() {
		if(mMark > 0)
			mMark--;
	}

	public bool IsTouching(Rect r) {
		var b = new Bounds(r.center, new Vector3(r.width, r.height, 10000f));

		return IsTouching(b);
	}

	public bool IsTouching(Bounds b) {
		return bounds.Intersects(b);
	}

	public bool IsTouching(Unit otherUnit) {
		return bounds.Intersects(otherUnit.bounds);
	}

	/// <summary>
	/// Move unit's y-position until it hits ground. Assumes pivot is at bottom. Returns true if grounded
	/// </summary>
	public bool FallDown() {
		var pos = position;

		float dist = GameData.instance.unitFallSpeed * Time.deltaTime;
		var hit = Physics2D.Raycast(pos, Vector2.down, dist, GameData.instance.groundLayerMask);
		if(hit.collider) {
			position = hit.point;

			return true;
		}
		else {
			pos.y -= dist;
			position = pos;

			return false;
		}
	}

	public void SnapToGround(bool adjustTilt) {
		GroundPoint gPt;
		if(GroundPoint.GetGroundPoint(position, out gPt)) {
			position = gPt.position;
			up = adjustTilt ? gPt.up : Vector2.up;
		}
	}

	public void Despawn() {
		if(mState == State.Despawning || mState == State.Death || mState == State.None) //already despawing/death, or is released
			return;

		mState = State.Despawning;
	}

	protected virtual void Init() { }

	protected virtual void Despawned() { }

	protected virtual void Spawned(M8.GenericParams parms) { }

	/// <summary>
	/// Called after spawn animation finished during Spawning state
	/// </summary>
	protected virtual void SpawnComplete() {
		//state = UnitState.Idle;
	}

	protected virtual void ClearState() {
		StopCurrentRout();

		ResetSprites();
	}

	protected virtual void ApplyState() {
		var gameDat = GameData.instance;

		switch(mState) {
			case State.Ghost:
				colorTint = gameDat.unitGhostColor;
				sortingLayer = gameDat.unitGhostSortLayer;

				if(takeIdle != -1)
					animator.Play(takeIdle);
				break;

			case State.Idle:
				if(takeIdle != -1)
					animator.Play(takeIdle);
				break;
		}
	}

	protected virtual void Play() {

	}

	protected virtual void Stop() {

	}
		
	protected virtual void Update() {
		switch(mState) {
			case State.Ghost:
				SnapToGround(false);
				break;
		}
	}

	protected void ResetSprites() {
		if(mSprites != null) {
			var sortLayer = GameData.instance.unitSortLayer;

			for(int i = 0; i < mSprites.Length; i++) {
				var spr = mSprites[i];
				if(spr) {
					spr.sortingLayerName = sortLayer;
					spr.color = mSpriteColors[i];
				}
			}
		}

		mSpriteIsColorApplied = false;
	}

	protected void RestartStateTime() {
		stateTimeLastChanged = Time.time;
	}

	void M8.IPoolInit.OnInit() {
		if(!root)
			root = transform;

		poolCtrl = GetComponent<M8.PoolDataController>();

		boxCollider = GetComponent<BoxCollider2D>();
		if(boxCollider)
			boxCollider.enabled = false;

		mSprites = root.GetComponentsInChildren<SpriteRenderer>(true);
		mSpriteColors = new Color[mSprites.Length];

		for(int i = 0; i < mSpriteColors.Length; i++)
			mSpriteColors[i] = mSprites[i].color;

		Init();
	}

	void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
		if(parms != null) {
			if(parms.ContainsKey(parmData))
				data = parms.GetValue<UnitData>(parmData);

			if(parms.ContainsKey(parmState))
				mState = parms.GetValue<State>(parmState);

			if(parms.ContainsKey(parmSpawnPoint))
				position = parms.GetValue<Vector2>(parmSpawnPoint);

			if(parms.ContainsKey(parmFacing)) {
				mFacing = parms.GetValue<Facing>(parmFacing);
				ApplyFacing();
			}
		}

		mHitpoints = hitpointsMax;
		RestartStateTime();

		Spawned(parms);

		if(signalListenPlay) signalListenPlay.callback += Play;
		if(signalListenStop) signalListenStop.callback += Stop;
	}

	void M8.IPoolSpawnComplete.OnSpawnComplete() {
		ApplyState();
	}

	void M8.IPoolDespawn.OnDespawned() {
		if(signalListenPlay) signalListenPlay.callback -= Play;
		if(signalListenStop) signalListenStop.callback -= Stop;

		state = State.None;

		Despawned();

		data = null;
	}

	private void ApplyFacing() {
		if(root) {
			var s = root.localScale;

			switch(mFacing) {
				case Facing.Left:
					s.x = -Mathf.Abs(s.x);
					break;

				default:
					s.x = Mathf.Abs(s.x);
					break;
			}

			root.localScale = s;
		}
	}

	private void StopCurrentRout() {
		if(mRout != null) {
			StopCoroutine(mRout);
			mRout = null;
		}
	}
}
