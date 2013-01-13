GoKitLite
=========

GoKitLite is a *crazy* fast tweening library for Unity. It is optimized for mobile with near zero allocations at runtime. GoKitLite can tween position, localPosition, scale, rotation, localRotation and material color. It will tween "to" a value or "from" one to the current value. GoKitLite can also call your own custom Action so that you can tween anything that you want.

GoKitLite usage is dead simple. Below are some examples (note that you need to call GoKitLite.init() before any other methods are valid):

    // tween the position of an object to 10, 10, 10 over 1 second
    GoKitLite.instance.positionTo( someTransform, 1s, new Vector3( 10, 10, 10 ) );

    // tween the rotation of an object to 0, 90, 0 over 0.5 seconds with a custom ease type
    GoKitLite.instance.rotationTo( someTransform, 0.5f, new Vector3( 0, 90f, 0 ), 0, GoKitLiteEasing.Back.EaseOut );

    // tween the color of a material to red over 1 second with a 3 second delay before starting
    GoKitLite.instance.colorTo( someTransform, 1, Color.red, 3 );

Up above we mentioned that you can use a custom Action to handle a tween as well. Here is an example:

    // tween the position of an object to 10, 10, 10 over 1 second
    GoKitLite.instance.customAction( someTransform, 1s, ( trans, t ) => {
        // do something really cool here like tweening a string or changing multiple objects/properties at once
    });

GoKitLite also has a tween queue system to setup a series of tweens that will all run one after the other. Here is an example alternating positionTo and rotationTo tweens with a completion handler that will fire when the entire chain is complete:

    new GoKitLite.TweenQueue().add( () => { return GoKitLite.instance.positionTo( cube, 0.4f, new Vector3( -8, -3, 0 ) ); } )
    	.add( () => { return GoKitLite.instance.rotationTo( cube, 0.4f, new Vector3( 90f, 0, 0 ) ); } )
    	.add( () => { return GoKitLite.instance.positionTo( cube, 0.4f, new Vector3( 1, 2, -5 ) ); } )
    	.add( () => { return GoKitLite.instance.rotationTo( cube, 0.4f, new Vector3( 0, 90, 90 ) ); } )
    	.add( () => { return GoKitLite.instance.positionTo( cube, 0.4f, new Vector3( 0, 0, 0 ) ); } )
    	.add( () => { return GoKitLite.instance.rotationTo( cube, 0.4f, new Vector3( 360, 360, 0 ), 0, GoKitLiteEasing.Quadratic.EaseInOut, true ); } )
    	.setCompletionHandler( () => { Debug.Log( "Position and Rotation Queue Done" ); } )
    	.start();

Building on the tween queue system there is also a tween flow system. TweenFlows let you setup a timeline of tweens each with a specific start time. Unlike TweenQueues, tweens in a TweenFlow can be running simultaneously. Here is an example of a position tween that has 4 rotation tweens applied while it is still in transit to it's final position. Note that the start method must be called as a coroutine:

    var flow = new GoKitLite.TweenFlow().add( 0, () => { return GoKitLite.instance.positionTo( cube, 5f, new Vector3( 10, 10, 10 ) ); } )
    	.add( 1f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 90, 0, 0 ) ); } )
    	.add( 2f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 0, 90, 0 ) ); } )
    	.add( 3f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 0, 0, 90 ) ); } )
    	.add( 4f, () => { return GoKitLite.instance.rotationTo( cube, 0.5f, new Vector3( 180, 180, 180 ) ); } )
    	.setCompletionHandler( () => { Debug.Log( "All done with the position/rotation flow" ); } );
    StartCoroutine( flow.start() );


What about GoKit?
=========

GoKit has a slightly different focus than GoKitLite. It is highly customizeable and can tween anything at all. GoKit has all kinds of nifty features like chains, flows and full tween control in real time that arent ever going to be in GoKitLite. GoKitLite is made for folks who want a really easy API and just want to tween stuff now without much thought.
