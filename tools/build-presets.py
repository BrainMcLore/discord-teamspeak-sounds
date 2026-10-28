"""Build bundled packs from an extracted official TeamSpeak 3 sound directory."""
import argparse, hashlib, json, pathlib, shutil, zipfile
p=argparse.ArgumentParser()
p.add_argument('sound_directory',type=pathlib.Path)
a=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[1]
mapping={'mute':'mic_muted','unmute':'mic_activated','deafen':'sound_muted','undeafen':'sound_resumed','user_join':'neutral_switched_tocurrentchannel','user_leave':'neutral_switched_awayfromcurrentchannel','disconnect':'disconnected'}
catalog=[]
for ident,name,folder,clicks in [('teamspeak-female','TeamSpeak Female','default',False),('teamspeak-male','TeamSpeak Male','default_male',False),('classic-clicks','Classic Clicks','default',True)]:
    files={k:a.sound_directory/folder/(v+'.wav') for k,v in mapping.items()}
    if clicks:
        files['mute']=a.sound_directory/'default'/'mic_click_off.wav'
        files['unmute']=a.sound_directory/'default'/'mic_click_on.wav'
    manifest={'schemaVersion':1,'name':name,'author':'TeamSpeak / preset selection','description':'Classic TeamSpeak 3 sounds'+(' with microphone clicks' if clicks else ''),'sounds':{k:{'file':k+'.wav','mime':'audio/wav'} for k in files}}
    target=root/'presets'/(ident+'.dspack');target.parent.mkdir(exist_ok=True)
    with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED) as z:
        z.writestr('pack.json',json.dumps(manifest,ensure_ascii=False,indent=2))
        for key,file in files.items():z.write(file,'sounds/'+key+'.wav')
    workshop=root/'workshop'/'packs'/target.name;workshop.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(target,workshop)
    catalog.append({'id':ident,'name':name,'author':manifest['author'],'description':manifest['description'],'url':'https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/workshop/packs/'+target.name,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
(root/'workshop'/'index.json').write_text(json.dumps({'schemaVersion':1,'packs':catalog},indent=2)+'\n',encoding='utf-8')
print('Built',len(catalog),'presets and workshop entries')
