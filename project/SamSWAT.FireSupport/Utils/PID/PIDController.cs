using System;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.PID
{
    public enum PIDControllerType
    {
        Default,
        Angle
    }

    public enum PIDDerivativeMeasurement
    {
        Velocity,
        ErrorRateOfChange
    }

    public class PIDController
    {
        private readonly float _proportionalGain;
        private readonly float _integralGain;
        private readonly float _derivativeGain;

        private readonly bool _shouldClampOutput;
        private readonly float _outputMin = -1f;
        private readonly float _outputMax = 1f;
        /// <summary>
        /// The value should be 1f if the output is clamped, otherwise increase until no steady state error.
        /// </summary>
        private readonly float _integralSaturation;

        private readonly PIDControllerType _controllerType;
        private readonly PIDDerivativeMeasurement _derivativeMeasurement;

        private float _previousValue;
        private float _previousError;
        private float _integrationStored;
        private bool _derivativeInitialized;

        public PIDController(PIDSettings settings)
        {
            _proportionalGain = settings.proportionalGain;
            _integralGain = settings.integralGain;
            _derivativeGain = settings.derivativeGain;
            _shouldClampOutput = settings.shouldClampOutput;
            _integralSaturation = settings.integralSaturation;
            _controllerType = settings.controllerType;
            _derivativeMeasurement = settings.derivativeMeasurement;
        }

        public void Reset()
        {
            _derivativeInitialized = false;
        }

        public float GetOutput(float currentValue, float targetValue, float deltaTime)
        {
            if (deltaTime <= 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));

            float error = _controllerType == PIDControllerType.Angle
                ? AngleDifference(targetValue, currentValue) 
                : targetValue - currentValue;

            // calculate P term
            float p = _proportionalGain * error;

            // calculate I term
            _integrationStored = Mathf.Clamp(_integrationStored + (error * deltaTime), -_integralSaturation, _integralSaturation);
            float i = _integralGain * _integrationStored;

            // calculate both D terms
            float errorRateOfChange = _controllerType == PIDControllerType.Angle
                ? AngleDifference(error, _previousError) / deltaTime 
                : (error - _previousError) / deltaTime;
            _previousError = error;

            float valueRateOfChange = _controllerType == PIDControllerType.Angle
                ? AngleDifference(currentValue, _previousValue) / deltaTime 
                : (currentValue - _previousValue) / deltaTime;
            _previousValue = currentValue;

            // choose D term to use
            float deriveMeasure = 0f;

            if (_derivativeInitialized)
            {
                if (_derivativeMeasurement == PIDDerivativeMeasurement.Velocity)
                    deriveMeasure = -valueRateOfChange;
                else
                    deriveMeasure = errorRateOfChange;
            }
            else _derivativeInitialized = true;

            float d = _derivativeGain * deriveMeasure;

            float result = p + i + d;

            return _shouldClampOutput 
                ? Mathf.Clamp(result, _outputMin, _outputMax) 
                : result;
        }

        private float AngleDifference(float a, float b)
            => (a - b + 540) % 360 - 180;
    }
}
