using System;
using Android.Views;

namespace objdisposed
{
	class FlingGestureListener : GestureDetector.SimpleOnGestureListener
	{
		const int Distance = 100;
		const int Velocity = 2000;

		public event Action OnFlingLeft;
		public event Action OnFlingRight;

		public override bool OnFling (MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			Android.Util.Log.Debug ("Velocity", "VelocityX = " + velocityX.ToString ());
			if (e1.GetX () - e2.GetX () > Distance && Math.Abs (velocityX) > Velocity) {
				// Справа налево
				if (OnFlingLeft != null)
					OnFlingLeft ();
				return false;
			} else if (e2.GetX () - e1.GetX () > Distance && Math.Abs (velocityX) > Velocity) {
				// Слева направо
				if (OnFlingRight != null)
					OnFlingRight ();
				return false;
			}
			return false;
		}
	}
}
