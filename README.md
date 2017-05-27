Sample Xamarin.Android project that crashes when using tarjan gc bridge and runs fine when using the old one

# STEPS TO REPRODUCE
1. Run project.
2. Swipe ImageView left and right.
3. See app crashes with one of the following:
```
System.ObjectDisposedException: Cannot access a disposed object. Object name: 'Android.Widget.Toast'.
System.ObjectDisposedException: Cannot access a disposed object. Object name: 'Android.Graphics.Matrix'.
System.ObjectDisposedException: Cannot access a disposed object. Object name: 'Android.Views.GestureDetector'.
```
then:

4. Open /objdisposed/environment.txt and change it's content to: **MONO_GC_PARAMS=bridge-implementation=old**
5. Clean project.
6. Run project again.
7. Make sure bridge implementation has switched to old, i.e. see GC_OLD_BRIDGE tag in console:
```
05-27 06:43:06.645 D/Mono(1949): GC_OLD_BRIDGE num-objects ....etc
```
8. Swipe ImageView left and right.
9. See app runs okay.


# PROJECT DETAILS
To make issue happen more quicker I am starting task in Activity's OnCreate method:
```
Task MemoryPressure ()
{
	return Task.Run (async () => {

		while (true) {
			byte [] f = new byte [1000 * 1000 * 5];
			await Task.Delay (300);
			f = null;
		}
	});
}
```
