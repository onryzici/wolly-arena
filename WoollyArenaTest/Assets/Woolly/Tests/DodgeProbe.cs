#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace WoollyArena
{
    public sealed class DodgeProbe : MonoBehaviour
    {
        const string Report = "Logs/dodge-review.txt";
        ArenaPlayer player;
        CharacterVitals vitals;
        HitReaction reaction;
        CharacterController motor;
        readonly Vector3 origin = new Vector3(0, .06f, 0);

        void Check(bool ok, string message)
        {
            File.AppendAllText(Report, (ok ? "PASS " : "FAIL ") + message + "\n");
            if (!ok) throw new InvalidOperationException(message);
        }

        void ResetPlayer()
        {
            player.TestMove = Vector2.zero; player.TestFire = false;
            player.TestAim = Vector2.up; player.SimulatedInput = true;
            player.Dodge.ResetAbility(); reaction.ResetReaction();
            motor.enabled = false; player.transform.position = origin; motor.enabled = true;
            vitals.ProtectFor(0); vitals.Heal(10000);
            Physics.SyncTransforms();
        }

        IEnumerator Begin()
        {
            Check(player.RequestDodge(), "Ready player accepts a dodge request");
            float deadline = Time.time + .5f;
            while (!player.Dodge.IsDodging && Time.time < deadline) yield return null;
            Check(player.Dodge.IsDodging, "Input starts the dodge on the next update");
        }

        IEnumerator FinishDodge()
        {
            float deadline = Time.time + 1;
            while (player.Dodge.IsDodging && Time.time < deadline) yield return null;
            Check(!player.Dodge.IsDodging, "Dodge completes within its short duration");
            yield return new WaitForEndOfFrame();
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory("Logs"); File.WriteAllText(Report, "Dash play-mode review\n");
            yield return new WaitForSeconds(.8f);
            player = GetComponent<ArenaPlayer>(); vitals = GetComponent<CharacterVitals>();
            reaction = GetComponent<HitReaction>(); motor = GetComponent<CharacterController>();
            var director = FindAnyObjectByType<EnemySpawnDirector>();
            Check(director && director.Enemies.Count == 4, "Authored arena and four enemies load");
            director.enabled = false;
            foreach (var enemy in director.Enemies) enemy.gameObject.SetActive(false);
            ResetPlayer(); yield return new WaitForSeconds(.12f);

            Vector3 before = transform.position;
            player.TestMove = Vector2.up;
            yield return Begin(); player.TestMove = Vector2.zero;
            Check(!vitals.Damage(25, Vector3.back) && reaction.FlashAmount == 0, "Early dodge frames prevent damage and hit recoil");
            Check(!player.RequestDodge(), "Active dodge rejects repeated input");
            yield return FinishDodge();
            var delta = transform.position - before; delta.y = 0;
            Check(Mathf.Abs(delta.magnitude - player.Dodge.distance) < .08f && Vector3.Dot(delta.normalized, Vector3.forward) > .98f, "Directional dash travels its configured range: " + delta.magnitude.ToString("F3"));
            Check(player.animator.transform.localRotation == Quaternion.identity && player.animator.transform.localScale == Vector3.one && player.animator.GetLayerWeight(1) == 1, "Dash restores aim without spinning or shrinking the model");
            Check(player.animator.transform.Find("Woolly_Rig/mixamorig:Hips"), "Existing animation bindings remain intact");
            Check(!player.RequestDodge() && player.Dodge.CooldownRemaining > .3f, "Cooldown rejects requests without buffering another dodge");
            Check(vitals.Damage(1, Vector3.back), "Damage works again after the protection window"); reaction.ResetReaction();
            yield return new WaitForSeconds(1.2f);
            Check(player.CanDodge && !player.Dodge.IsDodging, "Dodge becomes ready without automatically repeating");

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            before = transform.position; player.TestMove = Vector2.one;
            yield return Begin(); player.TestMove = Vector2.zero;
            yield return FinishDodge(); delta = transform.position - before; delta.y = 0;
            Check(Mathf.Abs(delta.magnitude - player.Dodge.distance) < .08f && Mathf.Abs(delta.x - delta.z) < .08f, "Diagonal dodge has the same range, without a speed boost");

            ResetPlayer(); player.TestAim = Vector2.left; yield return new WaitForSeconds(.35f);
            before = transform.position;
            yield return Begin();
            Check(Vector3.Dot(player.Dodge.Direction, player.LastMoveDirection) > .98f, "Stationary dash keeps the last movement direction instead of auto aim");
            yield return FinishDodge();

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Dodge review cover";
            wall.transform.position = origin + new Vector3(0, .8f, 1); wall.transform.localScale = new Vector3(4, 2, .3f);
            Physics.SyncTransforms(); before = transform.position;
            yield return Begin(); yield return FinishDodge();
            Check(transform.position.z < wall.GetComponent<Collider>().bounds.min.z - motor.radius + motor.skinWidth + .04f && transform.position.z > before.z, "CharacterController stops the dodge before a thin cover collider");
            Destroy(wall); yield return null;

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            int shots = player.weapon.Shots;
            player.TestFire = true;
            yield return Begin();
            bool firedDuringRoll = false;
            while (player.Dodge.IsDodging) { firedDuringRoll |= player.weapon.Shots != shots; yield return null; }
            Check(!firedDuringRoll, "Legacy manual firing is suppressed during dash");
            yield return null;
            Check(player.weapon.Shots == shots + 1, "Held fire resumes after landing"); player.TestFire = false;
            Check(player.weapon.Reload(Time.time), "Reload starts with a partially spent magazine");
            player.Dodge.ResetAbility(); yield return Begin(); yield return FinishDodge();
            yield return new WaitForSeconds(1);
            Check(!player.weapon.Reloading && player.weapon.Ammo == player.weapon.Capacity, "Reload continues through a dodge");

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            var button = FindAnyObjectByType<DodgeButton>();
            Check(button && button.player == player && button.raycastTarget, "Dodge control is connected to the player");
            var events = EventSystem.current;
            var uiCamera = button.canvas.worldCamera;
            var pointer = new PointerEventData(events) { pointerId = 41, position = RectTransformUtility.WorldToScreenPoint(uiCamera, button.transform.position) };
            var results = new List<RaycastResult>(); events.RaycastAll(pointer, results);
            Check(results.Count > 0 && results[0].gameObject == button.gameObject, "Dodge button receives its own pointer raycast");
            player.SimulatedInput = false;
            var stick = player.mobile.move;
            var movePointer = new PointerEventData(events) { pointerId = 40, position = RectTransformUtility.WorldToScreenPoint(uiCamera, stick.transform.TransformPoint(new Vector3(70, 0, 0))) };
            var stickHits = new List<RaycastResult>(); events.RaycastAll(movePointer, stickHits);
            movePointer.pointerPressRaycast = stickHits.Find(h => h.gameObject == stick.gameObject);
            stick.OnPointerDown(movePointer); button.OnPointerDown(pointer);
            yield return null;
            Check(player.Dodge.IsDodging && Vector3.Dot(player.Dodge.Direction, Vector3.right) > .98f, "Two-finger input dodges in the held mobile stick direction");
            button.OnPointerUp(pointer); stick.ResetInput(); player.SimulatedInput = true;
            yield return FinishDodge();

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            player.SimulatedInput = false;
            var keyboard = InputSystem.AddDevice<Keyboard>("Dodge review keyboard");
            before = transform.position;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.Space));
            yield return null; yield return null;
            Check(player.Dodge.IsDodging, "Space starts a dodge through the real keyboard input path");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(1.4f);
            delta = transform.position - before; delta.y = 0;
            Check(!player.Dodge.IsDodging && delta.magnitude < player.Dodge.distance + .2f, "Holding Space does not repeatedly dodge when cooldown expires");
            InputSystem.RemoveDevice(keyboard); player.SimulatedInput = true;

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            Time.timeScale = .2f;
            yield return Begin();
            while (player.Dodge.Progress < .9f) yield return null;
            Check(player.Dodge.IsDodging && vitals.Damage(1, Vector3.back), "The final recovery frames are vulnerable");
            reaction.ResetReaction(); yield return FinishDodge(); Time.timeScale = 1;

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            vitals.Damage(10000);
            Check(!player.RequestDodge(), "Dead player cannot dodge");
            yield return new WaitForSeconds(2.2f);
            Check(vitals.Health == vitals.maxHealth && player.CanDodge, "Respawn restores health and dodge availability");
            float protectedUntil = vitals.InvulnerableUntil;
            yield return Begin();
            Check(vitals.InvulnerableUntil >= protectedUntil, "Dodge preserves longer respawn protection");
            yield return FinishDodge();

            ResetPlayer(); yield return new WaitForSeconds(.12f);
            Time.timeScale = .3f;
            yield return Begin();
            while (player.Dodge.Progress < .4f) yield return null;
            yield return new WaitForEndOfFrame();
            Check(player.animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"), "Dash uses locomotion instead of the roll animation");
            var body = System.Array.Find(player.visual.GetComponentsInChildren<SkinnedMeshRenderer>(), r => r.name == "Woolly_Body");
            var mesh = new Mesh(); body.BakeMesh(mesh);
            float ground = float.PositiveInfinity;
            foreach (var vertex in mesh.vertices) ground = Mathf.Min(ground, body.transform.TransformPoint(vertex).y - player.transform.position.y);
            Destroy(mesh);
            Check(ground > -.25f && ground < .5f, "Dash keeps the running body near the ground: " + ground.ToString("F3"));
            Time.timeScale = 0;
            ScreenCapture.CaptureScreenshot("Logs/dash-in-action.png");
            yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
            Time.timeScale = 1;
            File.AppendAllText(Report, "COMPLETE: restoring a clean arena for manual play.\n");
            Debug.Log("DASH_REVIEW_OK");
            SceneManager.sceneLoaded += RunHitRegression;
            SceneManager.LoadScene("TrainingArena");
        }
        static void RunHitRegression(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= RunHitRegression;
            FindAnyObjectByType<ArenaPlayer>().gameObject.AddComponent<HitReactionProbe>();
        }
        void OnDisable() { Time.timeScale = 1; }
    }
}
#endif
