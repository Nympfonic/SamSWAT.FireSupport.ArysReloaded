using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public abstract class FireSupport<TVehicleBehaviour> : IFireSupport<TVehicleBehaviour>
        where TVehicleBehaviour : VehicleBehaviour
    {
        public virtual List<TVehicleBehaviour> Behaviours { get; set; } = new List<TVehicleBehaviour>();
        public abstract Task<List<TVehicleBehaviour>> Load(Transform poolTransform);

        public abstract T LoadVehicle<T>(T resource, Transform parent) where T : TVehicleBehaviour;

        public abstract void MakeRequest(GesturesMenu gesturesMenu, FireSupportSpotter spotter);

        public abstract IEnumerator SpotterSequence(FireSupportSpotter spotter, Action<bool, Vector3, Vector3> confirmation);

        public abstract IEnumerator SupportRequest(Vector3 v1, Vector3 v2);

        public virtual TVehicleBehaviour TakeFromPool()
        {
            return Behaviours.Find(x => !x.gameObject.activeSelf);
        }
    }
}
