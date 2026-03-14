using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum EnemyMode
{
    TargetPlayer,
    Free
}

public class LevelOption : Singleton<LevelOption>
{
    public bool isNeedFog;

    public EnemyMode enemyMode;

    public int enemyNum = 2;

    public LevelOptionSaveData ToSaveData()
    {
        return new LevelOptionSaveData(Instance);
    }

    public void LoadData(LevelOptionSaveData data)
    {
        Instance.isNeedFog = data.isNeedFog;
        Instance.enemyMode = data.enemyMode;
        Instance.enemyNum = data.enemyNum;
    }

}
