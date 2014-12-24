using UnityEngine;
using System.Collections;


/// <summary>
/// This class contains a few custom GoKitLite actions. Thanks to Prime31 for this great little tweening library.
/// </summary>
public static class GoKitLiteActions
{
	/// <summary>
	/// Shakes the position of the given transform at the starting shake intensity and decreases until dt = 1 at which point shakeIntensity = 0 and
	/// the transform's position is returned to its original value.
	/// </summary>
	/// <returns>
	/// An action delegate to be used in a GoKitLite.instance.customAction() method call.
	/// </returns>
	/// <param name='trans'>
	/// The transform of the object to shake
	/// </param>
	/// <param name='shakeIntensity'>
	/// The starting shake intensity.
	/// </param>
	public static System.Action<Transform, float> shakePosition( Transform trans, float shakeIntensity )
	{
		var origPos = trans.position;

		System.Action<Transform,float> action = ( t, dt ) =>
		{
			// start at full shake intensity and ramp down from there
			t.position = origPos + Random.insideUnitSphere * shakeIntensity * ( 1 - dt );
			
			if( dt >= 1 )
				t.position = origPos;
		};
		return action;
	}


	/// <summary>
	/// Ramps up the shake intensity until it reaches the specified value (occurs at dt = 0.5f) at which
	/// point the shake intensity ramps back down to 0 and the transform's position is returned to its
	/// original value.
	/// </summary>
	/// <returns>
	/// An action delegate to be used in a GoKitLite.instance.customAction() method call.
	/// </returns>
	/// <param name='trans'>
	/// The transform of the object to shake
	/// </param>
	/// <param name='maxShakeIntensity'>
	/// The maximum shake intensity (occurs at dt = 0.5 which is duration / 2)
	/// </param>
	public static System.Action<Transform, float> shakePositionRamp( Transform trans, float maxShakeIntensity )
	{
		// store original position
		var origPos = trans.position;
		
		System.Action<Transform,float> action = ( t, dt ) =>
		{
			// ramp up
			if( dt < 0.5f )
				t.position = origPos + Random.insideUnitSphere * maxShakeIntensity * 2 * dt;
			else // ramp down
				t.position = origPos + Random.insideUnitSphere * maxShakeIntensity * 2 * ( 1 - dt );
				
			// return to original position
			if( dt >= 1 )
				t.position = origPos;
		};
		return action;
	}


	/// <summary>
	/// Shakes the scale of the given transform at the starting shake intensity
	/// and decreases until dt = 1 at which point shakeIntensity = 0 and
	/// the transform's scale is returned to its original value.
	/// </summary>
	/// <returns>
	/// An action delegate to be used in a GoKitLite.instance.customAction() method call.
	/// </returns>
	/// <param name='trans'>
	/// The transform of the object to shake
	/// </param>
	/// <param name='shakeIntensity'>
	/// The starting shake intensity.
	/// </param>
	public static System.Action<Transform, float> shakeScale( Transform trans, float shakeIntensity )
	{
		var origScale = trans.localScale;

		System.Action<Transform,float> action = ( t, dt ) =>
		{
			t.localScale = origScale + Random.insideUnitSphere * shakeIntensity * ( 1 - dt );
				
			if( dt >= 1 )
				t.localScale = origScale;
				
		};
		return action;
	}


	/// <summary>
	/// Ramps up the shake intensity until it reaches the specified value (occurs at dt = 0.5f) at which
	/// point the shake intensity ramps back down to 0 and the transform's scale is returned to its
	/// original value.
	/// </summary>
	/// <returns>
	/// An action delegate to be used in a GoKitLite.instance.customAction() method call.
	/// </returns>
	/// <param name='trans'>
	/// The transform of the object to shake
	/// </param>
	/// <param name='maxShakeIntensity'>
	/// The maximum shake intensity (occurs at dt = 0.5 which is duration / 2)
	/// </param>
	public static System.Action<Transform, float> shakeScaleRamp( Transform trans, float maxShakeIntensity )
	{
		// store original position
		var origScale = trans.localScale;
		
		System.Action<Transform,float> action = ( t, dt ) =>
		{
			// ramp up
			if( dt < 0.5f )
				t.localScale = origScale + Random.insideUnitSphere * maxShakeIntensity * 2 * dt;
			else // ramp down
				t.localScale = origScale + Random.insideUnitSphere * maxShakeIntensity * 2 * ( 1 - dt );
				
			// return to original position
			if( dt >= 1 )
				t.localScale = origScale;
		};
		return action;
	}

}

