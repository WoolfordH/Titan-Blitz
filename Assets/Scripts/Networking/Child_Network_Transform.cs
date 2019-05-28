using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Child_Network_Transform : Network_Transform
{
    //this script exists to allow synchronising multiple transforms from 1 network identity
    //errors occour if you call network code when there are 2 of the same script
}
