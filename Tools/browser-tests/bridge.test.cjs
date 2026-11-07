const fs=require('fs'),vm=require('vm'),assert=require('assert');
const sent=[],writes=[],events={};let finishLoad,finishSave;
const context={LibraryManager:{library:{}},mergeInto:Object.assign,UTF8ToString:x=>x,
 window:{},SendMessage:(name,method,value)=>sent.push({name,method,value}),
 document:{getElementById:()=>({style:{}}),addEventListener:()=>{}},
 Browser:{mainLoop:{pause:()=>events.paused=true,resume:()=>events.paused=false}},
 ytgame:{IN_PLAYABLES_ENV:true,system:{isAudioEnabled:()=>false,onAudioEnabledChange:f=>events.audio=f,onPause:f=>events.pause=f,onResume:f=>events.resume=f},
 game:{loadData:()=>new Promise(r=>finishLoad=r),saveData:data=>{writes.push(data);return new Promise(r=>finishSave=r);},gameReady:()=>events.ready=true},
 ads:{requestRewardedAd:async id=>id==='earned'}}};
vm.createContext(context);vm.runInContext(fs.readFileSync('Assets/Plugins/WebGL/SockPlatform.jslib','utf8'),context);
const bridge=context.LibraryManager.library;
(async()=>{
 bridge.SP_Init('SockPairs');bridge.SP_Save('too early');assert.equal(writes.length,0);
 finishLoad('{"coins":7}');await new Promise(setImmediate);
 assert(sent.some(s=>s.method==='SaveLoaded'&&s.value==='{"coins":7}'));
 assert(sent.some(s=>s.method==='HostAudio'&&s.value==='0'));
 bridge.SP_Save('first');bridge.SP_Save('second');bridge.SP_Save('latest');assert.deepEqual(writes,['first']);
 finishSave();await new Promise(setImmediate);assert.deepEqual(writes,['first','latest']);finishSave();
 events.pause();assert(events.paused);events.resume();assert(!events.paused);
 bridge.SP_Ready();assert(events.ready);
 bridge.SP_Reward('earned');await new Promise(setImmediate);assert.equal(sent.at(-1).value,'1');
 bridge.SP_Reward('not-earned');await new Promise(setImmediate);assert.equal(sent.at(-1).value,'0');
 context.ytgame.ads.requestRewardedAd=async()=>({earned:false});
 bridge.SP_Reward('invalid-result');await new Promise(setImmediate);assert.equal(sent.at(-1).value,'0');
 context.ytgame.ads.requestRewardedAd=()=>{throw new Error('No inventory')};
 bridge.SP_Reward('throws');await new Promise(setImmediate);assert.equal(sent.at(-1).value,'0');
 const local={};context.localStorage={getItem:key=>local[key]||null,setItem:(key,value)=>local[key]=value};
 context.ytgame.IN_PLAYABLES_ENV=false;sent.length=0;bridge.SP_Init('SockPairs');
 assert(sent.some(s=>s.method==='SetYouTube'&&s.value==='0'));
 assert(sent.some(s=>s.method==='SetRewardedAdsAvailable'&&s.value==='0'));
 bridge.SP_Save('browser-only');assert.equal(local['sock-pairs-save-v1'],'browser-only');
 console.log('PASS: web local save/no rewarded ads; invalid and throwing reward requests rejected; host load-before-save, coalesced save ordering, audio, pause/resume, readiness and earned-only rewards.');
})().catch(e=>{console.error(e);process.exit(1)});
