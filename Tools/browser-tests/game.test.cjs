const {chromium}=require('playwright');
const fs=require('fs'),assert=require('assert');
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--enable-webgl','--ignore-gpu-blocklist']});
 const page=await browser.newPage({viewport:{width:540,height:960}});
 const errors=[],http=[];
 page.on('pageerror',e=>errors.push(e.message));
 page.on('response',r=>{if(r.status()>=400)http.push({url:r.url(),status:r.status()});});
 const root=process.env.SOCK_URL||'http://127.0.0.1:8765';
 const output='Builds/BrowserQA';fs.mkdirSync(output,{recursive:true});fs.writeFileSync(output+'/report.json',JSON.stringify({result:'RUNNING',startedAt:new Date().toISOString()}));
 async function loaded(){await page.waitForFunction(()=>window.sockUnity&&document.querySelector('#loading').style.display==='none',null,{timeout:60000});await page.waitForTimeout(400);}
 async function state(){return page.evaluate(()=>JSON.parse(localStorage.getItem('sock-pairs-save-v1')));}
 const start=Date.now();await page.goto(root);await loaded();const loadMs=Date.now()-start;
 await page.screenshot({path:output+'/map.png'});
 await page.mouse.click(270,885);await page.waitForTimeout(3600);
 let save=await state();assert.equal(save.active.level,1);assert.equal(save.active.moves,0,"Tutorial must guide, not make automatic moves");
 await page.screenshot({path:output+'/game.png'});
 const designs=[0,7,2,9,4,11];
 for(const design of designs){
  save=await state();if(!save.active)break;
  const pair=save.active.socks.filter(s=>s.design===design&&s.place!==3);
  for(const sock of pair){const x=270+(sock.slot%4-1.5)*117.5,y=480-(435-Math.floor(sock.slot/4)*350)/2;await page.mouse.click(x,y);await page.waitForTimeout(130);}
 }
 await page.waitForTimeout(1700);save=await state();assert.equal(save.unlocked,2);assert(save.stars[0]>=1);assert(!save.active||save.active.level===0);
 await page.screenshot({path:output+'/win.png'});
 await page.mouse.click(270,705);await page.waitForTimeout(850);assert.equal((await state()).active.level,2);
 await page.mouse.click(485,55);await page.waitForTimeout(350);await page.screenshot({path:output+'/pause.png'});
 await page.mouse.click(270,718);await page.waitForTimeout(350);await page.reload();await loaded();
 await page.mouse.click(270,885);await page.waitForTimeout(850);assert.equal((await state()).active.level,2);
 // A separate test save unlocks the map so scrolling to its end can be exercised.
 await page.addInitScript(()=>{const s=JSON.parse(localStorage.getItem('sock-pairs-save-v1'));if(s){s.unlocked=100;s.active=null;localStorage.setItem('sock-pairs-save-v1',JSON.stringify(s));}});
 await page.reload();await loaded();assert.equal((await state()).unlocked,100);const beforeScroll=await page.screenshot({path:output+'/map-100.png'});
 await page.mouse.move(270,450);await page.mouse.wheel(0,200);await page.waitForTimeout(750);const afterScroll=await page.screenshot({path:output+'/map-scrolled.png'});assert(!beforeScroll.equals(afterScroll),'The level map must actually scroll');
 await page.setViewportSize({width:960,height:540});await page.waitForTimeout(500);await page.screenshot({path:output+'/landscape.png'});
 const heap=await page.evaluate(()=>window.sockUnity?.Module?.HEAPU8?.buffer?.byteLength||null);
 const report={result:'PASS',testedAt:new Date().toISOString(),loadMs,wasmHeapBytes:heap,errors,http,checks:['actual pointer matching','tutorial flow','level win','star/progression save','next level','pause/home','reload/resume','100-level map wheel input','landscape resize']};
 assert.equal(errors.length,0,errors.join('\n'));assert.equal(http.length,0,JSON.stringify(http));fs.writeFileSync(output+'/report.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));await browser.close();
})().catch(e=>{fs.writeFileSync('Builds/BrowserQA/report.json',JSON.stringify({result:'FAIL',testedAt:new Date().toISOString(),error:String(e)},null,2));console.error(e);process.exit(1)});
