Current version:
3.3

Instructions:

Add the Rect Transform Extended component to any UI GameObject to practice and understand what this tool can do, but 
don't use it from code and don't use it in production, it's slow.

Instead use the following methods that are now added to the RectTransform:

yourRectTransform.GetPosition()
yourRectTransform.SetPosition()

yourRectTransform.GetAnchorsPosition()
yourRectTransform.SetAnchorsPosition()

yourRectTransform.SetPositionX()
yourRectTransform.SetPositionY()

yourRectTransform.SetWidth()
yourRectTransform.SetHeight()

yourRectTransform.SetAnchorsPositionX()
yourRectTransform.SetAnchorsPositionY()

yourRectTransform.SetAnchorsWidth()
yourRectTransform.SetAnchorsHeight()

yourRectTransform.GetPivotPosition()
yourRectTransform.SetPivotPosition()

Note:

Seriously, don't get a reference to the Rect Transform Extended component that is slow, difficult and leads to problems because 
the Rect Transform Extended component was not made to be used from code.

Also 4 options are added to the Tools menu:

	- UI Anchors To Corners
	- UI Corners To Anchors
	- UI Center Rect
	- UI Center Anchors

Usange example:
	
	RectTransfrom myRectTransform = transform as RectTransform;
	Vector2 size = myRectTransform.GetSize(CoordinateSystem.IgnoreAnchorsAndPivot);

If you need a more advanced usage converting coordinates instead of setting and getting, use the methods of the static classes: RteRectTools and RteAnchorTools
Also these static classes contains some extra coordinate conversion methods.

For questions write on this forum thread:
https://community.unity.com/t5/Asset-Store/RELEASED-EASIER-UI/td-p/2577628#post-2630254

For any other kind of feedback:
fermmm@gmail.com