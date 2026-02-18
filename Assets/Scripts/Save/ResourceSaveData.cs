using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ResourceSaveData
{
    public int ID;
    public string SOID;
    public int ResourceAreaID;
    public int leftResourceNum;
    public SerializableVector3 position;

    public ResourceSaveData()
    {

    }

    public ResourceSaveData(ResourceUnit unit)
    {
        this.ID = unit.uniqueID;
        this.SOID = unit.data.ID;

        this.position = new SerializableVector3(unit.transform.position);

        this.ResourceAreaID = unit.resourceAreaID;
        this.leftResourceNum = unit.resourceLeftNum;
    }
}

