"""Package the latest actual runtime captures and validated local browser builds."""
from pathlib import Path
import html, json, shutil, zipfile

root=Path(__file__).resolve().parent.parent
qa=root/'.gui-build-workspace/Builds/Windows/QA'
out=root/'Documentation/Reference-B-Correction/Review'
(out/'screens').mkdir(parents=True,exist_ok=True)
groups={
 'Main screens': [('map-reference-b.png','Reference B map — level 3'),('home.png','Level map — next day'),('map-progress.png','Level map — progress'),('level-01.png','Day 1 — compact matching grid'),('level-11.png','Day 11 — waiting basket'),('level-100.png','Day 100 — advanced board'),('album.png','Sock album'),('album-detail.png','Album detail')],
 'Basket and selection': [('selected-sock.png','Selected sock'),('Basket/540x960.png','One pair — centred inside the basket'),('Basket/five-slots.png','Five pairs — evenly spaced')],
 'Popups': [('pause.png','Pause'),('settings.png','Settings'),('win.png','Level complete'),('lose.png','Basket full'),('helper.png','Extra slot'),('helper-hint.png','Hint'),('helper-shuffle.png','Shuffle')],
 'Tutorials': [('tutorial.png','First match')]+[(f'tutorial-{i}.png',f'Chapter {(i-1)//10+1}') for i in range(11,100,10)],
 'Screen sizes': [('Readability-540x1170/audit-001.png','Tall phone — day 1'),('Readability-360x640/audit-005.png','Small phone — day 5'),('Readability-768x1024/audit-010.png','Tablet — day 10'),('Layout/540x1170.png','Tall phone — map')],
 'Native only': [('shop.png','Native shop — hidden in Web and YouTube builds')]
}
sections=[]
for group,entries in groups.items():
    cards=[]
    for src,title in entries:
        target=src.replace('/','-')
        shutil.copy2(qa/src,out/'screens'/target)
        cards.append(f'<figure><a href="screens/{target}" target="_blank"><img loading="lazy" src="screens/{target}" alt="{html.escape(title)}"></a><figcaption>{html.escape(title)}</figcaption></figure>')
    sections.append(f'<section><h2>{group}</h2><div class="grid">'+''.join(cards)+'</div></section>')

def comparison_card(name,source,actual,actual_box):
    x,y,w,h=source;ax,ay,aw,ah=actual_box
    ref=f'<div class="crop" style="aspect-ratio:{w}/{h}"><img alt="Approved {name}" src="../../Cosy-B/Approved-B.png" style="width:{1536/w*100}%;height:{1024/h*100}%;left:{-x/w*100}%;top:{-y/h*100}%"></div>'
    real=f'<div class="crop" style="aspect-ratio:{aw}/{ah}"><img alt="Runtime {name}" src="{actual}" style="width:{540/aw*100}%;height:{960/ah*100}%;left:{-ax/aw*100}%;top:{-ay/ah*100}%"></div>'
    return f'<article><h3>{name}</h3><div class="comparison"><div><p>Approved B</p>{ref}</div><div><p>Actual game</p>{real}</div></div></article>'
compare='<section><h2>Reference beside the actual game</h2><p>Both views are cropped to the wooden frame and shown at the same size. Counters reflect real progress; the game contains 24 sock designs.</p><div class="compare-grid">'+comparison_card('Level map',(76,115,450,812),'screens/map-reference-b.png',(10,11,520,938))+comparison_card('Gameplay',(535,115,465,812),'screens/level-01.png',(10,26,520,908))+comparison_card('Reward',(1005,115,461,812),'BrowserQA/win.png',(10,26,520,908))+'</div></section>'

page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Sock Pairs — GUI review</title>
<style>*{box-sizing:border-box}body{margin:0;background:#203f3a;color:#f3d9aa;font:16px/1.5 system-ui,sans-serif}main{max-width:1360px;margin:auto;padding:32px}h1{font-size:clamp(30px,4vw,48px);line-height:1.15}p{max-width:850px}h2{margin-top:48px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:24px}figure{margin:0;background:#315950;border:1px solid #72988b;border-radius:16px;padding:12px}img{display:block;width:100%;max-height:620px;object-fit:contain;background:#16332e;border-radius:10px}figcaption{padding:12px 4px 4px;font-weight:600}a{color:#ffd77d}.swatches{display:flex;gap:12px;flex-wrap:wrap}.swatches span{padding:12px 18px;border-radius:12px;font-weight:700}.compare-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:30px}.comparison{display:grid;grid-template-columns:1fr 1fr;gap:10px}.crop{position:relative;overflow:hidden}.crop img{position:absolute;max-width:none;max-height:none;object-fit:fill;border-radius:0;background:none}</style>
<main><h1>Sock Pairs · Cosy Laundry Room · B</h1><p>Actual runtime screenshots, 7 November 2026. Click any image to inspect it at full resolution. These are real game captures, not generated mockups. The approved concept is linked below. The map uses B’s two stacked actions; the separate native shop is retained only as a legacy screen.</p><p>Corrected from the approved reference itself: original wood, frame, reward decorations and menu-button artwork; fixed stage proportions; live counters; and a one-time soft-focus gameplay backdrop. <a href="../../Cosy-B/Approved-B.png">Open approved reference B</a>.</p><p><a href="BrowserQA/report.json">Browser interaction results</a> · <a href="../README.md">Assets, performance and known limits</a>. Native screenshots passed their layout assertions; the Windows capture player has an unresolved shutdown crash.</p><div class="swatches"><span style="background:#0d4647;color:#ffd7aa">Teal felt</span><span style="background:#ffa97c;color:#4a210e">Peach enamel</span><span style="background:#a45e30;color:#fff">Warm wood</span><span style="background:#ffd7aa;color:#4a210e">Brown text</span></div>'''+compare+''.join(sections)+'</main></html>'
(out/'index.html').write_text(page,encoding='utf-8')

dest=root/'Builds/Reference-B'
sizes={}
for target in ('Web','YouTube'):
    source=root/'.gui-build-workspace/Builds'/target
    shutil.copytree(source,dest/target,dirs_exist_ok=True)
    archive=dest/f'SockPairs-{target}.zip'
    with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for file in sorted((dest/target).rglob('*')):
            if file.is_file(): z.write(file,file.relative_to(dest/target))
    sizes[target]={'files_bytes':sum(f.stat().st_size for f in source.rglob('*') if f.is_file()),'zip_bytes':archive.stat().st_size}
(out/'build-sizes.json').write_text(json.dumps(sizes,indent=2))
print(json.dumps({'gallery':str(out/'index.html'),'sizes':sizes},indent=2))
