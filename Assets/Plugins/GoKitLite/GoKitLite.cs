using EaseFunction = System.Func<float, float, float>;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Prime31.GoKitLite
{
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
			internal Action<Transform> onLoopComplete;
			internal LoopType loopType;
			internal int loops = 0;
			internal Tween nextTween;

			// tweenable: Vector3
			internal Vector3 targetVector;
			private Vector3 _startVector;
			private Vector3 _diffVector;

			// tweenable: Color
			internal Color targetColor;
			private Color _startColor;
			private Color _diffColor;
			private Material _material;
			internal string materialProperty;

			// tweenable: Action and property
			internal Action<Transform,float> customAction;
			internal IGoKitLiteTweenProperty propertyTween;

			// internal state
			private float _elapsedTime;
			private TargetValueType targetValueType;


			internal void reset()
			{
				// any pointers or values that are not guaranteed to be set later are defaulted here
				transform = null;
				targetVector = _startVector = _diffVector = Vector3.zero;
				delay = 0;
				loopType = LoopType.None;
				easeFunction = null;
				isRelativeTween = false;
				onComplete = onLoopComplete = null;
				customAction = null;
				_material = null;
				materialProperty = null;

				if( nextTween != null )
				{
					// null out and return to the stack all additional tweens
					GoKitLite.instance._inactiveTweenStack.Push( nextTween );
					nextTween.reset();
				}

				nextTween = null;
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
					case TweenType.Property:
						targetValueType = TargetValueType.None;
						propertyTween.prepareForUse();
						break;
				}

				_elapsedTime = -delay;

				// we have to be careful with rotations because we always want to rotate in the shortest angle so we set the diffValue with that in mind
				if( targetValueType == TargetValueType.Vector3 && tweenType != TweenType.Rotation && tweenType != TweenType.LocalRotation )
				{
					if( isRelativeTween )
						_diffVector = targetVector;
					else
						_diffVector = targetVector - _startVector;
				}
				else if( targetValueType == TargetValueType.Color )
				{
					_material = transform.renderer.material;
					_startColor = _material.GetColor( materialProperty );

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
				else if( tweenType == TweenType.Property )
					propertyTween.tick( easedTime );

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
					_material.SetColor( materialProperty, new Color
					(
						_startColor.r + _diffColor.r * easedTime,
						_startColor.g + _diffColor.g * easedTime,
						_startColor.b + _diffColor.b * easedTime,
						_startColor.a + _diffColor.a * easedTime
					) );
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
						_material.SetColor( materialProperty, _startColor );
				}
				else // ping-pong
				{
					targetVector = _startVector;
					targetColor = _startColor;
				}

				if( loopType == GoKitLite.LoopType.RestartFromBeginning || loops % 2 == 1 )
				{
					if( onLoopComplete != null )
						onLoopComplete( transform );
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
			/// chainable. sets the action that should be called when a loop is complete. A loop is either when the first part of
			/// a ping-pong animation completes or when starting over when using a restart-from-beginning loop type. Note that ping-pong
			/// loops (which are really two part tweens) will not fire the loop completion handler on the last iteration. The normal
			/// tween completion handler will fire though
			/// </summary>
			public Tween setLoopCompletionHandler( Action<Transform> onLoopComplete )
			{
				this.onLoopComplete = onLoopComplete;
				return this;
			}


			/// <summary>
			/// chainable. set the loop type for the tween. do not store a reference to the tween!
			/// </summary>
			public Tween setLoopType( LoopType loopType, int loops = 1 )
			{
				this.loopType = loopType;

				// double the loop count for ping-pong
				if( loopType == LoopType.PingPong )
					loops = loops * 2 - 1;
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


			/// <summary>
			/// adds a vector tween using this tween's Transform and type that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Vector3 targetVector, float delay = 0 )
			{
				var tween = GoKitLite.instance.vectorTweenTo( transform, tweenType, duration, targetVector, delay, easeFunction, false );
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a vector tween using this tween's Transform and type that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Vector3 targetVector, float delay, EaseFunction easeFunction, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.vectorTweenTo( transform, tweenType, duration, targetVector, delay, easeFunction, isRelativeTween );
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a tween that will start as soon as this tween completes
			/// </summary>
			public Tween next( TweenType tweenType, float duration, Vector3 targetVector, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.vectorTweenTo( transform, tweenType, duration, targetVector, delay, easeFunction, isRelativeTween );
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a tween that will start as soon as this tween completes
			/// </summary>
			public Tween next( Transform trans, TweenType tweenType, float duration, Vector3 targetVector, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.vectorTweenTo( trans, tweenType, duration, targetVector, delay, easeFunction, isRelativeTween );
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a color tween using this tween's Transform and type that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Color targetColor )
			{
				var tween = GoKitLite.instance.colorTweenTo( transform, duration, targetColor, "_Color", 0, easeFunction, false );
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a color tween using this tween's Transform and type that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Color targetColor, string materialProperty, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.colorTweenTo( transform, duration, targetColor, materialProperty, delay, easeFunction, isRelativeTween );
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a property tween that will start as soon as the current tween completes
			/// </summary>
			public Tween next( float duration, IGoKitLiteTweenProperty newPropertyTween )
			{
				var tween = GoKitLite.instance.nextAvailableTween( transform, duration, TweenType.Property );
				//tween.delay = delay;
				tween.easeFunction = easeFunction;
				tween.propertyTween = newPropertyTween;

				nextTween = tween;

				return tween;
			}

		}


		public enum TweenType
		{
			Position,
			LocalPosition,
			Rotation,
			LocalRotation,
			Scale,
			Color,
			Action,
			Property
		}


		public enum LoopType
		{
			None,
			RestartFromBeginning,
			PingPong
		}

		#endregion


		private List<Tween> _activeTweens = new List<Tween>( 20 );
		internal Stack<Tween> _inactiveTweenStack = new Stack<Tween>();
		private int _tweenIdCounter = 0;
		public static EaseFunction defaultEaseFunction = GoKitLiteEasing.Quartic.EaseIn;

		/// <summary>
		/// holds the singleton instance. creates one on demand if none exists.
		/// </summary>
		private static GoKitLite _instance;
		public static GoKitLite instance
		{
			get
			{
				if( !_instance )
				{
					// check if there is a GoKitLite instance already available in the scene graph before creating one
					_instance = FindObjectOfType( typeof( GoKitLite ) ) as GoKitLite;

					if( !_instance )
					{
						var obj = new GameObject( "GoKitLite" );
						_instance = obj.AddComponent<GoKitLite>();
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
			var dt = Time.deltaTime;

			// loop backwards so we can remove completed tweens
			for( var i = _activeTweens.Count - 1; i >= 0; --i )
			{
				var tween = _activeTweens[i];
				if( tween.transform == null || tween.tick( dt ) )
				{
					if( tween.onComplete != null )
						tween.onComplete( tween.transform );

					// handle nextTween if we have a chain
					if( tween.nextTween != null )
					{
						tween.nextTween.prepareForUse();
						_activeTweens.Add( tween.nextTween );

						// null out the nextTween so that the reset method doesnt remove it!
						tween.nextTween = null;
					}

					removeTween( tween, i );
				}
			}

	#if UNITY_EDITOR
			gameObject.name = "GoKitLite. active tweens: " + _activeTweens.Count;
	#endif
		}

		#endregion


		#region Private

		internal Tween vectorTweenTo( Transform trans, TweenType tweenType, float duration, Vector3 targetVector, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
		{
			var tween = nextAvailableTween( trans, duration, tweenType );
			tween.delay = delay;
			tween.targetVector = targetVector;
			tween.easeFunction = easeFunction;
			tween.isRelativeTween = isRelativeTween;

			return tween;
		}


		internal Tween colorTweenTo( Transform trans, float duration, Color targetColor, string materialProperty = "_Color", float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
		{
			var tween = nextAvailableTween( trans, duration, TweenType.Color );
			tween.delay = delay;
			tween.targetColor = targetColor;
			tween.materialProperty = materialProperty;
			tween.easeFunction = easeFunction;
			tween.isRelativeTween = isRelativeTween;

			return tween;
		}


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
	        if ( _activeTweens.Contains( tween ) )
	        {
	            _activeTweens.RemoveAt( index );

	            tween.reset();
	            _inactiveTweenStack.Push( tween );
	        }
		}

		#endregion


		#region Public

		public Tween positionTo( Transform trans, float duration, Vector3 targetPosition, float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
		{
			var tween = vectorTweenTo( trans, TweenType.Position, duration, targetPosition, delay, easeFunction, isRelativeTween );

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
			var tween = vectorTweenTo( trans, TweenType.LocalPosition, duration, targetPosition, delay, easeFunction, isRelativeTween );

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
			var tween = vectorTweenTo( trans, TweenType.Scale, duration, targetScale, delay, easeFunction, isRelativeTween );

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
			var tween = vectorTweenTo( trans, TweenType.Rotation, duration, targetEulers, delay, easeFunction, isRelativeTween );

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
			var tween = vectorTweenTo( trans, TweenType.LocalRotation, duration, targetEulers, delay, easeFunction, isRelativeTween );

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


		public Tween colorTo( Transform trans, float duration, Color targetColor, string materialProperty = "_Color", float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
		{
			var tween = colorTweenTo( trans, duration, targetColor, materialProperty, delay, easeFunction, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween colorFrom( Transform trans, float duration, Color targetColor, string materialProperty = "_Color", float delay = 0, EaseFunction easeFunction = null, bool isRelativeTween = false )
		{
			var currentColor = trans.renderer.material.GetColor( materialProperty );
			trans.renderer.material.SetColor( materialProperty, targetColor );

			return colorTo( trans, duration, currentColor, materialProperty, delay, easeFunction, isRelativeTween );
		}


		public Tween customAction( Transform trans, float duration, Action<Transform,float> action, float delay = 0, EaseFunction easeFunction = null )
		{
			var tween = nextAvailableTween( trans, duration, TweenType.Action );
			tween.delay = delay;
			tween.easeFunction = easeFunction;
			tween.customAction = action;
			tween.prepareForUse();

			_activeTweens.Add( tween );

			return tween;
		}


		public Tween propertyTween( IGoKitLiteTweenProperty propertyTween, float duration, float delay = 0, EaseFunction easeFunction = null )
		{
			var tween = nextAvailableTween( this.transform, duration, TweenType.Property );
			tween.delay = delay;
			tween.easeFunction = easeFunction;
			tween.propertyTween = propertyTween;

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

					removeTween( _activeTweens[i], i );
					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// Stops all in-progress tweens optionally bringing them to their final values.
		/// </summary>
		/// <param name="bringToCompletion">If true, then all active tweens are broght to completion before they are stopped</param>
		public void stopAllTweens( bool bringToCompletion )
		{
			for( var i = _activeTweens.Count - 1; i >= 0; --i )
			{
				// send in a delta of float.max if we should be completing this tween before killing it
				if( bringToCompletion )
					_activeTweens[i].tick( float.MaxValue );

				removeTween( _activeTweens[i], i );
			}
		}


		/// <summary>
		/// Checks if the current tween is active
		/// </summary>
		/// <param name="id"></param>
		/// <returns>True if the tween is active, false otherwise</returns>
		public bool isTweenActive( int id )
		{
			for( var i = 0; i < _activeTweens.Count; i++ )
			{
				if( _activeTweens[i].id == id )
					return true;
			}

			return false;
		}

	    #endregion

	}
}