using Microsoft.Xna.Framework.Input;

namespace Chip8.Chip8
{
    public class Keyboard
    {
        /// <summary>
        /// Returns a byte corresponding to a
        /// key on the Chip-8 keyboard that's mapped to
        /// a given key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Byte representing to a key on the Chip-8 keyboard.</returns>
        private byte GetKey(Keys key)
        {
            byte keyCode = 0x0;

            switch (key)
            {
                case Keys.None:
                    break;
                case Keys.Back:
                    break;
                case Keys.Tab:
                    break;
                case Keys.Enter:
                    break;
                case Keys.CapsLock:
                    break;
                case Keys.Escape:
                    break;
                case Keys.Space:
                    break;
                case Keys.PageUp:
                    break;
                case Keys.PageDown:
                    break;
                case Keys.End:
                    break;
                case Keys.Home:
                    break;
                case Keys.Left:
                    break;
                case Keys.Up:
                    break;
                case Keys.Right:
                    break;
                case Keys.Down:
                    break;
                case Keys.Select:
                    break;
                case Keys.Print:
                    break;
                case Keys.Execute:
                    break;
                case Keys.PrintScreen:
                    break;
                case Keys.Insert:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Help:
                    break;
                case Keys.D0:
                    break;
                case Keys.D1:
                    break;
                case Keys.D2:
                    break;
                case Keys.D3:
                    break;
                case Keys.D4:
                    break;
                case Keys.D5:
                    break;
                case Keys.D6:
                    break;
                case Keys.D7:
                    break;
                case Keys.D8:
                    break;
                case Keys.D9:
                    break;
                case Keys.A:
                    keyCode = 0xA;
                    break;
                case Keys.B:
                    keyCode = 0xB;
                    break;
                case Keys.C:
                    keyCode = 0xC;
                    break;
                case Keys.D:
                    keyCode = 0xD;
                    break;
                case Keys.E:
                    keyCode = 0xE;
                    break;
                case Keys.F:
                    keyCode = 0xF;
                    break;
                case Keys.G:
                    break;
                case Keys.H:
                    break;
                case Keys.I:
                    break;
                case Keys.J:
                    break;
                case Keys.K:
                    break;
                case Keys.L:
                    break;
                case Keys.M:
                    break;
                case Keys.N:
                    break;
                case Keys.O:
                    break;
                case Keys.P:
                    break;
                case Keys.Q:
                    break;
                case Keys.R:
                    break;
                case Keys.S:
                    break;
                case Keys.T:
                    break;
                case Keys.U:
                    break;
                case Keys.V:
                    break;
                case Keys.W:
                    break;
                case Keys.X:
                    break;
                case Keys.Y:
                    break;
                case Keys.Z:
                    break;
                case Keys.LeftWindows:
                    break;
                case Keys.RightWindows:
                    break;
                case Keys.Apps:
                    break;
                case Keys.Sleep:
                    break;
                case Keys.NumPad0:
                    keyCode = 0x0;
                    break;
                case Keys.NumPad1:
                    keyCode = 0x1;
                    break;
                case Keys.NumPad2:
                    keyCode = 0x2;
                    break;
                case Keys.NumPad3:
                    keyCode = 0x3;
                    break;
                case Keys.NumPad4:
                    keyCode = 0x4;
                    break;
                case Keys.NumPad5:
                    keyCode = 0x5;
                    break;
                case Keys.NumPad6:
                    keyCode = 0x6;
                    break;
                case Keys.NumPad7:
                    keyCode = 0x7;
                    break;
                case Keys.NumPad8:
                    keyCode = 0x8;
                    break;
                case Keys.NumPad9:
                    keyCode = 0x9;
                    break;
                case Keys.Multiply:
                    break;
                case Keys.Add:
                    break;
                case Keys.Separator:
                    break;
                case Keys.Subtract:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Divide:
                    break;
                case Keys.F1:
                    break;
                case Keys.F2:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.Scroll:
                    break;
                case Keys.LeftShift:
                    break;
                case Keys.RightShift:
                    break;
                case Keys.LeftControl:
                    break;
                case Keys.RightControl:
                    break;
                case Keys.LeftAlt:
                    break;
                case Keys.RightAlt:
                    break;
                case Keys.BrowserBack:
                    break;
                case Keys.BrowserForward:
                    break;
                case Keys.BrowserRefresh:
                    break;
                case Keys.BrowserStop:
                    break;
                case Keys.BrowserSearch:
                    break;
                case Keys.BrowserFavorites:
                    break;
                case Keys.BrowserHome:
                    break;
                case Keys.VolumeMute:
                    break;
                case Keys.VolumeDown:
                    break;
                case Keys.VolumeUp:
                    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    break;
                case Keys.MediaPlayPause:
                    break;
                case Keys.LaunchMail:
                    break;
                case Keys.SelectMedia:
                    break;
                case Keys.LaunchApplication1:
                    break;
                case Keys.LaunchApplication2:
                    break;
                case Keys.OemSemicolon:
                    break;
                case Keys.OemPlus:
                    break;
                case Keys.OemComma:
                    break;
                case Keys.OemMinus:
                    break;
                case Keys.OemPeriod:
                    break;
                case Keys.OemQuestion:
                    break;
                case Keys.OemTilde:
                    break;
                case Keys.OemOpenBrackets:
                    break;
                case Keys.OemPipe:
                    break;
                case Keys.OemCloseBrackets:
                    break;
                case Keys.OemQuotes:
                    break;
                case Keys.Oem8:
                    break;
                case Keys.OemBackslash:
                    break;
                case Keys.ProcessKey:
                    break;
                case Keys.Attn:
                    break;
                case Keys.Crsel:
                    break;
                case Keys.Exsel:
                    break;
                case Keys.EraseEof:
                    break;
                case Keys.Play:
                    break;
                case Keys.Zoom:
                    break;
                case Keys.Pa1:
                    break;
                case Keys.OemClear:
                    break;
                case Keys.ChatPadGreen:
                    break;
                case Keys.ChatPadOrange:
                    break;
                case Keys.Pause:
                    break;
                case Keys.ImeConvert:
                    break;
                case Keys.ImeNoConvert:
                    break;
                case Keys.Kana:
                    break;
                case Keys.Kanji:
                    break;
                case Keys.OemAuto:
                    break;
                case Keys.OemCopy:
                    break;
                case Keys.OemEnlW:
                    break;
                default:
                    break;
            }

            return keyCode;
        }

        /// <summary>
        /// Returns the key that's mapped to one of the
        /// keys on the Chip-8 keyboard.
        /// </summary>
        /// <param name="key">Byte representing a key on the Chip-8 keyboard.</param>
        /// <returns>Key that's mapped to the key on the Chip-8 keyboard.</returns>
        private Keys GetKey(byte key)
        {
            Keys keyCode = 0x0;

            switch (key)
            {
                case 0x0:
                    keyCode = Keys.NumPad0;
                    break;
                case 0x1:
                    keyCode = Keys.NumPad1;
                    break;
                case 0x2:
                    keyCode = Keys.NumPad2;
                    break;
                case 0x3:
                    keyCode = Keys.NumPad3;
                    break;
                case 0x4:
                    keyCode = Keys.NumPad4;
                    break;
                case 0x5:
                    keyCode = Keys.NumPad5;
                    break;
                case 0x6:
                    keyCode = Keys.NumPad6;
                    break;
                case 0x7:
                    keyCode = Keys.NumPad7;
                    break;
                case 0x8:
                    keyCode = Keys.NumPad8;
                    break;
                case 0x9:
                    keyCode = Keys.NumPad9;
                    break;
                case 0xA:
                    keyCode = Keys.A;
                    break;
                case 0xB:
                    keyCode = Keys.B;
                    break;
                case 0xC:
                    keyCode = Keys.C;
                    break;
                case 0xD:
                    keyCode = Keys.D;
                    break;
                case 0xE:
                    keyCode = Keys.E;
                    break;
                case 0xF:
                    keyCode = Keys.F;
                    break;
            }

            return keyCode;
        }

        /// <summary>
        /// Returns wether or not a key from the Chip-8 keyboard is pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyPressed(byte key)
        {
            return Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(GetKey(key));
        }

        /// <summary>
        /// Returns one of the pressed keys from the Chip-8 keyboard.
        /// </summary>
        /// <returns></returns>
        public byte? GetPressedKey()
        {
            Keys[]  keysPressed = Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys();
            if (keysPressed.Length == 0)
                return null;

            return GetKey(keysPressed[0]);
        }
    }
}
