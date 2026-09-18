import bpy,numpy as np
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Refined_v3.blend'))
o=bpy.data.objects['Woolly_Body'];r=bpy.data.objects['Woolly_Rig']
for side,sign in [('Left',1),('Right',-1)]:
 vs=np.array([v.co[:] for v in o.data.vertices if v.co.x*sign>0 and v.co.z<.065]);print(side,'bounds',vs.min(0),vs.max(0))
 # lower envelope across a grid excludes vertical tread sides
 points=[]
 for x in np.linspace(vs[:,0].min(),vs[:,0].max(),9)[:-1]:
  for y in np.linspace(vs[:,1].min(),vs[:,1].max(),13)[:-1]:
   a=vs[(vs[:,0]>=x)&(vs[:,0]<x+np.ptp(vs[:,0])/8)&(vs[:,1]>=y)&(vs[:,1]<y+np.ptp(vs[:,1])/12)]
   if len(a):points.append(a[a[:,2].argmin()])
 a=np.array(points);co=np.linalg.lstsq(np.c_[a[:,:2],np.ones(len(a))],a[:,2],rcond=None)[0];print('PLANE',co,'roll deg',np.degrees(np.arctan(co[0])),'pitch deg',np.degrees(np.arctan(co[1])))
 for f in [1,3,5,7,9,11,13,15,17]:
  bpy.context.scene.frame_set(f);b=r.pose.bones['mixamorig:'+side+'Foot'];print(side,f,'anklez',b.head.z)
