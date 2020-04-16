The GUICreator is primarily an API Plugin to simplify generating/handling Rust GUI.
It also offers several premade GUI elements which can be used with a single function call.

Classes:

	Rectangle, derived from CuiRectTransformComponent
	Not reinventing the wheel here, just providing more flexibility for defining CuiRectTransformComponents.
	coordinates are fractions of the total resolution of each client screen. (e.g. "0.5 0.5" is the middle of the screen )
	Relevant properties:
		float anchorMinX - X value of the bottom left corner
        float anchorMinY - Y value of the bottom left corner
        float anchorMaxX - X value of the top right corner
        float anchorMaxY - Y value of the top right corner
	Constructors:
		The default constructor sets anchorMinX and anchorMinY to 0, anchorMaxX and anchorMaxY to 1. The rectangle spans the entire screen/parent
		
		Rectangle(float X, float Y, float W, float H, int resX = 1, int resY = 1, bool topLeftOrigin = false)
		- x and y are the coordinate of the bottom left (or top left if topLeftOrigin is true) corner. W is the width, H is H is the height of the rectangle.
		  resX is the horizontal resolution and resY the vertical resolution. (x = 10, y = 10, resX = 20, resY = 20) is the middle of the screen.
		  This was intended for use with figma (or similar UI Desing tools). For use with figma, set topLeftOrigin to true and set resX, resY to the dimensions of your frame.


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

	
	GuiContainer, derived from CuiElementContainer, derived from List<CuiElement>
	It's intended that all GUI Elements are bundled in GuiContainers.
	Relevant properties:
		Plugin plugin - reference to the Plugin that created this container
        string name - individual name (when trying to display a container with the name of an already existing container, the already existing container will be destroyed)
        string parent - parent container (when destroying a container, all children containers are also destroyed, container parenting has no impact on positioning though)
		List<Timer> timers - Collection for timers tied to this container. all timers are destroyed upon destruction of container
		enum Layer { overall, overlay, menu, hud, under } - layers on which GUI can be displayed. The Rust Hud and UI is displayed between hud and menu.
	Constructors:
		There is no default constructor
		GuiContainer(Plugin plugin, string name, string parent = null)
	Relevant Methods:
		display(BasePlayer player)
		destroy(BasePlayer player)
		addPanel(string name, CuiRectTransformComponent rectangle, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null, bool blur = false)
		addPlainPanel(string name, CuiRectTransformComponent rectangle, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, bool blur = false)
		addImage(string name, CuiRectTransformComponent rectangle, string imgName, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
		- displays image, that has previously been registered by its plugin

		addRawImage(string name, CuiRectTransformComponent rectangle, string imgData, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
		- displays image from raw image data. to be used with an API call to ImageLibrary or with the API Method "string getItemIcon(string shortname)"

		addText(string name, CuiRectTransformComponent rectangle, GuiText text = null, float FadeIn = 0, float FadeOut = 0, string parent = "Hud")
		addButton(string name, CuiRectTransformComponent rectangle, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, string imgName = null, string parent = "Hud")
		addPlainButton(string name, CuiRectTransformComponent rectangle, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, string parent = "Hud")
		addInput(string name, CuiRectTransformComponent rectangle, Action<BasePlayer, string[]> callback, string parent = "Hud", string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null)

	If you specify names of other elements within the container in the "close" strings of Buttons and Inputs, said elements and their children will be destroyed on click/enter
	I recommend using the least complex method (plainPanel, plainButton, text, image) for your task to reduce CuiElements that are sent to clients.
	Singular GUI Elements can only be parented to other Elements within their container. Children Elements are destroyed upon destruction of their parents and have their frame overridden to be the RectTransform of their parent.
	Instead of parents, all methods take a layer also. 


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
	
	customGameTip(BasePlayer player, string text, float duration = 0, gametipType type = gametipType.gametip)
	- displays a gametip-like popup above the players inventory. there are three types: gametip(default), warning, error
	  calling gametips destroys any gametip which was previously there. leaving duration on 0 is not recommended!

	registerImage(Plugin plugin, string name, string url)
	- registers an Image for later use. Image storage is handled by ImageLibrary.
	  the name can be referenced via the addImage/addPanel/addButton methods of GuiContainer.

	string getItemIcon(string shortname) - returns raw image data of an item icon for use with addRawImage

Commands:

	Console:
		gui.close - closes all gui sent by this plugin on a client.
		gui.input - used to call button/input callbacks. can be used manually for debugging
	Chat:
	-these require the player to have the oxide permission "gui.demo"
		/guidemo - demonstrates all elements
		/img [name] - displays an image from the ImageLibrary
		/registerimg [name] [url] - registers an image to the ImageLibrary
