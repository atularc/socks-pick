const {chromium}=require('playwright');
const fs=require('fs'),assert=require('assert');
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--enable-webgl','--ignore-gpu-blocklist']});
 const page=await browser.newPage({viewport:{width:540,height:960}});
 const errors=[],http=[];
 page.on('pageerror',e=>errors.push(e.message));
 page.on('response',r=>{if(r.status()>=400)http.push({url:r.url(),status:r.status()});});
 const root=process.env.SOCK_URL||'http://127.0.0.1:8765';
 const output=process.env.SOCK_QA_OUTPUT||'Documentation/Reference-B-Correction/Review/BrowserQA';fs.mkdirSync(output,{recursive:true});fs.writeFileSync(output+'/report.json',JSON.stringify({result:'RUNNING',startedAt:new Date().toISOString()}));
 async function loaded(){await page.waitForFunction(()=>window.sockUnity&&document.querySelector('#loading').style.display==='none',null,{timeout:60000});await page.waitForTimeout(400);}
 async function state(){return page.evaluate(()=>JSON.parse(localStorage.getItem('sock-pairs-save-v1')));}
 let releaseLoader;const loaderGate=new Promise(resolve=>releaseLoader=resolve);
 await page.route('**/*.loader.js',async route=>{await loaderGate;await route.continue();});
 await page.goto(root,{waitUntil:'domcontentloaded'});await page.screenshot({path:output+'/loading-browser.png'});
 const start=Date.now();releaseLoader();await loaded();const loadMs=Date.now()-start;
 const lockedMap=await page.screenshot({path:output+'/map.png'});
 await page.mouse.click(248,642);await page.waitForTimeout(300);
 assert(!(await state())?.active?.level,'Locked map nodes must not start levels in a release build');
 assert(lockedMap.equals(await page.screenshot()),'Clicking a locked level must leave the map visible and unchanged');
 await page.mouse.click(270,861);await page.waitForTimeout(400);await page.screenshot({path:output+'/album.png'});
 assert(!lockedMap.equals(await page.screenshot()),'Album button must open a different screen');
 for(let i=1;i<4;i++){await page.mouse.click(420,775);await page.waitForTimeout(350);await page.screenshot({path:output+'/album-page-'+(i+1)+'.png'});}
 await page.mouse.click(420,250);await page.waitForTimeout(400);await page.screenshot({path:output+'/after-album-back.png'}); // The subsequent Play click verifies that Close restored map navigation.
 await page.mouse.click(270,772);await page.waitForTimeout(3600);
 let save=await state();assert.equal(save.active.level,1);assert.equal(save.active.moves,0,"Tutorial must guide, not make automatic moves");
 await page.screenshot({path:output+'/game.png'});
 const cells=[...new Set(save.active.socks.map(s=>s.slot))].sort((a,b)=>a-b); const designs=[...new Set(save.active.socks.map(s=>s.design))];
 for(const design of designs){
  save=await state();if(!save.active)break;
  const pair=save.active.socks.filter(s=>s.design===design&&s.place!==3);
  for(const sock of pair){const cell=cells.indexOf(sock.slot),rows=Math.ceil(cells.length/4),top=15+(rows-1)*295/2; const x=270+(cell%4-1.5)*112.5,y=480-(top-Math.floor(cell/4)*295)/2;await page.mouse.click(x,y);await page.waitForTimeout(130);}
 }
 await page.waitForTimeout(2700);save=await state();assert.equal(save.unlocked,2);assert(save.stars[0]>=1);assert(!save.active||save.active.level===0);
 await page.screenshot({path:output+'/win.png'});
 await page.mouse.click(270,710);await page.waitForTimeout(850);assert.equal((await state()).active.level,2);
 await page.screenshot({path:output+'/before-pause.png'});
 await page.mouse.click(461,101);await page.waitForTimeout(350);await page.screenshot({path:output+'/pause.png'});
 await page.mouse.click(120,307);await page.waitForTimeout(350);const beforeMusic=(await state()).music;await page.mouse.click(405,445);await page.waitForTimeout(150);assert.notEqual((await state()).music,beforeMusic);await page.mouse.click(405,445);await page.waitForTimeout(150);assert.equal((await state()).music,beforeMusic);await page.screenshot({path:output+'/settings.png'});await page.mouse.click(270,705);await page.waitForTimeout(350);await page.mouse.click(461,101);await page.waitForTimeout(350);
 await page.mouse.click(270,718);await page.waitForTimeout(350);await page.reload();await loaded();
 await page.mouse.click(270,772);await page.waitForTimeout(850);assert.equal((await state()).active.level,2);
 // A separate test save unlocks the map so scrolling to its end can be exercised.
 await page.addInitScript(()=>{const s=JSON.parse(localStorage.getItem('sock-pairs-save-v1'));if(s){s.unlocked=100;s.active=null;localStorage.setItem('sock-pairs-save-v1',JSON.stringify(s));}});
 await page.reload();await loaded();assert.equal((await state()).unlocked,100);const beforeScroll=await page.screenshot({path:output+'/map-100.png'});
 await page.mouse.move(270,450);await page.mouse.wheel(0,-400);await page.waitForTimeout(750);const afterScroll=await page.screenshot({path:output+'/map-scrolled.png'});assert(!beforeScroll.equals(afterScroll),'The level map must actually scroll');
 await page.setViewportSize({width:960,height:540});await page.waitForTimeout(500);await page.screenshot({path:output+'/landscape.png'});
 for(const size of [{width:360,height:640},{width:540,height:1170},{width:768,height:1024}]){await page.setViewportSize(size);await page.waitForTimeout(500);await page.screenshot({path:output+'/map-'+size.width+'x'+size.height+'.png'});}
 const heap=await page.evaluate(()=>window.sockUnity?.Module?.HEAPU8?.buffer?.byteLength||null);
 const report={result:'PASS',testedAt:new Date().toISOString(),loadMs,wasmHeapBytes:heap,errors,http,checks:['album/back pointer input','settings via pause and live toggles','locked release-level input','actual pointer matching','tutorial flow','level win','star/progression save','next level','pause/home','reload/resume','100-level map wheel input','landscape resize','small phone, tall phone and tablet resize']};
 assert.equal(errors.length,0,errors.join('\n'));assert.equal(http.length,0,JSON.stringify(http));fs.writeFileSync(output+'/report.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));await browser.close();
})().catch(e=>{fs.writeFileSync((process.env.SOCK_QA_OUTPUT||'Documentation/Reference-B-Correction/Review/BrowserQA')+'/report.json',JSON.stringify({result:'FAIL',testedAt:new Date().toISOString(),error:String(e)},null,2));console.error(e);process.exit(1)});

