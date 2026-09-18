using UnityEngine;

namespace WoollyArena
{
    [DefaultExecutionOrder(220)]
    public sealed class DodgeAbility : MonoBehaviour
    {
        // Keep the component identity for existing scenes; the ability is now a dash.
        [Min(.1f)] public float distance = 2.2f;
        [Min(.1f)] public float duration = .22f;
        [Min(.1f)] public float cooldown = .65f;
        [Min(0)] public float protectionDuration = .18f;
        public bool IsDodging { get; private set; }
        public Vector3 Direction { get; private set; }
        public float CooldownRemaining => Mathf.Max(0, readyAt - Time.time);
        public bool Ready => !IsDodging && CooldownRemaining <= 0;
        public float Progress => IsDodging ? Mathf.Clamp01((Time.time - started) / duration) : 1;

        ArenaPlayer player;
        CharacterVitals vitals;
        HitReaction hitReaction;
        ParticleSystem dust;
        readonly TrailRenderer[] streaks = new TrailRenderer[2];
        Material streakMaterial;
        float started, readyAt, travelledFraction, nextDust, specialReadyAt;
        Transform poseRoot;Quaternion restRotation;Vector3 restScale;bool posed;int character;
        public float SpecialCooldownRemaining=>Mathf.Max(0,specialReadyAt-Time.time);

        public void Initialize(ArenaPlayer owner, CharacterVitals health, HitReaction hits)
        {
            player = owner;character=PlayableCharacter.Active;
            poseRoot=owner.animator.transform;restRotation=poseRoot.localRotation;restScale=poseRoot.localScale;
            distance=character==1?2.55f:2.2f;duration=character==1?.19f:.22f;
            vitals = health;
            hitReaction = hits;
            var feet = GetComponent<FootstepDust>();
            if (feet) dust = feet.dust;
            var shader = Resources.Load<Shader>("PowerInk");
            if (!shader) return;
            streakMaterial = new Material(shader);
            for (int i = 0; i < streaks.Length; i++)
            {
                var go = new GameObject("Dash Streak " + i);
                go.transform.SetParent(transform, false);
                var trail = go.AddComponent<TrailRenderer>();
                trail.sharedMaterial = streakMaterial;
                trail.time = .09f; trail.minVertexDistance = .035f;
                trail.startWidth = .075f; trail.endWidth = 0;
                trail.startColor = character==1?new Color(.3f,.9f,1,.8f):new Color(1,.85f,.4f,.8f);
                trail.endColor = character==1?new Color(.1f,.6f,1,0):new Color(1,.7f,.2f,0);
                trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                trail.receiveShadows = false; trail.emitting = false;
                streaks[i] = trail;
            }
        }

        public bool TryBegin(Vector3 direction, bool grounded)
        {
            if (!Ready || !grounded || !vitals || vitals.Health <= 0) return false;
            direction.y = 0;
            if (direction.sqrMagnitude < .001f) direction = player.LastMoveDirection;
            Direction = direction.normalized;
            started = Time.time;
            readyAt = started + Mathf.Max(duration, cooldown);
            travelledFraction = 0;
            nextDust = 0;
            IsDodging = true;
            if(player.Stats&&player.Stats.Run)player.Stats.Run.ActionFX?.Accent(CombatAccent.Dash,transform.position,Direction);
            if(player.Stats&&player.Stats.Run)player.Stats.Run.Sfx.Play(CombatCue.Whoosh);
            hitReaction.ResetReaction();
            // Dashing must not shorten the longer protection granted by respawning.
            vitals.ProtectFor(Mathf.Max(vitals.InvulnerableUntil - Time.time, Mathf.Min(protectionDuration, duration)));
            player.visual.rotation = Quaternion.LookRotation(Direction);
            if(Time.time>=specialReadyAt&&player.Stats&&player.Stats.Run){
                specialReadyAt=Time.time+8;var run=player.Stats.Run;
                if(character==1)player.Stats.GrantDashHaste();
                else{
                    vitals.ProtectFor(Mathf.Max(vitals.InvulnerableUntil-Time.time,.4f));
                    var director=run.GetComponent<EnemySpawnDirector>();if(director)foreach(var enemy in director.Enemies)if(enemy&&!enemy.Defeated&&Vector3.Distance(enemy.transform.position,transform.position)<1.8f)enemy.Damage(8,enemy.transform.position-transform.position);
                }
                run.ActionFX.Burst(transform.position+Vector3.up*.45f,character==1?new Color(.25f,.85f,1):new Color(1,.83f,.3f),8);
                run.Rewards.Popup(transform.position+Vector3.up,CharacterDefinition.Power(character),character==1?Color.cyan:Color.yellow,.65f,.75f);
            }
            // Each avatar gets a small authored presentation offset over its running pose.
            for (int i = 0; i < streaks.Length; i++)
            {
                if (!streaks[i]) continue;
                streaks[i].transform.position = transform.position + Vector3.up * .65f + Vector3.Cross(Vector3.up, Direction) * (i == 0 ? -.22f : .22f);
                streaks[i].Clear(); streaks[i].emitting = true;
            }
            return true;
        }

        public Vector3 ConsumeDisplacement()
        {
            if (!IsDodging) return Vector3.zero;
            float u = Progress;
            // A controlled burst that exits near running speed, with frame-independent range.
            float fraction = u + .14f * Mathf.Sin(Mathf.PI * u);
            var displacement = Direction * (distance * (fraction - travelledFraction));
            travelledFraction = fraction;
            if (u >= 1) { IsDodging = false; StopStreaks(false); }
            return displacement;
        }

        public void ReportMovement(Vector3 movement)
        {
            movement.y = 0;
            if (!IsDodging || !dust || movement.sqrMagnitude < .0001f || Time.time < nextDust) return;
            nextDust = Time.time + .08f;
            var puff = new ParticleSystem.EmitParams
            {
                position = transform.position + Vector3.up * .06f - Direction * .18f,
                velocity = -Direction * .9f + Vector3.up * .12f,
                startLifetime = .14f,
                startSize = .08f,
                startColor = new Color(1, .88f, .65f, .12f)
            };
            dust.Emit(puff, 1);
        }

        void LateUpdate(){
            if(!poseRoot)return;
            if(IsDodging){float blend=Mathf.Sin(Progress*Mathf.PI);poseRoot.localRotation=restRotation*Quaternion.Euler(character==1?20*blend:10*blend,0,character==1?-9*blend:0);poseRoot.localScale=Vector3.Scale(restScale,new Vector3(1+(character==0?.035f:0)*blend,1-.035f*blend,1));posed=true;}
            else if(posed){poseRoot.localRotation=restRotation;poseRoot.localScale=restScale;posed=false;}
        }
        void StopStreaks(bool clear)
        {
            foreach (var trail in streaks)
                if (trail) { trail.emitting = false; if (clear) trail.Clear(); }
        }

        public void ResetAbility()
        {
            IsDodging = false;
            readyAt = 0;
            travelledFraction = 0;
            StopStreaks(true);
            if(posed&&poseRoot){poseRoot.localRotation=restRotation;poseRoot.localScale=restScale;posed=false;}
        }

        void OnDisable() { ResetAbility(); }
        void OnDestroy() { if (streakMaterial) Destroy(streakMaterial); }
    }
}
