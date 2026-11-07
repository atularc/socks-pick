const {chromium}=require('playwright'),assert=require('assert'),fs=require('fs');
(async()=>{const browser=await chromium.launch({channel:'chrome',headless:true,args:['--enable-webgl','--ignore-gpu-blocklist']});fs.mkdirSync('Builds/BrowserQA',{recursive:true});
const initial={version:1,unlocked:12,coins:150,stars:Array(100).fill(0),collection:Array(24).fill(false),sound:false,music:false,haptics:false,reduceMotion:true};
for(const target of ['Web','YouTube']){
 const page=await browser.newPage({viewport:{width:390,height:844}});let errors=[];page.on('pageerror',e=>errors.push(e.message));
 if(target==='Web')await page.addInitScript(data=>localStorage.setItem('sock-pairs-save-v1',JSON.stringify(data)),initial);
 else await page.route('https://www.youtube.com/game_api/v1',route=>route.fulfill({contentType:'application/javascript',body:`window.hostSave=${JSON.stringify(JSON.stringify(initial))};window.hostEvents={};window.ytgame={IN_PLAYABLES_ENV:true,system:{isAudioEnabled:()=>false,onAudioEnabledChange:f=>hostEvents.audio=f,onPause:f=>hostEvents.pause=f,onResume:f=>hostEvents.resume=f},game:{firstFrameReady:()=>{},loadData:async()=>hostSave,saveData:async x=>{hostSave=x},gameReady:()=>{hostEvents.ready=true}},ads:{requestRewardedAd:async()=>false}};`}));
 await page.goto('http://127.0.0.1:8768/'+target+'/');await page.waitForFunction(()=>window.sockUnity&&document.querySelector('#loading').style.display==='none',null,{timeout:90000});await page.waitForTimeout(400);
 const before=await page.screenshot();await page.mouse.click(357,40);await page.mouse.move(0,0);await page.waitForTimeout(250);assert(before.equals(await page.screenshot()),target+' coin plus area must not open a store');
 await page.evaluate(()=>sockUnity.SendMessage('SockPairs','Shop'));await page.waitForTimeout(200);assert(before.equals(await page.screenshot()),target+' Shop entry point must be blocked');await page.screenshot({path:`Builds/BrowserQA/${target}-no-shop.png`});
 await page.mouse.click(137,795);await page.waitForTimeout(600);await page.mouse.click(195,806);await page.waitForTimeout(300);await page.screenshot({path:`Builds/BrowserQA/${target}-helper.png`});
 if(target==='YouTube'){await page.mouse.click(195,559);await page.waitForTimeout(500);const saved=await page.evaluate(()=>JSON.parse(hostSave));assert.equal(saved.coins,150,'Failed ad must not charge coins');assert.equal(saved.hints||0,0,'Failed ad must not grant a helper');}
 await page.mouse.click(195,496);await page.waitForTimeout(500);const saved=await page.evaluate(t=>JSON.parse(t==='Web'?localStorage.getItem('sock-pairs-save-v1'):hostSave),target);assert.equal(saved.coins,100,'Coin helper must spend exactly 50 coins');
 assert.deepEqual(errors,[]);console.log('PASS',target,'shop inaccessible, coin shortcut removed, coin helper works, no page errors');await page.close();}
 await browser.close();})().catch(e=>{console.error(e);process.exit(1)});
