using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using VirtualKeys;

namespace KeyboardM
{
    public class KeyboardRecorder
    {
        public KeyboardRecorder(int recordInterval = 1, int playInterval = 1)
        {
            if (recordInterval <= 0)
                recordInterval = 1;

            if (playInterval <= 0)
                playInterval = 1;

            RecordingTimer.Interval = recordInterval;
            RecordingTimer.Elapsed += new ElapsedEventHandler(RecordingTimer_Elapsed);

            PlayTimer.Interval = playInterval;
            PlayTimer.Elapsed += new ElapsedEventHandler(PlayTimer_Elapsed);
        }

        List<List<CustomKey>> keyList = new List<List<CustomKey>>();

        public int RecordInterval { get { return (int)RecordingTimer.Interval; } set { RecordingTimer.Interval = value; } }

        public int PlayInterval { get { return (int)PlayTimer.Interval; } set { PlayTimer.Interval = value; } }

        int counter = 0;

        public bool IsPlaying = false;
        public bool IsRecording = false;

        Timer RecordingTimer = new System.Timers.Timer();
        Timer PlayTimer = new System.Timers.Timer();

        #region PINVOKE

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern short GetKeyState(VK vKey);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetKeyboardState(byte[] keys);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(VK vKey);

        #endregion

        const uint KEYEVENTF_EXTENDEDKEY = 0x1;
        const uint KEYEVENTF_KEYUP = 0x2;

        public void StartRecording()
        {
            if (IsPlaying || IsRecording)
                return;

            IsRecording = true;

            for (var i = 0; i < keyList.Count; i++)
            {
                keyList[i].Clear();
                keyList[i] = null;
            }

            keyList.Clear();

            RecordingTimer.Start();
        }

        public void StopRecording()
        {
            if (IsPlaying)
                return;

            RecordingTimer.Stop();

            IsRecording = false;
        }

        public bool Play()
        {
            if (IsPlaying || IsRecording)
                return false;

            counter = 0;

            IsPlaying = true;

            PlayTimer.Start();

            return true;
        }

        public void Stop()
        {
            IsPlaying = false;
            counter = keyList.Count + 1;
        }

        private void RecordingTimer_Elapsed(object sender, EventArgs e)
        {
            SaveStates();
        }

        private void PlayTimer_Elapsed(object sender, EventArgs e)
        {
            if (IsPlaying == false)
            {
                PlayTimer.Stop();
                return;
            }

            if (keyList.Count == 0 || counter < 0 || counter >= keyList.Count)
            {
                PlayTimer.Stop();

                IsPlaying = false;

                counter = 0;

                return;
            }

            var liste = keyList[counter];

            for (var i = 0; i < liste.Count; i++)
            {
                var obj = liste[i];

                if (obj == null)
                    return;

                obj.Press();
            }

            counter++;
        }

        private void SaveStates()
        {
            var list = new List<CustomKey>();

            byte[] keys = new byte[256];

            //GetKeyboardState'teki bir hata aynı array'i döndürdüğünden klavyenin güncellenmesi için bu fonksiyonu (GetKeyState) çağırdık.
            //Kaynak : pinvoke.net -> GetKeyboardState
            GetAsyncKeyState(VK.CONTROL);

            if (!GetKeyboardState(keys))
            {
                //Hata var
                int err = Marshal.GetLastWin32Error();
                throw new Win32Exception(err);
            }

            for (var i = 0; i < keys.Length; i++)
            {
                try
                {
                    VK key = (VK)VK.ToObject(typeof(VK), i);

                    CustomKey tus = new CustomKey(key, IsKeyDown(key));

                    list.Add(tus);
                }
                catch (Exception hata)
                {
                    Debug.WriteLine(hata.Message + "\n" + hata.StackTrace);
                }
            }

            keyList.Add(list);

            keys = null;
        }

        private static bool IsKeyDown(VK tus)
        {
            return GetAsyncKeyState(tus) < 0;
        }

        public void ReleaseAllKeys()
        {

        }

        public static void Down(VK key, bool extended = false)
        {
            keybd_event((byte)key, 0, ((extended) ? KEYEVENTF_EXTENDEDKEY : 0), 0);
        }

        public static void Up(VK key, bool extended = false)
        {
            keybd_event((byte)key, 0, ((extended) ? KEYEVENTF_EXTENDEDKEY : 0) | KEYEVENTF_KEYUP, 0);
        }

        class CustomKey : ICloneable
        {
            private VK _key;
            private bool _isDown;

            public CustomKey()
            {

            }

            public CustomKey(VK key, bool isDown)
            {
                _key = key;
                _isDown = isDown;
            }

            public void Press()
            {
                bool isKeyDownNow = IsKeyDown(_key);

                if (_isDown && !isKeyDownNow)
                {
                    Down(_key);
                }
                else if (!_isDown && isKeyDownNow)
                {
                    Up(_key);
                }
            }

            public object Clone()
            {
                return new CustomKey(_key, _isDown);
            }
        }
    }
}

/*
To specify keys combined with any combination of the SHIFT, CTRL, and ALT keys, precede the key code with one or more of the following codes.
2 tablosu

Key 	Code

SHIFT 	+
CTRL 	^
ALT 	%

*/