using System;
namespace WoollyArena
{
    // Faster fire reaches the heat limit sooner; venting does not accelerate with attack speed.
    public sealed class RepeaterHeat
    {
        public const int BurstCapacity=18;
        public const float VentDuration=1.35f;
        public int Fired {get;private set;}
        public float VentUntil {get;private set;}
        public bool Ready(float now){if(now<VentUntil)return false;if(Fired>=BurstCapacity)Fired=0;return true;}
        public bool Fire(float now){if(!Ready(now))return false;Fired++;if(Fired==BurstCapacity)VentUntil=now+VentDuration;return true;}
        public float Cooling(float now)=>Math.Clamp((VentUntil-now)/VentDuration,0,1);
        public void Reset(){Fired=0;VentUntil=0;}
    }
}
