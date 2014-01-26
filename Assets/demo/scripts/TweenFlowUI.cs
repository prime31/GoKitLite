using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Prime31.GoKitLite;


public class TweenFlowUI : MonoBehaviour
{
	public Transform cube;

	
	void OnGUI()
	{
		GUI.matrix = Matrix4x4.Scale( new Vector3( 2, 2, 2 ) );
		
		if( GUILayout.Button( "Tween Position/Rotation Flow" ) )
		{
			Debug.Log( "Do a position tween that lasts 5 seconds and do a rotation tween every second for 0.5 seconds while the position tween is going" );

			var flow = new TweenFlow().add( 0, () => { return GoKitLite.instance.positionTo( cube, 5f, new Vector3( 10, 10, 10 ) ); } )
				.add( 1f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 90, 0, 0 ) ); } )
				.add( 2f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 0, 90, 0 ) ); } )
				.add( 3f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 0, 0, 90 ) ); } )
				.add( 4f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 180, 180, 180 ) ); } )
				.setCompletionHandler( () => { Debug.Log( "All done with the position/rotation flow" ); } );
			
			StartCoroutine( flow.start() );
		}


		if( GUILayout.Button( "Bounce and Tween Color" ) )
		{
			Debug.Log( "First move to 0,0,0 then do a position tween up/down/up/down while simultaneously tweening the color" );

			// first we add our position tweens
			var flow = new TweenFlow().add( 0, () => { return GoKitLite.instance.positionTo( cube, 1f, new Vector3( 0, 0, 0 ) ); } )
				.add( 1, () => { return GoKitLite.instance.positionTo( cube, 1f, new Vector3( 0, 5, 0 ), 0, GoKitLiteEasing.Bounce.EaseOut ); } )
				.add( 2, () => { return GoKitLite.instance.positionTo( cube, 1f, new Vector3( 0, 0, 0 ), 0, GoKitLiteEasing.Bounce.EaseOut ); } )
				.add( 3, () => { return GoKitLite.instance.positionTo( cube, 1f, new Vector3( 0, 5, 0 ), 0, GoKitLiteEasing.Bounce.EaseOut ); } )
				.add( 4, () => { return GoKitLite.instance.positionTo( cube, 1f, new Vector3( 0, 0, 0 ), 0, GoKitLiteEasing.Bounce.EaseOut ); } );

			// now we add the color tweens. each will start just after the position tween starts
			flow.add( 1.2f, () => { return GoKitLite.instance.colorTo( cube, 0.5f, Color.magenta ); } )
				.add( 2.2f, () => { return GoKitLite.instance.colorTo( cube, 0.5f, Color.blue ); } )
				.add( 3.2f, () => { return GoKitLite.instance.colorTo( cube, 0.5f, Color.yellow ); } )
				.add( 4.2f, () => { return GoKitLite.instance.colorTo( cube, 0.5f, Color.green ); } );

			StartCoroutine( flow.start() );
		}

	}
}