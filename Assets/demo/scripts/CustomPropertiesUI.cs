using UnityEngine;
using System.Collections;
using Prime31.GoKitLite;


public class CustomPropertiesUI : MonoBehaviour
{
	public Transform cube;
	
	// custom property example
	public float width
	{
		set
		{
			cube.localScale = new Vector3( value, cube.localScale.y, cube.localScale.z );
		}
		get
		{
			return cube.localScale.x;
		}
	}
	
	public float height
	{
		set
		{
			cube.localScale = new Vector3( cube.localScale.x, value, cube.localScale.z );
		}
		get
		{
			return cube.localScale.y;
		}
	}

	
	void OnGUI()
	{
		if( GUILayout.Button( "Tween Custom Property (width -> localScale.x)" ) )
		{
			var prop = new FloatTweenProperty( this, "width", 8f );
			GoKitLite.instance.propertyTween( prop, 2f, 0, GoKitLiteEasing.Bounce.EaseOut );
		}
		
		
		if( GUILayout.Button( "Relative Tween of Custom Property (height -> localScale.y)" ) )
		{
			var prop = new FloatTweenProperty( this, "height", 2f, true );
			GoKitLite.instance.propertyTween( prop, 1f, 0, GoKitLiteEasing.Bounce.EaseOut );
		}
		

		if( GUILayout.Button( "Position Tween" ) )
		{
			var toTenTween = new Vector3TweenProperty( cube, "position", new Vector3( 10, 10, 10 ) );
			var backHomeTween = new Vector3TweenProperty( cube, "position", Vector3.zero );
			
			GoKitLite.instance.propertyTween( toTenTween, 1f, 0, GoKitLiteEasing.Bounce.EaseOut )
				.next( 1f, backHomeTween );
		}
		
		
		if( GUILayout.Button( "Tween Main Texture Offset" ) )
		{
			var prop = new Vector2TweenProperty( cube.renderer.material, "mainTextureOffset", new Vector2( 50, 50 ) );
			GoKitLite.instance.propertyTween( prop, 60f, 0, GoKitLiteEasing.Linear.EaseNone );
		}
	}

}
