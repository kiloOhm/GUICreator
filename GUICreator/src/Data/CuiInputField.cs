namespace Oxide.Plugins
{
    using Oxide.Game.Rust.Cui;

    public partial class GUICreator
    {
        public class CuiInputField
        {
            //public CuiImageComponent Image { get; set; } = new CuiImageComponent();
            public CuiInputFieldComponent InputField { get; set; } = new CuiInputFieldComponent();

            public CuiRectTransformComponent RectTransform { get; set; } = new CuiRectTransformComponent();
            public bool CursorEnabled { get; set; }
            public float FadeOut { get; set; }
        }
    }
}