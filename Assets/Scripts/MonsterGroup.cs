using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonsterGroup : MonoBehaviour
{
    public List<Monster> monsters = new List<Monster>();
    int monsterCount = 0;
    UnityAction<Monster> action;
    public UnityEventMonster AfterAllMonstersDeath = new UnityEventMonster();
    public UICell UICellAfterAllDeath;
    // Start is called before the first frame update
    void Start()
    {
        action = new UnityAction<Monster>(OnChildDeath);
        foreach (var each in monsters)
        {
            monsterCount++;
            each.AfterDeath.AddListener(action);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnChildDeath(Monster lastMonster)
    {
        monsterCount--;
        if (monsterCount == 0)
        {
            //转化为UI位置
            Vector3 UIPos = Camera.main.WorldToScreenPoint(lastMonster.transform.position);
            UIPos.z = 0;
            UICellAfterAllDeath.GetCellAnim(UIPos);
        }
    }
    public bool IfAllDeath()
    {
        if (monsterCount == 0) return true;
        else return false;
    }
}
