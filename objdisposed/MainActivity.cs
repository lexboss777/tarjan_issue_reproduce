using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;

namespace objdisposed
{
	[Activity (Label = "objdisposed", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		int flingCount = 0;
		ScalableImageView customImageView;
		Toast pageToast;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.Main);

			Button button = FindViewById<Button> (Resource.Id.myButton);

			FrameLayout mainFrameLayout = FindViewById<FrameLayout> (Resource.Id.Main_frameLayout);
			customImageView = new ScalableImageView (this);
			mainFrameLayout.AddView (customImageView, 0);

			pageToast = Toast.MakeText(this, "", ToastLength.Short);

			customImageView.OnFlingLeft += delegate {
                ShowPageToast($"Left {++flingCount}");
			};

			customImageView.OnFlingRight += delegate {
                ShowPageToast($"Right {++flingCount}");
            };

			customImageView.SetImageResource (Resource.Drawable.sample);

			MemoryPressure ();
		}

		Task MemoryPressure ()
		{
			return Task.Run (async () => {

				while (true) {
					byte [] f = new byte [1000 * 1000 * 5];
					//byte [] f2 = new byte [1000 * 1000 * 5];
					//byte [] f3 = new byte [1000 * 1000 * 5];
					await Task.Delay (300);
					f = null;
					//f2 = null;
					//f3 = null;
				}
			});
		}

		private void ShowPageToast (string text)
		{
			pageToast.SetText (text);
			pageToast.Show ();
		}
	}
}

