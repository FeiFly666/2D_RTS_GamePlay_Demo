using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FilePath
{
    public static string GameSavePath { get; private set; }

    public static void Init()
    {
        GameSavePath = Path.Combine(
            Application.persistentDataPath,
            "/Save.json"
        );
    }
}
