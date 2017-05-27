using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Graphics;

namespace objdisposed
{
	class ScalableImageView : ImageView
	{
		Matrix matrix;

		public enum State
		{
			NONE, DRAG, ZOOM
		};

		protected static State currentState = State.NONE;

		public event Action OnFlingLeft;
		public event Action OnFlingRight;

		float minScale = 1f;
		float maxScale = 3f;
		float [] m;


		int viewWidth, viewHeight;
		public static int CLICK = 3;
		float saveScale = 1f;
		protected float origWidth, origHeight;
		int oldMeasuredHeight;

		ScaleGestureDetector mScaleDetector;
		GestureDetector flingGestureDetector;

		public ScalableImageView (Context context) : base (context)
		{
			SharedConstructor (context);
		}
		public ScalableImageView (Context context, IAttributeSet attrs) : base (context, attrs)
		{
			SharedConstructor (context);
		}

		void SharedConstructor (Context context)
		{
			this.Clickable = true;
			mScaleDetector = new ScaleGestureDetector (context, new ScaleListener (this));

			matrix = new Matrix ();
			m = new float [9];
			ImageMatrix = matrix;
			SetScaleType (ImageView.ScaleType.Matrix);

			FlingGestureListener flingGestureListener = new FlingGestureListener ();
			flingGestureListener.OnFlingLeft += delegate {
				if (OnFlingLeft != null && (int)(origWidth * saveScale) <= this.Width)
					OnFlingLeft ();
			};

			flingGestureListener.OnFlingRight += delegate {
				if (OnFlingRight != null && (int)(origWidth * saveScale) <= this.Width)
					OnFlingRight ();
			};
			flingGestureDetector = new GestureDetector (flingGestureListener);


			SetOnTouchListener (new ZoomOnTouchListener (context, mScaleDetector, flingGestureDetector));
		}

		public void setMaxZoom (float x)
		{
			maxScale = x;
		}

		public override void SetImageBitmap (Bitmap bm)
		{
			base.SetImageBitmap (bm);
			CalculateContentSizes ();
		}

		private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
		{
			ScalableImageView imgView;
			public ScaleListener (ScalableImageView imgView)
			{
				this.imgView = imgView;
			}

			override public bool OnScaleBegin (ScaleGestureDetector detector)
			{
				currentState = State.ZOOM;
				//mode = 2;
				return true;
			}

			public override
			bool OnScale (ScaleGestureDetector detector)
			{
				float mScaleFactor = detector.ScaleFactor;
				float origScale = imgView.saveScale;
				imgView.saveScale *= mScaleFactor;
				if (imgView.saveScale > imgView.maxScale) {
					imgView.saveScale = imgView.maxScale;
					mScaleFactor = imgView.maxScale / origScale;
				} else if (imgView.saveScale < imgView.minScale) {
					imgView.saveScale = imgView.minScale;
					mScaleFactor = imgView.minScale / origScale;
				}

				if (imgView.origWidth * imgView.saveScale <= imgView.viewWidth || imgView.origHeight * imgView.saveScale <= imgView.viewHeight)
					imgView.matrix.PostScale (mScaleFactor, mScaleFactor, imgView.viewWidth / 2, imgView.viewHeight / 2);
				else
					imgView.matrix.PostScale (mScaleFactor, mScaleFactor, detector.FocusX, detector.FocusY);

				imgView.fixTrans ();
				return true;
			}
		}

		void fixTrans ()
		{
			matrix.GetValues (m);
			float transX = m [Matrix.MtransX];
			float transY = m [Matrix.MtransY];

			float fixTransX = getFixTrans (transX, viewWidth, origWidth * saveScale);
			float fixTransY = getFixTrans (transY, viewHeight, origHeight * saveScale);

			if (fixTransX != 0 || fixTransY != 0)
				matrix.PostTranslate (fixTransX, fixTransY);
		}

		public static float getFixTrans (float trans, float viewSize, float contentSize)
		{
			float minTrans, maxTrans;

			if (contentSize <= viewSize) {
				minTrans = 0;
				maxTrans = viewSize - contentSize;
			} else {
				minTrans = viewSize - contentSize;
				maxTrans = 0;
			}

			if (trans < minTrans)
				return -trans + minTrans;
			if (trans > maxTrans)
				return -trans + maxTrans;
			return 0;
		}

		float getFixDragTrans (float delta, float viewSize, float contentSize)
		{
			if (contentSize <= viewSize) {
				return 0;
			}
			return delta;
		}


		public void CalculateContentSizes ()
		{

			Android.Graphics.Drawables.Drawable drawable = Drawable;
			if (drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
				return;
			int bmWidth = drawable.IntrinsicWidth;
			int bmHeight = drawable.IntrinsicHeight;

			float scaleX = (float)viewWidth / (float)bmWidth;
			float scaleY = (float)viewHeight / (float)bmHeight;
			float scale = Math.Min (scaleX, scaleY);
			matrix.SetScale (scale * saveScale, scale * saveScale);

			// Center the image
			float redundantYSpace = (float)viewHeight - (scale * (float)bmHeight);
			float redundantXSpace = (float)viewWidth - (scale * (float)bmWidth);
			redundantYSpace /= (float)2;
			redundantXSpace /= (float)2;

			matrix.PostTranslate (redundantXSpace, redundantYSpace);

			origWidth = viewWidth - 2 * redundantXSpace;
			origHeight = viewHeight - 2 * redundantYSpace;

			fixTrans ();

			ImageMatrix = matrix;

			// Invalidate();
		}

		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
			viewWidth = MeasureSpec.GetSize (widthMeasureSpec);
			viewHeight = MeasureSpec.GetSize (heightMeasureSpec);

			//
			// Rescales image on rotation
			//
			if (oldMeasuredHeight == viewWidth && oldMeasuredHeight == viewHeight
				|| viewWidth == 0 || viewHeight == 0)
				return;
			oldMeasuredHeight = viewHeight;

			if (saveScale == 1) {
				//Fit to screen.
				float scale;

				Android.Graphics.Drawables.Drawable drawable = Drawable;
				if (drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
					return;
				int bmWidth = drawable.IntrinsicWidth;
				int bmHeight = drawable.IntrinsicHeight;

				Log.Debug ("bmSize", "bmWidth: " + bmWidth + " bmHeight : " + bmHeight);

				float scaleX = (float)viewWidth / (float)bmWidth;
				float scaleY = (float)viewHeight / (float)bmHeight;
				scale = Math.Min (scaleX, scaleY);
				matrix.SetScale (scale, scale);

				// Center the image
				float redundantYSpace = (float)viewHeight - (scale * (float)bmHeight);
				float redundantXSpace = (float)viewWidth - (scale * (float)bmWidth);
				redundantYSpace /= (float)2;
				redundantXSpace /= (float)2;

				matrix.PostTranslate (redundantXSpace, redundantYSpace);

				origWidth = viewWidth - 2 * redundantXSpace;
				origHeight = viewHeight - 2 * redundantYSpace;
				ImageMatrix = matrix;
			}
			fixTrans ();
		}

		class ZoomOnTouchListener : View, View.IOnTouchListener
		{
			readonly ScaleGestureDetector mScaleDetector;
			readonly GestureDetector flingGestureDetector;

			public ZoomOnTouchListener (Context context, ScaleGestureDetector mScaleDetector, GestureDetector flingGestureDetector) : base (context)
			{
				this.mScaleDetector = mScaleDetector;
				this.flingGestureDetector = flingGestureDetector;
			}

			public PointF last = new PointF ();
			public PointF start = new PointF ();

			ScalableImageView imgView;
			#region IOnTouchListener implementation
			public bool OnTouch (View v, MotionEvent evnt)
			{
				imgView = (ScalableImageView)v;
				flingGestureDetector.OnTouchEvent (evnt);
				mScaleDetector.OnTouchEvent (evnt);
				PointF curr = new PointF (evnt.GetX (), evnt.GetY ());

				switch (evnt.Action) {
				case MotionEventActions.Down:
					last.Set (curr);
					start.Set (last);
					currentState = State.DRAG;
					//mode = TouchImageView.DRAG;
					break;

				case MotionEventActions.Move:
					//if (mode == DRAG)
					if (currentState == State.DRAG) {
						float deltaX = curr.X - last.X;
						float deltaY = curr.Y - last.Y;
						float fixTransX = imgView.getFixDragTrans (deltaX, imgView.viewWidth, imgView.origWidth * imgView.saveScale);
						float fixTransY = imgView.getFixDragTrans (deltaY, imgView.viewHeight, imgView.origHeight * imgView.saveScale);
						imgView.matrix.PostTranslate (fixTransX, fixTransY);
						imgView.fixTrans ();
						last.Set (curr.X, curr.Y);
					}
					break;

				case MotionEventActions.Up:
					//mode = NONE;
					currentState = State.NONE;
					int xDiff = (int)Math.Abs (curr.X - start.X);
					int yDiff = (int)Math.Abs (curr.Y - start.Y);
					if (xDiff < CLICK && yDiff < CLICK)
						PerformClick ();
					break;

				case MotionEventActions.PointerUp:
					//mode = NONE;
					currentState = State.NONE;
					break;
				}

				imgView.ImageMatrix = imgView.matrix;
				Invalidate ();
				//return true; // indicate event was handled
				return false;
			}
			#endregion
		}
	}
}
