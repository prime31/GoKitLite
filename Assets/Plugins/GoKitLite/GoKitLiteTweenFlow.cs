using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31.GoKitLite;


/// <summary>
/// totally optional classes for making tween flows
/// </summary>
namespace Prime31.GoKitLite
{
	public class TweenFlow
	{
		/// <summary>
		/// used internally as a wrapper to handle TweenFlows
		/// </summary>
		internal struct TweenFlowItem : IComparable
		{
			internal float startTime;
			internal System.Func<GoKitLite.Tween> actionTween;

	
			public TweenFlowItem( float startTime, System.Func<GoKitLite.Tween> actionTween )
			{
				this.actionTween = actionTween;
				this.startTime = startTime;
			}


			public int CompareTo( object obj )
			{
				return ((TweenFlowItem)obj).startTime.CompareTo( startTime );
			}

		}


		private List<TweenFlowItem> _tweenFlows = new List<TweenFlowItem>();
		private System.Action _onComplete;
		private int _currentlyRunningTweenId;
		private int _completionHandlersWaitingToFire = 0;

	
		private void onTweenComplete( Transform trans )
		{
			_completionHandlersWaitingToFire--;

			if( _completionHandlersWaitingToFire == 0 && _tweenFlows.Count == 0 && _onComplete != null )
				_onComplete();
		}
	

		public TweenFlow add( float startTime, System.Func<GoKitLite.Tween> actionTween )
		{
			_tweenFlows.Add( new TweenFlowItem( startTime, actionTween ) );
			return this;
		}
	

		public TweenFlow setCompletionHandler( System.Action onComplete )
		{
			_onComplete = onComplete;
			return this;
		}
	
	
		public IEnumerator start()
		{
			// sort our list so we can iterate backwards and remove items as necessary
			_tweenFlows.Sort();

			// state for the flow
			var elapsedTime = 0f;
			var running = true;

			while( running )
			{
				elapsedTime += Time.deltaTime;

				// loop backwards so we can remove items as we run them and break the loop when we get past any flow items set to run
				for( var i = _tweenFlows.Count - 1; i >= 0; i-- )
				{
					if( elapsedTime >= _tweenFlows[i].startTime )
					{
						var flowItem = _tweenFlows[i];
						_tweenFlows.RemoveAt( i );
						_currentlyRunningTweenId = flowItem.actionTween().setCompletionHandler( onTweenComplete ).getId();
						_completionHandlersWaitingToFire++;
					}
					else
					{
						break;
					}
				}

				yield return null;
			}
		}
	
	
		public void stop( bool bringCurrentlyRunningTweenToCompletion )
		{
			_tweenFlows.Clear();
			GoKitLite.instance.stopTween( _currentlyRunningTweenId, bringCurrentlyRunningTweenToCompletion );
		}
	
	}

}
