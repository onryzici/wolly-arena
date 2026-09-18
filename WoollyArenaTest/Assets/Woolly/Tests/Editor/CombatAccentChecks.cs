using System;
namespace WoollyArena.Editor
{
    public static class CombatAccentChecks
    {
        public static void Run(Action<bool,string> check)
        {
            var budget = new CombatAccentBudget();
            check(budget.TryTake(CombatAccent.Critical,0,out var first)&&first>=CombatAccentBudget.Reserved,"First critical accent uses the common pool");
            check(!budget.TryTake(CombatAccent.Critical,.05f,out _),"Rapid critical hits cannot flood accent rendering");
            check(budget.TryTake(CombatAccent.Critical,.1f,out _),"Critical accents resume at their cooldown boundary");
            check(budget.TryTake(CombatAccent.Pickup,.05f,out _),"Pickup feedback has an independent cooldown");
            bool bounded=true;var used=new bool[CombatAccentBudget.Capacity];
            for(int i=1;i<=1000;i++)
                foreach(CombatAccent kind in Enum.GetValues(typeof(CombatAccent)))
                    if(budget.TryTake(kind,i,out int slot)){
                        bool reserved=kind==CombatAccent.Dash||kind==CombatAccent.LevelUp||kind==CombatAccent.BossDefeat;
                        bounded &= slot>=0&&slot<CombatAccentBudget.Capacity&&(reserved?slot<CombatAccentBudget.Reserved:slot>=CombatAccentBudget.Reserved);
                        used[slot]=true;
                    }
            check(bounded&&Array.TrueForAll(used,x=>x),"Thousands of effects recycle all sixteen slots without hits replacing reserved hero effects");
            bool lifetimes=true;foreach(CombatAccent kind in Enum.GetValues(typeof(CombatAccent)))lifetimes &= CombatAccentBudget.Duration(kind)>0&&CombatAccentBudget.Duration(kind)<=1.1f;
            check(lifetimes,"Every accent expires within 1.1 seconds");
            check(!budget.TryTake((CombatAccent)99,2000,out _)&&!budget.TryTake(CombatAccent.Dash,float.NaN,out _)&&!budget.TryTake(CombatAccent.Dash,float.PositiveInfinity,out _)&&!budget.TryTake(CombatAccent.Dash,-1,out _),"Invalid effect kinds and clocks cannot corrupt the pool");
        }
    }
}
