from PIL import Image,ImageStat,ImageFilter
from pathlib import Path
import json
root=Path('Documentation/Editor-Screen-Audit/Browser')
region=(90,75,430,128)
def sharpness(name):
 im=Image.open(root/name).convert('L').crop(region)
 edge=im.filter(ImageFilter.Kernel((3,3),(0,1,0,1,-4,1,0,1,0),1,128)).crop((2,2,im.width-2,im.height-2))
 return ImageStat.Stat(edge).stddev[0]/max(1,ImageStat.Stat(im).stddev[0])
before=sharpness('before-pause.png');after=sharpness('pause.png');ratio=after/before
result={'region':region,'normalisedEdgeEnergyBefore':before,'normalisedEdgeEnergyAfter':after,'ratio':ratio,'result':'PASS' if ratio<.65 else 'FAIL','description':'Rendered browser backdrop loses high-frequency detail after opening Pause; metric normalised for dimming.'}
(root/'blur-check.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2));assert ratio<.65,'Browser popup background is not sufficiently blurred'
