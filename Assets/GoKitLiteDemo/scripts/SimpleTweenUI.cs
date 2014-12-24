using UnityEngine;
using System.Collections;
using Prime31.GoKitLite;


public class SimpleTweenUI : MonoBehaviour
{
	public Transform cube;
	public AnimationCurve easeCurve;


	void Start()
	{
		// start it off with our cube friend coming into view
		GoKitLite.instance.positionFrom( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 10, 0 ) );
	}


	void OnGUI()
	{
		if( GUILayout.Button( "Position Tween with 1s Delay and PingPong Loop" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 5, 0 ), 1f, GoKitLiteEasing.Bounce.EaseOut )
				.setLoopType( GoKitLite.LoopType.PingPong, 2 )
				.setLoopCompletionHandler( t => Debug.Log( "Loop iteration done" ) );
		}

		if( GUILayout.Button( "Relative Position Tween" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 1, 0, 0 ), 0, GoKitLiteEasing.Cubic.EaseIn, true );
		}


		if( GUILayout.Button( "AnimationCurve for Easing Scale" ) )
		{
			GoKitLite.instance.scaleTo( cube, 2f, new Vector3( 3, 3, 3 ), 0, GoKitLiteEasing.Custom.AnimationCurveEase( easeCurve ) );
		}


		if( GUILayout.Button( "Scale to 2" ) )
		{
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 2, 2, 2 ) );
		}


		if( GUILayout.Button( "Scale to 0.5" ) )
		{
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 0.5f, 0.5f, 0.5f ), 0, GoKitLiteEasing.Bounce.EaseOut );
		}


		if( GUILayout.Button( "Punch Scale to 3" ) )
		{
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 3, 3, 3 ), 0, GoKitLiteEasing.Elastic.Punch );
		}


		if( GUILayout.Button( "Rotation to 90,0,0" ) )
		{
			GoKitLite.instance.rotationTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 90f, 0, 0 ), 0, GoKitLiteEasing.Back.EaseOut );
		}


		if( GUILayout.Button( "Rotation to 270,0,0" ) )
		{
			GoKitLite.instance.rotationTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 270f, 0, 0 ), 0, GoKitLiteEasing.Back.EaseOut );
		}


		if( GUILayout.Button( "Rotation to 0,310,0" ) )
		{
			GoKitLite.instance.rotationTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 0, 310, 0 ), 0, GoKitLiteEasing.Back.EaseOut );
		}

		if( GUILayout.Button( "Rotation by 360,0,0 (relative tween)" ) )
		{
			GoKitLite.instance.rotationTo( cube, 1, new Vector3( 360f, 0, 0 ), 0, GoKitLiteEasing.Back.EaseOut, true );
		}


		if( GUILayout.Button( "Custom Action Tween of Alpha to 0 with 1s Delay" ) )
		{
			// dt 0 to 1 (except for bouce, punch they go a bit less than 0 and a bit more than 1)
			System.Action<Transform,float> action = ( trans, dt ) =>
			{
				var color = trans.renderer.material.color;
				color.a = 1 - dt;
				trans.renderer.material.color = color;
			};
			GoKitLite.instance.customAction( cube, Random.Range( 0.2f, 1 ), action, 1.0f, GoKitLiteEasing.Linear.EaseNone );
		}


		if( GUILayout.Button( "Custom Action Tween of Alpha to 1" ) )
		{
			System.Action<Transform,float> action = ( trans, dt ) =>
			{
				var color = trans.renderer.material.color;
				color.a = dt;
				trans.renderer.material.color = color;
			};

			GoKitLite.instance.customAction( cube, Random.Range( 0.2f, 1 ), action, 0, GoKitLiteEasing.Linear.EaseNone )
				.setCompletionHandler( t =>
			{
				Debug.Log( "All done with custom action" );
			} );
		}


		if( GUILayout.Button( "Color to Red" ) )
		{
			GoKitLite.instance.colorTo( cube, Random.Range( 0.2f, 1 ), Color.red );
		}


		if( GUILayout.Button( "Color Cycler" ) )
		{
			GoKitLite.instance.colorTo( cube, Random.Range( 0.2f, 1 ), Color.blue )
				.next( Random.Range( 0.2f, 1 ), Color.red )
				.next( Random.Range( 0.2f, 1 ), Color.blue )
				.setLoopType( GoKitLite.LoopType.PingPong, 10 );
		}


		if( GUILayout.Button( "Shake Position Ramp up/down Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.shakePositionRamp( cube, 0.6f ), 0, GoKitLiteEasing.Linear.EaseNone );
		}


		if( GUILayout.Button( "Shake Position Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.shakePosition( cube, 0.6f ), 0, GoKitLiteEasing.Linear.EaseNone );
		}


		if( GUILayout.Button( "Shake Scale Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.shakeScale( cube, 0.6f ), 0, GoKitLiteEasing.Linear.EaseNone );
		}


		if( GUILayout.Button( "Stop All tweens" ) )
		{
			GoKitLite.instance.stopAllTweens( true );
		}
	}

}
