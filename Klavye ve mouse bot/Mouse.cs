using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Timer = System.Timers.Timer;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MouseM
{
    public class MouseRecorder
    {
        public MouseRecorder(int recordInterval = 1, int playInterval = 1)
        {
            if (recordInterval <= 0)
                recordInterval = 1;

            if (playInterval <= 0)
                playInterval = 1;

            RecordingTimer.Interval = recordInterval;
            RecordingTimer.Elapsed += new System.Timers.ElapsedEventHandler(RecordingTimer_Elapsed);

            PlayTimer.Interval = playInterval;
            PlayTimer.Elapsed += new System.Timers.ElapsedEventHandler(PlayTimer_Elapsed);
        }

        List<MouseState> mouseStateList = new List<MouseState>();

        public int RecordInterval { get { return (int)RecordingTimer.Interval; } set { RecordingTimer.Interval = value; } }

        public int PlayInterval { get { return (int)PlayTimer.Interval; } set { PlayTimer.Interval = value; } }

        int counter = 0;
        bool IsPlaying = false;
        bool IsRecording = false;
        Timer RecordingTimer = new System.Timers.Timer();
        Timer PlayTimer = new System.Timers.Timer();

        #region PINVOKE

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const int MOUSEEVENTF_XDOWN = 0x0080;
        public const int MOUSEEVENTF_XUP = 0x0100;

        #endregion

        MouseState mouseState = new MouseState();

        public void StartRecording()
        {
            if (IsPlaying || IsRecording)
                return;

            IsRecording = true;

            mouseStateList.Clear();

            RecordingTimer.Start();
        }

        public void StopRecording()
        {
            if (IsPlaying)
                return;

            RecordingTimer.Stop();

            IsRecording = false;
        }

        //If play attempt is successfull it returns true, otherwise false.
        public bool Play()
        {
            if (IsPlaying || IsRecording)
                return false;

            IsPlaying = true;

            PlayTimer.Start();

            return true;
        }

        public void Stop()
        {
            IsPlaying = false;
            counter = mouseStateList.Count + 1;
        }

        private void RecordingTimer_Elapsed(object sender, EventArgs e)
        {
            mouseState = new MouseState
            {
                X = Cursor.Position.X,
                Y = Cursor.Position.Y
            };

            SaveStates();

            mouseStateList.Add(mouseState);
        }

        private void PlayTimer_Elapsed(object sender, EventArgs e)
        {
            if (IsPlaying == false)
            {
                PlayTimer.Stop();
                return;
            }

            if (mouseStateList.Count == 0 || counter < 0 || (counter >= mouseStateList.Count && counter != 0))
            {
                PlayTimer.Stop();

                IsPlaying = false;

                counter = 0;

                return;
            }

            var obj = mouseStateList[counter];

            if (obj == null)
                return;

            obj.Uygula();

            counter++;
        }

        private void SaveStates()
        {
            if (GetAsyncKeyState(Keys.LButton) < 0)
                mouseState.isLeftButtonDown = true;
            else
                mouseState.isLeftButtonDown = false;

            if (GetAsyncKeyState(Keys.RButton) < 0)
                mouseState.isRightButtonDown = true;
            else
                mouseState.isRightButtonDown = false;

            if (GetAsyncKeyState(Keys.MButton) < 0)
                mouseState.isMiddleButtonDown = true;
            else
                mouseState.isMiddleButtonDown = false;

            if (GetAsyncKeyState(Keys.XButton1) < 0)
                mouseState.isXButton1Down = true;
            else
                mouseState.isXButton1Down = false;

            if (GetAsyncKeyState(Keys.XButton2) < 0)
                mouseState.isXButton2Down = true;
            else
                mouseState.isXButton2Down = false;
        }

        private bool AreKeysDown(params Keys[] keys)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                if (!(GetAsyncKeyState(keys[i]) < 0))
                    return false;
            }
            return true;
        }

        private static bool IsKeyDown(Keys tus)
        {
            return GetAsyncKeyState(tus) < 0;
        }

        private static void Up(int MouseEventTF, int xpos, int ypos, int XButton = 0)
        {
            mouse_event(MouseEventTF, xpos, ypos, XButton, 0);
        }

        private static void Down(int MouseEventTF, int xpos, int ypos, int XButton = 0)
        {
            mouse_event(MouseEventTF, xpos, ypos, XButton, 0);
        }

        private static void CClick(int MouseEventTF, int xpos, int ypos, int XButton = 0)
        {
            Down(MouseEventTF, xpos, ypos, XButton);
            Up(MouseEventTF, xpos, ypos, XButton);
        }

        public void ReleaseAllKeys()
        {
            Up(MOUSEEVENTF_LEFTUP, int.MaxValue, int.MaxValue);
            Up(MOUSEEVENTF_RIGHTUP, int.MaxValue, int.MaxValue);
            Up(MOUSEEVENTF_MIDDLEUP, int.MaxValue, int.MaxValue);
            Up(MOUSEEVENTF_XUP, int.MaxValue, int.MaxValue, 1);
            Up(MOUSEEVENTF_XUP, int.MaxValue, int.MaxValue, 2);
        }

        public void ReleasePressedKeys()
        {
            if (IsKeyDown(Keys.LButton))
                Up(MOUSEEVENTF_LEFTUP, int.MaxValue, int.MaxValue);

            if (IsKeyDown(Keys.RButton))
                Up(MOUSEEVENTF_RIGHTUP, int.MaxValue, int.MaxValue);

            if (IsKeyDown(Keys.MButton))
                Up(MOUSEEVENTF_MIDDLEUP, int.MaxValue, int.MaxValue);

            if (IsKeyDown(Keys.XButton1))
                Up(MOUSEEVENTF_XUP, int.MaxValue, int.MaxValue, 1);

            if (IsKeyDown(Keys.XButton2))
                Up(MOUSEEVENTF_XUP, int.MaxValue, int.MaxValue, 2);
        }

        class MouseState
        {
            public bool isLeftButtonDown = false;
            public bool isRightButtonDown = false;
            public bool isMiddleButtonDown = false;
            public bool isXButton1Down = false;
            public bool isXButton2Down = false;

            public int X, Y;

            public MouseState()
            {

            }

            public MouseState(int x, int y)
            {
                X = x;
                Y = y;
            }

            public void Uygula()
            {
                SetCursorPos(X, Y);

                //Kayıt esnasında basılı ve şuan basılı değil ise bas.
                if (isLeftButtonDown && !IsKeyDown(Keys.LButton))
                    CClick(MOUSEEVENTF_LEFTDOWN, X, Y);
                //Kayıt esnasında basılı değil ve şuan basılı ise bırak.
                else if (!isLeftButtonDown && IsKeyDown(Keys.LButton))
                    Up(MOUSEEVENTF_LEFTUP, X, Y);

                //Kayıt esnasında basılı ve şuan basılı değil ise bas.
                if (isRightButtonDown && !IsKeyDown(Keys.RButton))
                    CClick(MOUSEEVENTF_RIGHTDOWN, X, Y);
                //Kayıt esnasında basılı değil ve şuan basılı ise bırak.
                else if (!isRightButtonDown && IsKeyDown(Keys.RButton))
                    Up(MOUSEEVENTF_RIGHTUP, X, Y);

                //Kayıt esnasında basılı ve şuan basılı değil ise bas.
                if (isMiddleButtonDown && !IsKeyDown(Keys.MButton))
                    CClick(MOUSEEVENTF_MIDDLEDOWN, X, Y);
                //Kayıt esnasında basılı değil ve şuan basılı ise bırak.
                else if (!isMiddleButtonDown && IsKeyDown(Keys.MButton))
                    Up(MOUSEEVENTF_MIDDLEUP, X, Y);

                //Kayıt esnasında basılı ve şuan basılı değil ise bas.
                if (isXButton1Down && !IsKeyDown(Keys.XButton1))
                    CClick(MOUSEEVENTF_XDOWN, X, Y, 1);
                //Kayıt esnasında basılı değil ve şuan basılı ise bırak.
                else if (!isXButton1Down && IsKeyDown(Keys.XButton1))
                    Up(MOUSEEVENTF_XUP, X, Y, 1);

                //Kayıt esnasında basılı ve şuan basılı değil ise bas.
                if (isXButton2Down && !IsKeyDown(Keys.XButton2))
                    CClick(MOUSEEVENTF_XDOWN, X, Y, 2);
                //Kayıt esnasında basılı değil ve şuan basılı ise bırak.
                else if (!isXButton2Down && IsKeyDown(Keys.XButton2))
                    Up(MOUSEEVENTF_XUP, X, Y, 2);
            }

            public MouseState Clone()
            {
                return new MouseState(X, Y);
            }
        }
    }
}