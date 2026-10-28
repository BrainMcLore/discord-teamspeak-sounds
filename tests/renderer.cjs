const fs=require('fs'),path=require('path'),vm=require('vm'),assert=require('assert');
const root=path.join(__dirname,'..');
const source=fs.readFileSync(path.join(root,'teamspeak-sounds.js'),'utf8');
const fn=source.slice(source.indexOf('function renderer('),source.indexOf('\nfunction attach('));
const assets=JSON.parse(fs.readFileSync(path.join(root,'sound-assets.json'),'utf8'));
function setup(payload){
 class Media {play(){return 'played';}}
 Object.defineProperty(Media.prototype,'src',{configurable:true,get(){return this._src||'';},set(v){this._src=v;}});
 let forwards=0;
 const context={window:{fetch:async()=>{forwards++;return new Response('original');}},HTMLMediaElement:Media,Blob,URL,Uint8Array,atob,Response,location:{href:'https://discord.com/channels/@me'},console:{info(){}}};
 vm.createContext(context);const result=vm.runInContext('('+fn+')',context)(payload,assets);
 return {context,Media,result,forwards:()=>forwards};
}
(async()=>{
 const payload=Object.fromEntries(['mute','unmute','deafen','undeafen','user_join','user_leave','disconnect'].map(k=>[k,{data:fs.readFileSync(path.join(root,'sounds',k+'.wav')).toString('base64'),mime:'audio/wav'}]));
 const all=setup(payload);assert.equal(all.result.files,21);
 for(const [file,key] of Object.entries(assets)){
  const response=await all.context.window.fetch('https://discord.com/assets/'+file);assert.equal(response.headers.get('content-type'),'audio/wav');assert.deepEqual(Buffer.from(await response.arrayBuffer()),Buffer.from(payload[key].data,'base64'));
  const media=new all.Media();media.src='https://discord.com/assets/'+file;assert.match(media.src,/^blob:/);assert.equal(media.play(),'played');
 }
 const selective=setup({mute:{data:Buffer.from('ID3-test-audio').toString('base64'),mime:'audio/mpeg'}});
 assert.equal(selective.result.files,3);
 let response=await selective.context.window.fetch('https://discord.com/assets/2d3b4ba32c34c862.mp3');assert.equal(response.headers.get('content-type'),'audio/mpeg');assert.equal(await response.text(),'ID3-test-audio');
 response=await selective.context.window.fetch('https://discord.com/assets/2d3b4ba32c34c862.mp3',{method:'HEAD'});assert.equal((await response.arrayBuffer()).byteLength,0);
 for(const url of ['https://discord.com/assets/e74c4a06134a20e4.mp3','https://discord.com/api/example','https://other.invalid/assets/2d3b4ba32c34c862.mp3'])assert.equal(await(await selective.context.window.fetch(url)).text(),'original');
 assert.equal(selective.forwards(),3);assert.equal(setup({}).result.files,0);
 console.log('PASS: 21 WAV routes, media preview routing, selective MP3 MIME, HEAD, disabled events, unrelated requests, empty selection.');
})().catch(e=>{console.error(e);process.exitCode=1;});
