using System;
using System.Windows.Media;
using System.Windows.Threading;
public static class PreviewTests {
 [STAThread]public static int Main(string[] files){foreach(var file in files){int result=1;var frame=new DispatcherFrame();var player=new MediaPlayer();var timer=new DispatcherTimer {Interval=TimeSpan.FromSeconds(8)};timer.Tick+=(s,e)=>{timer.Stop();frame.Continue=false;};player.MediaOpened+=(s,e)=>{Console.WriteLine("Opened: "+player.NaturalDuration);player.Play();player.Close();result=0;timer.Stop();frame.Continue=false;};player.MediaFailed+=(s,e)=>{Console.Error.WriteLine(e.ErrorException);timer.Stop();frame.Continue=false;};player.Open(new Uri(file));timer.Start();Dispatcher.PushFrame(frame);if(result!=0)return result;}Console.WriteLine("PASS: native WAV and MP3 preview open/play/close.");return 0;}
}
