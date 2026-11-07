"""Copy generated assets and inspect alpha bounds; does not alter image pixels."""
from pathlib import Path
from PIL import Image
import json, shutil
root=Path(__file__).resolve().parent.parent
src=Path(r'C:\Users\zod\.codex\generated_images\01a1b218-a009-7d20-b0d2-f4071d8c8e3f')
files={
'CosyLogo':'5e77412b-30bd-44cc-82fb-82a87bf0607b',
'CosyVine':'4f334965-f91b-44ad-88a5-64f3ad31c27b',
'CosyTowels':'d4a619b9-8f45-4ea3-8ef4-b4268b285e6e',
'CosyBoard':'0215d98d-ba1a-4e4e-a6d7-0c40d057370e',
'CosyShell':'76635e70-f895-4fab-bcd7-a373277442cb',
'CosyButton':'43a31519-5a18-43c5-bd33-4cfa4ec50c85',
'CosySecondary':'cf6bc1de-3355-4b87-818f-b4e6922746e4',
'CosyPopup':'e806164f-28ce-4ccd-bfee-57b90d31ed74',
'CosyBasket':'08f7892c-942c-480d-a433-e92dbc2519de',
'CosyNode':'4d9c6800-9b62-42a2-b937-91a4c6c2984f',
'CosyGold':'4198695e-9199-426b-9a95-a10f98f78235',
'CosyCurrent':'4b473f93-10c7-43d8-a922-f0c908a7803e',
'CosyTeal':'64d77914-416c-4fce-9ce2-437deccecfb2',
'CosyRoom':'244c4b9f-9b55-438a-86d9-11a3fa0c2e53',
'CosyStar':'a0bf1701-2c1e-4f89-8d07-8d2cdecee547'}
docs=root/'Documentation/Cosy-B';docs.mkdir(parents=True,exist_ok=True)
shutil.copy2(src/'exec-aa16ef88-d56e-46fd-b504-4495074cf6ae.png',docs/'Approved-B.png')
backup=docs/'Before-B'
if not backup.exists():
    shutil.copytree(root/'Assets/Scripts',backup/'Scripts')
    shutil.copytree(root/'Assets/Editor',backup/'Editor')
spec=[]
for name,id in files.items():
    source=src/f'exec-{id}.png';dest=root/'Assets/Resources/Art'/f'{name}.png';shutil.copy2(source,dest)
    im=Image.open(source);w,h=im.size
    if name=='CosyRoom':bounds=(0,0,w,h)
    else:
        # Alpha-only analysis. Exclude faint fringe pixels, retain antialias margin.
        a=im.getchannel('A');box=a.point(lambda x:255 if x>160 else 0).getbbox()
        bounds=(max(0,box[0]-3),max(0,box[1]-3),min(w,box[2]+3),min(h,box[3]+3))
    x,y,r,b=bounds;bw,bh=r-x,b-y
    if name in ('CosyBoard','CosyShell'):border=[w*.10,h*.10,w*.10,h*.10]
    elif name=='CosyPopup':border=[w*.10,h*.10,w*.10,h*.25]
    elif name in ('CosyButton','CosySecondary','CosyTeal'):border=[bh*.52,bh*.22,bh*.52,bh*.22]
    else:border=[0,0,0,0]
    spec.append(dict(name=name,x=x,y=h-b,width=bw,height=bh,border=border,source=str(source)))
(root/'Assets/Editor/CosyAssetSpecs.json').write_text(json.dumps({'assets':spec},indent=2))
(docs/'asset-manifest.json').write_text(json.dumps({'mode':'built-in imagegen, one asset per call','reference':'Approved-B.png','assets':spec},indent=2))
print('\n'.join(f"{a['name']} {a['width']}x{a['height']}" for a in spec))
