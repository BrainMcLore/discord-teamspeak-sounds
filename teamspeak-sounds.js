'use strict';
// Runs in Discord's existing process. No service and no network requests.
const { app, webContents } = require('electron');
const fs = require('fs');
const path = require('path');
function log(value) {
  try { fs.appendFileSync(path.join(__dirname, 'status.log'), new Date().toISOString() + ' ' + value + '\n'); } catch (_) {}
}
const sounds = Object.fromEntries(['mute','unmute','deafen','undeafen','user_join','user_leave','disconnect'].map(name =>
  [name, fs.readFileSync(path.join(__dirname, 'sounds', name + '.wav')).toString('base64')]));
const assets = JSON.parse(fs.readFileSync(path.join(__dirname,'sound-assets.json'),'utf8'));
function renderer(payload,assets) {
  if (window.__teamSpeakSounds) return window.__teamSpeakSounds;
  const byFile = new Map();
  for (const [name,b64] of Object.entries(payload)) {
    const bytes = Uint8Array.from(atob(b64), c=>c.charCodeAt(0));
    const item = {name,bytes,uri:URL.createObjectURL(new Blob([bytes], {type:'audio/wav'}))};
    for (const [file,key] of Object.entries(assets)) if(key===name) byFile.set(file,item);
  }
  function match(input) {
    try {
      const u = new URL(typeof input==='string' ? input : input.url, location.href);
      if (u.origin !== 'https://discord.com') return;
      return byFile.get(u.pathname.replace(/^\/assets\//,''));
    } catch (_) {}
  }
  function hit(item) { console.info('TS-SOUNDS-HIT:'+item.name); }
  const originalFetch=window.fetch;
  window.fetch=function(input,init) {
    const item=match(input);
    if (!item) return Reflect.apply(originalFetch,this,arguments);
    hit(item);
    const head=(init?.method || input?.method || 'GET').toUpperCase()==='HEAD';
    return Promise.resolve(new Response(head?null:item.bytes,{status:200,headers:{'Content-Type':'audio/wav','Content-Length':String(item.bytes.length)}}));
  };
  const proto=HTMLMediaElement.prototype;
  const descriptor=Object.getOwnPropertyDescriptor(proto,'src');
  Object.defineProperty(proto,'src',{...descriptor,set(value){const item=match(value);if(item)hit(item);return descriptor.set.call(this,item?item.uri:value);}});
  const originalPlay=proto.play;
  proto.play=function(){const item=match(descriptor.get.call(this));if(item){hit(item);descriptor.set.call(this,item.uri);}return Reflect.apply(originalPlay,this,arguments);};
  window.__teamSpeakSounds={ready:true,files:byFile.size};
  return window.__teamSpeakSounds;
}
function attach(wc) {
  if(wc.__tsSoundsAttached)return;
  wc.__tsSoundsAttached=true;
  wc.on('console-message',(_event,...args)=>{
    const messages=args.filter(a=>typeof a==='string');
    if(_event?.message)messages.push(_event.message);
    for(const message of new Set(messages))if(/^TS-SOUNDS-HIT:(mute|unmute|deafen|undeafen|user_join|user_leave|disconnect)$/.test(message))log(message);
  });
  async function apply(){
    if(wc.isDestroyed())return;
    for(const frame of wc.mainFrame.framesInSubtree){
      if(!/^https:\/\/(?:canary\.|ptb\.)?discord\.com\//.test(frame.url))continue;
      try{const result=await frame.executeJavaScript('('+renderer.toString()+')('+JSON.stringify(sounds)+','+JSON.stringify(assets)+')');log(JSON.stringify(result));}catch(e){log('ERROR '+e.message);}
    }
  }
  wc.on('dom-ready',apply);
  wc.on('did-frame-finish-load',apply);
  apply();
}
app.on('web-contents-created',(_event,wc)=>attach(wc));
app.whenReady().then(()=>{for(const wc of webContents.getAllWebContents())attach(wc);});
log('Loader ready');
