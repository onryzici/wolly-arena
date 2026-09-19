using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

namespace WoollyArena.Editor
{
    [InitializeOnLoad]
    public static class WeaponVFXReview
    {
        const string Active="Woolly.WeaponVFXReview",Report="Logs/weapon-vfx-review.txt";
        static SurvivalRun run;static EnemyAgent target;static int step,weapon,frame,shots,health,errors,pausedFlames;static double next,started;static bool persistence;
        static readonly string[] Weapons={"axe","flame","blade","saw"};
        static WeaponVFXReview(){if(SessionState.GetBool(Active,false))Attach();EditorApplication.update+=Requests;}
        static void Requests(){if(!File.Exists("Logs/weapon-vfx-review.request")||EditorApplication.isCompiling||EditorApplication.isUpdating)return;File.Delete("Logs/weapon-vfx-review.request");Begin();}
        static void Begin(){if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();EditorApplication.delayCall+=Begin;return;}File.WriteAllText(Report,"Close VFX review: actual automatic attacks, slowed frame captures\n");EditorSceneManager.OpenScene("Assets/Woolly/Scenes/TrainingArena.unity");SessionState.SetBool(Active,true);Attach();EditorApplication.EnterPlaymode();}
        static void Attach(){persistence=SurvivalRun.CheckpointsEnabled;SurvivalRun.CheckpointsEnabled=false;RunSession.ResumeRequested=false;run=null;step=weapon=frame=errors=0;started=EditorApplication.timeSinceStartup;next=started+3;Application.runInBackground=true;EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
        static void Log(string text,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors++;File.AppendAllText(Report,"ERROR "+text+"\n"+trace+"\n");}}
        static void Check(bool okay,string message){if(!okay)throw new Exception(message);File.AppendAllText(Report,"PASS "+message+"\n");}
        static void Set(object obj,string field,object value)=>obj.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(obj,value);
        static void Tick(){try{
            if(EditorApplication.timeSinceStartup-started>100)throw new TimeoutException("VFX review step="+step);
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
            if(!run)run=Object.FindAnyObjectByType<SurvivalRun>();if(!run)return;
            switch(step){
                case 0:
                    run.enabled=false;run.Player.GetComponent<CharacterVitals>().ProtectFor(999);run.Player.Loadout.enabled=false;
                    run.Director.CancelSquads();foreach(var e in run.Director.Enemies)if(e)Object.Destroy(e.gameObject);run.Director.Enemies.Clear();
                    var motor=run.Player.GetComponent<CharacterController>();motor.enabled=false;run.Player.transform.position=new Vector3(-2,0,0);motor.enabled=true;
                    Camera.main.GetComponent<ArenaCamera>().enabled=false;
                    Camera.main.orthographicSize=3.8f;Camera.main.transform.position=run.Player.transform.position+new Vector3(.8f,12,-9);Camera.main.transform.LookAt(run.Player.transform.position+new Vector3(.8f,0,0));
                    next=EditorApplication.timeSinceStartup+.3;step++;break;
                case 1:
                    run.Director.Spawn();target=run.Director.Enemies.Last();target.target=null;target.moveSpeed=0;target.GetComponent<NavMeshAgent>().enabled=false;
                    Check(NavMesh.SamplePosition(run.Player.transform.position+Vector3.right*(Weapons[weapon]=="flame"?3.1f:1.65f),out var nav,.6f,NavMesh.AllAreas),"Review target cell is on NavMesh");
                    target.transform.position=nav.position;Set(target,"born",Time.time-1);target.vitals.ResetHealth(9999);health=target.vitals.Health;
                    run.Build.Weapons.Clear();run.Build.Weapons.Add(new OwnedGear(RunCatalog.Find(Weapons[weapon]),1,20));Set(run.Build,"<Revision>k__BackingField",run.Build.Revision+1);run.Player.Stats.Apply();
                    shots=run.Player.Loadout.Shots;run.Player.Loadout.enabled=true;Time.timeScale=.25f;frame=0;step++;next=EditorApplication.timeSinceStartup+.04;break;
                case 2:
                    if(run.Player.Loadout.Shots<=shots){next=EditorApplication.timeSinceStartup+.015;break;}
                    Check(target.vitals.Health<health,Weapons[weapon]+" VFX triggered by a real damaging attack");
                    if(Weapons[weapon]=="flame")next=EditorApplication.timeSinceStartup+.55;
                    step++;break;
                case 3:
                    if(frame==0&&Weapons[weapon]!="flame"){
                        var trails=Object.FindAnyObjectByType<CartoonWeaponFX>().GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Sweeping blade trail"&&r.enabled).ToArray();
                        var expected=run.Player.transform.position+Vector3.right*1.7f+Vector3.up*.85f;
                        Check(trails.Length>0&&trails.All(r=>Weapons[weapon]=="saw"?Vector3.Distance(r.bounds.center,run.Player.transform.position)<2.6f:Vector3.Distance(r.bounds.center,expected)<1.2f),"Melee VFX stays at the actual strike when the player is away from the world origin");
                    }
                    Capture(Weapons[weapon]+"-"+frame);
                    frame++;next=EditorApplication.timeSinceStartup+.06;if(frame<8)break;
                    var fx=Object.FindAnyObjectByType<CartoonWeaponFX>();Check(fx&&fx.ActiveFlames<=CartoonWeaponFX.MaximumFlames,"Bounded cartoon effect pool after "+Weapons[weapon]);
                    if(Weapons[weapon]=="flame")Check(fx.ActiveFlames>0&&target.Ailments.Burning(Time.time),"Flame tongues and burning damage are both live");
                    Time.timeScale=1;run.Player.Loadout.enabled=false;run.Player.Loadout.CancelAttacks();Object.Destroy(target.gameObject);run.Director.Enemies.Clear();
                    weapon++;if(weapon<Weapons.Length){step=1;next=EditorApplication.timeSinceStartup+.25;}else{step=4;next=EditorApplication.timeSinceStartup+.2;}break;
                case 4:
                    var cartoon=Object.FindAnyObjectByType<CartoonWeaponFX>();Check(cartoon.ActiveFlames==0&&cartoon.ActiveCuts==0,"Cancelling attacks clears cut trails and all flame emitters");
                    var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=new Vector3(25,1,2);Physics.SyncTransforms();
                    Check(Mathf.Abs(CombatSight.DistanceToCover(new Vector3(25,1,0),Vector3.forward,4)-1.5f)<.05f,"Flame cover query stops at scenery");Object.DestroyImmediate(blocker);
                    Check(Resources.Load<Texture2D>("VFX/CartoonFire"),"Authored eight-frame fire texture is loaded");
                    run.Director.Spawn();target=run.Director.Enemies.Last();target.target=null;target.moveSpeed=0;target.GetComponent<NavMeshAgent>().enabled=false;
                    target.transform.position=run.Player.transform.position+Vector3.right*3;Set(target,"born",Time.time-1);target.vitals.ResetHealth(9999);
                    run.Build.Weapons.Clear();for(int i=0;i<6;i++)run.Build.Weapons.Add(new OwnedGear(RunCatalog.Find("flame"),1,20));Set(run.Build,"<Revision>k__BackingField",run.Build.Revision+1);run.Player.Stats.Apply();run.Player.Loadout.enabled=true;
                    next=EditorApplication.timeSinceStartup+1.5;step++;break;
                case 5:
                    var crowded=Object.FindAnyObjectByType<CartoonWeaponFX>();Check(crowded.ActiveFlames>0&&crowded.ActiveFlames<=CartoonWeaponFX.MaximumFlames,"Six simultaneous flamethrowers respect the shared particle capacity");
                    pausedFlames=crowded.ActiveFlames;run.SetPaused(true);next=EditorApplication.timeSinceStartup+.3;step++;break;
                case 6:
                    Check(Object.FindAnyObjectByType<CartoonWeaponFX>().ActiveFlames==pausedFlames,"Pause freezes the active flame pool");run.SetPaused(false);
                    Check(errors==0,"No runtime errors");File.AppendAllText(Report,"COMPLETE: axe, flame, sword and saw actual attacks; eight close frames each; six-flame pool, cover and pause. Presentation fixture, not phone performance or difficulty benchmark.\n");Finish();break;
            }
        }catch(Exception e){File.AppendAllText(Report,"FAIL "+e+"\n");Finish();}}
        static void Capture(string label){var camera=Camera.main;var targetTexture=new RenderTexture(1000,800,24);var image=new Texture2D(1000,800,TextureFormat.RGB24,false);var old=camera.targetTexture;var active=RenderTexture.active;var canvas=Object.FindAnyObjectByType<ArenaHUD>().GetComponent<Canvas>();bool visible=canvas.enabled;try{canvas.enabled=false;camera.targetTexture=targetTexture;camera.Render();RenderTexture.active=targetTexture;image.ReadPixels(new Rect(0,0,1000,800),0,0);image.Apply();File.WriteAllBytes("Logs/vfx-close-"+label+".png",image.EncodeToPNG());}finally{canvas.enabled=visible;camera.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(image);Object.DestroyImmediate(targetTexture);}}
        static void Finish(){Time.timeScale=1;SessionState.SetBool(Active,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SurvivalRun.CheckpointsEnabled=persistence;EditorApplication.ExitPlaymode();}
    }
}
