"""Original 144 BPM electro-western combat loop and synthesized combat cues. No sampled recordings."""
from pathlib import Path
import numpy as np,wave,json
OUT=Path(__file__).resolve().parents[1]/'WoollyArenaTest/Assets/Woolly/Resources/Audio';OUT.mkdir(parents=True,exist_ok=True)
sr=44100;beat=60/144;length=32*4*beat;mix=np.zeros((round(sr*length),2));rng=np.random.default_rng(218)
def add(signal,at,gain=1,pan=0):
 n=len(signal);ix=(np.arange(n)+round(at*sr))%len(mix);mix[ix,0]+=signal*gain*np.sqrt((1-pan)/2);mix[ix,1]+=signal*gain*np.sqrt((1+pan)/2)
def note(midi,dur,kind='bass'):
 t=np.arange(round(sr*dur))/sr;f=440*2**((midi-69)/12);attack=np.minimum(t/.008,1)
 if kind=='bass':v=np.sin(2*np.pi*f*t)+.28*np.sin(4*np.pi*f*t)+.12*np.sin(6*np.pi*f*t);env=attack*np.exp(-t*5)
 else:v=sum(np.sin(2*np.pi*f*k*t)*np.exp(-t*k*2)/k for k in range(1,7));env=attack*np.exp(-t*7)
 return v*env*np.minimum((dur-t)/.035,1)
roots=[40,40,43,38,40,45,43,38]
for bar in range(32):
 root=roots[bar%8];at=bar*4*beat
 for step in range(8):
  t=np.arange(round(sr*.14))/sr;noise=rng.normal(0,1,len(t));hp=np.diff(noise,prepend=0)
  add(hp*np.exp(-t*(28 if step%2 else 50)),at+step*beat/2,.023 if step%2 else .018,(-1)**step*.38)
  add(note(root+(12 if step in [3,7] else 0),beat*.43),at+step*beat/2,.18)
 for b in [0,1.5,2,3.5]:
  t=np.arange(round(sr*.24))/sr;phase=2*np.pi*(48*t+105*.025*(1-np.exp(-t/.025)))
  add(np.sin(phase)*np.exp(-t*17)+rng.normal(0,.09,len(t))*np.exp(-t*120),at+b*beat,.47)
 for b in [1,3]:
  t=np.arange(round(sr*.18))/sr;noise=rng.normal(0,1,len(t));v=(noise*.45+np.sin(2*np.pi*185*t)*.3)*np.exp(-t*23)
  add(v,at+b*beat,.23,-.12)
 # Short picked minor-pentatonic melody; alternate response phrases and breakdown.
 if bar%16 not in [12,13]:
  offsets=[12,15,19,22,19,15,17,12] if bar%4<2 else [24,22,19,17,15,19,12,10]
  for step,n in enumerate(offsets):
   v=note(root+n,beat*.65,'pluck');when=at+step*beat/2
   add(v,when,.105,.25);add(v,when+beat*.75,.025,-.4)
 if bar%8==7:
  for step in range(4):
   t=np.arange(round(sr*.12))/sr;add(rng.normal(0,1,len(t))*np.exp(-t*35),at+(3+step/4)*beat,.06)
# Periodic synthesis wraps tails across the boundary. Limit headroom without clipping.
mix=np.tanh(mix*1.2);mix*=.84/max(.84,np.max(np.abs(mix)))
fade=round(sr*.006);mix[:fade]*=np.linspace(0,1,fade)[:,None];mix[-fade:]*=np.linspace(1,0,fade)[:,None]
def write(name,data):
 data=np.asarray(data);channels=2 if data.ndim==2 else 1
 with wave.open(str(OUT/(name+'.wav')),'wb')as f:f.setnchannels(channels);f.setsampwidth(2);f.setframerate(sr);f.writeframes((np.clip(data,-1,1)*32767).astype('<i2').tobytes())
write('ArenaRush',mix)
for name,duration,start,end in [('LaserCharge',.75,180,1200),('LaserFire',.20,1600,180),('EnergyImpact',.25,240,60),('WaveReady',.65,440,880)]:
 t=np.arange(round(sr*duration))/sr;phase=2*np.pi*(start*t+(end-start)*t*t/(2*duration));envelope=np.sin(np.pi*t/duration)**1.4
 v=(np.sin(phase)+.25*np.sin(phase*2)+(.13*rng.normal(size=len(t))if name=='EnergyImpact'else 0))*envelope*.35
 write(name,v)
report={'seconds':length,'bpm':144,'bars':32,'peak':float(abs(mix).max()),'rms':float(np.sqrt((mix*mix).mean())),'seam_step':float(abs(mix[0]-mix[-1]).max()),'rights':'Original procedural composition; no third-party samples.'}
(OUT/'ArenaRush.notes.txt').write_text(json.dumps(report,indent=2));print(report)
