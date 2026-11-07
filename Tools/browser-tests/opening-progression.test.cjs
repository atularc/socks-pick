const {chromium}=require('playwright');
const fs=require('fs'),assert=require('assert');
const output=process.env.SOCK_QA_OUTPUT||'Documentation/Opening-Progression/Browser';
fs.mkdirSync(output,{recursive:true});
const fixtures=JSON.parse(fs.readFileSync('Documentation/Opening-Progression/layout-fixtures.json'));
const key='sock-pairs-save-v1';
(async()=>{
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--enable-webgl','--ignore-gpu-blocklist']});
 try {
 const page=await browser.newPage({viewport:{width:540,height:960}}),errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{const injected=localStorage.getItem('__testSave');if(injected){localStorage.setItem('sock-pairs-save-v1',injected);localStorage.removeItem('__testSave');}if(!localStorage.getItem('sock-pairs-save-v1'))localStorage.setItem('sock-pairs-save-v1',JSON.stringify({version:1,unlocked:1,coins:150,reduceMotion:true,sound:false,music:false}));});
 async function loaded(){await page.waitForFunction(()=>window.sockUnity&&document.querySelector('#loading').style.display==='none',null,{timeout:60000});await page.waitForTimeout(500);}
 async function state(){return page.evaluate(k=>JSON.parse(localStorage.getItem(k)),key);}
 async function click(x,y,delay=200){await page.mouse.click(x,y);await page.waitForTimeout(delay);}
 async function shot(name){await page.screenshot({path:output+'/'+name+'.png'});}
 async function inject(save){await page.evaluate(save=>localStorage.setItem('__testSave',JSON.stringify(save)),save);await page.reload();await loaded();}
 function point(sock,active,c){
  let x,y;
  if(sock.place===2){const i=active.basket.indexOf(sock.id),width=224*1524/793;x=width*(.16+.68*i/(active.capacity-1)-.5);y=-505+33.6;}
  else if(c.basket===0){const slots=[...new Set(active.socks.map(s=>s.slot))].sort((a,b)=>a-b),cell=slots.indexOf(sock.slot),rows=Math.ceil(c.pairs*2/4);x=(cell%4-1.5)*225;y=15+(rows-1)*295/2-Math.floor(cell/4)*295;}
  else{x=(sock.slot%6-2.5)*136+(sock.depth%2===0?-1:1)*sock.depth*3;y=(540-Math.floor(sock.slot/6)*150+sock.depth*6)*.68+31;}
  return [270+x/2,480-y/2];
 }
 await page.goto(process.env.SOCK_URL||'http://127.0.0.1:8871');await loaded();assert.equal((await state()).active.level,1,'New player opens level 1 without a Play tap');
 let oldLayoutSave,currentSave;const completed=[];
 for(let n=1;n<=10;n++){
  const f=fixtures[n-1];let s=await state();assert.equal(s.active.level,n);assert.equal(s.active.moves,0);assert.equal(s.active.levelRevision,1);
  if(n===3||n===4){await shot('level-'+n+'-tutorial');await click(270,662.5,350);assert.equal((await state()).active.moves,0);}
  await shot('level-'+n+'-start');
  if(n===3)oldLayoutSave=structuredClone(await state());
  for(const id of f.solution){
   s=await state();const a=s.active,sock=a.socks[id];if(sock.place===3||a.selected===id)continue;
   const finalPair=a.matched===f.config.pairs-1&&a.selected>=0&&a.socks[a.selected].design===sock.design;
   const xy=point(sock,a,f.config);await click(...xy);
   const after=await state();if(!finalPair)assert(after.active.moves>a.moves,`Level ${n}, sock ${id}: pointer did not perform a move at ${xy}`);assert(!after.active.lost,`Level ${n}: lost`);
   if(n===3&&after.active.matched===1)await shot('level-3-reveal');
   if(n===4&&after.active.basket.length){currentSave=structuredClone(after);await shot('level-4-holding-sock');}
  }
  await page.waitForTimeout(1200);s=await state();assert.equal(s.unlocked,n+1);assert(s.stars[n-1]>=1);assert(!s.active||!s.active.level);await shot('level-'+n+'-complete');completed.push(n);
  if(n<10)await click(270,710,700);
 }
 // Old layouts restart the active round while retaining campaign rewards.
 const campaign=await state();oldLayoutSave={...campaign,active:oldLayoutSave.active};oldLayoutSave.active.levelRevision=0;oldLayoutSave.active.moves=17;
 await inject(oldLayoutSave);let resumed=await state();assert.equal(resumed.active.level,3);assert.equal(resumed.active.levelRevision,1);assert.equal(resumed.active.moves,0);assert.equal(resumed.unlocked,campaign.unlocked);assert.equal(resumed.coins,campaign.coins);assert.deepEqual(resumed.stars,campaign.stars);assert.deepEqual(resumed.collection,campaign.collection);await shot('old-layout-safe-restart');
 // Matching revisions preserve the selected sock and occupied basket on reload.
 await inject(currentSave);resumed=await state();
 // Resume must persist immediately, before the eight-second autosave tick.
 assert.equal(resumed.active.moves,currentSave.active.moves);assert.equal(resumed.active.selected,currentSave.active.selected);assert.deepEqual(resumed.active.basket,currentSave.active.basket);await shot('current-layout-resumed');
 assert.equal(errors.length,0,errors.join('\n'));
 const report={result:'PASS',testedAt:new Date().toISOString(),completedLevels:completed,checks:['actual pointer completion','layer reveal','basket holding and retrieval','tutorial timing','stars and level unlocks','legacy active layout restart preserves rewards','current layout resume preserves basket'],errors};
 fs.writeFileSync(output+'/report.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
 }finally{await browser.close();}
})().catch(e=>{fs.writeFileSync(output+'/report.json',JSON.stringify({result:'FAIL',error:String(e)},null,2));console.error(e);process.exit(1)});
