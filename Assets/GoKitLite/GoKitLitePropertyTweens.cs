using UnityEngine;
using System.Collections;
using System;
using System.Reflection;


namespace Prime31.GoKitLite
{
	/// <summary>
	/// helper class to fetch property delegates
	/// </summary>
	internal class Utils
	{
		/// <summary>
		/// either returns a super fast Delegate to set the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T setterForProperty<T>( System.Object targetObject, string propertyName )
		{
				// first get the property
#if NETFX_CORE
				var propInfo = targetObject.GetType().GetRuntimeProperty( propertyName );
#else
				var propInfo = targetObject.GetType().GetProperty( propertyName );
#endif

				if( propInfo == null )
				{
					Debug.Log( "could not find property with name: " + propertyName );
					return default( T );
				}

#if NETFX_CORE
				// Windows Phone/Store new API
				return (T)(object)propInfo.SetMethod.CreateDelegate( typeof( T ), targetObject );
#else
				return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, propInfo.GetSetMethod() );
#endif
		}


		/// <summary>
		/// either returns a super fast Delegate to get the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T getterForProperty<T>( System.Object targetObject, string propertyName )
		{
				// first get the property
#if NETFX_CORE
				var propInfo = targetObject.GetType().GetRuntimeProperty( propertyName );
#else
				var propInfo = targetObject.GetType().GetProperty( propertyName );
#endif

				if( propInfo == null )
				{
					Debug.Log( "could not find property with name: " + propertyName );
					return default( T );
				}

#if NETFX_CORE
				// Windows Phone/Store new API
				return (T)(object)propInfo.GetMethod.CreateDelegate( typeof( T ), targetObject );
#else
				return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, propInfo.GetGetMethod() );
#endif
		}

	}


	/// <summary>
	/// interface to make working with property and custom tweens easier
	/// </summary>
	public interface ITweenable
	{
		void prepareForUse();
		void tick( float easedTime );
	}


	/// <summary>
	/// tweens any float property
	/// </summary>
	public struct FloatTweenProperty : ITweenable
	{
		private Action<float> _setter;
		private Func<float> _getter;
		private bool _isRelative;
		private float _targetValue;
		private float _startValue;
		private float _diffValue;


		public FloatTweenProperty( object target, string propertyName, float endValue, bool isRelative = false )
		{
			_setter = Utils.setterForProperty<Action<float>>( target, propertyName );
			_getter = Utils.getterForProperty<Func<float>>( target, propertyName );
			_targetValue = endValue;
			_isRelative = isRelative;
			_startValue = _diffValue = 0;

#if UNITY_EDITOR
			if( _setter == null || _getter == null )
				Debug.LogError( "either the property (" + propertyName + ") setter or getter could not be found on the object " + target );
#endif
		}


		public void prepareForUse()
		{
			_startValue = _getter();

			if( _isRelative )
				_diffValue = _targetValue;
			else
				_diffValue = _targetValue - _startValue;
		}


		public void tick( float easedTime )
		{
			_setter( _startValue + _diffValue * easedTime );
		}

	}


	/// <summary>
	/// tweens any Vector2 property
	/// </summary>
	public struct Vector2TweenProperty : ITweenable
	{
		private Action<Vector2> _setter;
		private Func<Vector2> _getter;
		private bool _isRelative;
		private Vector2 _targetValue;
		private Vector2 _startValue;
		private Vector2 _diffValue;


		public Vector2TweenProperty( object target, string propertyName, Vector2 endValue, bool isRelative = false )
		{
			_setter = Utils.setterForProperty<Action<Vector2>>( target, propertyName );
			_getter = Utils.getterForProperty<Func<Vector2>>( target, propertyName );
			_targetValue = endValue;
			_isRelative = isRelative;
			_startValue = _diffValue = Vector2.zero;

#if UNITY_EDITOR
			if( _setter == null || _getter == null )
				Debug.LogError( "either the property (" + propertyName + ") setter or getter could not be found on the object " + target );
#endif
		}


		public void prepareForUse()
		{
			_startValue = _getter();

			if( _isRelative )
				_diffValue = _targetValue;
			else
				_diffValue = _targetValue - _startValue;
		}


		public void tick( float easedTime )
		{
			var vec = new Vector2
			(
				_startValue.x + _diffValue.x * easedTime,
            	_startValue.y + _diffValue.y * easedTime
			);
			_setter( vec );
		}

	}


	/// <summary>
	/// tweens any Vector3 property
	/// </summary>
	public struct Vector3TweenProperty : ITweenable
	{
		private Action<Vector3> _setter;
		private Func<Vector3> _getter;
		private bool _isRelative;
		private Vector3 _targetValue;
		private Vector3 _startValue;
		private Vector3 _diffValue;


		public Vector3TweenProperty( object target, string propertyName, Vector3 endValue, bool isRelative = false )
		{
			_setter = Utils.setterForProperty<Action<Vector3>>( target, propertyName );
			_getter = Utils.getterForProperty<Func<Vector3>>( target, propertyName );
			_targetValue = endValue;
			_isRelative = isRelative;
			_startValue = _diffValue = Vector3.zero;

#if UNITY_EDITOR
			if( _setter == null || _getter == null )
				Debug.LogError( "either the property (" + propertyName + ") setter or getter could not be found on the object " + target );
#endif
		}


		public void prepareForUse()
		{
			_startValue = _getter();

			if( _isRelative )
				_diffValue = _targetValue;
			else
				_diffValue = _targetValue - _startValue;
		}


		public void tick( float easedTime )
		{
			var vec = new Vector3
			(
				_startValue.x + _diffValue.x * easedTime,
            	_startValue.y + _diffValue.y * easedTime,
            	_startValue.z + _diffValue.z * easedTime
			);
			_setter( vec );
		}

	}


	/// <summary>
	/// tweens any Color property
	/// </summary>
	public struct ColorTweenProperty : ITweenable
	{
		private Action<Color> _setter;
		private Func<Color> _getter;
		private bool _isRelative;
		private Color _targetValue;
		private Color _startValue;
		private Color _diffValue;


		public ColorTweenProperty( object target, string propertyName, Color endValue, bool isRelative = false )
		{
			_setter = Utils.setterForProperty<Action<Color>>( target, propertyName );
			_getter = Utils.getterForProperty<Func<Color>>( target, propertyName );
			_targetValue = endValue;
			_isRelative = isRelative;
			_startValue = _diffValue = Color.white;

#if UNITY_EDITOR
			if( _setter == null || _getter == null )
				Debug.LogError( "either the property (" + propertyName + ") setter or getter could not be found on the object " + target );
#endif
		}


		public void prepareForUse()
		{
			_startValue = _getter();

			if( _isRelative )
				_diffValue = _targetValue;
			else
				_diffValue = _targetValue - _startValue;
		}


		public void tick( float easedTime )
		{
			var vec = new Color
			(
				_startValue.r + _diffValue.r * easedTime,
            	_startValue.g + _diffValue.g * easedTime,
            	_startValue.b + _diffValue.b * easedTime
			);
			_setter( vec );
		}

	}

}