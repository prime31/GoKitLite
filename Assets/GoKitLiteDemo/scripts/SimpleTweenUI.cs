using UnityEngine;
using System.Collections;
using Prime31.GoKitLite;
using UnityEngine.UI;


public class SimpleTweenUI : MonoBehaviour
{
	public Transform cube;
	public AnimationCurve easeCurve;
	public RectTransform panel;


	void Start()
	{
		// start it off with our cube friend coming into view
		GoKitLite.instance.positionFrom( cube, Random.Range( 0.2f, 1 ), new Vector3( 10, 10, 0 ) );
	}


	void OnGUI()
	{
		if( GUILayout.Button( "Position Tween with PingPong Loop" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 1f, 2f ), new Vector3( 10, 5, 0 ) )
				.setEaseType( EaseType.BounceOut )
				.setLoopType( GoKitLite.LoopType.PingPong, 2 )
				.setLoopCompletionHandler( t => Debug.Log( "Loop iteration done" ) )
				.setCompletionHandler( t => Debug.Log( "Tween complete" ) );
		}


		if( GUILayout.Button( "Relative Position Tween" ) )
		{
			GoKitLite.instance.positionTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 1, 0, 0 ), true )
				.setEaseType( EaseType.CubicIn );
		}


		if( GUILayout.Button( "AnimationCurve for Easing Scale" ) )
		{
			GoKitLite.instance.scaleTo( cube, 2f, new Vector3( 3, 3, 3 ) )
				.setAnimationCurve( easeCurve );
		}


		if( GUILayout.Button( "Scale to 2" ) )
		{
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 2, 2, 2 ) );
		}


		if( GUILayout.Button( "Scale to 0.5" ) )
		{
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 0.5f, 0.5f, 0.5f ) )
				.setEaseType( EaseType.BounceOut );
		}


		if( GUILayout.Button( "Punch Scale to 3" ) )
		{
			GoKitLite.instance.scaleTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 3, 3, 3 ) )
				.setEaseType( EaseType.Punch );
		}


		if( GUILayout.Button( "Rotation to 90,0,0" ) )
		{
			GoKitLite.instance.rotationTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 90f, 0, 0 ) )
				.setEaseType( EaseType.BackOut );
		}


		if( GUILayout.Button( "Rotation to 270,0,0" ) )
		{
			GoKitLite.instance.rotationTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 270f, 0, 0 ) )
				.setEaseType( EaseType.BackOut );
		}


		if( GUILayout.Button( "Rotation to 0,310,0" ) )
		{
			GoKitLite.instance.rotationTo( cube, Random.Range( 0.2f, 1 ), new Vector3( 0, 310, 0 ) )
				.setEaseType( EaseType.BackOut );
		}

		if( GUILayout.Button( "Rotation by 360,0,0 (relative tween)" ) )
		{
			GoKitLite.instance.rotationTo( cube, 1, new Vector3( 360f, 0, 0 ), true )
				.setEaseType( EaseType.BackOut );
		}


		if( GUILayout.Button( "Custom Action Tween of Alpha to 0 with 1s Delay" ) )
		{
			// dt 0 to 1 (except for bouce, punch they go a bit less than 0 and a bit more than 1)
			System.Action<Transform,float> action = ( trans, dt ) =>
			{
				var color = trans.GetComponent<Renderer>().material.color;
				color.a = 1 - dt;
				trans.GetComponent<Renderer>().material.color = color;
			};

			GoKitLite.instance.customAction( cube, Random.Range( 0.2f, 1 ), action )
				.setDelay( 1f )
				.setEaseType( EaseType.Linear );
		}


		if( GUILayout.Button( "Custom Action Tween of Alpha to 1" ) )
		{
			System.Action<Transform,float> action = ( trans, dt ) =>
			{
				var color = trans.GetComponent<Renderer>().material.color;
				color.a = dt;
				trans.GetComponent<Renderer>().material.color = color;
			};

			GoKitLite.instance.customAction( cube, Random.Range( 0.2f, 1 ), action )
				.setEaseType( EaseType.Linear )
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
				.setLoopType( GoKitLite.LoopType.PingPong, 2 );
		}


		if( GUILayout.Button( "Shake Position Ramp up/down Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.shakePositionRamp( cube, 0.6f ) )
				.setEaseType( EaseType.Linear );
		}


		if( GUILayout.Button( "Shake Position Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.shakePosition( cube, 0.6f ) )
				.setEaseType( EaseType.Linear );
		}


		if( GUILayout.Button( "Shake Scale Tween" ) )
		{
			GoKitLite.instance.customAction( cube, 2, GoKitLiteActions.shakeScale( cube, 0.6f ) )
				.setEaseType( EaseType.Linear );
		}


		if( GUILayout.Button( "RectTransform Panel Position Tween" ) )
		{
			GoKitLite.instance.rectTransformPositionTo( panel, 1.0f, new Vector3( -Screen.width * 0.5f, -Screen.height * 0.5f ), true )
				.setEaseType( EaseType.BounceOut )
				.setLoopType( GoKitLite.LoopType.PingPong, 2, 0.2f );
		}


		if( GUILayout.Button( "RectTransform Button Position Tween" ) )
		{
			GoKitLite.instance.rectTransformPositionTo( panel.GetChild( 0 ) as RectTransform, 1.0f, new Vector3( 0f, panel.rect.height * 0.8f ), true )
				.setLoopType( GoKitLite.LoopType.PingPong, 1, 0.5f );

			GoKitLite.instance.rectTransformPositionTo( panel.GetChild( 1 ) as RectTransform, 1.0f, new Vector3( 0f, panel.rect.height * 0.8f ), true )
				.setDelay( 0.2f )
				.setLoopType( GoKitLite.LoopType.PingPong, 1, 0.5f )
				.setEaseType( EaseType.BackOut );
		}


		if( GUILayout.Button( "Stop All tweens" ) )
		{
			GoKitLite.instance.stopAllTweens( true );
		}
	}

}
