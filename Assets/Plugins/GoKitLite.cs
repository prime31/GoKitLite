using EaseFunction = System.Func<float, float, float, float, float>;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class GoKitLite : MonoBehaviour
{
	public class Tween
	{
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
		
		// tweenable properties
		internal Vector3 targetVector;
		private Vector3 _startVector;
		private Vector3 _diffVector;
		internal Action<float> customAction;

		private float _elapsedTime;
		
		
		internal void reset()
		{
			isRelativeTween = false;
			transform = null;
			targetVector = _startVector = _diffVector = Vector3.zero;
			_elapsedTime = 0;
			delay = 0;
			easeFunction = null;
			onComplete = null;
			customAction = null;
			loopType = LoopType.None;
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
					_startVector = transform.position;
					break;
				case TweenType.LocalPosition:
					_startVector = transform.localPosition;
					break;
				case TweenType.Scale:
					_startVector = transform.localScale;
					break;
				case TweenType.Rotation:
					_startVector = transform.rotation.eulerAngles;

					if( isRelativeTween )
						_diffVector = targetVector;
					else
						_diffVector = new Vector3( Mathf.DeltaAngle( _startVector.x, targetVector.x ), Mathf.DeltaAngle( _startVector.y, targetVector.y ), Mathf.DeltaAngle( _startVector.z, targetVector.z ) );
					break;
				case TweenType.LocalRotation:
					_startVector = transform.localRotation.eulerAngles;

					if( isRelativeTween )
						_diffVector = targetVector;
					else
						_diffVector = new Vector3( Mathf.DeltaAngle( _startVector.x, targetVector.x ), Mathf.DeltaAngle( _startVector.y, targetVector.y ), Mathf.DeltaAngle( _startVector.z, targetVector.z ) );
					break;
			}
			
			_elapsedTime = -delay;

			// we have to be careful with rotations because we always want to rotate in the shortest angle so we set the diffValue with that in mind
			if( tweenType != TweenType.Rotation && tweenType != TweenType.LocalRotation && tweenType != TweenType.Action )
			{
				if( isRelativeTween )
					_diffVector = targetVector;
				else
					_diffVector = targetVector - _startVector;
			}
		}

		
		/// <summary>
		/// handles the tween. returns true if it is complete and ready for removal
		/// </summary>
		internal bool tick( float deltaTime )
		{
			if( transform == null || transform.Equals( null ) )
				return true;

			// add deltaTime to our elapsed time and clamp it from -delay to duration
			_elapsedTime = Mathf.Clamp( _elapsedTime + deltaTime, -delay, duration );

			// if we have a delay, we will have a negative elapsedTime until the delay is complete
			if( _elapsedTime <= 0 )
				return false;
			
			var easedTime = easeFunction( _elapsedTime, 0, 1, duration );

			// special case: Action tweens
			if( tweenType == TweenType.Action )
			{
				customAction( easedTime );
				return _elapsedTime == duration;
			}

			var vec = unclampedVector3Lerp( _startVector, _diffVector, easedTime );
			setVectorAsRequired( vec );

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
				setVectorAsRequired( _startVector );
			}
			else // ping-pong
			{
				targetVector = _startVector;
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
		private void setVectorAsRequired( Vector3 vec )
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
		/// unclamped lerp from v1 to v2. diff should be v2 - v1 (or just v2 for relative lerps)
		/// </summary>
	    private Vector3 unclampedVector3Lerp( Vector3 v1, Vector3 diff, float value )
		{
	        return new Vector3
			(
				v1.x + diff.x * value,
	            v1.y + diff.y * value,
	            v1.z + diff.z * value
			);
	    }


		/// <summary>
		/// chainable. sets the action that should be called when the tween is complete
		/// </summary>
		public Tween setCompletionHandler( Action<Transform> onComplete )
		{
			this.onComplete = onComplete;
			return this;
		}


		/// <summary>
		/// chainable. set the loop type for the tween
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
	
	
	internal enum TweenType
	{
		Position,
		LocalPosition,
		Rotation,
		LocalRotation,
		Scale,
		Action
	}


	public enum LoopType
	{
		None,
		RestartFromBeginning,
		PingPong
	}
	
	private List<Tween> _activeTweens = new List<Tween>();
	private Queue<Tween> _tweenQueue;
	private int _tweenIdCounter = 0;

	public static EaseFunction defaultEaseFunction = GoKitLiteEasing.Quartic.EaseIn;
	
	// only one GoKitLite can exist
	static GoKitLite _instance = null;
	public static GoKitLite instance
	{
		get
		{
			if( !_instance )
			{
				// check if there is a GO instance already available in the scene graph
				_instance = FindObjectOfType( typeof( GoKitLite ) ) as GoKitLite;

				// nope, create a new one
				if( !_instance )
				{
					var obj = new GameObject( "GoKitLite" );
					_instance = obj.AddComponent<GoKitLite>();
					_instance._tweenQueue = new Queue<Tween>();
					DontDestroyOnLoad( obj );
				}
			}

			return _instance;
		}
	}
	
	
	#region MonoBehaviour
	
	private void OnApplicationQuit()
	{
		_instance = null;
		Destroy( gameObject );
	}
	
	
	private void Update()
	{
		// loop backwards so we can remove completed tweens
		for( var i = _activeTweens.Count - 1; i >= 0; --i )
		{
			var tween = _activeTweens[i];
			if( tween.tick( Time.deltaTime ) )
			{
				if( tween.onComplete != null )
					tween.onComplete( tween.transform );
				removeTween( tween );
			}
		}
	}
	
	#endregion
	
	
	#region Private
	
	private Tween nextAvailableTween( Transform trans, float duration, TweenType tweenType )
	{
		Tween tween = null;
		if( _tweenQueue.Count > 0 )
		{
			tween = _tweenQueue.Dequeue();
			tween.reset();
		}
		else
		{
			tween = new Tween();
		}
		
		tween.id = ++_tweenIdCounter;
		tween.transform = trans;
		tween.duration = duration;
		tween.tweenType = tweenType;
		
		return tween;
	}
	
	
	private void removeTween( Tween tween )
	{
		_activeTweens.Remove( tween );
		tween.reset();
		_tweenQueue.Enqueue( tween );
	}
	
	#endregion
	
	
	#region Public
	
	public Tween positionTo( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Position );
		tween.delay = delay;
		tween.targetVector = targetPosition;
		tween.easeFunction = easeFunction;
		tween.prepareForUse();
		
		_activeTweens.Add( tween );
		
		return tween;
	}
	
	
	public Tween positionFrom( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null )
	{
		var currentPosition = trans.position;
		trans.position = targetPosition;

		var tween = nextAvailableTween( trans, duration, TweenType.Position );
		tween.delay = delay;
		tween.targetVector = currentPosition;
		tween.prepareForUse();
		
		_activeTweens.Add( tween );
		
		return tween;
	}
		
	
	public int localPositionTo( Transform trans, float duration, Vector3 targetPosition )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.LocalPosition );
		tween.targetVector = targetPosition;
		tween.prepareForUse();
		
		_activeTweens.Add( tween );
		
		return tween.id;
	}
	
	
	public int scaleTo( Transform trans, float duration, Vector3 targetScale, float delay = 0, EaseFunction easeFunction = null )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Scale );
		tween.targetVector = targetScale;
		tween.easeFunction = easeFunction;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween.id;
	}


	public int rotationTo( Transform trans, float duration, Vector3 targetRotation, float delay = 0, EaseFunction easeFunction = null )
	{
		var tween = nextAvailableTween( trans, duration, TweenType.Rotation );
		tween.targetVector = targetRotation;
		tween.easeFunction = easeFunction;
		tween.prepareForUse();

		_activeTweens.Add( tween );

		return tween.id;
	}


	public Tween customAction( Transform trans, float duration, Action<float> action, float delay = 0, EaseFunction easeFunction = null )
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

				removeTween( _activeTweens[i] );
				return true;
			}
		}
		
		return false;
	}
	
	#endregion
	
}
