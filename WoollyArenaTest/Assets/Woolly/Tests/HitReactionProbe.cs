#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace WoollyArena
{
    // Created only by the explicit review command; excluded from player builds.
    public sealed class HitReactionProbe : MonoBehaviour
    {
        const string Report = "Logs/hit-reaction-review.txt";
        ArenaPlayer player;
        EnemyAgent enemy;

        void Check(bool pass, string message)
        {
            File.AppendAllText(Report, (pass ? "PASS " : "FAIL ") + message + "\n");
            if (!pass) throw new InvalidOperationException(message);
        }

        void PlacePlayer(Vector3 point)
        {
            var motor = player.GetComponent<CharacterController>();
            motor.enabled = false;
            player.transform.position = point + Vector3.up * .02f;
            motor.enabled = true;
            Physics.SyncTransforms();
        }

        bool Clear(Vector3 point)
        {
            return !Physics.CheckCapsule(point + Vector3.up * .4f, point + Vector3.up * 1.2f, .39f, ~0, QueryTriggerInteraction.Ignore);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, "Hit reaction play-mode review\n");
            yield return new WaitForSeconds(.8f);
            var director = FindAnyObjectByType<EnemySpawnDirector>();
            player = FindAnyObjectByType<ArenaPlayer>();
            Check(director && director.Enemies.Count == 4 && player, "Existing arena and four enemies load");
            director.enabled = false;
            foreach (var bot in director.Enemies) bot.gameObject.SetActive(false);
            player.SimulatedInput = true;
            player.TestMove = Vector2.zero;
            player.TestAim = Vector2.up;
            player.GetComponent<CharacterVitals>().Heal(10000);

            Vector3 start = default, end = default;
            bool found = false;
            for (int column = 0; column <= 12 && !found; column++)
                for (int z = -4; z <= 3 && !found; z++)
                {
                    int x = column % 2 == 0 ? column / 2 : -(column + 1) / 2;
                    if (!NavMesh.SamplePosition(new Vector3(x, 0, z), out var a, .25f, NavMesh.AllAreas) ||
                        !NavMesh.SamplePosition(new Vector3(x, 0, z + 3), out var b, .25f, NavMesh.AllAreas)) continue;
                    if (!Clear(a.position) || !Clear(b.position) || !Clear(b.position + Vector3.forward * .6f) || !Clear(b.position + Vector3.right * .6f)) continue;
                    if (NavMesh.Raycast(b.position, b.position + Vector3.forward * .6f, out _, NavMesh.AllAreas) ||
                        NavMesh.Raycast(b.position, b.position + Vector3.right * .6f, out _, NavMesh.AllAreas)) continue;
                    if (Physics.Linecast(a.position + Vector3.up, b.position + Vector3.up, ~0, QueryTriggerInteraction.Ignore)) continue;
                    start = a.position; end = b.position; found = true;
                }
            Check(found, "Found an unobstructed firing lane in the authored map");
            PlacePlayer(start);
            player.visual.rotation = Quaternion.identity;
            var idleTarget = new GameObject("Review idle target");
            var idleVitals = idleTarget.AddComponent<CharacterVitals>();
            idleVitals.Damage(idleVitals.maxHealth);
            enemy = Instantiate(director.prefab, end, Quaternion.identity);
            enemy.target = idleTarget.transform;
            var reaction = enemy.GetComponent<HitReaction>();
            Check(!enemy.Damage(25, Vector3.forward) && enemy.vitals.Health == 100 && reaction.FlashAmount == 0, "Spawn protection rejects damage and feedback");
            yield return new WaitForSeconds(.5f);

            Vector3 before = enemy.transform.position;
            int hits = player.Hits, shots = player.weapon.Shots;
            player.TestFire = true;
            float deadline = Time.time + 1;
            while (player.weapon.Shots == shots && Time.time < deadline) yield return null;
            player.TestFire = false;
            yield return new WaitForEndOfFrame();
            Check(enemy.vitals.Health == 75 && player.Hits == hits + 1, "Real player raycast damages the enemy exactly once");
            Check(reaction.Recoiling && reaction.FlashAmount > .1f, "A confirmed shot starts recoil and visible flash");
            ScreenCapture.CaptureScreenshot("Logs/hit-reaction-impact.png");
            yield return new WaitForSeconds(.24f);
            yield return new WaitForEndOfFrame();
            Vector3 movement = enemy.transform.position - before; movement.y = 0;
            Check(movement.magnitude > .32f && movement.magnitude < .44f && Vector3.Dot(movement.normalized, Vector3.forward) > .98f, "Enemy moves a small distance along the bullet direction: " + movement.magnitude.ToString("F3") + " m");
            var pivot = enemy.visual.Find("Hit reaction pivot");
            Check(reaction.FlashAmount == 0 && Quaternion.Angle(pivot.localRotation, Quaternion.identity) < .01f && pivot.localScale == Vector3.one, "Flash, lean and squash restore without transform drift");
            Check(enemy.animator.transform.Find("Woolly_Rig/mixamorig:Hips"), "Animator bone paths stay intact under the flinch pivot");
            foreach (var renderer in enemy.visual.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials)
                    if (material && material.HasProperty("_HitFlash"))
                        Check(material.GetFloat("_HitFlash") == 0, "Shared body material remains unchanged");

            enemy.vitals.ProtectFor(.15f);
            Check(!enemy.Damage(25, Vector3.right) && !enemy.Damage(0) && !enemy.Damage(-1) && reaction.FlashAmount == 0, "Protected and nonpositive damage cause no false hit feedback");
            yield return new WaitForSeconds(.18f);
            before = enemy.transform.position;
            for (int i = 0; i < 5; i++) enemy.Damage(1, Vector3.right);
            yield return new WaitForSeconds(.25f);
            yield return new WaitForEndOfFrame();
            movement = enemy.transform.position - before; movement.y = 0;
            Check(movement.magnitude > .32f && movement.magnitude < .44f, "Repeated hits refresh the impulse without stacking excessive velocity");

            enemy.Damage(1, Vector3.right);
            Vector3 coarse = Vector3.zero;
            for (int i = 0; i < 10; i++) coarse += reaction.ConsumeDisplacement(1f / 30);
            enemy.Damage(1, Vector3.right);
            Vector3 fine = Vector3.zero;
            for (int i = 0; i < 30; i++) fine += reaction.ConsumeDisplacement(1f / 120);
            Check(Mathf.Abs(coarse.magnitude - .38f) < .001f && Vector3.Distance(coarse, fine) < .001f, "Impulse distance is consistent at 30 and 120 fps");
            yield return new WaitForSeconds(.25f);
            yield return new WaitForEndOfFrame();

            var navigation = enemy.GetComponent<NavMeshAgent>();
            navigation.Warp(end);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Review temporary cover";
            wall.transform.position = end + new Vector3(.7f, 1, 0);
            wall.transform.localScale = new Vector3(.3f, 2, 2);
            Physics.SyncTransforms();
            before = enemy.transform.position;
            enemy.Damage(1, Vector3.right);
            yield return new WaitForSeconds(.24f);
            yield return new WaitForEndOfFrame();
            float wallTravel = Vector3.Dot(enemy.transform.position - before, Vector3.right);
            Check(wallTravel > .03f && wallTravel < .24f, "Swept knockback stops before a live cover collider: " + wallTravel.ToString("F3") + " m");
            Destroy(wall);
            yield return null;

            navigation.Warp(end);
            Check(navigation.Raycast(end + Vector3.forward * 25, out var edge), "Located a navigation boundary for the recoil test");
            Vector3 nearEdge = edge.position - Vector3.forward * .08f;
            Check(NavMesh.SamplePosition(nearEdge, out var edgeStart, .2f, NavMesh.AllAreas) && navigation.Warp(edgeStart.position), "Placed the enemy just inside the navigation boundary");
            enemy.Damage(1, Vector3.forward);
            yield return new WaitForSeconds(.24f);
            yield return new WaitForEndOfFrame();
            Check(navigation.isOnNavMesh && Vector3.Dot(enemy.transform.position - edge.position, Vector3.forward) < .035f, "Recoil does not leave the navigation surface or cross its boundary");

            navigation.Warp(end);
            enemy.target = player.transform;
            idleVitals.Heal(10000);
            before = enemy.transform.position;
            yield return new WaitForSeconds(1.1f);
            Check(!reaction.Recoiling && Vector3.Distance(before, enemy.transform.position) > .1f, "Enemy AI resumes movement after the brief reaction");
            Check(enemy.Damage(10000, Vector3.forward) && enemy.Defeated && !enemy.GetComponent<CapsuleCollider>().enabled, "Lethal hit enters defeat and disables the hitbox");
            Check(!enemy.Damage(1, Vector3.right), "Defeated enemies reject further hits");
            yield return new WaitForSeconds(.35f);
            Check(!enemy, "Defeated enemy is removed normally");

            PlacePlayer(start);
            var playerVitals = player.GetComponent<CharacterVitals>();
            var playerReaction = player.GetComponent<HitReaction>();
            playerVitals.ProtectFor(0);
            before = player.transform.position;
            Check(playerVitals.Damage(10, Vector3.forward) && playerReaction.FlashAmount > .9f, "Player damage also starts a visible reaction");
            yield return new WaitForSeconds(.24f);
            yield return new WaitForEndOfFrame();
            movement = player.transform.position - before; movement.y = 0;
            Check(movement.magnitude > .11f && movement.magnitude < .21f, "Player receives a gentler motor-resolved push: " + movement.magnitude.ToString("F3") + " m");
            playerVitals.ProtectFor(1);
            Check(!playerVitals.Damage(10, Vector3.forward) && playerReaction.FlashAmount == 0, "Player invulnerability suppresses feedback");
            playerVitals.ProtectFor(0);
            playerVitals.Damage(10000, Vector3.right);
            yield return new WaitForSeconds(2.2f);
            yield return new WaitForEndOfFrame();
            Check(playerVitals.Health == playerVitals.maxHealth && !playerReaction.Recoiling && playerReaction.FlashAmount == 0, "Respawn clears old impulses and restores health");
            File.AppendAllText(Report, "COMPLETE: reloading a clean arena for manual play.\n");
            Debug.Log("HIT_REACTION_REVIEW_OK");
            SceneManager.LoadScene("TrainingArena");
        }
    }
}
#endif
