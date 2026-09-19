using System;
using System.Collections.Generic;
using UnityEngine;

namespace WoollyArena.Editor
{
    public static class SurvivorChecks
    {
        public static void Run(Action<bool, string> check)
        {
            var enemy = new Vector3(8, 0, 0);
            var left = EnemyTactics.Goal(EnemyRole.Flanker, enemy, Vector3.zero, Vector3.zero, 0, true);
            var right = EnemyTactics.Goal(EnemyRole.Flanker, enemy, Vector3.zero, Vector3.zero, 1, true);
            check(left.z > 2 && right.z < -2, "Flankers approach from opposing sides instead of the same destination");
            var intercept = EnemyTactics.Goal(EnemyRole.Interceptor, enemy, Vector3.zero, Vector3.right * 40, 0, true);
            check(intercept.x > 2 && intercept.x <= 2.4f, "Interception leads movement but caps dash-speed prediction");
            foreach (var role in new[] { EnemyRole.Pursuer, EnemyRole.Flanker, EnemyRole.Interceptor })
                check(EnemyTactics.Goal(role, Vector3.right, Vector3.zero, Vector3.right * 4, 0, true) == Vector3.zero, role + " commits to a close-range strike");
            var retreat = EnemyTactics.Goal(EnemyRole.Skirmisher, Vector3.right * 3, Vector3.zero, Vector3.zero, 0, true);
            check(retreat.magnitude > 6, "Ranged enemies retreat when approached");
            check(EnemyTactics.Goal(EnemyRole.Skirmisher, Vector3.right * 6, Vector3.zero, Vector3.zero, 0, true) == Vector3.right * 6, "Ranged enemy holds an unobstructed firing distance");
            check(EnemyTactics.Goal(EnemyRole.Skirmisher, Vector3.right * 6, Vector3.zero, Vector3.zero, 0, false).z > 2, "Blocked ranged enemy changes its firing angle");
            var regions = new HashSet<int>(); for (int i = 0; i < 8; i++) regions.Add(SquadRules.Sector(i));
            check(regions.Count == 8, "Squad rotation visits all eight arena regions");
            check(SquadRules.Available(56, 3, 60) == 1 && SquadRules.Available(60, 4, 60) == 0, "Living and announced enemies share the population limit");
            check(SquadRules.Available(0, 24, 60) == 0, "Spawn warnings cannot exceed their fixed pool");
            check(!SquadRules.Safe(3, true, true) && !SquadRules.Safe(8, false, true) && !SquadRules.Safe(8, true, false), "Nearby, obstructed or unreachable squad cells are rejected");
            check(SquadRules.Safe(5, true, true) && SquadRules.Warning >= .9f, "Valid arrivals retain a readable warning");
            check(SquadRules.Size(20) > SquadRules.Size(1) && SquadRules.Interval(20) < SquadRules.Interval(1), "Later waves increase squad size and arrival frequency");
            for (int character = 0; character < 3; character++) foreach (RunStat stat in Enum.GetValues(typeof(RunStat)))
            {
                var build = new SurvivalBuild(901, character); build.AddExperience(16);
                build.LevelChoices[0] = stat; build.LevelChoiceTiers[0] = 4;
                int expected = build.StatAfterBonus(stat, SurvivalBuild.LevelBonus(stat, 4));
                check(build.ChooseLevel(0) && build.Stat(stat) == expected, "Level preview matches applied " + stat + " for hero " + character);
            }
            var capped = new SurvivalBuild(1); for (int i = 0; i < 20; i++) capped.Items.Add(new OwnedGear(RunCatalog.Find("scope"), 4, 12));
            check(capped.StatAfterBonus(RunStat.Critical, 9) == 75, "Level preview honors the critical chance cap");
        }
    }
}
