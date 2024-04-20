using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.VHACD
{
    public class ComplexColliderData : ScriptableObject
    {
        [HideInInspector]
        public int quality = 0;

        public Parameters parameters;

        public Mesh[] baseMeshes = new Mesh[0];

        public Mesh[] computedMeshes = new Mesh[0];
    }
}
