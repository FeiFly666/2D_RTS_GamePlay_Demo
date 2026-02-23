using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FactionSaveData
{
    public int side;
    public int WoodNum;
    public int GoldNum;

    public FactionSaveData()
    {

    }
    public FactionSaveData(FactionData faction)
    {
        side = (int)faction.side;
        WoodNum = faction.WoodNum;
        GoldNum = faction.GoldNum;
    }
}
