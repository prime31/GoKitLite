using UnityEngine;
using System.Collections;

/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
/// and associated documentation files (the "Software"), to deal in the Software without 
/// restriction, including without limitation the rights to use, copy, modify, merge, publish, 
/// distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
/// Software is furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or 
/// substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
/// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
/// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
/// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

/// <summary>
/// This class contains a few custom GoKitLite actions. Thanks to Prime31 for this great little 
/// tweening library.
/// </summary>
public static class GoKitLiteActions
{
	/// <summary>
	/// Shakes the position of the given transform at the starting shake intensity
	/// and decreases until dt = 1 at which point shakeIntensity = 0 and 
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
	public static System.Action<Transform, float> ShakePosition (Transform trans, float shakeIntensity)
	{
		Vector3 origPos = trans.position;

		System.Action<Transform,float> action = ( t, dt ) =>
		{
			//Start at full shake intensity and ramp down from there
			t.position = origPos + Random.insideUnitSphere * shakeIntensity * (1 - dt);
			
			if (dt >= 1)
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
	public static System.Action<Transform, float> ShakePositionRamp (Transform trans, float maxShakeIntensity)
	{
		//Store original position
		Vector3 origPos = trans.position;
		
		System.Action<Transform,float> action = ( t, dt ) =>
		{
			//Ramp up
			if (dt < 0.5f)
				t.position = origPos + Random.insideUnitSphere * maxShakeIntensity * 2 * dt;
				//Ramp down
			else
				t.position = origPos + Random.insideUnitSphere * maxShakeIntensity * 2 * (1 - dt);
				
			//Return to original position
			if (dt >= 1)
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
	public static System.Action<Transform, float> ShakeScale (Transform trans, float shakeIntensity)
	{
		Vector3 origScale = trans.localScale;

		System.Action<Transform,float> action = ( t, dt ) =>
		{
			t.localScale = origScale + Random.insideUnitSphere * shakeIntensity * (1 - dt);
				
			if (dt >= 1)
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
	public static System.Action<Transform, float> ShakeScaleRamp (Transform trans, float maxShakeIntensity)
	{
		//Store original position
		Vector3 origScale = trans.localScale;
		
		System.Action<Transform,float> action = ( t, dt ) =>
		{
			//Ramp up
			if (dt < 0.5f)
				t.localScale = origScale + Random.insideUnitSphere * maxShakeIntensity * 2 * dt;
				//Ramp down
			else
				t.localScale = origScale + Random.insideUnitSphere * maxShakeIntensity * 2 * (1 - dt);
				
			//Return to original position
			if (dt >= 1)
				t.localScale = origScale;
		};
		return action;
	}
}

