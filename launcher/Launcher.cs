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
[assembly:System.Reflection.AssemblyVersion("2.0.0.0")]
[assembly:System.Reflection.AssemblyFileVersion("2.0.0.0")]

namespace DiscordSoundLauncher {
 public sealed class SoundSpec { public string File; public string Mime; }
 public sealed class Pack { public string Name; public string Author; public string Description; public string Root; public Dictionary<string,SoundSpec> Sounds=new Dictionary<string,SoundSpec>(); }
 public sealed class WorkshopItem { public string Name; public string Author; public string Description; public string Url; public string Sha256; public override string ToString(){return Name+"  /  "+Author;} }
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
 public sealed class Launcher : Form {
  static readonly Color Bg=Color.FromArgb(18,21,29), Card=Color.FromArgb(28,33,45), Ink=Color.FromArgb(238,242,250), Muted=Color.FromArgb(160,173,194), Accent=Color.FromArgb(108,92,231);
  readonly string baseDir=AppDomain.CurrentDomain.BaseDirectory;
  readonly Dictionary<string,string> selected=new Dictionary<string,string>();
  readonly Dictionary<string,CheckBox> checks=new Dictionary<string,CheckBox>();
  readonly Dictionary<string,Label> paths=new Dictionary<string,Label>();
  readonly Dictionary<string,Button> pickers=new Dictionary<string,Button>(), players=new Dictionary<string,Button>();
  readonly List<Pack> presets=new List<Pack>();
  readonly ComboBox preset=new ComboBox(), language=new ComboBox();
  readonly TextBox packName=new TextBox(), log=new TextBox();
  readonly ListBox workshop=new ListBox();
  readonly Label title=new Label(), subtitle=new Label(), presetLabel=new Label(), nameLabel=new Label(), hint=new Label(), workshopHint=new Label();
  readonly Button apply=new Button(), restore=new Button(), status=new Button(), import=new Button(), export=new Button(), refresh=new Button(), download=new Button();
  readonly TabControl tabs=new TabControl();
  readonly List<WorkshopItem> catalog=new List<WorkshopItem>();
  string lang="RU";bool changing=false,busy=false,initializing=true;
  [DllImport("winmm.dll",CharSet=CharSet.Unicode)]static extern int mciSendString(string command,StringBuilder result,int length,IntPtr callback);
  [DllImport("winmm.dll",CharSet=CharSet.Unicode)]static extern bool mciGetErrorString(int error,StringBuilder text,int length);
  string T(string ru,string en,string uk){return lang=="EN"?en:lang=="UK"?uk:ru;}
  string EventName(string key){switch(key){case "mute":return T("Микрофон выключен","Microphone muted","Мікрофон вимкнено");case "unmute":return T("Микрофон включён","Microphone unmuted","Мікрофон увімкнено");case "deafen":return T("Наушники выключены","Sound muted","Навушники вимкнено");case "undeafen":return T("Наушники включены","Sound resumed","Навушники увімкнено");case "user_join":return T("Вход в канал","Channel join","Вхід до каналу");case "user_leave":return T("Выход участника","Participant leaves","Вихід учасника");default:return T("Отключение","Disconnect","Відключення");}}
  void Style(Control c){c.BackColor=Card;c.ForeColor=Ink;c.Font=new Font("Segoe UI",10);}
  Button Button(string text,EventHandler action){var b=new Button {Text=text,FlatStyle=FlatStyle.Flat,Height=36,Width=120,Cursor=Cursors.Hand};Style(b);b.FlatAppearance.BorderColor=Color.FromArgb(63,72,93);if(action!=null)b.Click+=action;return b;}
  void Error(Exception e){MessageBox.Show(this,e.Message,T("Не удалось выполнить","Could not complete","Не вдалося виконати"),MessageBoxButtons.OK,MessageBoxIcon.Warning);}
  void Write(string text){log.AppendText(text+Environment.NewLine);}
  public Launcher(){
   Text="Discord Sound Launcher";BackColor=Bg;ForeColor=Ink;Font=new Font("Segoe UI",10);ClientSize=new Size(1050,820);MinimumSize=new Size(980,790);StartPosition=FormStartPosition.CenterScreen;AutoScaleMode=AutoScaleMode.Dpi;
   title.SetBounds(26,20,790,38);title.Font=new Font("Segoe UI",23,FontStyle.Bold);title.ForeColor=Ink;Controls.Add(title);
   subtitle.SetBounds(28,66,820,24);subtitle.ForeColor=Muted;Controls.Add(subtitle);
   language.SetBounds(923,28,98,30);language.DropDownStyle=ComboBoxStyle.DropDownList;language.Items.AddRange(new object[]{"RU","EN","UK"});language.SelectedIndex=0;language.SelectedIndexChanged+=(s,e)=>{lang=language.Text;Localize();SaveState();};Controls.Add(language);
   tabs.SetBounds(24,106,1002,510);tabs.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
   var editor=new TabPage();var shop=new TabPage();editor.BackColor=Bg;shop.BackColor=Bg;tabs.TabPages.Add(editor);tabs.TabPages.Add(shop);Controls.Add(tabs);
   presetLabel.SetBounds(16,15,115,24);presetLabel.ForeColor=Muted;editor.Controls.Add(presetLabel);
   preset.SetBounds(132,12,320,30);preset.DropDownStyle=ComboBoxStyle.DropDownList;preset.SelectedIndexChanged+=(s,e)=>{if(!changing&&preset.SelectedIndex>=0)UsePack(presets[preset.SelectedIndex]);};Style(preset);editor.Controls.Add(preset);
   import.SetBounds(468,10,230,34);import.Click+=(s,e)=>ImportPack();editor.Controls.Add(import);
   export.SetBounds(712,10,230,34);export.Click+=(s,e)=>ExportPack();editor.Controls.Add(export);
   int y=58;foreach(var key in Packs.Keys){string id=key;var row=new Panel {Left=14,Top=y,Width=950,Height=48,BackColor=Card};editor.Controls.Add(row);
    var check=new CheckBox {Left=12,Top=12,Width=246,Height=24,Checked=true,ForeColor=Ink};checks[key]=check;row.Controls.Add(check);check.CheckedChanged+=(s,e)=>{if(!changing)SaveState();};
    var path=new Label {Left=266,Top=15,Width=390,Height=23,AutoEllipsis=true,ForeColor=Muted};paths[key]=path;row.Controls.Add(path);
    var pick=Button("",(s,e)=>Choose(id));pick.SetBounds(664,6,148,35);row.Controls.Add(pick);pickers[key]=pick;
    var play=Button("",(s,e)=>Preview(id));play.SetBounds(822,6,114,35);row.Controls.Add(play);players[key]=play;y+=50;
   }
   nameLabel.SetBounds(18,445,118,25);nameLabel.ForeColor=Muted;editor.Controls.Add(nameLabel);packName.SetBounds(140,441,400,28);Style(packName);packName.Text="My sound pack";packName.TextChanged+=(s,e)=>{if(!changing)SaveState();};editor.Controls.Add(packName);
   hint.SetBounds(16,413,940,25);hint.ForeColor=Muted;editor.Controls.Add(hint);
   workshopHint.SetBounds(18,15,920,52);workshopHint.ForeColor=Muted;shop.Controls.Add(workshopHint);
   workshop.SetBounds(18,74,940,275);Style(workshop);workshop.ItemHeight=28;shop.Controls.Add(workshop);
   refresh.SetBounds(18,366,220,38);refresh.Click+=async(s,e)=>await RefreshWorkshop();shop.Controls.Add(refresh);
   download.SetBounds(252,366,280,38);download.Click+=async(s,e)=>await DownloadPack();shop.Controls.Add(download);
   var share=Button("GitHub / contribute",(s,e)=>Process.Start(new ProcessStartInfo("https://github.com/BrainMcLore/discord-teamspeak-sounds/blob/main/workshop/README.md"){UseShellExecute=true}));share.SetBounds(550,366,260,38);shop.Controls.Add(share);
   apply.SetBounds(24,632,318,42);apply.BackColor=Accent;apply.Click+=async(s,e)=>await RunPatch("Apply");Controls.Add(apply);
   restore.SetBounds(354,632,250,42);restore.Click+=async(s,e)=>await RunPatch("Restore");Controls.Add(restore);
   status.SetBounds(616,632,240,42);status.Click+=async(s,e)=>await RunPatch("Status");Controls.Add(status);
   foreach(var b in new[]{apply,restore,status,import,export,refresh,download}){b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderColor=Color.FromArgb(63,72,93);b.ForeColor=Ink;b.Font=Font;if(b!=apply)b.BackColor=Card;}
   log.SetBounds(24,688,1002,108);log.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;log.Multiline=true;log.ReadOnly=true;log.ScrollBars=ScrollBars.Vertical;Style(log);Controls.Add(log);
   FormClosing+=(s,e)=>{if(busy){e.Cancel=true;return;}mciSendString("close preview",null,0,IntPtr.Zero);SaveState();};
   Localize();LoadPresets();LoadState();initializing=false;SaveState();Write(T("Выберите пресет или свои WAV/MP3. Закройте Discord перед применением.","Choose a preset or your own WAV/MP3. Quit Discord before applying.","Виберіть пресет або власні WAV/MP3. Закрийте Discord перед застосуванням."));
  }
  void Localize(){
   title.Text="Discord Sound Launcher";subtitle.Text=T("Свои звуки. Пресеты. Паки сообщества.","Your sounds. Presets. Community packs.","Власні звуки. Пресети. Паки спільноти.");tabs.TabPages[0].Text=T("Мои звуки","My sounds","Мої звуки");tabs.TabPages[1].Text=T("Воркшоп","Workshop","Воркшоп");
   presetLabel.Text=T("Пресет","Preset","Пресет");nameLabel.Text=T("Имя пака","Pack name","Назва пака");hint.Text=T("Снимите галочку, чтобы оставить звук Discord. WAV / MP3, до 10 МБ на звук.","Uncheck an event to keep Discord's sound. WAV / MP3, up to 10 MB per sound.","Зніміть позначку, щоб залишити звук Discord. WAV / MP3, до 10 МБ на звук.");
   foreach(var key in Packs.Keys){checks[key].Text=EventName(key);pickers[key].Text=T("Выбрать файл","Choose file","Вибрати файл");players[key].Text=T("Слушать","Preview","Слухати");}
   apply.Text=T("Применить в Discord","Apply to Discord","Застосувати в Discord");restore.Text=T("Вернуть оригинал","Restore original","Повернути оригінал");status.Text=T("Проверить статус","Check status","Перевірити стан");import.Text=T("Импорт пака","Import pack","Імпорт пака");export.Text=T("Экспорт пака","Export pack","Експорт пака");refresh.Text=T("Обновить каталог","Refresh catalog","Оновити каталог");download.Text=T("Скачать выбранный пак","Download selected pack","Завантажити вибраний пак");workshopHint.Text=T("Каталог паков из GitHub. Скачивание проверяется по SHA256. Свои паки можно импортировать и экспортировать; публикация — через вклад в репозиторий.","GitHub pack catalog. Downloads are checked using SHA256. Import and export your packs; submit catalog entries through the repository.","Каталог паків із GitHub. Завантаження перевіряються за SHA256. Імпортуйте та експортуйте власні паки; публікація — через внесок у репозиторій.");
  }
  void LoadPresets(){try{string root=Path.Combine(baseDir,"presets");if(!Directory.Exists(root))throw new DirectoryNotFoundException("Extract the complete launcher ZIP, including presets and scripts.");foreach(var file in Directory.GetFiles(root,"*.dspack").OrderBy(x=>Path.GetFileName(x)=="teamspeak-female.dspack"?0:Path.GetFileName(x)=="teamspeak-male.dspack"?1:2)){var p=Packs.Import(file);presets.Add(p);preset.Items.Add(p.Name);}if(presets.Count>0)preset.SelectedIndex=0;}catch(Exception e){Error(e);}}
  void UsePack(Pack p){changing=true;foreach(var key in Packs.Keys){SoundSpec spec;bool has=p.Sounds.TryGetValue(key,out spec);selected[key]=has?Path.Combine(p.Root,"sounds",spec.File):null;checks[key].Checked=has;paths[key].Text=has?Path.GetFileName(selected[key]):"Discord default";}packName.Text=p.Name;changing=false;SaveState();Write("Pack: "+p.Name+" / "+p.Author);}
  void Choose(string key){using(var dialog=new OpenFileDialog {Filter="Audio (*.wav;*.mp3)|*.wav;*.mp3",CheckFileExists=true})if(dialog.ShowDialog(this)==DialogResult.OK)try{Packs.ValidateAudio(dialog.FileName);string ext=Path.GetExtension(dialog.FileName).ToLowerInvariant();string dir=Path.Combine(Packs.DataRoot,"custom",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string file=Path.Combine(dir,key+ext);File.Copy(dialog.FileName,file);selected[key]=file;checks[key].Checked=true;paths[key].Text=Path.GetFileName(dialog.FileName);SaveState();}catch(Exception e){Error(e);}}
  void Preview(string key){try{string file;if(!selected.TryGetValue(key,out file)||String.IsNullOrEmpty(file))return;mciSendString("close preview",null,0,IntPtr.Zero);int error=mciSendString("open \""+file+"\" alias preview",null,0,IntPtr.Zero);if(error==0)error=mciSendString("play preview",null,0,IntPtr.Zero);if(error!=0){var text=new StringBuilder(300);mciGetErrorString(error,text,text.Capacity);throw new InvalidOperationException(text.ToString());}}catch(Exception e){Error(e);}}
  Dictionary<string,string> Selection(){var result=new Dictionary<string,string>();foreach(var key in Packs.Keys){string file;if(checks[key].Checked){if(!selected.TryGetValue(key,out file)||String.IsNullOrEmpty(file))throw new InvalidDataException("Choose an audio file: "+key);result[key]=file;}}return result;}
  void ImportPack(){using(var d=new OpenFileDialog {Filter="Discord sound packs (*.dspack)|*.dspack"})if(d.ShowDialog(this)==DialogResult.OK)try{UsePack(Packs.Import(d.FileName));}catch(Exception e){Error(e);}}
  void ExportPack(){using(var d=new SaveFileDialog {Filter="Discord sound packs (*.dspack)|*.dspack",FileName="my-sounds.dspack",OverwritePrompt=true})if(d.ShowDialog(this)==DialogResult.OK)try{Packs.Export(d.FileName,packName.Text,Selection());Write("Exported: "+d.FileName);}catch(Exception e){Error(e);}}
  void SaveState(){if(changing||initializing)return;try{Directory.CreateDirectory(Packs.DataRoot);var enabled=checks.ToDictionary(x=>x.Key,x=>x.Value.Checked);var d=new {language=lang,name=packName.Text,files=selected,enabled=enabled};File.WriteAllText(Path.Combine(Packs.DataRoot,"settings.json"),Packs.Json.Serialize(d),new UTF8Encoding(false));}catch(Exception e){Write("Settings: "+e.Message);}}
  void LoadState(){try{string file=Path.Combine(Packs.DataRoot,"settings.json");if(!File.Exists(file))return;var d=Packs.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(file,Encoding.UTF8));changing=true;var files=Packs.Dict(d["files"]);var enabled=Packs.Dict(d["enabled"]);foreach(var key in Packs.Keys){string value=Packs.Text(files,key);if(File.Exists(value)){selected[key]=value;paths[key].Text=Path.GetFileName(value);checks[key].Checked=enabled.ContainsKey(key)&&Convert.ToBoolean(enabled[key]);}else if(enabled.ContainsKey(key)&&!Convert.ToBoolean(enabled[key]))checks[key].Checked=false;}packName.Text=Packs.Text(d,"name");string l=Packs.Text(d,"language");if(new[]{"RU","EN","UK"}.Contains(l)){lang=l;language.SelectedItem=l;}changing=false;Localize();}catch(Exception e){changing=false;Write("Settings: "+e.Message);}}
  void Busy(bool value){busy=value;foreach(var b in new[]{apply,restore,status,import,export,refresh,download})b.Enabled=!value;preset.Enabled=!value;language.Enabled=!value;UseWaitCursor=value;}
  async Task RunPatch(string mode){if(busy)return;try{if(mode!="Status"&&Process.GetProcessesByName("Discord").Length>0)throw new InvalidOperationException(T("Полностью закройте Discord через значок возле часов и повторите.","Fully quit Discord using its tray icon, then try again.","Повністю закрийте Discord через значок біля годинника та повторіть."));string profile=mode=="Apply"?Packs.Stage(Selection()):null;SaveState();Busy(true);mciSendString("close preview",null,0,IntPtr.Zero);Write("Running: "+mode);var result=await Task.Run(()=>{var info=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -File \""+Path.Combine(baseDir,"Manage-Sounds.ps1")+"\" -Mode "+mode+(profile==null?"":" -PackPath \""+profile+"\"")){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};using(var p=Process.Start(info)){var output=p.StandardOutput.ReadToEndAsync();var error=p.StandardError.ReadToEndAsync();p.WaitForExit();Task.WaitAll(output,error);return Tuple.Create(p.ExitCode,output.Result+error.Result);}});Write(result.Item2);if(result.Item1!=0)throw new InvalidOperationException(T("Операция не завершена. Подробности в журнале ниже.","Operation failed. See the log below.","Операцію не завершено. Подробиці в журналі нижче."));}catch(Exception e){Error(e);}finally{Busy(false);}}
  WebClient Client(){ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;var c=new WebClient();c.Headers["User-Agent"]="Discord-Sound-Launcher/2.0";c.Encoding=Encoding.UTF8;return c;}
  async Task RefreshWorkshop(){if(busy)return;Busy(true);try{string json;using(var c=Client())json=await c.DownloadStringTaskAsync("https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/workshop/index.json");if(json.Length>1024*1024)throw new InvalidDataException("Catalog is too large.");var d=Packs.Json.Deserialize<Dictionary<string,object>>(json);if(Packs.Text(d,"schemaVersion")!="1")throw new InvalidDataException("Unsupported catalog version.");var fresh=new List<WorkshopItem>();foreach(var item in (IEnumerable)d["packs"]){var entry=Packs.Dict(item);var p=new WorkshopItem {Name=Packs.Text(entry,"name"),Author=Packs.Text(entry,"author"),Description=Packs.Text(entry,"description"),Url=Packs.Text(entry,"url"),Sha256=Packs.Text(entry,"sha256")};Uri uri;if(p.Name.Length>80||p.Description.Length>1000||p.Author.Length>80||!Uri.TryCreate(p.Url,UriKind.Absolute,out uri)||uri.Scheme!="https"||uri.Host!="raw.githubusercontent.com"||!uri.AbsolutePath.StartsWith("/BrainMcLore/discord-teamspeak-sounds/main/workshop/packs/",StringComparison.Ordinal)||p.Sha256.Length!=64||p.Sha256.Any(ch=>!Uri.IsHexDigit(ch)))throw new InvalidDataException("Invalid catalog entry.");fresh.Add(p);if(fresh.Count>200)throw new InvalidDataException("Too many catalog entries.");}catalog.Clear();catalog.AddRange(fresh);workshop.Items.Clear();foreach(var p in catalog)workshop.Items.Add(p);if(workshop.Items.Count>0)workshop.SelectedIndex=0;Write("Workshop: "+catalog.Count+" packs");}catch(Exception e){Error(e);}finally{Busy(false);}}
  async Task DownloadPack(){if(busy||workshop.SelectedIndex<0)return;var item=(WorkshopItem)workshop.SelectedItem;Busy(true);try{string file=Path.Combine(Packs.DataRoot,"downloads",Guid.NewGuid().ToString("N")+".dspack");Directory.CreateDirectory(Path.GetDirectoryName(file));using(var c=Client()){c.DownloadProgressChanged+=(s,e)=>{if(e.BytesReceived>75L*1024*1024)c.CancelAsync();};await c.DownloadFileTaskAsync(item.Url,file);}if(!String.Equals(Packs.Hash(file),item.Sha256,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Pack SHA256 mismatch.");UsePack(Packs.Import(file));tabs.SelectedIndex=0;}catch(Exception e){Error(e);}finally{Busy(false);}}
  [STAThread] public static int Main(string[] args){
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   if(args.Length==2&&args[0]=="--validate-pack"){try{Packs.Import(args[1]);return 0;}catch{return 1;}}
   Application.Run(new Launcher());return 0;
  }
 }
}
