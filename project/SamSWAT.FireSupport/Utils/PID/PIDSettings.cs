namespace SamSWAT.FireSupport.ArysReloaded.Utils.PID
{
    public class PIDSettings
    {
        public float proportionalGain;
        public float integralGain;
        public float derivativeGain;

        public bool shouldClampOutput = false;
        /// <summary>
        /// The maximum value should be 1.0f if the output is clamped, otherwise increase until no steady state error.
        /// </summary>
        public float integralSaturation = 1f;

        public PIDControllerType controllerType = PIDControllerType.Angle;
        public PIDDerivativeMeasurement derivativeMeasurement = PIDDerivativeMeasurement.Velocity;
    }
}
