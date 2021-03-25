GUICreator is an API Plugin to simplify generating/handling Rust GUI.
At its core, it tracks all GUI containers sent to players, destroys GUI in the right order and allows developers to use callbacks with buttons/inputs.
There are also several handy functions in the GuiContainer class to keep the lines of code required for a complex UI to a minimum.

Classes:

	Rectangle, WIP


	GuiColor
	Just a wrapper for the color class in UnityEngine, just providing more flexibility for defining Colors
	Some CuiComponents require a color string of the format "red green blue alpha"
	Relevant properties:
		float color.r - red value 0 to 1
		float color.g - green value 0 to 1
		float color.b - blue value 0 to 1
		float color.a - opacity (transparent) 0 to 1 (opaque)
	Constructors:
		The default Constructor sets color to opaque white

		GuiColor(float R, float G, float B, float alpha)
		- should be intuitive. if rgb values are greater than one they are assumed to be a value between 0 and 255

		GuiColor(string hex) - interprets a hex Color Value of format "#RRGGBB" or "#RRGGBBAA". Can also interpret some color names like "black", "white", "red"...
	Methods:
		setAlpha(float alpha) - sets alpha value of an instance
		static GuiColor setAlpha(GuiColor color, float alpha) - static version of above method
		getColorString() - returns the formatted string required for some CuiComponents


	GuiText, derived from CuiTextComponent
	Again, more flexibility and intuitive defaults for defining CuiTextComponents
	Relevant properties:
		string Text
		int FontSize
		TextAnchor Align - TextAnchor is an enum of common text alignments within their containers. (e.g MiddleCenter, UpperLeft, LowerRight)
		string Color - string of format "R G B A", as mentioned in GuiColor
		float Fadein -  seconds it takes for the element to fade to its max opacity on creation
	Constructors
		GuiText(string text, int fontSize = 14, GuiColor color = null, TextAnchor align = TextAnchor.MiddleCenter, float FadeIn = 0)

	
	CuiInputField
	Wrapper for CuiInputfieldComponent, RectTransformComponent because it's not included in Umod's CuiHelper class

	
	GuiContainer, WIP


	GuiTracker, derived from MonoBehaviour
	Every player is given a single GuiTracker when they have active GUI elements (sent by this plugin). 
	Relevant Properties:
		BasePlayer player
        List<GuiContainer> activeGuiContainers
	Constructor:
		There is no constructor. This is a (kind of) Singleton. You can access a players GuiTracker by using static method "GuiTracker.getGuiTracker(BasePlayer player)"
	Relevant Methods:
		GuiTracker getGuiTracker(BasePlayer player) - see Constructor above
		GuiContainer getContainer(Plugin plugin, string name) - useful for checking whether a player has a particular container
		destroyGui(Plugin plugin, string containerName, string name = null) - destroys either an entire container and its children or a specific Element
		destroyAllGui(Plugin plugin) - destroys all GUI sent by the specified plugin.

API Methods:

	WIP

Commands:

	Console:
		gui.close - closes all gui sent by this plugin on a client.
		gui.input - used to call button/input callbacks. can be used manually for debugging
		gui.list - lists all active UI elements for debugging
	Chat:
	-these require the player to have the oxide permission "gui.demo"
		/guidemo - demonstrates all elements
		/img [url] - displays an image after it's been downloaded
