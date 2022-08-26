using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class FireSupportSpotter : ScriptableObject
    {
        [SerializeField] private GameObject[] _spotterParticles;
        private Vector3 _spotterPosition;
        private Vector3 _strafeStartPosition;
        private Vector3 _strafeEndPosition;
        private bool _requsetCanceled;
        private static FireSupportSpotter _instance;

        public static FireSupportSpotter Instance
        {
            get
            {
                return _instance;
            }
        }

        private IEnumerator SpotterSequence()
        {

        }

        private IEnumerator SpotterVertical()
        {

        }

        private IEnumerator SpotterHorizontal()
        {

        }

        private IEnumerator SpotterConfirmation()
        {

        }
    }
}
