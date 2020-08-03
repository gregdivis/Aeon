using System;

namespace Aeon.Emulator.Mouse
{
    /// <summary>
    /// Assists the mouse handler in tracking mouse click events.
    /// </summary>
    internal sealed class ButtonPressTracker
    {
        private ButtonInfo leftPress;
        private ButtonInfo rightPress;
        private ButtonInfo middlePress;
        private ButtonInfo leftRelease;
        private ButtonInfo rightRelease;
        private ButtonInfo middleRelease;

        /// <summary>
        /// Notifies the tracker of a button press.
        /// </summary>
        /// <param name="button">Pressed button.</param>
        /// <param name="x">X-coordinate of the cursor.</param>
        /// <param name="y">Y-courdinate of the cursor.</param>
        public void ButtonPress(MouseButtons button, int x, int y)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    UpdateButtonInfo(ref leftPress, x, y);
                    break;

                case MouseButtons.Right:
                    UpdateButtonInfo(ref rightPress, x, y);
                    break;

                case MouseButtons.Middle:
                    UpdateButtonInfo(ref middlePress, x, y);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }
        }
        /// <summary>
        /// Notifies the tracker of a button release.
        /// </summary>
        /// <param name="button">Released button.</param>
        /// <param name="x">X-coordinate of the cursor.</param>
        /// <param name="y">Y-courdinate of the cursor.</param>
        public void ButtonRelease(MouseButtons button, int x, int y)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    UpdateButtonInfo(ref leftRelease, x, y);
                    break;

                case MouseButtons.Right:
                    UpdateButtonInfo(ref rightRelease, x, y);
                    break;

                case MouseButtons.Middle:
                    UpdateButtonInfo(ref middleRelease, x, y);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }
        }
        /// <summary>
        /// Returns information about a pressed button.
        /// </summary>
        /// <param name="button">Index of button to get information for.</param>
        /// <returns>Information about the button press.</returns>
        public ButtonInfo GetButtonPressInfo(int button)
        {
            ButtonInfo result;

            switch (button)
            {
                case 0:
                    result = leftPress;
                    leftPress.Count = 0;
                    break;

                case 1:
                    result = rightPress;
                    rightPress.Count = 0;
                    break;

                case 2:
                    result = middlePress;
                    middlePress.Count = 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }

            return result;
        }
        /// <summary>
        /// Returns information about a released button.
        /// </summary>
        /// <param name="button">Index of button to get information for.</param>
        /// <returns>Information about the button release.</returns>
        public ButtonInfo GetButtonReleaseInfo(int button)
        {
            ButtonInfo result;

            switch (button)
            {
                case 0:
                    result = leftRelease;
                    leftRelease.Count = 0;
                    break;

                case 1:
                    result = rightRelease;
                    rightRelease.Count = 0;
                    break;

                case 2:
                    result = middleRelease;
                    middleRelease.Count = 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(button));
            }

            return result;
        }

        private static void UpdateButtonInfo(ref ButtonInfo info, int x, int y)
        {
            info.Count++;
            info.X = x;
            info.Y = y;
        }
    }

    /// <summary>
    /// Stores information about a button press or release.
    /// </summary>
    internal struct ButtonInfo
    {
        /// <summary>
        /// Number of times the button was pressed or released.
        /// </summary>
        public uint Count;
        /// <summary>
        /// X-coordinate of the cursor at the most recent event.
        /// </summary>
        public int X;
        /// <summary>
        /// Y-coordinate of the cursor at the most recent event.
        /// </summary>
        public int Y;
    }
}
