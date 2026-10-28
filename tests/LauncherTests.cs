using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using DiscordSoundLauncher;
public static class LauncherTests {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Reject(string file){try{Packs.Import(file);}catch(InvalidDataException){return;}throw new Exception("Unsafe pack accepted: "+file);}
 public static int Main(string[] args){try{
  string repo=args[0], temp=args[1];Directory.CreateDirectory(temp);Packs.DataRoot=Path.Combine(temp,"data");
  foreach(var file in Directory.GetFiles(Path.Combine(repo,"presets"),"*.dspack")){var p=Packs.Import(file);Check(p.Sounds.Count==7,"Preset missing sounds");Check(Packs.Import(file).Name==p.Name,"Repeated import failed");}
  var selection=new Dictionary<string,string>{{"mute",Path.Combine(repo,"sounds","mute.wav")},{"deafen",args[2]}};
  string archive=Path.Combine(temp,"roundtrip.dspack");Packs.Export(archive,"Mixed WAV and MP3",selection);var imported=Packs.Import(archive);Check(imported.Sounds.Count==2,"Disabled events did not remain original");Check(imported.Sounds["deafen"].Mime=="audio/mpeg","MP3 MIME missing");Check(Packs.Hash(args[2])==Packs.Hash(Path.Combine(imported.Root,"sounds","deafen.mp3")),"Audio bytes changed");
  string profile=Packs.Stage(selection);var json=File.ReadAllText(Path.Combine(profile,"sounds.json"));Check(json.Contains("audio/mpeg")&&!json.Contains("user_join"),"Profile is not selective");
  string attack=Path.Combine(temp,"traversal.dspack");using(var z=ZipFile.Open(attack,ZipArchiveMode.Create)){using(var w=new StreamWriter(z.CreateEntry("../escape.txt").Open()))w.Write("x");}Reject(attack);Check(!File.Exists(Path.Combine(temp,"escape.txt")),"Path escaped");
  string script=Path.Combine(temp,"script.dspack");using(var z=ZipFile.Open(script,ZipArchiveMode.Create)){using(var w=new StreamWriter(z.CreateEntry("evil.ps1").Open()))w.Write("throw 'never execute'");}Reject(script);
  string wrong=Path.Combine(temp,"invalid-manifest.dspack");using(var z=ZipFile.Open(wrong,ZipArchiveMode.Create)){using(var w=new StreamWriter(z.CreateEntry("pack.json").Open()))w.Write("{\"schemaVersion\":1,\"name\":\"Bad\",\"sounds\":{\"mute\":{\"file\":\"../file.wav\",\"mime\":\"audio/wav\"}}}");}Reject(wrong);
  Console.WriteLine("PASS: three presets, repeat import, selective WAV/MP3 export/import, exact bytes, profiles, traversal/script/manifest rejection.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
