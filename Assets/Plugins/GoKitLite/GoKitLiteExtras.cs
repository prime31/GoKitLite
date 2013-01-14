using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// totally optional classes for making queues of tweens and tween flows
/// </summary>
public partial class GoKitLite : MonoBehaviour
{
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
						//
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
