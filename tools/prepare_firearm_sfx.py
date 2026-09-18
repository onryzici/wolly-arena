"""Edit CC0 recorded firearms into short, bounded game cues (requires numpy/ffmpeg)."""
from pathlib import Path
import subprocess,numpy as np,wave,json,hashlib
ROOT=Path(__file__).resolve().parents[1];src=ROOT/'art/third-party/firearms/Prepared SFX Library';out=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Audio/SFX';report=[];rate=44100
for name,files,length in [('Revolver',['1917/B_16P.wav','1917/B_24P.wav'],.55),('Repeater',['AR-15/D_24P.wav','AR-15/D_32P.wav'],.33),('Shotgun',['CD/H_16P.wav','CD/H_21P.wav'],.75)]:
 for index,file in enumerate(files):
  path=src/file;x=np.frombuffer(subprocess.check_output(['ffmpeg','-v','error','-i',str(path),'-f','f32le','-ac','1','-ar',str(rate),'-']),dtype='<f4').copy()
  peak=float(np.max(np.abs(x)));onset=int(np.flatnonzero(np.abs(x)>peak*.25)[0]);start=max(0,onset-int(.003*rate));clip=x[start:start+int(length*rate)]
  clip*=.78/max(.001,float(np.max(np.abs(clip))));attack=min(44,len(clip));clip[:attack]*=np.linspace(0,1,attack);fade=min(int(.08*rate),len(clip));clip[-fade:]*=np.linspace(1,0,fade)
  dest=out/f'{name}{index}.wav'
  with wave.open(str(dest),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes((clip*32767).astype('<i2').tobytes())
  report.append({'resource':f'Audio/SFX/{name}{index}','source':file,'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'start_seconds':round(start/rate,4),'duration':round(len(clip)/rate,4),'peak':round(float(np.max(np.abs(clip))),4),'license':'CC0'})
folder=ROOT/'WoollyArenaTest/Assets/Woolly/ThirdParty/FreeFirearm';folder.mkdir(parents=True,exist_ok=True);(folder/'selected-assets.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
