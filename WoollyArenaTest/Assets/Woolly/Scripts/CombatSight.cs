using UnityEngine;
namespace WoollyArena
{
    public static class CombatSight
    {
        // Characters share the scenery layer in legacy prefabs. A layer mask alone is not a cover test.
        static readonly RaycastHit[] hits=new RaycastHit[128];
        public static float DistanceToCover(Vector3 origin, Vector3 direction, float distance)
        {
            int count=Physics.RaycastNonAlloc(origin,direction,hits,distance,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return 0;
            float nearest=distance;
            for(int i=0;i<count;i++)if(!hits[i].collider.GetComponentInParent<EnemyAgent>()&&!hits[i].collider.GetComponentInParent<ArenaPlayer>())nearest=Mathf.Min(nearest,hits[i].distance);
            return nearest;
        }
        public static bool Clear(Vector3 from,Vector3 to)
        {
            var delta=to-from;float distance=delta.magnitude;if(distance<.001f)return true;
            int count=Physics.RaycastNonAlloc(from,delta/distance,hits,distance,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++)if(!hits[i].collider.GetComponentInParent<EnemyAgent>()&&!hits[i].collider.GetComponentInParent<ArenaPlayer>())return false;
            return true;
        }
    }
}
