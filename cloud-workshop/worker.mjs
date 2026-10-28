const KEYS=['mute','unmute','deafen','undeafen','user_join','user_leave','disconnect'];
const CATEGORIES=['Classic','Gaming','Memes','Anime','Minimal','Music','Other'];
const enc=new TextEncoder();
export class ApiError extends Error {constructor(status,message){super(message);this.status=status;}}
const check=(ok,message,status=400)=>{if(!ok)throw new ApiError(status,message);};
const hex=bytes=>Array.from(new Uint8Array(bytes),x=>x.toString(16).padStart(2,'0')).join('');
const unhex=text=>Uint8Array.from(text.match(/../g)||[],v=>parseInt(v,16));
export const sha=async bytes=>hex(await crypto.subtle.digest('SHA-256',bytes));
export async function passwordHash(password,salt){const key=await crypto.subtle.importKey('raw',enc.encode(password),'PBKDF2',false,['deriveBits']);return hex(await crypto.subtle.deriveBits({name:'PBKDF2',salt:unhex(salt),iterations:100000,hash:'SHA-256'},key,256));}
const equal=(a,b)=>{if(a.length!==b.length)return false;let diff=0;for(let i=0;i<a.length;i++)diff|=a.charCodeAt(i)^b.charCodeAt(i);return diff===0;};
const json=(value,status=200)=>new Response(JSON.stringify(value),{status,headers:{'Content-Type':'application/json; charset=utf-8','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'}});
async function boundedBody(request,limit){const length=Number(request.headers.get('content-length')||0);check(length<=limit,'Request too large',413);const reader=request.body?.getReader();if(!reader)return new Uint8Array();const chunks=[];let size=0;try{for(;;){const {done,value}=await reader.read();if(done)break;size+=value.length;check(size<=limit,'Request too large',413);chunks.push(value);}}catch(e){await reader.cancel();throw e;}const result=new Uint8Array(size);let offset=0;for(const chunk of chunks){result.set(chunk,offset);offset+=chunk.length;}return result;}
const crcTable=Uint32Array.from({length:256},(_,value)=>{for(let bit=0;bit<8;bit++)value=(value>>>1)^((value&1)?0xedb88320:0);return value>>>0;});
function crc32(bytes){let crc=0xffffffff;for(const byte of bytes)crc=crcTable[(crc^byte)&255]^(crc>>>8);return (crc^0xffffffff)>>>0;}
export async function validatePack(bytes){
 check(bytes.length>=22&&bytes.length<=8*1024*1024,'Pack must be a .dspack ZIP up to 8 MB');
 const view=new DataView(bytes.buffer,bytes.byteOffset,bytes.byteLength);let end=-1;
 for(let p=bytes.length-22;p>=Math.max(0,bytes.length-65557);p--)if(view.getUint32(p,true)===0x06054b50){end=p;break;}
 check(end>=0,'Invalid ZIP archive');check(view.getUint16(end+4,true)===0&&view.getUint16(end+6,true)===0,'Multipart ZIP is unsupported');
 const count=view.getUint16(end+10,true),size=view.getUint32(end+12,true),start=view.getUint32(end+16,true);check(count>=1&&count<=8&&start+size<=end,'Invalid ZIP directory');
 const entries=new Map();let pos=start,total=0;
 for(let i=0;i<count;i++){
  check(pos+46<=bytes.length&&view.getUint32(pos,true)===0x02014b50,'Invalid ZIP entry');
  const flags=view.getUint16(pos+8,true),method=view.getUint16(pos+10,true),crc=view.getUint32(pos+16,true),compressed=view.getUint32(pos+20,true),raw=view.getUint32(pos+24,true),nameLength=view.getUint16(pos+28,true),extra=view.getUint16(pos+30,true),comment=view.getUint16(pos+32,true),local=view.getUint32(pos+42,true);
  check((flags&1)===0&&(method===0||method===8),'Encrypted or unsupported ZIP');check(raw<=10*1024*1024&&compressed<=bytes.length,'Audio too large');total+=raw;check(total<=70*1024*1024,'Expanded pack too large');
  check(pos+46+nameLength+extra+comment<=start+size,'Invalid ZIP entry length');const name=new TextDecoder('utf-8',{fatal:true}).decode(bytes.subarray(pos+46,pos+46+nameLength));
  check(name==='pack.json'||KEYS.some(k=>name===`sounds/${k}.wav`||name===`sounds/${k}.mp3`),'Packs may contain only pack.json and audio');check(!entries.has(name),'Duplicate ZIP entry');
  check(local+30<=start&&view.getUint32(local,true)===0x04034b50,'Invalid local ZIP header');const localNameLength=view.getUint16(local+26,true),localExtra=view.getUint16(local+28,true),dataStart=local+30+localNameLength+localExtra;
  check(dataStart+compressed<=start,'Invalid compressed range');check(new TextDecoder().decode(bytes.subarray(local+30,local+30+localNameLength))===name,'ZIP filename mismatch');
  const source=bytes.subarray(dataStart,dataStart+compressed);let output;
  if(method===0)output=source;else{const stream=new Blob([source]).stream().pipeThrough(new DecompressionStream('deflate-raw'));output=await boundedBody(new Request('https://local.invalid',{method:'POST',body:stream,duplex:'half'}),10*1024*1024);}
  check(output.length===raw&&crc32(output)===crc,'Corrupt audio or ZIP');entries.set(name,output);pos+=46+nameLength+extra+comment;
 }
 check(entries.has('pack.json')&&entries.get('pack.json').length<=32768,'Missing or oversized pack.json');let manifest;try{manifest=JSON.parse(new TextDecoder('utf-8',{fatal:true}).decode(entries.get('pack.json')));}catch{throw new ApiError(400,'Invalid pack.json');}
 check(manifest.schemaVersion===1&&typeof manifest.name==='string'&&manifest.name.length>=1&&manifest.name.length<=80,'Invalid pack metadata');check(manifest.sounds&&typeof manifest.sounds==='object'&&!Array.isArray(manifest.sounds),'Invalid sounds object');
 const expected=new Set(['pack.json']);check(Object.keys(manifest.sounds).length>0,'Select at least one sound');
 for(const [key,spec]of Object.entries(manifest.sounds)){
  check(KEYS.includes(key)&&spec&&[`${key}.wav`,`${key}.mp3`].includes(spec.file),'Invalid sound mapping');const mp3=spec.file.endsWith('.mp3');check(spec.mime===(mp3?'audio/mpeg':'audio/wav'),'Invalid audio MIME');const name='sounds/'+spec.file;check(entries.has(name),'Missing audio');expected.add(name);const data=entries.get(name);check(data.length>=12,'Audio file is empty');
  if(mp3)check(new TextDecoder().decode(data.subarray(0,3))==='ID3'||(data[0]===255&&(data[1]&224)===224),'Invalid MP3');else check(new TextDecoder().decode(data.subarray(0,4))==='RIFF'&&new TextDecoder().decode(data.subarray(8,12))==='WAVE','Invalid WAV');
 }
 check(entries.size===expected.size,'Unreferenced files are not allowed');return manifest;
}
async function rate(env,bucket,max,seconds){const now=Date.now();await env.DB.prepare('INSERT INTO limits(bucket,hits,expires_at) VALUES(?,1,?) ON CONFLICT(bucket) DO UPDATE SET hits=CASE WHEN expires_at<? THEN 1 ELSE hits+1 END, expires_at=CASE WHEN expires_at<? THEN excluded.expires_at ELSE expires_at END').bind(bucket,now+seconds*1000,now,now).run();const r=await env.DB.prepare('SELECT hits FROM limits WHERE bucket=?').bind(bucket).first();check(r.hits<=max,'Too many attempts. Please try later.',429);}
async function authenticate(request,env){const token=request.headers.get('authorization')?.replace(/^Bearer /,'')||'';check(/^[a-f0-9]{64}$/.test(token),'Sign in first',401);const user=await env.DB.prepare('SELECT u.id,u.username FROM sessions s JOIN users u ON u.id=s.user_id WHERE s.token_hash=? AND s.expires_at>?').bind(await sha(enc.encode(token)),Date.now()).first();check(user,'Session expired. Sign in again.',401);return user;}
async function session(env,user){const token=hex(crypto.getRandomValues(new Uint8Array(32)));await env.DB.prepare('INSERT INTO sessions(token_hash,user_id,expires_at) VALUES(?,?,?)').bind(await sha(enc.encode(token)),user.id,Date.now()+7*86400000).run();return json({token,user:{username:user.username}});}
async function identity(env,provider,subject,display){
 check(typeof subject==='string'&&subject.length>0&&subject.length<200,'Invalid identity',401);
 let user=await env.DB.prepare('SELECT u.id,u.username FROM identities i JOIN users u ON u.id=i.user_id WHERE i.provider=? AND i.subject=?').bind(provider,subject).first();if(user)return user;
 const base=(display||'user').replace(/[^a-zA-Z0-9_-]/g,'').slice(0,16)||'user';user={id:crypto.randomUUID(),username:base+'_'+hex(crypto.getRandomValues(new Uint8Array(4)))};
 // Existing names/emails are never linked implicitly: provider subject is the identity.
 await env.DB.batch([env.DB.prepare('INSERT INTO users(id,username,normalized,salt,password_hash,created_at) VALUES(?,?,?,?,?,?)').bind(user.id,user.username,user.username.toLowerCase(),'external','external',Date.now()),env.DB.prepare('INSERT INTO identities(provider,subject,user_id) VALUES(?,?,?)').bind(provider,subject,user.id)]);return user;
}
async function providerFetch(env,url,options){return (env.PROVIDER_FETCH||fetch)(url,options);}
export async function handle(request,env){
 const url=new URL(request.url),path=url.pathname,method=request.method;
 if(method==='GET'&&path==='/api/health')return json({service:'SoundSpace Workshop',version:1,storage:'D1',limits:{packBytes:8388608,userBytes:Number(env.MAX_USER_BYTES||67108864)}});
 if(method==='GET'&&path==='/api/auth/providers')return json({github:{enabled:!!env.GITHUB_CLIENT_ID},email:{enabled:!!env.FIREBASE_API_KEY&&!!env.FIREBASE_PROJECT_ID,apiKey:env.FIREBASE_API_KEY||'',projectId:env.FIREBASE_PROJECT_ID||''}});
 if(method==='POST'&&path==='/api/auth/firebase'){
  check(env.FIREBASE_API_KEY&&env.FIREBASE_PROJECT_ID,'Email sign-in is not configured',503);let body;try{body=JSON.parse(new TextDecoder().decode(await boundedBody(request,16384)));}catch{throw new ApiError(400,'Invalid identity request');}
  check(typeof body.idToken==='string'&&body.idToken.length<12000,'Invalid identity token',401);
  // Check project audience before asking Firebase to validate the token and account state.
  let claims;try{const part=body.idToken.split('.')[1].replace(/-/g,'+').replace(/_/g,'/');claims=JSON.parse(atob(part));}catch{throw new ApiError(401,'Invalid identity token');}
  check(claims.aud===env.FIREBASE_PROJECT_ID&&claims.iss==='https://securetoken.google.com/'+env.FIREBASE_PROJECT_ID,'Wrong identity project',401);
  const response=await providerFetch(env,'https://identitytoolkit.googleapis.com/v1/accounts:lookup?key='+encodeURIComponent(env.FIREBASE_API_KEY),{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({idToken:body.idToken})});check(response.ok,'Identity token rejected',401);const result=await response.json(),account=result.users?.[0];check(account&&!account.disabled&&account.localId===claims.sub,'Identity token rejected',401);check(account.emailVerified===true,'Confirm your email address first',403);
  const user=await identity(env,'firebase',account.localId,account.displayName||account.email?.split('@')[0]);return session(env,user);
 }
 if(method==='POST'&&path==='/api/auth/github/start'){
  check(env.GITHUB_CLIENT_ID,'GitHub sign-in is not configured',503);const ip=await sha(enc.encode(request.headers.get('CF-Connecting-IP')||'local'));await rate(env,'github-start:'+ip,20,3600);
  const response=await providerFetch(env,'https://github.com/login/device/code',{method:'POST',headers:{Accept:'application/json','Content-Type':'application/x-www-form-urlencoded'},body:new URLSearchParams({client_id:env.GITHUB_CLIENT_ID,scope:'read:user user:email'})});const data=await response.json();check(response.ok&&data.device_code&&data.user_code&&data.verification_uri==='https://github.com/login/device','GitHub authorization unavailable',502);
  const flow=hex(crypto.getRandomValues(new Uint8Array(32))),interval=Math.max(5,Number(data.interval)||5),expires=Math.min(900,Number(data.expires_in)||900);await env.DB.prepare('DELETE FROM oauth_flows WHERE expires_at<?').bind(Date.now()).run();await env.DB.prepare('INSERT INTO oauth_flows(flow_hash,device_code,expires_at,interval_seconds,next_poll) VALUES(?,?,?,?,?)').bind(await sha(enc.encode(flow)),data.device_code,Date.now()+expires*1000,interval,Date.now()+interval*1000).run();return json({flow,userCode:data.user_code,verificationUri:data.verification_uri,interval,expiresIn:expires});
 }
 if(method==='POST'&&path==='/api/auth/github/poll'){
  check(env.GITHUB_CLIENT_ID,'GitHub sign-in is not configured',503);let body;try{body=JSON.parse(new TextDecoder().decode(await boundedBody(request,1024)));}catch{throw new ApiError(400,'Invalid login flow');}check(/^[a-f0-9]{64}$/.test(body.flow||''),'Invalid login flow',400);const hash=await sha(enc.encode(body.flow)),flow=await env.DB.prepare('SELECT * FROM oauth_flows WHERE flow_hash=?').bind(hash).first();check(flow&&flow.expires_at>Date.now(),'Login flow expired',410);
  if(flow.next_poll>Date.now())return json({pending:true,interval:flow.interval_seconds});await env.DB.prepare('UPDATE oauth_flows SET next_poll=? WHERE flow_hash=?').bind(Date.now()+flow.interval_seconds*1000,hash).run();
  const response=await providerFetch(env,'https://github.com/login/oauth/access_token',{method:'POST',headers:{Accept:'application/json','Content-Type':'application/x-www-form-urlencoded'},body:new URLSearchParams({client_id:env.GITHUB_CLIENT_ID,device_code:flow.device_code,grant_type:'urn:ietf:params:oauth:grant-type:device_code'})});const data=await response.json();
  if(data.error==='authorization_pending')return json({pending:true,interval:flow.interval_seconds});if(data.error==='slow_down'){await env.DB.prepare('UPDATE oauth_flows SET interval_seconds=interval_seconds+5 WHERE flow_hash=?').bind(hash).run();return json({pending:true,interval:flow.interval_seconds+5});}check(response.ok&&data.access_token,'GitHub authorization rejected',401);
  // Consume the flow before issuing a session, so an authorization cannot be replayed.
  const consumed=await env.DB.prepare('DELETE FROM oauth_flows WHERE flow_hash=?').bind(hash).run();check(consumed.meta?.changes===1||consumed.changes===1,'Login flow already used',409);
  const headers={Authorization:'Bearer '+data.access_token,Accept:'application/vnd.github+json','User-Agent':'SoundSpace'};const profileResponse=await providerFetch(env,'https://api.github.com/user',{headers}),emailResponse=await providerFetch(env,'https://api.github.com/user/emails',{headers});check(profileResponse.ok&&emailResponse.ok,'GitHub profile unavailable',401);const profile=await profileResponse.json(),emails=await emailResponse.json();check(Array.isArray(emails)&&emails.some(e=>e.verified===true),'Verify your email in GitHub first',403);return session(env,await identity(env,'github',String(profile.id),profile.login));
 }
 if(method==='GET'&&path==='/api/me/packs'){
  const user=await authenticate(request,env);const result=await env.DB.prepare('SELECT p.id,p.name,p.description,p.category,p.sha256,p.bytes,p.created_at AS createdAt,p.downloads,u.username AS author FROM packs p JOIN users u ON u.id=p.owner_id WHERE p.owner_id=? ORDER BY p.created_at DESC LIMIT 100').bind(user.id).all();return json({packs:result.results});
 }
 const removal=path.match(/^\/api\/packs\/([a-f0-9-]{36})$/);
 if(method==='DELETE'&&removal){
  const user=await authenticate(request,env),pack=await env.DB.prepare('SELECT owner_id FROM packs WHERE id=?').bind(removal[1]).first();check(pack,'Pack not found',404);check(pack.owner_id===user.id,'Only the owner can delete this pack',403);
  await env.DB.batch([env.DB.prepare('DELETE FROM pack_chunks WHERE pack_id=? AND EXISTS(SELECT 1 FROM packs WHERE id=? AND owner_id=?)').bind(removal[1],removal[1],user.id),env.DB.prepare('DELETE FROM packs WHERE id=? AND owner_id=?').bind(removal[1],user.id)]);return json({deleted:true});
 }
 if(method==='POST'&&/^\/api\/auth\/(register|login)$/.test(path)){
  if(path.endsWith('/register'))check(env.ALLOW_LEGACY_REGISTRATION==='true','Use GitHub or email registration',410);
  const ip=await sha(enc.encode(request.headers.get('CF-Connecting-IP')||'local'));await rate(env,'auth:'+ip,60,3600);let body;try{body=JSON.parse(new TextDecoder().decode(await boundedBody(request,4096)));}catch{throw new ApiError(400,'Invalid account request');}
  check(typeof body.username==='string'&&/^[a-zA-Z0-9_-]{3,24}$/.test(body.username),'Username: 3–24 letters, numbers, _ or -');check(typeof body.password==='string'&&body.password.length>=10&&body.password.length<=128,'Password: 10–128 characters');
  const normalized=body.username.toLowerCase();let user=await env.DB.prepare('SELECT * FROM users WHERE normalized=?').bind(normalized).first();
  if(path.endsWith('/register')){await rate(env,'register:'+ip,10,86400);check(!user,'Username is already taken',409);const salt=hex(crypto.getRandomValues(new Uint8Array(16)));user={id:crypto.randomUUID(),username:body.username};await env.DB.prepare('INSERT INTO users(id,username,normalized,salt,password_hash,created_at) VALUES(?,?,?,?,?,?)').bind(user.id,user.username,normalized,salt,await passwordHash(body.password,salt),Date.now()).run();}
  else{const salt=user?.salt||'00000000000000000000000000000000',hash=await passwordHash(body.password,salt);check(user&&equal(hash,user.password_hash),'Invalid username or password',401);}
  return session(env,user);
 }
 if(method==='GET'&&path==='/api/packs'){
  const q=(url.searchParams.get('q')||'').slice(0,80),category=url.searchParams.get('category')||'All',sort=url.searchParams.get('sort')==='popular'?'popular':'new';const offset=Number(url.searchParams.get('cursor')||0);check(Number.isInteger(offset)&&offset>=0&&offset<=100000,'Invalid cursor');
  const conditions=[],values=[];if(q){conditions.push('(p.name LIKE ? ESCAPE \'\\\' OR u.username LIKE ? ESCAPE \'\\\')');const term='%'+q.replace(/[\\%_]/g,'\\$&')+'%';values.push(term,term);}if(category!=='All'){check(CATEGORIES.includes(category),'Invalid category');conditions.push('p.category=?');values.push(category);}
  const where=conditions.length?' WHERE '+conditions.join(' AND '):'',order=sort==='popular'?'p.downloads DESC,p.created_at DESC':'p.created_at DESC';const rows=await env.DB.prepare('SELECT p.id,p.name,p.description,p.category,p.sha256,p.bytes,p.created_at AS createdAt,p.downloads,u.username AS author FROM packs p JOIN users u ON u.id=p.owner_id'+where+' ORDER BY '+order+' LIMIT 19 OFFSET ?').bind(...values,offset).all();return json({packs:rows.results.slice(0,18),nextCursor:rows.results.length>18?String(offset+18):''});
 }
 if(method==='POST'&&path==='/api/packs'){
  const user=await authenticate(request,env);const verified=await env.DB.prepare('SELECT user_id FROM identities WHERE user_id=? LIMIT 1').bind(user.id).first();check(verified,'Use GitHub or a verified email to publish',403);await rate(env,'upload:'+user.id,5,86400);const name=(url.searchParams.get('name')||'').trim(),description=(url.searchParams.get('description')||'').trim(),category=url.searchParams.get('category')||'Other';check(name.length>=1&&name.length<=80&&description.length<=1000&&CATEGORIES.includes(category),'Invalid pack details');
  const bytes=await boundedBody(request,8*1024*1024);await validatePack(bytes);const own=await env.DB.prepare('SELECT COALESCE(SUM(bytes),0) AS n FROM packs WHERE owner_id=?').bind(user.id).first(),total=await env.DB.prepare('SELECT COALESCE(SUM(bytes),0) AS n FROM packs').first();check(own.n+bytes.length<=Number(env.MAX_USER_BYTES||67108864),'Your storage quota is full',413);check(total.n+bytes.length<=Number(env.MAX_STORAGE_BYTES||536870912),'Workshop storage is full',507);
  const id=crypto.randomUUID(),digest=await sha(bytes),statements=[env.DB.prepare('INSERT INTO packs(id,owner_id,name,description,category,sha256,bytes,created_at) VALUES(?,?,?,?,?,?,?,?)').bind(id,user.id,name,description,category,digest,bytes.length,Date.now())];
  for(let offset=0,part=0;offset<bytes.length;offset+=262144,part++)statements.push(env.DB.prepare('INSERT INTO pack_chunks(pack_id,part,data) VALUES(?,?,?)').bind(id,part,bytes.slice(offset,offset+262144).buffer));await env.DB.batch(statements);return json({id,name,author:user.username,sha256:digest},201);
 }
 const download=path.match(/^\/api\/packs\/([a-f0-9-]{36})\/download$/);
 if(method==='GET'&&download){const id=download[1],pack=await env.DB.prepare('SELECT * FROM packs WHERE id=?').bind(id).first();check(pack,'Pack not found',404);const rows=await env.DB.prepare('SELECT data FROM pack_chunks WHERE pack_id=? ORDER BY part').bind(id).all();const bytes=new Uint8Array(pack.bytes);let offset=0;for(const row of rows.results){const data=row.data instanceof ArrayBuffer?new Uint8Array(row.data):new Uint8Array(row.data);bytes.set(data,offset);offset+=data.length;}check(offset===pack.bytes,'Incomplete pack storage',500);await env.DB.prepare('UPDATE packs SET downloads=downloads+1 WHERE id=?').bind(id).run();return new Response(bytes,{headers:{'Content-Type':'application/octet-stream','Content-Disposition':'attachment; filename="'+id+'.dspack"','Content-Length':String(bytes.length),'X-Content-Type-Options':'nosniff','Cache-Control':'public,max-age=300'}});}
 return json({error:'Not found'},404);
}
export default {async fetch(request,env){try{return await handle(request,env);}catch(error){return json({error:error instanceof ApiError?error.message:'Internal server error'},error instanceof ApiError?error.status:500);}}};
