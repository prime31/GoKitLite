using UnityEngine;
using System.Collections;


public static class GoKitLiteEasing
{
	public static class Linear
	{
		public static float EaseNone( float t, float b, float c, float d )
		{
			return c * t / d + b;
		}


	    public static float Punch( float t, float b, float c, float d )
	    {
	        if( t == 0 )
	            return 0;
	
	        if( ( t /= d ) == 1 )
	            return 0;
	
	        const float p = 0.3f;
	        return ( c * Mathf.Pow( 2, -10 * t ) * Mathf.Sin( t * ( 2 * Mathf.PI ) / p ) );
	    }
	}
	
	
	public static class Quadratic
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return -c * ( t /= d ) * ( t - 2 ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( ( t /= d / 2 ) < 1 )
			{
				return c / 2 * t * t + b;
			}
			return -c / 2 * ( ( --t ) * ( t - 2 ) - 1 ) + b;
		}
	}
	
	
	public static class Back
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * ( ( 1.70158f + 1 ) * t - 1.70158f ) + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return c * ( ( t = t / d - 1 ) * t * ( ( 1.70158f + 1 ) * t + 1.70158f ) + 1 ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			float s = 1.70158f;
			if( ( t /= d / 2 ) < 1 )
			{
				return c / 2 * ( t * t * ( ( ( s *= ( 1.525f ) ) + 1 ) * t - s ) ) + b;
			}
			return c / 2 * ( ( t -= 2 ) * t * ( ( ( s *= ( 1.525f ) ) + 1 ) * t + s ) + 2 ) + b;
		}
	}
	
	
	public static class Bounce
	{
		public static float EaseOut( float t, float b, float c, float d )
		{
			if( ( t /= d ) < ( 1 / 2.75 ) )
			{
				return c * ( 7.5625f * t * t ) + b;
			}
			else if( t < ( 2 / 2.75 ) )
			{
				return c * ( 7.5625f * ( t -= ( 1.5f / 2.75f ) ) * t + .75f ) + b;
			}
			else if( t < ( 2.5 / 2.75 ) )
			{
				return c * ( 7.5625f * ( t -= ( 2.25f / 2.75f ) ) * t + .9375f ) + b;
			}
			else
			{
				return c * ( 7.5625f * ( t -= ( 2.625f / 2.75f ) ) * t + .984375f ) + b;
			}
		}


		public static float EaseIn( float t, float b, float c, float d )
		{
			return c - EaseOut( d - t, 0, c, d ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( t < d / 2 )
				return EaseIn( t * 2, 0, c, d ) * 0.5f + b;
			else
				return EaseOut( t * 2 - d, 0, c, d ) * .5f + c * 0.5f + b;
		}
	}


	public static class Circular
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return -c * ( Mathf.Sqrt( 1 - ( t /= d ) * t ) - 1 ) + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return c * Mathf.Sqrt( 1 - ( t = t / d - 1 ) * t ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( ( t /= d / 2 ) < 1 )
			{
				return -c / 2 * ( Mathf.Sqrt( 1 - t * t ) - 1 ) + b;
			}
			return c / 2 * ( Mathf.Sqrt( 1 - ( t -= 2 ) * t ) + 1 ) + b;
		}
	}


	public static class Cubic
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * t + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return c * ( ( t = t / d - 1 ) * t * t + 1 ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( ( t /= d / 2 ) < 1 )
			{
				return c / 2 * t * t * t + b;
			}
			return c / 2 * ( ( t -= 2 ) * t * t + 2 ) + b;
		}
	}


	public class Elastic
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			if( t == 0 )
			{
				return b;
			}
			if( ( t /= d ) == 1 )
			{
				return b + c;
			}
			float p = d * .3f;
			float s = p / 4;
			return -(float)( c * Mathf.Pow( 2, 10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) ) + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			if( t == 0 )
			{
				return b;
			}
			if( ( t /= d ) == 1 )
			{
				return b + c;
			}
			float p = d * .3f;
			float s = p / 4;
			return (float)( c * Mathf.Pow( 2, -10 * t ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) + c + b );
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( t == 0 )
			{
				return b;
			}
			if( ( t /= d / 2 ) == 2 )
			{
				return b + c;
			}
			float p = d * ( .3f * 1.5f );
			float a = c;
			float s = p / 4;
			if( t < 1 )
			{
				return -.5f * (float)( a * Mathf.Pow( 2, 10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) ) + b;
			}
			return (float)( a * Mathf.Pow( 2, -10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) * .5 + c + b );
		}
	}


	public static class Exponential
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return ( t == 0 ) ? b : c * (float)Mathf.Pow( 2, 10 * ( t / d - 1 ) ) + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return ( t == d ) ? b + c : c * (float)( -Mathf.Pow( 2, -10 * t / d ) + 1 ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( t == 0 )
			{
				return b;
			}
			if( t == d )
			{
				return b + c;
			}
			if( ( t /= d / 2 ) < 1 )
			{ 
				return c / 2 * (float)Mathf.Pow( 2, 10 * ( t - 1 ) ) + b; 
			}
			return c / 2 * (float)( -Mathf.Pow( 2, -10 * --t ) + 2 ) + b;
		}
	}


	public static class Quartic
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * t * t + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return -c * ( ( t = t / d - 1 ) * t * t * t - 1 ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( ( t /= d / 2 ) < 1 )
			{
				return c / 2 * t * t * t * t + b;
			}
			return -c / 2 * ( ( t -= 2 ) * t * t * t - 2 ) + b;
		}
	}


	public static class Quintic
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * t * t * t + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return c * ( ( t = t / d - 1 ) * t * t * t * t + 1 ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			if( ( t /= d / 2 ) < 1 )
			{
				return c / 2 * t * t * t * t * t + b;
			}
			return c / 2 * ( ( t -= 2 ) * t * t * t * t + 2 ) + b;
		}
	}


	public static class Sinusoidal
	{
		public static float EaseIn( float t, float b, float c, float d )
		{
			return -c * (float)Mathf.Cos( t / d * ( Mathf.PI / 2 ) ) + c + b;
		}


		public static float EaseOut( float t, float b, float c, float d )
		{
			return c * (float)Mathf.Sin( t / d * ( Mathf.PI / 2 ) ) + b;
		}


		public static float EaseInOut( float t, float b, float c, float d )
		{
			return -c / 2 * ( (float)Mathf.Cos( Mathf.PI * t / d ) - 1 ) + b;
		}
	}

}
