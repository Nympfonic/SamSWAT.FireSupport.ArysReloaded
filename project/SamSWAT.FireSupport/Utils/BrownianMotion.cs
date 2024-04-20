using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    // Thanks to keijiro's KlakMotion Unity package for this code
    public class BrownianMotion : MonoBehaviour, IBatchUpdate
    {
        [SerializeField] private float _positionFrequency = 0.2f;
        [SerializeField] private float _positionAmplitude = 0.5f;
        [SerializeField] private Vector3 _positionScale = Vector3.one;
        [SerializeField, Range(0, 8)] private int _positionFractalLevel = 3;

        [SerializeField] private float _rotationFrequency = 0.2f;
        [SerializeField] private float _rotationAmplitude = 10.0f;
        [SerializeField] private Vector3 _rotationScale = new Vector3(1, 1, 0);
        [SerializeField, Range(0, 8)] private int _rotationFractalLevel = 3;

        [HideInInspector] public Vector3 PositionOffset { get; private set; }
        /// <summary>
        /// This should be first in the Quaternion multiplication order: e.g. RotationOffset * transform.rotation
        /// </summary>
        [HideInInspector] public Quaternion RotationOffset { get; private set; }

        private const float _fbmNorm = 1 / 0.75f;
        private float[] _time;

        public void BatchUpdate()
        {
            CalculatePositionOffset();

            CalculateRotationOffset();
        }

        private void Start()
        {
            _time = new float[6];
            Rehash();

            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
        }

        private void OnDestroy()
        {
            UpdateManager.Instance.DeregisterSlicedUpdate(this);
        }

        private void Rehash()
        {
            for (var i = 0; i < 6; i++)
                _time[i] = Random.Range(-10000.0f, 0.0f);
        }

        private void CalculatePositionOffset()
        {
            var dt = Time.deltaTime;

            for (var i = 0; i < 3; i++)
                _time[i] += _positionFrequency * dt;

            var n = new Vector3(
                Perlin.Fbm(_time[0], _positionFractalLevel),
                Perlin.Fbm(_time[1], _positionFractalLevel),
                Perlin.Fbm(_time[2], _positionFractalLevel));

            n = Vector3.Scale(n, _positionScale);
            n *= _positionAmplitude * _fbmNorm;

            PositionOffset = n;
        }

        private void CalculateRotationOffset()
        {
            var dt = Time.deltaTime;

            for (var i = 0; i < 3; i++)
                _time[i + 3] += _rotationFrequency * dt;

            var n = new Vector3(
                Perlin.Fbm(_time[3], _rotationFractalLevel),
                Perlin.Fbm(_time[4], _rotationFractalLevel),
                Perlin.Fbm(_time[5], _rotationFractalLevel));

            n = Vector3.Scale(n, _rotationScale);
            n *= _rotationAmplitude * _fbmNorm;

            RotationOffset = Quaternion.Euler(n);
        }
    }
}
