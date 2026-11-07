"""Original, deterministic shaded UI assets. Pillow + NumPy; no reference crops.
256x384 socks fit a 2048 atlas with the rolled pairs and reusable UI surfaces.
"""
from pathlib import Path
import math
import numpy as np
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/Resources/Art'
S=3
COLORS=['f63aa8','32b5fa','ffcc00','70d418','a748f7','ff4c6a']
def bez(points):
    out=[points[0]]
    for a,b,c in zip(points[1::3],points[2::3],points[3::3]):
        p=out[-1]
        for t in np.linspace(0,1,28)[1:]:
            out.append(tuple((1-t)**3*p[j]+3*(1-t)**2*t*a[j]+3*(1-t)*t*t*b[j]+t**3*c[j] for j in (0,1)))
    return [(round(x*S),round(y*S)) for x,y in out]
def pattern(size,design):
    im=Image.new('RGBA',size);d=ImageDraw.Draw(im);typ=design//6
    white=(255,251,231,245)
    if typ==0:
        for y in range(82*S,size[1],50*S):d.rectangle((0,y,size[0],y+19*S),fill=white)
    elif typ==1:
        for row,y in enumerate(range(84*S,size[1],48*S)):
            for x in range((30+row%2*24)*S,size[0],48*S):d.ellipse((x-8*S,y-8*S,x+8*S,y+8*S),fill=white)
    elif typ==2:
        for row,y in enumerate(range(90*S,size[1],60*S)):
            for x in range((30+row%2*30)*S,size[0],60*S):
                d.polygon([(x+math.cos(-math.pi/2+i*math.pi/5)*(13 if i%2==0 else 6)*S,y+math.sin(-math.pi/2+i*math.pi/5)*(13 if i%2==0 else 6)*S) for i in range(10)],fill=white)
    else:
        for y in range(85*S,size[1],45*S):d.line([(x*S,y+(0 if i%2 else 15*S)) for i,x in enumerate(range(-20,290,20))],fill=white,width=10*S,joint='curve')
    return im

def shade(im,mask,base,ball=False):
    a=np.array(im).astype(float);h,w=a.shape[:2];yy,xx=np.mgrid[0:h,0:w];x=xx/w;y=yy/h
    if ball:
        f=.80+.23*np.exp(-((x-.35)**2+(y-.28)**2)/.24)-.04*y
    else:
        f=.69+.38*np.exp(-((x-.38)/.23)**2)-.09*y
    a[:,:,:3]*=f[:,:,None]
    a[:,:,:3]+=np.maximum(0,1-((x-.33)**2+(y-.19)**2)/.06)[:,:,None]*20
    a[:,:,3]=np.array(mask)
    return Image.fromarray(np.clip(a,0,255).astype('uint8'))
for design in range(24):
    c=tuple(bytes.fromhex(COLORS[design%6]));edge=tuple(int(v*.62) for v in c)
    size=(256*S,384*S);mask=Image.new('L',size)
    pts=bez([(64,24),(85,20),(137,20),(157,25),(158,88),(155,158),(155,215),(155,240),(190,241),(213,262),(246,291),(238,334),(210,349),(188,362),(145,360),(111,360),(55,360),(27,339),(29,307),(30,280),(48,271),(50,240),(53,173),(60,85),(64,24)])
    ImageDraw.Draw(mask).polygon(pts,fill=255)
    im=Image.new('RGBA',size,c+(255,));im.alpha_composite(pattern(size,design));d=ImageDraw.Draw(im)
    # Toe and heel patches are clipped by the smooth silhouette below.
    d.ellipse((183*S,274*S,260*S,375*S),fill=tuple(int(v*.83) for v in c)+(255,))
    d.ellipse((12*S,269*S,78*S,333*S),fill=tuple(int(v*.83) for v in c)+(255,))
    d.rounded_rectangle((61*S,23*S,157*S,73*S),radius=9*S,fill='#fff8e9')
    for x in range(71,155,13):d.line((x*S,32*S,(x-2)*S,65*S),fill='#d5d7cf',width=3*S)
    im=shade(im,mask,c);d=ImageDraw.Draw(im)
    d.line(pts+[pts[0]],fill=edge+(255,),width=3*S,joint='curve')
    d.line(bez([(71,85),(65,144),(64,194),(62,235)]),fill=(255,255,255,95),width=3*S,joint='curve')
    # Sock sprites now come from the approved CandySocks sheet; do not regenerate old socks.
    size=(160*S,160*S);mask=Image.new('L',size);ImageDraw.Draw(mask).ellipse((8*S,8*S,152*S,152*S),fill=255)
    im=Image.new('RGBA',size,c+(255,));im.alpha_composite(pattern(size,design));im=shade(im,mask,c,True);d=ImageDraw.Draw(im)
    d.ellipse((8*S,8*S,152*S,152*S),outline=edge+(255,),width=3*S)
    d.arc((24*S,31*S,138*S,139*S),5,175,fill=edge+(230,),width=5*S)
    d.arc((29*S,27*S,136*S,128*S),8,160,fill=(255,255,255,190),width=3*S)
    d.ellipse((41*S,25*S,83*S,36*S),fill=(255,255,255,130))
    im.resize((160,160),Image.Resampling.LANCZOS).save(ART/f'ball_{design:02}.png',optimize=True)
# Neutral bevel surface is tinted in Unity and nine-sliced: depth without lighting or post FX.
for name,radius,bevel in [('round',26,True),('button',30,True),('medal',64,True)]:
    n=128;size=(n*S,n*S);mask=Image.new('L',size);d=ImageDraw.Draw(mask)
    d.rounded_rectangle((1*S,1*S,(n-1)*S,(n-1)*S),radius=radius*S,fill=255)
    yy,xx=np.mgrid[0:n*S,0:n*S];v=255-(8 if name=="round" else 28)*(yy/(n*S))
    a=np.zeros((n*S,n*S,4),dtype='uint8');a[:,:,:3]=v[:,:,None];a[:,:,3]=np.array(mask)
    im=Image.fromarray(a);d=ImageDraw.Draw(im)
    d.rounded_rectangle((3*S,3*S,(n-3)*S,(n-3)*S),radius=(radius-2)*S,outline=(255,255,255,255),width=2*S)
    d.arc((6*S,5*S,(n-6)*S,55*S),185,355,fill=(255,255,255,210),width=2*S)
    im.resize((n,n),Image.Resampling.LANCZOS).save(ART/(name+'.png'),optimize=True)
im=Image.new('RGBA',(128*S,128*S));ImageDraw.Draw(im).ellipse((0,0,128*S-1,128*S-1),fill='white')
im.resize((128,128),Image.Resampling.LANCZOS).save(ART/'pill.png',optimize=True)
print('Original art:',sum(p.stat().st_size for p in ART.glob('*.png')),'bytes')
