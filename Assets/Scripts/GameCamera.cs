using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.U2D;

using M8;

public class GameCamera : MonoBehaviour {
    [Header("Bounds Settings")]
    public float boundsChangeDelay = 0.3f;
    public Signal signalBoundsChangeFinish;

    [Header("Move Settings")]
    public float moveDelay = 0.3f;

    [Header("Bounds")]
    [SerializeField]
    bool _boundLocked = true;

    public Vector2 position { get { return transform.position; } }

    public bool boundLocked {
        get { return _boundLocked; }
        set { _boundLocked = value; }
    }

    /// <summary>
    /// Local space
    /// </summary>
    public Rect cameraViewRect { get; private set; }
    public Vector2 cameraViewExtents { get; private set; }

    public bool isMoving { get { return mMoveToRout != null; } }

    public static GameCamera main {
        get {
            if(mMain == null) {
                Camera cam = Camera.main;
                mMain = cam != null ? cam.GetComponentInParent<GameCamera>() : null;

                if(mMain) {
                    mMain.mCam = cam;
                    mMain.mCamPixel = cam.GetComponent<PixelPerfectCamera>();
                }
            }
            return mMain;
        }
    }

    public Camera cam {
        get {
            if(!mCam) mCam = GetComponentInChildren<Camera>();
            return mCam;
        }
    }

    public PixelPerfectCamera camPixel {
        get {
            if(!mCamPixel) {
                var _cam = cam;
                if(_cam)
                    mCamPixel = _cam.GetComponent<PixelPerfectCamera>();
            }

            return mCamPixel;
		}
    }

    /// <summary>
    /// World-space
    /// </summary>
    public Rect bounds {
        get { return mBoundsRectNext; }
    }

    private static GameCamera mMain;

    private Camera mCam;
    private PixelPerfectCamera mCamPixel;

    private Coroutine mMoveToRout;

    //interpolate
    private Rect mBoundsRectNext;
    private Coroutine mBoundsChangeRout;

    private Rect mBoundsRect;
    private Vector2 mMoveDest;

    public void SetBounds(Rect newBounds, bool interpolate) {
        //don't interpolate if current bounds is invalid
        if(mBoundsRect.size.x == 0f || mBoundsRect.size.y == 0f)
            interpolate = false;

        if(interpolate) {
            mBoundsRectNext = newBounds;

            if(mBoundsChangeRout != null)
                StopCoroutine(mBoundsChangeRout);

            mBoundsChangeRout = StartCoroutine(DoBoundsChange());
        }
        else {
            mBoundsRect = mBoundsRectNext = newBounds;

            SetPosition(transform.position); //refresh clamp
        }
    }

    public void MoveTo(Vector2 dest) {
        //clamp
        if(boundLocked)
            dest = mBoundsRect.Clamp(dest, cameraViewExtents);

        //ignore if we are exactly on dest
        if(position == dest)
            return;

        mMoveDest = dest;

        if(mMoveToRout == null)
			mMoveToRout = StartCoroutine(DoMoveTo());
    }

    public void StopMoveTo() {
        if(mMoveToRout != null) {
            StopCoroutine(mMoveToRout);
            mMoveToRout = null;
        }
    }

    public void SetPosition(Vector2 pos) {
        //clamp
        if(boundLocked)
            pos = mBoundsRect.Clamp(pos, cameraViewExtents);

        transform.position = pos;
    }

    public bool isVisible(Rect rect) {
        rect.center = transform.worldToLocalMatrix.MultiplyPoint3x4(rect.center);

        return cameraViewRect.Overlaps(rect);
    }
                
    void OnDisable() {
        StopMoveTo();

        if(mBoundsChangeRout != null) {
            StopCoroutine(mBoundsChangeRout);
            mBoundsChangeRout = null;

            mBoundsRect = mBoundsRectNext;
        }
    }

	void OnDestroy() {
        if(mMain == this)
            mMain = null;
	}

	void Awake() {
        var unityCam = cam;

        //setup view bounds
        var minExt = unityCam.ViewportToWorldPoint(Vector3.zero);
        var maxExt = unityCam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        var mtxToLocal = transform.worldToLocalMatrix;

        var minExtL = mtxToLocal.MultiplyPoint3x4(minExt);
        var maxExtL = mtxToLocal.MultiplyPoint3x4(maxExt);

        cameraViewRect = new Rect(minExt, new Vector2(Mathf.Abs(maxExtL.x - minExtL.x), Mathf.Abs(maxExtL.y - minExtL.y)));
        cameraViewExtents = cameraViewRect.size * 0.5f;
    }

    IEnumerator DoMoveTo() {
        Vector2 pos = transform.position;
        Vector2 vel = Vector2.zero;

        while(!MathUtil.CompareApprox(pos, mMoveDest, 0.001f)) {
            pos = Vector2.SmoothDamp(pos, mMoveDest, ref vel, moveDelay);
            SetPosition(pos);

            yield return null;
        }

		SetPosition(mMoveDest);

		mMoveToRout = null;
    }

    IEnumerator DoBoundsChange() {
        //ease out
        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutCirc);

        float curTime = 0f;
        float delay = boundsChangeDelay;

        Rect prevBoundsRect = mBoundsRect;

        while(curTime < delay) {
            yield return null;

            curTime += Time.deltaTime;

            float t = easeFunc(curTime, delay, 0f, 0f);

            mBoundsRect.min = Vector2.Lerp(prevBoundsRect.min, mBoundsRectNext.min, t);
            mBoundsRect.max = Vector2.Lerp(prevBoundsRect.max, mBoundsRectNext.max, t);

            if(!isMoving)
                SetPosition(transform.position); //update clamp
        }

        mBoundsChangeRout = null;

        if(signalBoundsChangeFinish != null)
            signalBoundsChangeFinish.Invoke();
    }
}