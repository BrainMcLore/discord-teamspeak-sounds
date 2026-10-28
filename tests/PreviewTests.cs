using System;
using System.Runtime.InteropServices;
using System.Text;
public static class PreviewTests {
 [DllImport("winmm.dll",CharSet=CharSet.Unicode)]static extern int mciSendString(string command,StringBuilder text,int length,IntPtr callback);
 public static int Main(string[] files){try{foreach(var file in files){int error=mciSendString("open \""+file+"\" alias testpreview",null,0,IntPtr.Zero);if(error!=0)throw new Exception("Audio open failed: "+error);error=mciSendString("play testpreview",null,0,IntPtr.Zero);if(error!=0)throw new Exception("Audio play failed: "+error);mciSendString("close testpreview",null,0,IntPtr.Zero);}Console.WriteLine("PASS: native WAV and MP3 preview open/play/close.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
