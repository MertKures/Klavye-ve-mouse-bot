using System.Text;
using System;
using MouseM;
using KeyboardM;

namespace Klavye_ve_mouse_bot
{
	class Program
	{
		static readonly string Options = string.Join("\n",
			"1)Klavye tuşlarını kaydetmeye başla",
			"2)Klavye kaydını durdur",
			"3)Klavye kaydını oynat",
			"4)Mouse olaylarını kaydetmeye başla",
			"5)Mouse kaydını durdur",
			"6)Mouse kaydını oynat",
			"7)Hem mouse hem klavyeyi kaydet",
			"8)Mouse ve klavye kaydını durdur",
			"9)Mouse ve klavye kaydını oynat",
			"Not: Klavye ve mouse'tan biri kaydedilirken diğeri de kaydedilecek ise ilk önce klavyeyi başlatın.Öbür türlü mouse tuşları ya kaydedilmiyor ya da bot bu tuşları kullanamıyor.",
			"->");
		
		static StringBuilder stringBuilder = new StringBuilder();
		static MouseRecorder mouseRecorder = new MouseRecorder();
		static KeyboardRecorder keyboardRecorder = new KeyboardRecorder();
		
		static void Main(string[] args)
		{
			repeat:;
			
			Console.Clear();
			
			Console.Write(Options);

			stringBuilder.Clear();
	
			stringBuilder.Append(Console.ReadLine().Trim());
			
			if (stringBuilder.ToString() == "1")
			{
				keyboardRecorder.StartRecording();
			}
			else if (stringBuilder.ToString() == "2")
			{
				keyboardRecorder.StopRecording();
			}
			else if (stringBuilder.ToString() == "3")
			{
				keyboardRecorder.Play();
			}
			else if (stringBuilder.ToString() == "4")
			{
				mouseRecorder.StartRecording();
			}
			else if (stringBuilder.ToString() == "5")
			{
				mouseRecorder.StopRecording();
			}
			else if (stringBuilder.ToString() == "6")
			{
				mouseRecorder.Play();
			}
			else if (stringBuilder.ToString() == "7")
			{
				//SIRAYI DEĞİŞTİRME. Aksi takdirde mouse tuşları geçersiz kalıyor.
				keyboardRecorder.StartRecording();
				mouseRecorder.StartRecording();
			}
			else if (stringBuilder.ToString() == "8")
			{
				//SIRAYI DEĞİŞTİRME. Aksi takdirde mouse tuşları geçersiz kalıyor.
				keyboardRecorder.StopRecording();
				mouseRecorder.StopRecording();
			}
			else if (stringBuilder.ToString() == "9")
			{
				//SIRAYI DEĞİŞTİRME. Aksi takdirde mouse tuşları geçersiz kalıyor.
				keyboardRecorder.Play();
				mouseRecorder.Play();
			}
			
			goto repeat;
		}
	}
}