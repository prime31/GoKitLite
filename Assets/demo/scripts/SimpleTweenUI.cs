using UnityEngine;
using System.Collections;



public class SimpleTweenUI : MonoBehaviour
{
	public Transform cube;
	
	
	void Start()
	{
		// prep the GoKitLite object first
		GoKitLite.init();

		// start it off with our cube friend coming into view
		GoKitLite.instance.positionFrom( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 10, 0 ) );
	}
	
	
	void OnGUI()
	{
		//GUI.matrix = Matrix4x4.Scale( new Vector3( 2, 2, 2 ) );
		
		if( GUILayout.Button( "Position Tween with 1s Delay and PingPong Loop" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 5, 0 ), 1f, GoKitLiteEasing.Bounce.EaseOut )
				.setLoopType( GoKitLite.LoopType.PingPong, 3 );
		}
		
		if( GUILayout.Button( "Relative Position Tween" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 1, 0, 0 ), 0, GoKitLiteEasing.Cubic.EaseIn, true );
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
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 3, 3, 3 ), 0, GoKitLiteEasing.Linear.Punch );
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
			//trans
			//dt 0 to 1 (except for bouce, punch they go a bit less than 0 and a bit more than 1)
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
				});
		}


		if( GUILayout.Button( "Color to Red" ) )
		{
			GoKitLite.instance.colorTo( cube, Random.Range( 0.2f, 1 ), Color.red );
		}


		if( GUILayout.Button( "Color Cycler" ) )
		{
			GoKitLite.instance.colorTo( cube, Random.Range( 0.2f, 1 ), Color.blue )
				.setCompletionHandler( trans =>
				{
					GoKitLite.instance.colorTo( cube, Random.Range( 0.2f, 1 ), Color.red )
						.setCompletionHandler( tran =>
						{
							GoKitLite.instance.colorTo( cube, Random.Range( 0.2f, 1 ), Color.green ).setLoopType( GoKitLite.LoopType.PingPong, 10 );
						});
				});
		}
			
//		if( GUILayout.Button( "Shake Position Ramp up/down Tween" ) )
//		{
//			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.ShakePositionRamp(cube, 0.6f), 0, GoKitLiteEasing.Linear.EaseNone );
//		}
		
		if( GUILayout.Button( "Shake Position Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.ShakePosition(cube, 0.6f), 0, GoKitLiteEasing.Linear.EaseNone );
		}
		
		if( GUILayout.Button( "Shake Scale Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.ShakeScale(cube, 0.6f) , 0, GoKitLiteEasing.Linear.EaseNone );
		}
		
//		if( GUILayout.Button( "Shake Scale Ramp up/down Tween" ) )
//		{
//			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.ShakeScaleRamp(cube, 0.6f) , 0, GoKitLiteEasing.Linear.EaseNone );
//		}

	}

}
