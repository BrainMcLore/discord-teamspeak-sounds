"""Generate the original SoundSpace waveform icon in PNG and Windows ICO sizes."""
from pathlib import Path
from PIL import Image, ImageDraw
root=Path(__file__).resolve().parents[1]/'launcher'/'assets'
root.mkdir(exist_ok=True)
im=Image.new('RGBA',(1024,1024),(0,0,0,0));d=ImageDraw.Draw(im)
d.rounded_rectangle((30,30,994,994),radius=240,fill='#111113',outline='#333338',width=8)
d.arc((220,180,804,800),180,360,fill='#ff5900',width=72)
d.rounded_rectangle((192,472,292,720),radius=45,fill='#ff5900')
d.rounded_rectangle((732,472,832,720),radius=45,fill='#ff5900')
for x,h in [(380,140),(480,320),(580,220)]:
    d.rounded_rectangle((x,565-h//2,x+62,565+h//2),radius=31,fill='#fff4ed')
im.save(root/'soundspace.png');im.save(root/'soundspace.ico',sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
