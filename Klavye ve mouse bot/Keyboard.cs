using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Timers;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
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
		
		CustomKey tus = new CustomKey();
		
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
			
			for (var i = 0;i < liste.Count;i++)
			{
				var obj = liste[i];
				
				if (obj == null)
					return;
				}
				
				obj.Uygula();		
			}

			counter++;
        }
		
		private void SaveStates()
		{
			var liste = new List<CustomKey>();
			
			byte[] keys = new byte[256];
			
			//GetKeyboardState'teki bir hata aynı array'i döndürdüğünden klavyenin güncellenmesi için bu fonksiyonu (GetKeyState) çağırdık.
			//Kaynak : pinvoke.net -> GetKeyboardState
			GetAsyncKeyState(VK.CONTROL);
			
			if(!GetKeyboardState(keys))
			{
				//Hata var
				int err = Marshal.GetLastWin32Error();
				throw new Win32Exception(err);
			}
			
			for (var i = 0;i < keys.Length;i++)
			{
				try
				{
					VK key = (VK)VK.ToObject(typeof(VK), i);
					
					tus = new CustomKey(key, IsKeyDown(key));
					
					liste.Add(tus);
				}
				catch(Exception hata)
				{
					Log(hata.Message + "\n" + hata.StackTrace);
				}
			}
			
			keyList.Add(liste);
			
			keys = null;
		}
		
		private static bool IsKeyDown(VK tus)
		{
			return GetAsyncKeyState(tus) < 0;
		}
		
		private static void Log(string mesaj)
		{
			System.Diagnostics.Debug.WriteLine(mesaj);
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

		class CustomKey
		{
			VK key;
			bool isDown;
			
			public CustomKey()
			{
				
			}

			public CustomKey(VK _key, bool basili)
			{
				key = _key;
				isDown = basili;
			}
			
			public void Uygula()
			{
				bool suanBasili = IsKeyDown(key);

				//Kayıt esnasında basılı ve şuan basılı değil ise bas.
				if (isDown && !suanBasili)
				{
					Down(key);
					//Console.Write("Down : " + key.ToString() + " ");
				}
				//Kayıt esnasında basılı değil ama şuan basılı ise bırak.
				else if (!isDown && suanBasili)
				{
					Up(key);
					//Console.Write("Up : " + key.ToString() + " ");
				}
			}

			public CustomKey Kopyala()
			{
				return new CustomKey(key, isDown);
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

/*
//StringBuilder sb = new StringBuilder();

//if (IsKeyDown(VK.CONTROL))
//	sb.Append("{^}");
//if (IsKeyDown(VK.MENU))
//	sb.Append("{%}");
//if (IsKeyDown(VK.SHIFT))
//	sb.Append("{+}");

//char c = (char)int.Parse(((int)Key).ToString(), System.Globalization.NumberStyles.HexNumber);

//Console.WriteLine($"Hex key : {(int)Key}, ASCII key : {int.Parse(((int)Key).ToString(), .Globalization.NumberStyles.HexNumber)}, Char : {c}");

//sb.Append(c);

//try
//{
//	if (sb != null)
//		System.Windows.Forms.SendKeys.SendWait(sb.ToString());

//	//System.Threading.Thread.Sleep(2);
//}
//catch { }

//sb.Clear();
//sb = null;

*/