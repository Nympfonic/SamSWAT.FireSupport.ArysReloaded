using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using System;
using System.Collections;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public interface IFireSupport<out T>
        where T : VehicleBehaviour
    {
        void MakeRequest(GesturesMenu gesturesMenu, FireSupportSpotter spotter);
        IEnumerator SpotterSequence(FireSupportSpotter spotter, Action<bool, Vector3, Vector3> confirmation);
        IEnumerator SupportRequest(Vector3 v1, Vector3 v2);
        T TakeFromPool();
    }
}
