using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[assembly:System.Reflection.AssemblyTitle("Discord Sound Launcher")]
[assembly:System.Reflection.AssemblyVersion("3.0.1.0")]
[assembly:System.Reflection.AssemblyFileVersion("3.0.1.0")]

namespace DiscordSoundLauncher {
 public sealed class SoundSpec { public string File; public string Mime; }
 public sealed class Pack { public string Name; public string Author; public string Description; public string Root; public Dictionary<string,SoundSpec> Sounds=new Dictionary<string,SoundSpec>(); }
 public static class Packs {
  public static readonly string[] Keys={"mute","unmute","deafen","undeafen","user_join","user_leave","disconnect"};
  public static readonly JavaScriptSerializer Json=new JavaScriptSerializer { MaxJsonLength=1024*1024 };
  public static string DataRoot=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DiscordTeamSpeakSounds","launcher");
  public static Dictionary<string,object> Dict(object o){var d=o as Dictionary<string,object>;if(d==null)throw new InvalidDataException("Invalid JSON object.");return d;}
  public static string Text(Dictionary<string,object> d,string k){object v;return d.TryGetValue(k,out v)?Convert.ToString(v):"";}
  public static string Hash(string path){using(var sha=SHA256.Create())using(var input=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();}
  public static string Mime(string file){return Path.GetExtension(file).ToLowerInvariant()==".mp3"?"audio/mpeg":"audio/wav";}
  public static void ValidateAudio(string path){
   var info=new FileInfo(path);if(!info.Exists||info.Length<12||info.Length>10*1024*1024)throw new InvalidDataException("Audio must be between 12 bytes and 10 MB.");
   var ext=info.Extension.ToLowerInvariant();var h=new byte[12];using(var s=info.OpenRead())s.Read(h,0,h.Length);
   if(ext==".wav") {if(Encoding.ASCII.GetString(h,0,4)!="RIFF"||Encoding.ASCII.GetString(h,8,4)!="WAVE")throw new InvalidDataException("Invalid WAV header.");}
   else if(ext==".mp3") {if(!(h[0]==(byte)'I'&&h[1]==(byte)'D'&&h[2]==(byte)'3')&&!(h[0]==255&&(h[1]&224)==224))throw new InvalidDataException("Invalid MP3 header.");}
   else throw new InvalidDataException("Choose a WAV or MP3 file.");
  }
  public static Pack Read(string root){
   var d=Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(root,"pack.json"),Encoding.UTF8));
   if(Text(d,"schemaVersion")!="1")throw new InvalidDataException("Unsupported pack version.");
   var p=new Pack {Name=Text(d,"name"),Author=Text(d,"author"),Description=Text(d,"description"),Root=root};
   if(p.Name.Length<1||p.Name.Length>80||p.Author.Length>80||p.Description.Length>1000)throw new InvalidDataException("Invalid pack metadata.");
   foreach(var pair in Dict(d["sounds"])){
    if(!Keys.Contains(pair.Key))throw new InvalidDataException("Unknown sound event.");
    var spec=Dict(pair.Value);var file=Text(spec,"file");
    if(file!=pair.Key+".wav"&&file!=pair.Key+".mp3")throw new InvalidDataException("Invalid sound filename.");
    if(Text(spec,"mime")!=Mime(file))throw new InvalidDataException("Invalid audio MIME type.");
    ValidateAudio(Path.Combine(root,"sounds",file));p.Sounds[pair.Key]=new SoundSpec {File=file,Mime=Mime(file)};
   }
   return p;
  }
  public static Pack Import(string archive){
   if(new FileInfo(archive).Length>75*1024*1024)throw new InvalidDataException("Pack exceeds 75 MB.");
   string root=Path.Combine(DataRoot,"library",Hash(archive));
   if(File.Exists(Path.Combine(root,"pack.json")))return Read(root);
   string temp=Path.Combine(DataRoot,"imports",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
   using(var z=ZipFile.OpenRead(archive)){
    if(z.Entries.Count>8)throw new InvalidDataException("Pack contains too many entries.");
    var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);long total=0;
    foreach(var e in z.Entries){
     string name=e.FullName.Replace('\\','/');
     bool audio=Keys.Any(k=>name=="sounds/"+k+".wav"||name=="sounds/"+k+".mp3");
     if((name!="pack.json"&&!audio)||!seen.Add(name)||e.Length>10*1024*1024)throw new InvalidDataException("Unexpected or duplicate file in pack.");
     total+=e.Length;if(total>71*1024*1024)throw new InvalidDataException("Unpacked pack is too large.");
     string target=Path.Combine(temp,name.Replace('/',Path.DirectorySeparatorChar));Directory.CreateDirectory(Path.GetDirectoryName(target));
     using(var input=e.Open())using(var output=File.Create(target)){var buffer=new byte[8192];int n;long count=0;while((n=input.Read(buffer,0,buffer.Length))>0){count+=n;if(count>10*1024*1024)throw new InvalidDataException("Audio exceeds 10 MB.");output.Write(buffer,0,n);}}
    }
   }
   Read(temp);Directory.CreateDirectory(Path.GetDirectoryName(root));Directory.Move(temp,root);return Read(root);
  }
  public static Dictionary<string,object> SoundMap(Dictionary<string,string> selection){
   var map=new Dictionary<string,object>();foreach(var pair in selection){if(String.IsNullOrEmpty(pair.Value))continue;ValidateAudio(pair.Value);string ext=Path.GetExtension(pair.Value).ToLowerInvariant();map[pair.Key]=new {file=pair.Key+ext,mime=Mime(pair.Value)};}return map;
  }
  public static string Stage(Dictionary<string,string> selection){
   var map=SoundMap(selection);string root=Path.Combine(DataRoot,"profiles",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(root,"sounds"));
   foreach(var pair in selection)if(!String.IsNullOrEmpty(pair.Value))File.Copy(pair.Value,Path.Combine(root,"sounds",pair.Key+Path.GetExtension(pair.Value).ToLowerInvariant()));
   File.WriteAllText(Path.Combine(root,"sounds.json"),Json.Serialize(map),new UTF8Encoding(false));return root;
  }
  public static void Export(string file,string name,Dictionary<string,string> selection){
   if(String.IsNullOrWhiteSpace(name)||name.Length>80)throw new InvalidDataException("Pack name must contain 1-80 characters.");
   var map=SoundMap(selection);string stage=Stage(selection);
   var manifest=new {schemaVersion=1,name=name,author="Community",description="Custom Discord sound pack",sounds=map};
   using(var output=new FileStream(file,FileMode.Create,FileAccess.Write))using(var z=new ZipArchive(output,ZipArchiveMode.Create)){
    var e=z.CreateEntry("pack.json");using(var w=new StreamWriter(e.Open(),new UTF8Encoding(false)))w.Write(Json.Serialize(manifest));
    foreach(var audio in Directory.GetFiles(Path.Combine(stage,"sounds")))z.CreateEntryFromFile(audio,"sounds/"+Path.GetFileName(audio));
   }
  }
 }
 public static class Program {
  [STAThread] public static int Main(string[] args){
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   if(args.Length==2&&args[0]=="--validate-pack"){try{Packs.Import(args[1]);return 0;}catch{return 1;}}
   new System.Windows.Application().Run(new ModernLauncher().Window);return 0;
  }
 }
}
