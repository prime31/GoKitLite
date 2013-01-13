using EaseFunction = System.Func<float, float, float>;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class GoKitLite : MonoBehaviour
{
	#region Internal classes and enums

	public class Tween
	{
		private enum TargetValueType
		{
			None,
			Vector3,
			Color
		}

		// common properties
		internal int id;
		internal Transform transform;
		internal TweenType tweenType;
		internal float duration;
		internal float delay;
		internal EaseFunction easeFunction;
		internal bool isRelativeTween;
		internal Action<Transform> onComplete;
		internal LoopType loopType;
		internal int loops = 0;
		
		// tweenable properties: Vector3
		internal Vector3 targetVector;
		private Vector3 _startVector;
		private Vector3 _diffVector;

		// Color
		internal Color targetColor;
		private Color _startColor;
		private Color _diffColor;
		private Material _material;

		internal Action<Transform,float> customAction;

		// internal state
		private float _elapsedTime;
		private TargetValueType targetValueType;
		
		
		internal void reset()
		{
			// any pointers or values that are not guarnateed to be set later are defaulted here
			transform = null;
			targetVector = _startVector = _diffVector = Vector3.zero;
			delay = 0;
			loopType = LoopType.None;
			easeFunction = null;
			isRelativeTween = false;
			onComplete = null;
			customAction = null;
			_material = null;
		}


		/// <summary>
		/// sets the appropriate start value and calculates the diffValue
		/// </summary>
		internal void prepareForUse()
		{
			if( easeFunction == null )
				easeFunction = defaultEaseFunction;
			
			switch( tweenType )
			{
				case TweenType.Position:
					targetValueType = TargetValueType.Vector3;
					_startVector = transform.position;
					break;
				case TweenType.LocalPosition:
					targetValueType = TargetValueType.Vector3;
					_startVector = transform.localPosition;
					break;
				case TweenType.Scale:
					targetValueType = TargetValueType.Vector3;
					_startVector = transform.localScale;
					break;
				case TweenType.Rotation:
					targetValueType = TargetValueType.Vector3;
					_startVector = transform.rotation.eulerAngles;

					if( isRelativeTween )
						_diffVector = targetVector;
					else
						_diffVector = new Vector3( Mathf.DeltaAngle( _startVector.x, targetVector.x ), Mathf.DeltaAngle( _startVector.y, targetVector.y ), Mathf.DeltaAngle( _startVector.z, targetVector.z ) );
					break;
				case TweenType.LocalRotation:
					targetValueType = TargetValueType.Vector3;
					_startVector = transform.localRotation.eulerAngles;

					if( isRelativeTween )
						_diffVector = targetVector;
					else
						_diffVector = new Vector3( Mathf.DeltaAngle( _startVector.x, targetVector.x ), Mathf.DeltaAngle( _startVector.y, targetVector.y ), Mathf.DeltaAngle( _startVector.z, targetVector.z ) );
					break;
				case TweenType.Color:
					targetValueType = TargetValueType.Color;
					break;
				case TweenType.Action:
					targetValueType = TargetValueType.None;
					break;
			}
			
			_elapsedTime = -delay;

			// we have to be careful with rotations because we always want to rotate in the shortest angle so we set the diffValue with that in mind
			if( targetValueType == TargetValueType.Vector3 && tweenType != TweenType.Rotation && tweenType != TweenType.LocalRotation)
			{
				if( isRelativeTween )
					_diffVector = targetVector;
				else
					_diffVector = targetVector - _startVector;
			}
			else if( targetValueType == TargetValueType.Color )
			{
				_material = transform.renderer.material;
				_startColor = _material.color;

				if( isRelativeTween )
					_diffColor = targetColor;
				else
					_diffColor = targetColor - _startColor;
			}
		}

		
		/// <summary>
		/// handles the tween. returns true if it is complete and ready for removal
		/// </summary>
		internal bool tick( float deltaTime )
		{
			// add deltaTime to our elapsed time and clamp it from -delay to duration
			_elapsedTime = Mathf.Clamp( _elapsedTime + deltaTime, -delay, duration );

			// if we have a delay, we will have a negative elapsedTime until the delay is complete
			if( _elapsedTime <= 0 )
				return false;
			
			var easedTime = easeFunction( _elapsedTime, duration );

			// special case: Action tweens
			if( tweenType == TweenType.Action )
				customAction( transform, easedTime );

			if( targetValueType == TargetValueType.Vector3 )
			{
				var vec = new Vector3
				(
					_startVector.x + _diffVector.x * easedTime,
	            	_startVector.y + _diffVector.y * easedTime,
	            	_startVector.z + _diffVector.z * easedTime
				);
				setVectorAsRequiredPerCurrentTweenType( ref vec );
			}
			else if( targetValueType == TargetValueType.Color )
			{
				_material.color = new Color
				(
					_startColor.r + _diffColor.r * easedTime,
					_startColor.g + _diffColor.g * easedTime,
					_startColor.b + _diffColor.b * easedTime,
					_startColor.a + _diffColor.a * easedTime
				);
			}

			// if we have a loopType and we are done implement it
			if( loopType != GoKitLite.LoopType.None && _elapsedTime == duration )
				handleLooping();
			
			return _elapsedTime == duration;
		}


		/// <summary>
		/// handles loop logic
		/// </summary>
		private void handleLooping()
		{
			loops--;
			if( loopType == GoKitLite.LoopType.RestartFromBeginning )
			{
				if( targetValueType == TargetValueType.Vector3 )
					setVectorAsRequiredPerCurrentTweenType( ref _startVector );
				else if( targetValueType == TargetValueType.Color )
					_material.color = _startColor;
			}
			else // ping-pong
			{
				targetVector = _startVector;
				targetColor = _startColor;
			}

			// kill our loop if we have no loops left and zero out the delay then prepare for use
			if( loops == 0 )
				loopType = GoKitLite.LoopType.None;

			delay = 0;
			prepareForUse();
		}


		/// <summary>
		/// if we have an appropriate tween type that takes a vector value this will correctly set it
		/// </summary>
		private void setVectorAsRequiredPerCurrentTweenType( ref Vector3 vec )
		{
			switch( tweenType )
			{
				case TweenType.Position:
					transform.position = vec;
					break;
				case TweenType.LocalPosition:
					transform.localPosition = vec;
					break;
				case TweenType.Scale:
					transform.localScale = vec;
					break;
				case TweenType.Rotation:
					transform.eulerAngles = vec;
					break;
				case TweenType.LocalRotation:
					transform.localEulerAngles = vec;
					break;
			}
		}


		/// <summary>
		/// chainable. sets the action that should be called when the tween is complete. do not store a reference to the tween!
		/// </summary>
		public Tween setCompletionHandler( Action<Transform> onComplete )
		{
			this.onComplete = onComplete;
			return this;
		}


		/// <summary>
		/// chainable. set the loop type for the tween. do not store a reference to the tween!
		/// </summary>
		public Tween setLoopType( LoopType loopType, int loops = 1 )
		{
			this.loopType = loopType;
			this.loops = loops;
			return this;
		}


		/// <summary>
		/// gets the id which can be used to stop the tween later
		/// </summary>
		public int getId()
		{
			return id;
		}

	}


	public class TweenQueue
	{
		private Queue<System.Func<GoKitLite.Tween>> _queue = new Queue<System.Func<GoKitLite.Tween>>();
		private System.Action _onComplete;
		private int _currentlyRunningTweenId;
	
	
		private void onTweenComplete( Transform trans )
		{
			runNextTween();
		}
	
	
		private void runNextTween()
		{
			// if there is nothing left in the stack fire the completionHandler and exit
			if( _queue.Count == 0 )
			{
				if( _onComplete != null )
					_onComplete();
				return;
			}
	
			var func = _queue.Dequeue();
			_currentlyRunningTweenId = func().setCompletionHandler( onTweenComplete ).getId();
		}
	
	
		public TweenQueue add( System.Func<GoKitLite.Tween> actionTween )
		{
			_queue.Enqueue( actionTween );
			return this;
		}
	
	
		public TweenQueue setCompletionHandler( System.Action onComplete )
		{
			_onComplete = onComplete;
			return this;
		}
	
	
		public void start()
		{
			runNextTween();
		}
	
	
		public void stop( bool bringCurrentlyRunningTweenToCompletion )
		{
			_queue.Clear();
			GoKitLite.instance.stopTween( _currentlyRunningTweenId, bringCurrentlyRunningTweenToCompletion );
			runNextTween();
		}
	
	}

	
	internal enum TweenType
	{
		Position,
		LocalPosition,
		Rotation,
		LocalRotation,
		Scale,
	    Color,
		Action
	}


	public enum LoopType
	{
		None,
		RestartFromBeginning,
		PingPong
	}

	#endregion

	
	private List<Tween> _activeTweens = new List<Tween>( 1500 );
	private Stack<Tween> _inactiveTweenStack = new Stack<Tween>();
	private int _tweenIdCounter = 0;

	public static EaseFunction defaultEaseFunction = GoKitLiteEasing.Quartic.EaseIn;

	// holds the singleton instance. note that this is not an enforced singleton.
	public static GoKitLite instance;
	
	
	#region MonoBehaviour
	
	private void OnApplicationQuit()
	{
		instance = null;
		Destroy( gameObject );
	}
	
	
	private void Update()
	{
		var dt = Time.deltaTime;

		// loop backwards so we can remove completed tweens
		for( var i = _activeTweens.Count - 1; i >= 0; --i )
		{
			var tween = _activeTweens[i];
			if( tween.transform == null || tween.tick( dt ) )
			{
				if( tween.onComplete != null )
					tween.onComplete( tween.transform );
				removeTween( tween, i );
			}
		}
	}
	
	#endregion
	
	
	#region Private
	
	private Tween nextAvailableTween( Transform trans, float duration, TweenType tweenType )
	{
		Tween tween = null;
		if( _inactiveTweenStack.Count > 0 )
			tween = _inactiveTweenStack.Pop();
		else
			tween = new Tween();
		
		tween.id = ++_tweenIdCounter;
		tween.transform = trans;
		tween.duration = duration;
		tween.tweenType = tweenType;
		
		return tween;
	}
	

	private void removeTween( Tween tween, int index )
	{
		_activeTweens.RemoveAt( index );
		tween.reset();
		_inactiveTweenStack.Push( tween );
	}
	
	#endregion
	
	
	#region Public
	
	public Tween positionTo( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Position );
		tween.delay = delay;
		tween.targetVector = targetPosition;
		tween.easeFunction = easeFunction;
		tween.isRelativeTween = isRelativeTween;
		tween.prepareForUse();
		
		_activeTweens.Add( tween );
		
		return tween;
	}
	
	
	public Tween positionFrom( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var currentPosition = trans.position;
		trans.position = targetPosition;

		return positionTo( trans, duration, currentPosition, delay, easeFunction, isRelativeTween );
	}
		
	
	public Tween localPositionTo( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.LocalPosition );
		tween.delay = delay;
		tween.targetVector = targetPosition;
		tween.easeFunction = easeFunction;
		tween.isRelativeTween = isRelativeTween;
		tween.prepareForUse();
		
		_activeTweens.Add( tween );
		
		return tween;
	}


	public Tween localPositionFrom( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var currentPosition = trans.localPosition;
		trans.localPosition = targetPosition;

		return localPositionTo( trans, duration, currentPosition, delay, easeFunction, isRelativeTween );
	}
	
	
	public Tween scaleTo( Transform trans, float duration, Vector3 targetScale, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Scale );
		tween.targetVector = targetScale;
		tween.easeFunction = easeFunction;
		tween.isRelativeTween = isRelativeTween;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween;
	}


	public Tween scaleFrom( Transform trans, float duration, Vector3 targetScale, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var currentScale = trans.localScale;
		trans.localScale = targetScale;

		return scaleTo( trans, duration, currentScale, delay, easeFunction, isRelativeTween );
	}


	public Tween rotationTo( Transform trans, float duration, Vector3 targetEulers, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Rotation );
		tween.targetVector = targetEulers;
		tween.easeFunction = easeFunction;
		tween.isRelativeTween = isRelativeTween;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween;
	}


	public Tween rotationFrom( Transform trans, float duration, Vector3 targetEulers, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var currentEulers = trans.eulerAngles;
		trans.eulerAngles = targetEulers;

		return rotationTo( trans, duration, currentEulers, delay, easeFunction, isRelativeTween );
	}


	public Tween localRotationTo( Transform trans, float duration, Vector3 targetEulers, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.LocalRotation );
		tween.targetVector = targetEulers;
		tween.easeFunction = easeFunction;
		tween.isRelativeTween = isRelativeTween;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween;
	}


	public Tween localRotationFrom( Transform trans, float duration, Vector3 targetEulers, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var currentEulers = trans.localEulerAngles;
		trans.localEulerAngles = targetEulers;

		return localRotationTo( trans, duration, currentEulers, delay, easeFunction, isRelativeTween );
	}


	public Tween colorTo( Transform trans, float duration, Color targetColor, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Color );
		tween.delay = delay;
		tween.targetColor = targetColor;
		tween.easeFunction = easeFunction;
		tween.isRelativeTween = isRelativeTween;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween;
	}


	public Tween colorFrom( Transform trans, float duration, Color targetColor, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
	{
		var currentColor = trans.renderer.material.color;
		trans.renderer.material.color = targetColor;

		return colorTo( trans, duration, currentColor, delay, easeFunction, isRelativeTween );
	}


	public Tween customAction( Transform trans, float duration, Action<Transform,float> action, float delay = 0, EaseFunction easeFunction = null )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Action );
		tween.easeFunction = easeFunction;
		tween.customAction = action;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween;
	}


	#endregion


	#region Tween Management

	public static void init()
	{
		if( !instance )
		{
			// check if there is a GoKitLite instance already available in the scene graph before creating one
			instance = FindObjectOfType( typeof( GoKitLite ) ) as GoKitLite;

			if( !instance )
			{
				var obj = new GameObject( "GoKitLite" );
				instance = obj.AddComponent<GoKitLite>();
				DontDestroyOnLoad( obj );
			}
		}
	}


	/// <summary>
	/// stops the tween optionally bringing it to its final value first. returns true if the tween was found and stopped.
	/// </summary>
	public bool stopTween( int id, bool bringToCompletion )
	{
		for( var i = 0; i < _activeTweens.Count; i++ )
		{
			if( _activeTweens[i].id == id )
			{
				// send in a delta of float.max if we should be completing this tween before killing it
				if( bringToCompletion )
					_activeTweens[i].tick( float.MaxValue );

				removeTween( _activeTweens[i], i );
				return true;
			}
		}
		
		return false;
	}
	
	#endregion
	
}
