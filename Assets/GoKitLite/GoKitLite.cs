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
			internal bool isTimeScaleIndependent;
			internal bool isRunningInReverse;
			internal float duration;
			internal float delay;
			internal float delayBetweenLoops;
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
			internal ITweenable propertyTween;

			// internal state
			private float _elapsedTime;
			private TargetValueType targetValueType;

            // pause state
            internal bool paused;

			internal void reset()
			{
				// any pointers or values that are not guaranteed to be set later are defaulted here
				transform = null;
				targetVector = _startVector = _diffVector = Vector3.zero;
				delay = delayBetweenLoops = 0f;
				isTimeScaleIndependent = isRunningInReverse = false;
				loopType = LoopType.None;
				easeFunction = null;
				isRelativeTween = false;
				onComplete = onLoopComplete = null;
				customAction = null;
				propertyTween = null;
				_material = null;
				materialProperty = null;
                paused = false;

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
			internal bool tick( bool completeTweenThisStep = false )
			{
				// fetch our deltaTime. It will either be taking this to completion or standard delta/unscaledDelta
				var deltaTime = completeTweenThisStep ? float.MaxValue : ( isTimeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime );

				// add deltaTime to our elapsed time and clamp it from -delay to duration
				_elapsedTime = Mathf.Clamp( _elapsedTime + deltaTime, -delay, duration );

				// if we have a delay, we will have a negative elapsedTime until the delay is complete
				if( _elapsedTime <= 0 )
					return false;

				var modifiedElapsedTime = isRunningInReverse ? duration - _elapsedTime : _elapsedTime;
				var easedTime = easeFunction( modifiedElapsedTime, duration );

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

				// if we have a loopType and we are done do the loop
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

					prepareForUse();
				}
				else // ping-pong
				{
					isRunningInReverse = !isRunningInReverse;
				}

				if( loopType == GoKitLite.LoopType.RestartFromBeginning || loops % 2 == 1 )
				{
					if( onLoopComplete != null )
						onLoopComplete( transform );
				}

				// kill our loop if we have no loops left and zero out the delay then prepare for use
				if( loops == 0 )
					loopType = GoKitLite.LoopType.None;

				delay = delayBetweenLoops;
				_elapsedTime = -delay;
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
			/// reverses the current tween. if it was going forward it will be going backwards and vice versa.
			/// </summary>
			public void reverseTween()
			{
				isRunningInReverse = !isRunningInReverse;
				_elapsedTime = duration - _elapsedTime;
			}


			/// <summary>
			/// chainable. Sets the EaseFunction used by the tween.
			/// </summary>
			public Tween setEaseFunction( EaseFunction easeFunction )
			{
				this.easeFunction = easeFunction;
				return this;
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
			/// chainable. set the loop type for the tween. a single pingpong loop means going from start-finish-start.
			/// </summary>
			public Tween setLoopType( LoopType loopType, int loops = 1, float delayBetweenLoops = 0f )
			{
				this.loopType = loopType;
				this.delayBetweenLoops = delayBetweenLoops;

				// double the loop count for ping-pong
				if( loopType == LoopType.PingPong )
					loops = loops * 2 - 1;
				this.loops = loops;

				return this;
			}


			/// <summary>
			/// chainable. sets the delay for the tween.
			/// </summary>
			public Tween setDelay( float delay )
			{
				this.delay = delay;
                _elapsedTime = -delay;
				return this;
			}


			/// <summary>
			/// sets the tween to be time scale independent
			/// </summary>
			/// <returns>The Tween</returns>
			public Tween setTimeScaleIndependent()
			{
				isTimeScaleIndependent = true;
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
				var tween = GoKitLite.instance.vectorTweenTo( transform, tweenType, duration, targetVector, false );
				tween.delay = delay;
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a vector tween using this tween's Transform and type that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Vector3 targetVector, float delay, EaseFunction easeFunction, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.vectorTweenTo( transform, tweenType, duration, targetVector, isRelativeTween );
				tween.delay = delay;
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a tween that will start as soon as this tween completes
			/// </summary>
			public Tween next( TweenType tweenType, float duration, Vector3 targetVector, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.vectorTweenTo( transform, tweenType, duration, targetVector, isRelativeTween );
				tween.delay = delay;
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a tween that will start as soon as this tween completes
			/// </summary>
			public Tween next( Transform trans, TweenType tweenType, float duration, Vector3 targetVector, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.vectorTweenTo( trans, tweenType, duration, targetVector, isRelativeTween );
				tween.delay = delay;
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a color tween using this tween's Transform and type that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Color targetColor )
			{
				var tween = GoKitLite.instance.colorTweenTo( transform, duration, targetColor, "_Color", false );
				tween.easeFunction = easeFunction;
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a color tween using this tween's Transform that will start as soon as this completes
			/// </summary>
			public Tween next( float duration, Color targetColor, string materialProperty, bool isRelativeTween = false )
			{
				var tween = GoKitLite.instance.colorTweenTo( transform, duration, targetColor, materialProperty, isRelativeTween );
				tween.easeFunction = easeFunction;
				nextTween = tween;

				return tween;
			}


			/// <summary>
			/// adds a property tween that will start as soon as the current tween completes
			/// </summary>
			public Tween next( float duration, ITweenable newPropertyTween )
			{
				var tween = GoKitLite.instance.nextAvailableTween( transform, duration, TweenType.Property );
				tween.easeFunction = easeFunction;
				tween.propertyTween = newPropertyTween;

				nextTween = tween;

				return tween;
			}

            
            /// <summary>
            /// add a custom action tween using this tween's Transform that will start as soon as the current tween completes
            /// </summary>
            public Tween next(float duration, Action<Transform, float> action, float delay = 0)
            {
                var tween = GoKitLite.instance.nextAvailableTween(transform, duration, TweenType.Action);
                tween.delay = delay;
                tween.easeFunction = easeFunction;
                tween.customAction = action;

                nextTween = tween;

                return tween;
            }


            /// <summary>
            /// add a custom action tween that will start as soon as the current tween completes
            /// </summary>
            public Tween next(Transform trans, float duration, Action<Transform, float> action, float delay = 0)
            {
                var tween = GoKitLite.instance.nextAvailableTween(trans, duration, TweenType.Action);
                tween.delay = delay;
                tween.easeFunction = easeFunction;
                tween.customAction = action;

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
		internal Stack<Tween> _inactiveTweenStack = new Stack<Tween>( 20 );
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
			// loop backwards so we can remove completed tweens
			for( var i = _activeTweens.Count - 1; i >= 0; --i )
			{
				var tween = _activeTweens[i];
                if (tween.paused)
                {
                    continue;
                }
				if( tween.transform == null || tween.tick() )
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

		internal Tween vectorTweenTo( Transform trans, TweenType tweenType, float duration, Vector3 targetVector, bool isRelativeTween = false )
		{
			var tween = nextAvailableTween( trans, duration, tweenType );
			tween.targetVector = targetVector;
			tween.isRelativeTween = isRelativeTween;

			return tween;
		}


		internal Tween colorTweenTo( Transform trans, float duration, Color targetColor, string materialProperty = "_Color", bool isRelativeTween = false )
		{
			var tween = nextAvailableTween( trans, duration, TweenType.Color );
			tween.targetColor = targetColor;
			tween.materialProperty = materialProperty;
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
            _activeTweens.RemoveAt( index );

            tween.reset();
            _inactiveTweenStack.Push( tween );
		}

		#endregion


		#region Public

		public Tween positionTo( Transform trans, float duration, Vector3 targetPosition, bool isRelativeTween = false )
		{
			var tween = vectorTweenTo( trans, TweenType.Position, duration, targetPosition, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween positionFrom( Transform trans, float duration, Vector3 targetPosition, bool isRelativeTween = false )
		{
			var currentPosition = trans.position;
			trans.position = targetPosition;

			return positionTo( trans, duration, currentPosition, isRelativeTween );
		}


		public Tween localPositionTo( Transform trans, float duration, Vector3 targetPosition, bool isRelativeTween = false )
		{
			var tween = vectorTweenTo( trans, TweenType.LocalPosition, duration, targetPosition, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween localPositionFrom( Transform trans, float duration, Vector3 targetPosition, bool isRelativeTween = false )
		{
			var currentPosition = trans.localPosition;
			trans.localPosition = targetPosition;

			return localPositionTo( trans, duration, currentPosition, isRelativeTween );
		}


		public Tween scaleTo( Transform trans, float duration, Vector3 targetScale, bool isRelativeTween = false )
		{
			var tween = vectorTweenTo( trans, TweenType.Scale, duration, targetScale, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween scaleFrom( Transform trans, float duration, Vector3 targetScale, bool isRelativeTween = false )
		{
			var currentScale = trans.localScale;
			trans.localScale = targetScale;

			return scaleTo( trans, duration, currentScale, isRelativeTween );
		}


		public Tween rotationTo( Transform trans, float duration, Vector3 targetEulers, bool isRelativeTween = false )
		{
			var tween = vectorTweenTo( trans, TweenType.Rotation, duration, targetEulers, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween rotationFrom( Transform trans, float duration, Vector3 targetEulers, bool isRelativeTween = false )
		{
			var currentEulers = trans.eulerAngles;
			trans.eulerAngles = targetEulers;

			return rotationTo( trans, duration, currentEulers, isRelativeTween );
		}


		public Tween localRotationTo( Transform trans, float duration, Vector3 targetEulers, bool isRelativeTween = false )
		{
			var tween = vectorTweenTo( trans, TweenType.LocalRotation, duration, targetEulers, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween localRotationFrom( Transform trans, float duration, Vector3 targetEulers, bool isRelativeTween = false )
		{
			var currentEulers = trans.localEulerAngles;
			trans.localEulerAngles = targetEulers;

			return localRotationTo( trans, duration, currentEulers, isRelativeTween );
		}


		public Tween colorTo( Transform trans, float duration, Color targetColor, string materialProperty = "_Color", bool isRelativeTween = false )
		{
			var tween = colorTweenTo( trans, duration, targetColor, materialProperty, isRelativeTween );

			tween.prepareForUse();
			_activeTweens.Add( tween );

			return tween;
		}


		public Tween colorFrom( Transform trans, float duration, Color targetColor, string materialProperty = "_Color", bool isRelativeTween = false )
		{
			var currentColor = trans.renderer.material.GetColor( materialProperty );
			trans.renderer.material.SetColor( materialProperty, targetColor );

			return colorTo( trans, duration, currentColor, materialProperty, isRelativeTween );
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


		public Tween propertyTween( ITweenable propertyTween, float duration )
		{
			var tween = nextAvailableTween( this.transform, duration, TweenType.Property );
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
						_activeTweens[i].tick( true );

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
					_activeTweens[i].tick( true );

				removeTween( _activeTweens[i], i );
			}
		}


        /// <summary>
        /// set the tween's pause state. returns true if the tween was found.
        /// </summary>
        public bool setTweenPauseState(int id, bool paused)
        {
            for (var i = 0; i < _activeTweens.Count; i++)
            {
                if (_activeTweens[i].id == id)
                {
                    _activeTweens[i].paused = paused;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// set all in-progress tween's pause state.
        /// </summary>
        public void setAllTweensPauseState(bool paused)
        {
            for (var i = 0; i < _activeTweens.Count; i++)
            {
                _activeTweens[i].paused = paused;
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


        /// <summary>
        /// find an active tween with given id, do not store a reference to the tween!
        /// </summary>
        public Tween getActiveTween(int id)
        {
            for (var i = 0; i < _activeTweens.Count; i++)
            {
                if (_activeTweens[i].id == id)
                    return _activeTweens[i];
            }

            return null;
        }


		/// <summary>
		/// reverses the tween. if it was going forward it will be going backwards and vice versa.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>True if the tween is active, false otherwise</returns>
		private bool reverseTween( int id )
		{
			for( var i = 0; i < _activeTweens.Count; i++ )
			{
				if( _activeTweens[i].id == id )
				{
					_activeTweens[i].reverseTween();
					return true;
				}
			}

			return false;
		}

	    #endregion
	}
}