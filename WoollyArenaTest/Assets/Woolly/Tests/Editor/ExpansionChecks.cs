using System;
using System.IO;
using System.Security.Cryptography;
namespace WoollyArena.Editor {
 public static class ExpansionChecks {
  public static void Run(Action<bool,string> check){
   check(WaveDifficulty.HealthScale(5)==1.2f&&WaveDifficulty.DamageBonus(5)==0,"First five waves keep their baseline enemy stats");
   check(WaveDifficulty.HealthScale(6)>1&&WaveDifficulty.SpawnInterval(6)<WaveDifficulty.SpawnInterval(5),"Post-five waves increase health and spawn pressure immediately");
   bool monotonic=true;for(int w=7;w<=20;w++)monotonic&=WaveDifficulty.HealthScale(w)>WaveDifficulty.HealthScale(w-1)&&WaveDifficulty.SpawnInterval(w)<=WaveDifficulty.SpawnInterval(w-1);
   check(monotonic,"Late-wave difficulty ramps monotonically through wave twenty");
   check(WaveDifficulty.EnemyLimit(20)==60&&WaveDifficulty.SpeedBonus(20)<=.55f&&WaveDifficulty.SpawnInterval(20)>=.20f,"Late-wave population, movement and spawn rate remain bounded");
   int bosses=0;for(int w=1;w<=20;w++)if(WaveDifficulty.BossWave(w))bosses++;
   check(bosses==4&&WaveDifficulty.BossWave(5)&&WaveDifficulty.BossWave(20),"Four boss milestones include the first and final boss waves");
   check(!WaveDifficulty.CanClear(5,false,false),"Failed boss spawn cannot bypass a boss milestone");
   check(!WaveDifficulty.CanClear(10,true,true),"Timer expiry cannot remove a living boss");
   check(WaveDifficulty.CanClear(10,true,false)&&WaveDifficulty.CanClear(6,false,false),"Boss defeat unlocks wave clear; ordinary waves need no boss");
   check(WaveDifficulty.BossHealth(20)>WaveDifficulty.BossHealth(5),"Later bosses gain health");
   var saved=new RunCheckpoint{Build=new SurvivalBuild(8),Wave=6,Phase=0,Health=100,Biome=1};
   check(RunCheckpointStore.Decode(RunCheckpointStore.Encode(saved)).Biome==1,"Selected biome survives checkpoint roundtrip");
   saved.Biome=0;var bytes=RunCheckpointStore.Encode(saved);
   // Reconstruct the real v1 layout (no trailing biome field) and its checksum.
   var legacy=new byte[bytes.Length-28];Array.Copy(bytes,legacy,bytes.Length-60);legacy[4]=1;
   using(var sha=SHA256.Create()){var hash=sha.ComputeHash(legacy,0,legacy.Length-32);Array.Copy(hash,0,legacy,legacy.Length-32,32);}
   check(RunCheckpointStore.Decode(legacy).Biome==0,"Existing version-one saves migrate to the canyon without losing progress");
   saved.Biome=1;saved.Character=1;
   check(RunCheckpointStore.Decode(RunCheckpointStore.Encode(saved)).Character==1,"Punk Vera selection survives checkpoint roundtrip");
   var current=RunCheckpointStore.Encode(saved);var versionTwo=new byte[current.Length-24];Array.Copy(current,versionTwo,current.Length-56);versionTwo[4]=2;
   using(var sha=SHA256.Create()){var hash=sha.ComputeHash(versionTwo,0,versionTwo.Length-32);Array.Copy(hash,0,versionTwo,versionTwo.Length-32,32);}
   var oldBiome=RunCheckpointStore.Decode(versionTwo);check(oldBiome.Biome==1&&oldBiome.Character==0,"Version-two biome saves migrate with Woolly as the existing character");
   saved.Character=CharacterDefinition.Count;bool invalidCharacter=false;try{RunCheckpointStore.Encode(saved);}catch(InvalidDataException){invalidCharacter=true;}check(invalidCharacter,"Unknown playable character cannot enter a checkpoint");saved.Character=0;
   saved.Biome=2;bool rejected=false;try{RunCheckpointStore.Encode(saved);}catch(InvalidDataException){rejected=true;}
   check(rejected,"Unknown biome cannot enter a saved run");
  }
 }
}
