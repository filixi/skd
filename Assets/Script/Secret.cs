using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secret : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if enemy reach secret in attack range, set it target to its current location
        var gd = GameObject.Find("GameData").GetComponent<GameData>();
        if (gd.secret == null || gd.current_enemies.Count <= 0)
            return;

        // secret taking damage computed based on the enemy attack speed and dph
        foreach (var e in gd.current_enemies)
        {
            var distance = Vector3.Distance(e.transform.localPosition, gd.secret.transform.localPosition);
            var re = e.GetComponent<RegularEnemy>();
            if (distance > re.GetEnemyPackage().attack_range)
                continue;

            if (distance <= re.GetEnemyPackage().attack_range)
                re.GetEnemyPackage().end = re.transform.position;

            if (re.GetEnemyPackage().last_attack_tick + re.GetEnemyPackage().attack_interval <= Tick.tick)
            {
                gd.secret.GetComponent<RegularEnemy>().TakeDamage(DamageType.Standard, re.GetEnemyPackage().dph, Tick.tick);
                re.GetEnemyPackage().last_attack_tick = Tick.tick;
            }
        }

        //if (!gd.secret.GetComponent<RegularEnemy>().IsAlive())
        //    Debug.Log("Game over");
    }
}
