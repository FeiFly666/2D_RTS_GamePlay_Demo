using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LevelOptionSaveData
{
    public bool isNeedFog;

    public EnemyMode enemyMode;

    public int enemyNum;
    
    public LevelOptionSaveData()
    {

    }
    public LevelOptionSaveData(LevelOption option)
    {
        this.isNeedFog = option.isNeedFog;
        this.enemyMode = option.enemyMode;
        this.enemyNum = option.enemyNum;
    }
}
