using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IUnitState
{
    void Enter();
    void Update();
    void Exit();
}
