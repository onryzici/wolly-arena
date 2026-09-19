using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace WoollyArena.Editor
{
    [InitializeOnLoad]
    public static class ArsenalReview
    {
        const string Active="Woolly.ArsenalReview";
        static string Report=>SessionState.GetBool("Woolly.ArsenalPressureOnly",false)?"Logs/arsenal-pressure-review.txt":"Logs/arsenal-runtime-review.txt";
        static readonly string[] Weapons=RunCatalog.All.Where(i=>i.IsWeapon).Select(i=>i.Id).ToArray();
        static int step,index,startHealth,shots,errors;
        static double started,next;
        static EnemyAgent dummy;
        static SurvivalRun run;
        static bool previousPersistence;
        static float pressureStarted;
        static bool survivedOpening;
        static readonly NavMeshPath travelPath=new NavMeshPath();
        static int travelGoal;
        static float stationarySurvival;
        static Vector3 previousDrive;
        static float drivenDistance;
        static ArsenalReview(){if(SessionState.GetBool(Active,false))Attach();EditorApplication.update+=Requests;}
        // A local request lets the authorized review be rerun in an already-open Editor.
        static void Requests(){if(File.Exists("Logs/arsenal-review.request")&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating){SessionState.SetBool("Woolly.ArsenalPressureOnly",File.ReadAllText("Logs/arsenal-review.request").Trim()=="pressure");File.Delete("Logs/arsenal-review.request");SessionState.SetBool(Active,false);EditorApplication.update-=Tick;Begin();}}
        [MenuItem("Woolly/Review Arsenal Expansion")]
        public static void Run(){SessionState.SetBool("Woolly.ArsenalPressureOnly",false);Begin();}
        static void Begin()
        {
            if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();EditorApplication.delayCall+=Begin;return;}
            Directory.CreateDirectory("Logs");File.WriteAllText(Report,"Arsenal Play Mode review\n");
            ArsenalChecks.Run(Check);
            EditorSceneManager.OpenScene("Assets/Woolly/Scenes/TrainingArena.unity");
            SessionState.SetBool(Active,true);Attach();EditorApplication.EnterPlaymode();
        }
        static void Attach(){previousPersistence=SurvivalRun.CheckpointsEnabled;SurvivalRun.CheckpointsEnabled=false;RunSession.ResumeRequested=false;Application.runInBackground=true;step=index=errors=0;run=null;started=EditorApplication.timeSinceStartup;next=started+3;EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
        static void Check(bool okay,string message){if(!okay)throw new Exception(message);File.AppendAllText(Report,"PASS "+message+"\n");}
        static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors++;File.AppendAllText(Report,"RUNTIME ERROR "+message+"\n"+trace+"\n");}}
        static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(obj,value);
        static void Equip(params string[] ids){run.Build.Weapons.Clear();foreach(var id in ids.Take(6))run.Build.Weapons.Add(new OwnedGear(RunCatalog.Find(id),1,20));Set(run.Build,"<Revision>k__BackingField",run.Build.Revision+1);run.Player.Stats.Apply();run.Player.Loadout.CancelAttacks();run.Powers.Cancel();for(int i=0;i<3;i++)run.Powers.SetLevel(i,run.Build.PowerLevel(i));}
        static void ClearEnemies(){run.Director.CancelSquads();foreach(var enemy in run.Director.Enemies)if(enemy)Object.Destroy(enemy.gameObject);run.Director.Enemies.Clear();}
        static void RepeaterBuild(int tier,int speed,int damage,int health,int armor,int critical){
            Equip("repeater","repeater","repeater","repeater","repeater","repeater");run.Build.Items.Clear();foreach(var weapon in run.Build.Weapons)weapon.Tier=tier;
            var bonuses=(int[])typeof(SurvivalBuild).GetField("bonuses",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(run.Build);Array.Clear(bonuses,0,bonuses.Length);
            bonuses[(int)RunStat.AttackSpeed]=speed;bonuses[(int)RunStat.Damage]=damage;bonuses[(int)RunStat.MaxHealth]=health;bonuses[(int)RunStat.Armor]=armor;bonuses[(int)RunStat.Critical]=critical;
            Set(run.Build,"<Revision>k__BackingField",run.Build.Revision+1);run.Player.Stats.Apply();run.Player.GetComponent<CharacterVitals>().Heal(9999);run.Player.Loadout.enabled=true;
        }
        static void StartBenchmark(){ClearEnemies();Set(run,"<Wave>k__BackingField",20);RepeaterBuild(4,300,100,200,15,25);dummy=Target();dummy.ConfigureBoss();dummy.vitals.ResetHealth(WaveDifficulty.BossHealth(20));pressureStarted=Time.time;survivedOpening=false;File.AppendAllText(Report,"BENCHMARK six tier-IV repeaters, +300 raw attack speed, +100% damage, +25 crit; final boss plating, stationary visible target.\n");next=EditorApplication.timeSinceStartup+1;step=9;}
        static void StartPressure(){
            ClearEnemies();var motor=run.Player.GetComponent<CharacterController>();motor.enabled=false;run.Player.transform.position=Vector3.zero;motor.enabled=true;
            Set(run,"<Phase>k__BackingField",SurvivalRun.RunPhase.Wave);Set(run,"<Wave>k__BackingField",16);Set(run,"<Remaining>k__BackingField",35f);Set(run,"nextAssault",Time.time+WavePressure.AssaultInterval(16));Set(run,"nextSpawn",Time.time+.2f);Set(run.Director,"serial",0);Set(run.Director,"lastPoint",-1);
            RepeaterBuild(3,120,60,0,0,10);var health=run.Player.GetComponent<CharacterVitals>();health.ResetHealth(run.Build.Stat(RunStat.MaxHealth));health.ProtectFor(1);run.Player.CombatPaused=false;run.ShopUI.Refresh();run.enabled=true;UnityEngine.Random.InitState(1138);travelGoal=0;previousDrive=run.Player.transform.position;drivenDistance=0;pressureStarted=Time.time;
        }
        static EnemyAgent Target()
        {
            var player=run.Player;var position=player.transform.position;
            for(int i=0;i<24;i++){
                float angle=i*Mathf.PI/12;var candidate=position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*1.55f;
                if(!NavMesh.SamplePosition(candidate,out var nav,.25f,NavMesh.AllAreas)||Physics.Linecast(position+Vector3.up*.8f,nav.position+Vector3.up*.7f,1,QueryTriggerInteraction.Ignore))continue;
                if(!run.Director.Spawn())throw new Exception("Could not spawn a target");
                var enemy=run.Director.Enemies.Last();enemy.target=null;enemy.transform.position=nav.position;enemy.moveSpeed=0;enemy.vitals.ResetHealth(10000);
                Set(enemy,"born",Time.time-1);return enemy;
            }
            throw new Exception("No reachable nearby target position");
        }
        static void DriveMobile(){
            var player=run.Player;var waypoints=new[]{new Vector3(-7,0,-6),new Vector3(7,0,-6),new Vector3(7,0,6),new Vector3(-7,0,6)};
            drivenDistance+=Vector3.Distance(previousDrive,player.transform.position);previousDrive=player.transform.position;
            if(Vector3.Distance(player.transform.position,waypoints[travelGoal])<1.2f)travelGoal=(travelGoal+1)%4;
            Vector3 direction=waypoints[travelGoal]-player.transform.position;
            if(NavMesh.SamplePosition(waypoints[travelGoal],out var destination,1,NavMesh.AllAreas)&&NavMesh.CalculatePath(player.transform.position,destination.position,NavMesh.AllAreas,travelPath)&&travelPath.corners.Length>1)direction=travelPath.corners[1]-player.transform.position;
            float best=float.NegativeInfinity;var goal=direction.normalized;
            if(NavMesh.SamplePosition(player.transform.position,out var start,.6f,NavMesh.AllAreas))for(int i=0;i<24;i++){
                float angle=i*Mathf.PI/12;var candidate=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var future=start.position+candidate*1.8f;
                if(NavMesh.Raycast(start.position,future,out _,NavMesh.AllAreas))continue;
                float risk=0,nearest=8;
                foreach(var enemy in run.Director.Enemies){if(!enemy||enemy.Defeated)continue;float d=Vector3.Distance(future,enemy.transform.position);risk+=1/Mathf.Max(.1f,d*d);nearest=Mathf.Min(nearest,d);}
                float score=nearest*1.4f-risk*2+Vector3.Dot(candidate,goal)*.65f-Mathf.Max(0,Mathf.Abs(future.x)-7)*2-Mathf.Max(0,Mathf.Abs(future.z)-7)*2;
                if(score>best){best=score;direction=candidate;}
            }
            direction.y=0;direction.Normalize();var right=player.view.transform.right;right.y=0;right.Normalize();var forward=player.view.transform.forward;forward.y=0;forward.Normalize();
            var stick=player.mobile.move;var local=new Vector3(Vector3.Dot(direction,right),Vector3.Dot(direction,forward),0)*stick.radius;
            var canvas=stick.GetComponentInParent<Canvas>();var position=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,stick.transform.TransformPoint(local));var pointer=new PointerEventData(EventSystem.current){pointerId=91,position=position};
            var pointerHits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,pointerHits);if(pointerHits.Count>0)pointer.pointerPressRaycast=pointerHits[0];
            if(!stick.Held)stick.OnPointerDown(pointer);else stick.OnDrag(pointer);
            foreach(var enemy in run.Director.Enemies)if(enemy&&!enemy.Defeated&&Vector3.Distance(enemy.transform.position,player.transform.position)<2.7f){player.RequestDodge();break;}
        }
        static void Tick()
        {
            try{
                if(EditorApplication.timeSinceStartup-started>240)throw new TimeoutException("Review step "+step);
                if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
                if(!run)run=Object.FindAnyObjectByType<SurvivalRun>();if(!run)return;
                switch(step){
                    case 0:
                        run.enabled=false;run.Player.GetComponent<CharacterVitals>().ProtectFor(999);ClearEnemies();
                        if(!File.Exists("Assets/Woolly/Resources/WeaponIcons/mortar.png"))ArsenalIconBake.Bake();
                        Check(run.Player.Loadout&&Object.FindAnyObjectByType<ArsenalVFX>(),"Arsenal and pooled VFX initialize in the real arena");
                        if(SessionState.GetBool("Woolly.ArsenalPressureOnly",false)){StartBenchmark();break;}
                        next=EditorApplication.timeSinceStartup+.3;step=1;break;
                    case 1:
                        ClearEnemies();Equip(Weapons[index]);dummy=Target();startHealth=dummy.vitals.Health;shots=run.Player.Loadout.Shots;
                        next=EditorApplication.timeSinceStartup+2.5;step=2;break;
                    case 2:
                        Capture("weapon-"+Weapons[index],1600,900);
                        Check(dummy&&dummy.vitals.Health<startHealth,Weapons[index]+" automatically hits a real enemy collider; health="+(dummy?dummy.vitals.Health:-1)+" shots="+run.Player.Loadout.Shots+" target="+run.Player.FindTarget()+" paused="+run.IsPaused+" combat="+run.Player.CombatPaused+" simulated="+run.Player.SimulatedInput+" player="+run.Player.transform.position+" dummy="+(dummy?dummy.transform.position:Vector3.zero));
                        if(Weapons[index]=="flame")Check(dummy.Ailments.Burning(Time.time),"Flamethrower applies damage over time");
                        if(Weapons[index]=="frost")Check(dummy.Ailments.Slow(Time.time,false)>0,"Frost applies a live movement slow");
                        Capture("weapon-"+Weapons[index],1600,900);
                        index++;if(index<Weapons.Length){step=1;next=EditorApplication.timeSinceStartup+.1;}else{step=3;next=EditorApplication.timeSinceStartup+.1;}break;
                    case 3:
                        Equip("blade","railgun","flame","frost","grenade","saw");run.SetPaused(true);shots=run.Player.Loadout.Shots;next=EditorApplication.timeSinceStartup+.5;step=4;break;
                    case 4:
                        Check(shots==run.Player.Loadout.Shots,"Pause freezes all weapon clocks and damage");run.SetPaused(false);
                        Set(run,"<Phase>k__BackingField",SurvivalRun.RunPhase.Upgrade);run.Player.CombatPaused=true;run.Powers.Cancel();run.Player.Loadout.CancelAttacks();
                        run.Build.OpenShop(8);run.Build.AddMaterials(250);
                        string[] offers={"burst","ricochet","boomerang","mortar"};for(int i=0;i<4;i++)run.Build.Offers[i]=new ShopOffer{Item=RunCatalog.Find(offers[i]),Tier=2,Price=35};
                        run.ShopUI.Refresh();Capture("shop",1600,900);Capture("shop",2400,1080);Capture("shop",1440,1080);
                        shots=run.Player.Loadout.Shots;next=EditorApplication.timeSinceStartup+1;step=5;break;
                    case 5:
                        Check(shots==run.Player.Loadout.Shots,"Shop transition cancels pending attacks");
                        Set(run,"<Phase>k__BackingField",SurvivalRun.RunPhase.Wave);Set(run,"<Wave>k__BackingField",16);run.Player.CombatPaused=false;run.ShopUI.Refresh();ClearEnemies();
                        for(int i=0;i<40;i++)run.Director.Spawn();
                        Check(run.Director.Enemies.Count>=12,"Late-wave encounter spawns varied enemies");
                        next=EditorApplication.timeSinceStartup+5;step=6;break;
                    case 6:
                        Capture("late-wave",1600,900);
                        Check(run.Director.Enemies.Any(e=>e&&e.IsElite),"Late-wave encounter includes telegraphing elite enemies");
                        ClearEnemies();run.Player.Loadout.enabled=false;dummy=Target();next=EditorApplication.timeSinceStartup+.25;step=7;break;
                    case 7:
                        dummy.vitals.ResetHealth(120);dummy.Damage(200,Vector3.right,true);next=EditorApplication.timeSinceStartup+.16;step=8;break;
                    case 8:
                        Check(run.DeathFX.ActiveCount>0&&run.DeathFX.ActiveCount<=EnemyDeathVFX.MaximumFragments,"Defeat emits bounded moving skeleton fragments");Capture("disintegration",1600,900);
                        StartBenchmark();break;
                    case 9:
                        float elapsed=Time.time-pressureStarted;
                        if(!survivedOpening&&elapsed>=5){Check(dummy&&!dummy.Defeated,"Maximum-speed six-repeater loadout cannot erase the final boss in five seconds");survivedOpening=true;}
                        if(dummy&&!dummy.Defeated&&elapsed<60){next=EditorApplication.timeSinceStartup+.1;break;}
                        File.AppendAllText(Report,"MEASURED final boss stationary target duration: "+elapsed.ToString("F2")+" s; defeated="+(!dummy||dummy.Defeated)+"\n");
                        StartPressure();
                        next=EditorApplication.timeSinceStartup+1;step=10;break;
                    case 10:
                        if(run.Phase==SurvivalRun.RunPhase.Wave&&Time.time-pressureStarted<35){next=EditorApplication.timeSinceStartup+.25;break;}
                        Check(run.Phase==SurvivalRun.RunPhase.Defeat,"Stationary tier-III repeater / attack-speed build loses under real wave-sixteen pressure");
                        stationarySurvival=Time.time-pressureStarted;File.AppendAllText(Report,"MEASURED stationary wave-16 survival: "+stationarySurvival.ToString("F2")+" s.\n");
                        StartPressure();
                        next=EditorApplication.timeSinceStartup;step=11;break;
                    case 11:
                        if(run.Phase==SurvivalRun.RunPhase.Wave&&Time.time-pressureStarted<35){DriveMobile();next=EditorApplication.timeSinceStartup+.08;break;}
                        run.Player.mobile.move.ResetInput();float mobileSurvival=Time.time-pressureStarted;
                        File.AppendAllText(Report,"MEASURED mobile-stick / dash route: "+mobileSurvival.ToString("F2")+" s; phase="+run.Phase+" health="+run.Player.GetComponent<CharacterVitals>().Health+" distance="+drivenDistance.ToString("F2")+" m. Heuristic route, not a human full-run or device benchmark.\n");
                        Check(mobileSurvival>stationarySurvival*1.5f,"Moving and dashing materially outlasts the stationary build");
                        Check(errors==0,"No runtime exceptions or error logs during the review");
                        File.AppendAllText(Report,SessionState.GetBool("Woolly.ArsenalPressureOnly",false)?"COMPLETE: pressure scenarios. Desktop Editor; phone performance not measured.\n":"COMPLETE: 20 real weapon hits, ailments, pause, shop, elite encounter, disintegration and pressure scenarios. Desktop Editor; phone performance not measured.\n");
                        Finish();break;
                }
            }catch(Exception e){File.AppendAllText(Report,"FAIL "+e+"\n");Finish();}
        }
        static void Finish(){SessionState.SetBool(Active,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SurvivalRun.CheckpointsEnabled=previousPersistence;EditorApplication.ExitPlaymode();}
        static void Capture(string label,int width,int height)
        {
            var camera=Camera.main;var hud=Object.FindAnyObjectByType<ArenaHUD>();var canvas=hud.GetComponent<Canvas>();var scaler=canvas.GetComponent<CanvasScaler>();
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance,scale=canvas.scaleFactor;bool scaling=scaler.enabled;
            var target=new RenderTexture(width,height,24);var picture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                scaler.enabled=false;camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.scaleFactor=height/900f;
                Canvas.ForceUpdateCanvases();foreach(var text in hud.GetComponentsInChildren<TMP_Text>())text.ForceMeshUpdate();camera.Render();RenderTexture.active=target;
                picture.ReadPixels(new Rect(0,0,width,height),0,0);picture.Apply();File.WriteAllBytes($"Logs/arsenal-{label}-{width}x{height}.png",picture.EncodeToPNG());
            }finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;canvas.scaleFactor=scale;scaler.enabled=scaling;Object.DestroyImmediate(picture);Object.DestroyImmediate(target);}
        }
    }
}
