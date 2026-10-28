import test from 'node:test';
import assert from 'node:assert/strict';
import {DatabaseSync} from 'node:sqlite';
import {readFileSync} from 'node:fs';
import worker,{validatePack,sha} from './worker.mjs';
function environment(){const sqlite=new DatabaseSync(':memory:');for(const file of ['0001_initial.sql','0002_identity.sql'])sqlite.exec(readFileSync(new URL('./migrations/'+file,import.meta.url),'utf8'));const convert=x=>x instanceof ArrayBuffer?new Uint8Array(x):x;const DB={prepare(sql){let args=[];return{bind(...values){args=values.map(convert);return this;},async run(){return sqlite.prepare(sql).run(...args);},async first(){return sqlite.prepare(sql).get(...args)||null;},async all(){return{results:sqlite.prepare(sql).all(...args)};}};},async batch(statements){sqlite.exec('BEGIN');try{for(const s of statements)await s.run();sqlite.exec('COMMIT');}catch(e){sqlite.exec('ROLLBACK');throw e;}}};return{DB,MAX_USER_BYTES:'67108864',MAX_STORAGE_BYTES:'536870912'};}
test('real pack validation and malformed archive rejection',async()=>{const bytes=readFileSync(new URL('../presets/teamspeak-female.dspack',import.meta.url));const manifest=await validatePack(bytes);assert.equal(Object.keys(manifest.sounds).length,7);await assert.rejects(validatePack(new Uint8Array(100)));const corrupt=Buffer.from(bytes);corrupt[70]^=255;await assert.rejects(validatePack(corrupt));});
test('multi-user accounts, upload, search, sorting and byte-exact downloads',async()=>{
 const env=environment();env.ALLOW_LEGACY_REGISTRATION='true';const call=(path,options={})=>worker.fetch(new Request('https://workshop.test'+path,options),env);
 const account=async name=>{const response=await call('/api/auth/register',{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify({username:name,password:'correct-test-password'})});assert.equal(response.status,200,await response.clone().text());return response.json();};
 const alice=await account('alice'),bob=await account('bob');assert.notEqual(alice.token,bob.token);
 let denied=await call('/api/packs?name=Unverified&category=Classic',{method:'POST',headers:{authorization:'Bearer '+alice.token},body:'x'});assert.equal(denied.status,403);
 for(const name of ['alice','bob']){const user=await env.DB.prepare('SELECT id FROM users WHERE username=?').bind(name).first();await env.DB.prepare('INSERT INTO identities(provider,subject,user_id) VALUES(?,?,?)').bind('fixture',name,user.id).run();}
 let response=await call('/api/auth/login',{method:'POST',body:JSON.stringify({username:'alice',password:'incorrect-password'})});assert.equal(response.status,401);
 response=await call('/api/packs?name=Test&category=Classic',{method:'POST',body:'x'});assert.equal(response.status,401);
 const bytes=readFileSync(new URL('../presets/teamspeak-female.dspack',import.meta.url));
 response=await call('/api/packs?name=Alice%20Classic&category=Classic&description=My%20sounds',{method:'POST',headers:{authorization:'Bearer '+alice.token},body:bytes});assert.equal(response.status,201,await response.clone().text());const uploaded=await response.json();assert.equal(uploaded.author,'alice');
 response=await call('/api/packs?name=Bob%20Minimal&category=Minimal',{method:'POST',headers:{authorization:'Bearer '+bob.token},body:bytes});assert.equal(response.status,201,await response.clone().text());
 let result=await(await call('/api/packs')).json();assert.equal(result.packs.length,2);
 result=await(await call('/api/packs?q=alice')).json();assert.equal(result.packs.length,1);assert.equal(result.packs[0].author,'alice');
 result=await(await call('/api/packs?category=Minimal')).json();assert.equal(result.packs.length,1);assert.equal(result.packs[0].author,'bob');
 response=await call('/api/packs/'+uploaded.id+'/download');assert.equal(response.status,200);const downloaded=new Uint8Array(await response.arrayBuffer());assert.equal(await sha(downloaded),await sha(bytes));
 result=await(await call('/api/packs?sort=popular')).json();assert.equal(result.packs[0].id,uploaded.id);assert.equal(result.packs[0].downloads,1);
 response=await call('/api/packs?name=Invalid&category=Classic',{method:'POST',headers:{authorization:'Bearer '+alice.token},body:new Uint8Array(24)});assert.equal(response.status,400);
 response=await call('/api/me/packs',{headers:{authorization:'Bearer '+alice.token}});assert.equal((await response.json()).packs.length,1);
 response=await call('/api/packs/'+uploaded.id,{method:'DELETE',headers:{authorization:'Bearer '+bob.token}});assert.equal(response.status,403);
 response=await call('/api/packs/'+uploaded.id,{method:'DELETE'});assert.equal(response.status,401);
 response=await call('/api/packs/'+uploaded.id,{method:'DELETE',headers:{authorization:'Bearer '+alice.token}});assert.equal(response.status,200);
 response=await call('/api/packs/'+uploaded.id+'/download');assert.equal(response.status,404);
 result=await(await call('/api/packs?q=alice')).json();assert.equal(result.packs.length,0);
 assert.equal((await env.DB.prepare('SELECT COUNT(*) AS n FROM pack_chunks WHERE pack_id=?').bind(uploaded.id).first()).n,0);
});
test('GitHub device sign-in verifies identity, email and replay protection',async()=>{
 const env=environment();env.GITHUB_CLIENT_ID='test-client';let verified=true;
 env.PROVIDER_FETCH=async url=>{if(url.endsWith('/login/device/code'))return Response.json({device_code:'private-device-code',user_code:'TEST-CODE',verification_uri:'https://github.com/login/device',interval:5,expires_in:900});if(url.endsWith('/login/oauth/access_token'))return Response.json({access_token:'provider-access'});if(url.endsWith('/user/emails'))return Response.json([{email:'user@example.test',verified}]);if(url.endsWith('/user'))return Response.json({id:123,login:'GitHubUser'});throw Error('Unexpected URL');};
 const call=(p,body)=>worker.fetch(new Request('https://test'+p,{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify(body||{})}),env);
 let response=await call('/api/auth/github/start');const flow=await response.json();assert.equal(flow.userCode,'TEST-CODE');assert.equal(flow.device_code,undefined);
 response=await call('/api/auth/github/poll',{flow:flow.flow});assert.equal((await response.json()).pending,true);
 await env.DB.prepare('UPDATE oauth_flows SET next_poll=0').run();response=await call('/api/auth/github/poll',{flow:flow.flow});const login=await response.json();assert.equal(response.status,200);assert.ok(login.token);assert.match(login.user.username,/^GitHubUser_/);
 response=await call('/api/auth/github/poll',{flow:flow.flow});assert.equal(response.status,410);
 verified=false;const next=await(await call('/api/auth/github/start')).json();await env.DB.prepare('UPDATE oauth_flows SET next_poll=0').run();response=await call('/api/auth/github/poll',{flow:next.flow});assert.equal(response.status,403);
});
test('Firebase sign-in rejects wrong projects and unverified email',async()=>{
 const env=environment();env.FIREBASE_API_KEY='public-test-key';env.FIREBASE_PROJECT_ID='test-project';let verified=false;
 env.PROVIDER_FETCH=async()=>Response.json({users:[{localId:'firebase-user-1',email:'alice@example.test',emailVerified:verified}]});
 const token=project=>'header.'+Buffer.from(JSON.stringify({aud:project,iss:'https://securetoken.google.com/'+project,sub:'firebase-user-1'})).toString('base64url')+'.signature';
 const call=idToken=>worker.fetch(new Request('https://test/api/auth/firebase',{method:'POST',body:JSON.stringify({idToken})}),env);
 let response=await call(token('another-project'));assert.equal(response.status,401);
 response=await call(token('test-project'));assert.equal(response.status,403);
 verified=true;response=await call(token('test-project'));assert.equal(response.status,200);assert.ok((await response.json()).token);
});
test('legacy registration is disabled by default',async()=>{const response=await worker.fetch(new Request('https://test/api/auth/register',{method:'POST',body:JSON.stringify({username:'new_user',password:'long-test-password'})}),environment());assert.equal(response.status,410);});
