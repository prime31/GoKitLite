using UnityEngine;
using System.Collections;
using System;


namespace Prime31.GoKitLite
{
	public static class GoKitLiteEasing
	{
		public static class Custom
		{
			/// <summary>
			/// uses an AnimationCurve for easing. the curve should have a start time of 0. The end time can be anything since
			/// it will be scaled but it is usually easiest to just use the 0 - 1 time range.
			/// </summary>
			/// <returns>The curve ease.</returns>
			/// <param name="curve">Curve.</param>
			public static Func<float,float,float> AnimationCurveEase( AnimationCurve curve )
			{
				// we need the curve's total duration so we can scale it to the actual tween's duration
				var curveDuration = curve.keys[curve.length - 1].time;

				Func<float,float,float> func = ( t, d ) =>
				{
					var timeScaler = curveDuration / d;
					return curve.Evaluate( timeScaler * t );
				};

				return func;
			}
		}


		public static class Linear
		{
			public static float EaseNone( float t, float d )
			{
				return t / d;
			}
		}


		public static class Quadratic
		{
			public static float EaseIn( float t, float d )
			{
				return ( t /= d ) * t;
			}


			public static float EaseOut( float t, float d )
			{
				return -1 * ( t /= d ) * ( t - 2 );
			}


			public static float EaseInOut( float t, float d )
			{
				if( ( t /= d / 2 ) < 1 )
					return 0.5f * t * t;

				return -0.5f * ( ( --t ) * ( t - 2 ) - 1 );
			}
		}


		public static class Back
		{
			public static float EaseIn( float t, float d )
			{
				return ( t /= d ) * t * ( ( 1.70158f + 1 ) * t - 1.70158f );
			}


			public static float EaseOut( float t, float d )
			{
				return ( ( t = t / d - 1 ) * t * ( ( 1.70158f + 1 ) * t + 1.70158f ) + 1 );
			}


			public static float EaseInOut( float t, float d )
			{
				float s = 1.70158f;
				if( ( t /= d / 2 ) < 1 )
				{
					return 0.5f * ( t * t * ( ( ( s *= ( 1.525f ) ) + 1 ) * t - s ) );
				}
				return 0.5f * ( ( t -= 2 ) * t * ( ( ( s *= ( 1.525f ) ) + 1 ) * t + s ) + 2 );
			}
		}


		public static class Bounce
		{
			public static float EaseOut( float t, float d )
			{
				if( ( t /= d ) < ( 1 / 2.75 ) )
				{
					return ( 7.5625f * t * t );
				}
				else if( t < ( 2 / 2.75 ) )
				{
					return ( 7.5625f * ( t -= ( 1.5f / 2.75f ) ) * t + .75f );
				}
				else if( t < ( 2.5 / 2.75 ) )
				{
					return ( 7.5625f * ( t -= ( 2.25f / 2.75f ) ) * t + .9375f );
				}
				else
				{
					return ( 7.5625f * ( t -= ( 2.625f / 2.75f ) ) * t + .984375f );
				}
			}


			public static float EaseIn( float t, float d )
			{
				return 1 - EaseOut( d - t, d );
			}


			public static float EaseInOut( float t, float d )
			{
				if( t < d / 2 )
					return EaseIn( t * 2, d ) * 0.5f;
				else
					return EaseOut( t * 2 - d, d ) * .5f + 1 * 0.5f;
			}
		}


		public static class Circular
		{
			public static float EaseIn( float t, float d )
			{
				return -( Mathf.Sqrt( 1 - ( t /= d ) * t ) - 1 );
			}


			public static float EaseOut( float t, float d )
			{
				return Mathf.Sqrt( 1 - ( t = t / d - 1 ) * t );
			}


			public static float EaseInOut( float t, float d )
			{
				if( ( t /= d / 2 ) < 1 )
					return -0.5f * ( Mathf.Sqrt( 1 - t * t ) - 1 );

				return 0.5f * ( Mathf.Sqrt( 1 - ( t -= 2 ) * t ) + 1 );
			}
		}


		public static class Cubic
		{
			public static float EaseIn( float t, float d )
			{
				return ( t /= d ) * t * t;
			}


			public static float EaseOut( float t, float d )
			{
				return ( ( t = t / d - 1 ) * t * t + 1 );
			}


			public static float EaseInOut( float t, float d )
			{
				if( ( t /= d / 2 ) < 1 )
					return 0.5f * t * t * t;

				return 0.5f * ( ( t -= 2 ) * t * t + 2 );
			}
		}


		public class Elastic
		{
			public static float EaseIn( float t, float d )
			{
				if( t == 0 )
					return 0;

				if( ( t /= d ) == 1 )
					return 1;

				float p = d * .3f;
				float s = p / 4;
				return -( 1 * Mathf.Pow( 2, 10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) );
			}


			public static float EaseOut( float t, float d )
			{
				if( t == 0 )
					return 0;

				if( ( t /= d ) == 1 )
					return 1;

				float p = d * .3f;
				float s = p / 4;
				return ( 1 * Mathf.Pow( 2, -10 * t ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) + 1 );
			}


			public static float EaseInOut( float t, float d )
			{
				if( t == 0 )
					return 0;

				if( ( t /= d / 2 ) == 2 )
					return 1;

				float p = d * ( .3f * 1.5f );
				float s = p / 4;

				if( t < 1 )
					return -.5f * ( Mathf.Pow( 2, 10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) );

				return ( Mathf.Pow( 2f, -10f * ( t -= 1f ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + 1f );
			}


		    public static float Punch( float t, float d )
		    {
		        if( t == 0 )
		            return 0;

		        if( ( t /= d ) == 1 )
		            return 0;

		        const float p = 0.3f;
		        return ( Mathf.Pow( 2, -10 * t ) * Mathf.Sin( t * ( 2 * Mathf.PI ) / p ) );
		    }
		}


		public static class Exponential
		{
			public static float EaseIn( float t, float d )
			{
				return ( t == 0 ) ? 0 : Mathf.Pow( 2, 10 * ( t / d - 1 ) );
			}


			public static float EaseOut( float t, float d )
			{
				return t == d ? 1 : ( -Mathf.Pow( 2, -10 * t / d ) + 1 );
			}


			public static float EaseInOut( float t, float d )
			{
				if( t == 0 )
					return 0;

				if( t == d )
					return 1;

				if( ( t /= d / 2 ) < 1 )
				{
					return 0.5f * Mathf.Pow( 2, 10 * ( t - 1 ) );
				}
				return 0.5f * ( -Mathf.Pow( 2, -10 * --t ) + 2 );
			}
		}


		public static class Quartic
		{
			public static float EaseIn( float t, float d )
			{
				return ( t /= d ) * t * t * t;
			}


			public static float EaseOut( float t, float d )
			{
				return -1 * ( ( t = t / d - 1 ) * t * t * t - 1 );
			}


			public static float EaseInOut( float t, float d )
			{
				t /= d / 2;
				if( t < 1 )
					return 0.5f * t * t * t * t;

				t -= 2;
				return -0.5f * ( t * t * t * t - 2 );
			}
		}


		public static class Quintic
		{
			public static float EaseIn( float t, float d )
			{
				return ( t /= d ) * t * t * t * t;
			}


			public static float EaseOut( float t, float d )
			{
				return ( ( t = t / d - 1 ) * t * t * t * t + 1 );
			}


			public static float EaseInOut( float t, float d )
			{
				if( ( t /= d / 2 ) < 1 )
					return 0.5f * t * t * t * t * t;

				return 0.5f * ( ( t -= 2 ) * t * t * t * t + 2 );
			}
		}


		public static class Sinusoidal
		{
			public static float EaseIn( float t, float d )
			{
				return -1 * Mathf.Cos( t / d * ( Mathf.PI / 2 ) ) + 1f;
			}


			public static float EaseOut( float t, float d )
			{
				return Mathf.Sin( t / d * ( Mathf.PI / 2 ) );
			}


			public static float EaseInOut( float t, float d )
			{
				return -0.5f * ( Mathf.Cos( Mathf.PI * t / d ) - 1 );
			}
		}
	}
}
