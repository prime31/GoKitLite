using UnityEngine;
using System.Collections;



public class SimpleTweenUI : MonoBehaviour
{
	public Transform cube;
	
	
	void Start()
	{
		// start it off with our cube friend coming into view
		GoKitLite.instance.positionFrom( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 10, 0 ), 0.5f );
	}
	
	
	void OnGUI()
	{
		GUI.matrix = Matrix4x4.Scale( new Vector3( 2, 2, 2 ) );
		
		if( GUILayout.Button( "Position Tween with 1s Delay" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 10, 10 ), 1f, GoKitLiteEasing.Bounce.EaseOut )
				.setLoopType( GoKitLite.LoopType.PingPong, 3 );
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


		if( GUILayout.Button( "Custom Action Tween of Alpha to 0" ) )
		{
			System.Action<float> action = dt =>
			{
				var color = cube.renderer.material.color;
				color.a = 1 - dt;
				cube.renderer.material.color = color;
			};
			GoKitLite.instance.customAction( cube, Random.Range( 0.2f, 1 ), action, 0, GoKitLiteEasing.Back.EaseOut );
		}


		if( GUILayout.Button( "Custom Action Tween of Alpha to 1" ) )
		{
			System.Action<float> action = dt =>
			{
				var color = cube.renderer.material.color;
				color.a = dt;
				cube.renderer.material.color = color;
			};
			GoKitLite.instance.customAction( cube, Random.Range( 0.2f, 1 ), action, 0, GoKitLiteEasing.Back.EaseOut )
				.setCompletionHandler( t =>
				{
					Debug.Log( "All done with custom action" );
				});
		}

	}
}
