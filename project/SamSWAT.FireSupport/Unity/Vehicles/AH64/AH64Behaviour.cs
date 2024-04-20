using Comfort.Common;
using EFT;
using EFT.UI;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree;
using SamSWAT.FireSupport.ArysReloaded.Utils.PID;
using System;
//using SamSWAT.FireSupport.ArysReloaded.Utils.VHACD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public enum AH64MovementState
    {
        Idle,
        MoveToPosition,
        SideStrafe
    }

    public enum AH64StrafeDirection
    {
        None,
        Left,
        Right
    }

    public class AH64Behaviour : VehicleBehaviour, ITurret, IController, IBatchFixedUpdate
    {
        private const float MAX_THROTTLE = 400f;
        private const float MAX_PITCH_ANGLE = 35f;
        private const float MAX_ROLL_ANGLE = 35f;
        private const float DETECTION_RADIUS = 200f;
        private const float DETECTION_CLOSE_RADIUS = 15f;
        private const float DETECTION_ANGLE = 160f;
        private const float IN_RANGE_SQR_MAGNITUDE = 25f;
        private const float M230_ROTATION_SPEED = 60f * Mathf.Deg2Rad;

        public AH64AudioController audioController;
        public Rigidbody rb;
        public BrownianMotion brownianMotion;
        [HideInInspector] public AH64MovementState currentMovementState;
        [HideInInspector] public AH64StrafeDirection currentStrafeDirection;

        private AH64BehaviourTree _behaviourTree;
        private BotSpawner _botSpawner;

        private PIDController _xRotationAxisPID, _yRotationAxisPID, _zRotationAxisPID;
        private PIDController _yPositionPID;
        private bool _shouldResetAngles = false;
        private float _throttle = 0f;
        private float _pitchAngle = 0f;
        private float _yawAngle = 0f;
        private float _rollAngle = 0f;
        private Coroutine _activeFieldTimerCoroutine;

        private bool _canPlayVoiceover = true;
        private Coroutine _voiceoverCooldownCoroutine;

        public override string VehicleName => ModHelper.AH64_NAME;

        [HideInInspector] public Vector3 SpawnPosition { get; set; }
        [HideInInspector] public float TargetAltitude { get; set; } = 120f;
        [HideInInspector] public bool HasArrivedInActiveField { get; set; } = false;
        [HideInInspector] public Vector3 ActiveFieldPosition { get; set; }
        public float ActiveFieldRadius => 100f;
        [HideInInspector] private bool _isLeaving = false;
        [HideInInspector] public Vector3 TargetDestination { get; private set; } = Vector3.positiveInfinity;

        public Transform m230Base;
        public Transform m230Gun;
        public Transform hydraLeft;
        public Transform hydraRight;
        [HideInInspector] public BotOwner LastEnemyTarget { get; set; } = null;
        [HideInInspector] public BotOwner CurrentEnemyTarget { get; set; } = null;
        [HideInInspector] public Vector3 EnemyLastSeenPosition { get; set; } = Vector3.zero;
        [HideInInspector] public List<BotOwner> EnemyTargets { get; private set; }
        [HideInInspector] public List<Player> EnemiesKilled { get; set; } = new List<Player>();
        private Coroutine _firingM230Coroutine;
        private Coroutine _firingHydraPodsCoroutine;
        private readonly BulletClass _m230Ammo = WeaponClass.GetAmmo(ModHelper.M230_AMMO_TPL);
        private readonly BulletClass _hydra70Ammo = WeaponClass.GetAmmo(ModHelper.HYDRA70_AMMO_TPL);
        [HideInInspector] public bool CanFireM230 { get; set; } = false;
        [HideInInspector] public bool IsM230Firing { get; set; } = false;
        [HideInInspector] public bool HasSeenEnemy { get; set; } = false;
        [HideInInspector] public float TimeSinceLastSeenEnemy { get; set; } = 0f;
        [HideInInspector] public bool HasFiredHydras { get; set; } = false;
        [HideInInspector] public bool AreHydrasFiring { get; set; } = false;

        public override void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            EnemyTargets = ModHelper.GameWorld?.AllAlivePlayersList
                .Where(player =>
                {
                    return player != null
                        && player.IsAI
                        && player.AIData.BotOwner.BotsGroup.Enemies.ContainsKey(ModHelper.MainPlayer);
                })
                .Select(bot => bot.AIData.BotOwner)
                .ToList();

            ActiveFieldPosition = position + 120f * Vector3.up;
            TargetAltitude = position.y + 120f;
            float randomDirX = Random.Range(-1f, 1f);
            float randomDirZ = Random.Range(-1f, 1f);
            Vector3 randomDir = new Vector3(randomDirX, 0f, randomDirZ).normalized;
            Vector3 ah64StartPos = position + randomDir * 600f + 120f * Vector3.up;
            Vector3 ah64Heading = ActiveFieldPosition - ah64StartPos;
            SpawnPosition = ah64StartPos;
            TargetDestination = ActiveFieldPosition + (ActiveFieldRadius - 5f) * Vector3.Normalize(ah64StartPos - ActiveFieldPosition);
            float ah64YAngle = Mathf.Atan2(ah64Heading.x, ah64Heading.z) * Mathf.Rad2Deg;

            transform.SetPositionAndRotation(ah64StartPos, Quaternion.Euler(0, ah64YAngle, 0));
            Physics.SyncTransforms();
            rb.isKinematic = false;
            gameObject.SetActive(true);

            // TODO:
            // Delete AH64AudioController component from Apache prefab in Unity
            audioController = gameObject.AddComponent<AH64AudioController>();
            audioController.EnableAudioSources();

            ChangeMovementState(AH64MovementState.MoveToPosition);

            _behaviourTree.enabled = true;
        }

        public override void MoveTo(Vector3 destination)
        {
            float dt = Time.fixedDeltaTime;

            Vector3 targetDir = Vector3.Scale(
                destination - transform.position,
                ModHelper.VECTOR3_IGNORE_Y);

            RotateTowards(targetDir);

            Vector3 rotDir = Vector3.RotateTowards(
                transform.forward,
                targetDir.normalized,
                80f * Mathf.Deg2Rad,
                0f);

            Quaternion targetRot = Quaternion.LookRotation(rotDir);

            float sqrMagnitudeToTarget = targetDir.sqrMagnitude;

            float xTorqueCorrection = _xRotationAxisPID.GetOutput(
                rb.rotation.eulerAngles.x,
                _pitchAngle,
                dt);

            float zTorqueCorrection = _zRotationAxisPID.GetOutput(
                rb.rotation.eulerAngles.z,
                _rollAngle,
                dt);

            float yAngleError = Mathf.DeltaAngle(
                transform.rotation.eulerAngles.y,
                targetRot.eulerAngles.y);

            float rollDelta = MAX_ROLL_ANGLE * dt;

            if (yAngleError < -10f)
            {
                ClampIncrementValue(ref _rollAngle, 3f * rollDelta, -MAX_ROLL_ANGLE, MAX_ROLL_ANGLE);
            }
            else if (yAngleError > 10f)
            {
                ClampIncrementValue(ref _rollAngle, -3f * rollDelta, -MAX_ROLL_ANGLE, MAX_ROLL_ANGLE);
            }
            else
            {
                float rollDeviation = brownianMotion.RotationOffset.eulerAngles.z;

                if (_rollAngle < rollDeviation)
                {
                    ClampIncrementValue(ref _rollAngle, rollDelta, -MAX_ROLL_ANGLE, rollDeviation);
                }
                else if (_rollAngle > rollDeviation)
                {
                    ClampIncrementValue(ref _rollAngle, -rollDelta, rollDeviation, MAX_ROLL_ANGLE);
                }
            }

            float pitchDeviation = brownianMotion.RotationOffset.eulerAngles.x;

            if (sqrMagnitudeToTarget > IN_RANGE_SQR_MAGNITUDE
                && _pitchAngle < MAX_PITCH_ANGLE
                && Mathf.Abs(yAngleError) < 20f)
            {
                ClampIncrementValue(ref _pitchAngle, 20f * dt, pitchDeviation, MAX_PITCH_ANGLE);
            }
            else if (sqrMagnitudeToTarget <= IN_RANGE_SQR_MAGNITUDE
                && _pitchAngle > pitchDeviation)
            {
                _pitchAngle = pitchDeviation;
                //ClampIncrementValue(ref _pitchAngle, -12f * Time.fixedDeltaTime, 5f, _maxPitchAngle);
            }

            rb.AddRelativeTorque(Vector3.right * xTorqueCorrection + Vector3.forward * zTorqueCorrection);

            if (sqrMagnitudeToTarget > IN_RANGE_SQR_MAGNITUDE)
            {
                rb.AddRelativeForce(
                    Vector3.forward * MAX_THROTTLE * Mathf.Lerp(0.5f, 1f, _pitchAngle / MAX_PITCH_ANGLE),
                    ForceMode.Impulse);
            }

            if (sqrMagnitudeToTarget < 9f)
            {
                _shouldResetAngles = true;
                ChangeMovementState(AH64MovementState.Idle);
                if (_isLeaving)
                {
                    ReturnToPool();
                }
            }
        }

        private void RotateTowards(Vector3 targetDirection)
        {
            float dt = Time.fixedDeltaTime;

            Vector3 rotDir = Vector3.RotateTowards(
                transform.forward,
                targetDirection.normalized,
                80f * Mathf.Deg2Rad,
                0f);

            Quaternion targetRot = Quaternion.LookRotation(rotDir);

            float yTorqueCorrection = _yRotationAxisPID.GetOutput(
                rb.rotation.eulerAngles.y,
                targetRot.eulerAngles.y,
                dt);

            rb.AddRelativeTorque(Vector3.up * yTorqueCorrection);
        }

        private void RotateTowards(BotOwner enemy)
        {
            Vector3 enemyDir = Vector3.Scale(
                enemy.Position - transform.position,
                ModHelper.VECTOR3_IGNORE_Y);

            RotateTowards(enemyDir);
        }

        public bool IsLeavingActiveField()
        {
            return _isLeaving;
        }

        private void Awake()
        {
            //SetupColliders();

            _behaviourTree = gameObject.AddComponent<AH64BehaviourTree>();
            _behaviourTree.enabled = false;

            SetupPIDControllers();

            ChangeMovementState(AH64MovementState.Idle);
            ChangeStrafeDirection(AH64StrafeDirection.None);

            _botSpawner = Singleton<IBotGame>.Instance.BotsController.BotSpawner;
            _botSpawner.OnBotCreated += AddBotToTargetList;

            FireSupportController.Instance.OnEnemyKilledByFireSupport += AddEnemyKilled;
        }

        private void AddEnemyKilled(Player player, string killedBy)
        {
            if (killedBy == ModHelper.AH64_NAME && !EnemiesKilled.Contains(player))
            {
#if DEBUG
            ConsoleScreen.Log($"Enemy {player.Id} added to Apache {nameof(EnemiesKilled)} list");
#endif
                EnemiesKilled.Add(player);
            }
        }

        private void SetupPIDControllers()
        {
            _xRotationAxisPID = new PIDController(
                new PIDSettings()
                {
                    proportionalGain = 100f,
                    integralGain = 200f,
                    derivativeGain = 5f
                });
            _yRotationAxisPID = new PIDController(
                new PIDSettings()
                {
                    proportionalGain = 40f,
                    integralGain = 100f,
                    derivativeGain = 5f
                });
            _zRotationAxisPID = new PIDController(
                new PIDSettings()
                {
                    proportionalGain = 25f,
                    integralGain = 100f,
                    derivativeGain = 5f
                });
            _yPositionPID = new PIDController(
                new PIDSettings()
                {
                    proportionalGain = 1f,
                    integralGain = 2f,
                    derivativeGain = 5f,
                    shouldClampOutput = true,
                    controllerType = PIDControllerType.Default
                });
        }

        public override void BatchUpdate()
        {
            switch (currentMovementState)
            {
                case AH64MovementState.Idle:
                    IdleState();
                    break;
                case AH64MovementState.MoveToPosition:
                    MoveToPositionState();
                    break;
                case AH64MovementState.SideStrafe:
                    SideStrafeState();
                    break;
            }

            if (!HasArrivedInActiveField)
            {
                float sqrDistToActiveField = Vector3.SqrMagnitude(Vector3.Scale(transform.position, new Vector3(1, 0, 1)) - Vector3.Scale(ActiveFieldPosition, new Vector3(1, 0, 1)));
                if (sqrDistToActiveField > Mathf.Pow(ActiveFieldRadius, 2))
                {
                    return;
                }

                HasArrivedInActiveField = true;
                StartActiveFieldTimer();
            }

            // Below code only executes when Apache has arrived at the active field

            if (_isLeaving && !IsM230Firing && TargetDestination != SpawnPosition)
            {
                TargetDestination = SpawnPosition;
                ChangeMovementState(AH64MovementState.MoveToPosition);
            }

            if (!HasSeenEnemy)
            {
                TimeSinceLastSeenEnemy += Time.deltaTime;
            }

            if (TimeSinceLastSeenEnemy >= 1f)
            {
                CanFireM230 = false;
            }
        }

        public void BatchFixedUpdate()
        {
            Physics.Simulate(Time.fixedDeltaTime); // EFT does not always simulate physics every fixed update so this is needed to prevent the Apache from "stuttering"

            CorrectAltitude();

            switch (currentMovementState)
            {
                case AH64MovementState.Idle:
                    StabiliseHelicopter();
                    break;
                case AH64MovementState.MoveToPosition:
                    MoveToPosition(TargetDestination);
                    break;
                case AH64MovementState.SideStrafe:
                    SideStrafeInDirection(currentStrafeDirection);
                    break;
            }
        }

        private void Start()
        {
            ReflectionHelper.SupportRigidbody(rb);

            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.BucketA);
            UpdateManager.Instance.RegisterSlicedFixedUpdate(this);
        }

        private void OnDestroy()
        {
            StaticManager.KillCoroutine(ref _voiceoverCooldownCoroutine);
            StaticManager.KillCoroutine(ref _firingHydraPodsCoroutine);
            StaticManager.KillCoroutine(ref _firingM230Coroutine);
            StaticManager.KillCoroutine(ref _activeFieldTimerCoroutine);

            if (_botSpawner != null)
            {
                _botSpawner.OnBotCreated -= AddBotToTargetList;
            }

            UpdateManager.Instance.DeregisterSlicedUpdate(this);
            UpdateManager.Instance.DeregisterSlicedFixedUpdate(this);
        }

        private void IdleState()
        {
            if (_shouldResetAngles)
            {
                ResetRotation();

                _shouldResetAngles = false;
            }

            if (!HasArrivedInActiveField) return;

            if (CurrentEnemyTarget != null && HasEnemyInSight(CurrentEnemyTarget))
            {
                AcquireTarget();

                RotateTowards(CurrentEnemyTarget);
            }

            if (TimeSinceLastSeenEnemy >= 1.5f && !IsM230Firing)
            {
                CalculateSideStrafeDirection();
            }
        }

        private void MoveToPositionState()
        {
            if (!HasArrivedInActiveField && !HasFiredHydras && !AreHydrasFiring)
            {
                Vector3 target = ActiveFieldPosition - 120f * Vector3.up;
                if (Vector3.SqrMagnitude(target - transform.position) <= 40000f)
                {
                    bool hydraLeftRaycast = Physics.Raycast(hydraLeft.position, hydraLeft.forward, out RaycastHit hydraLeftHit);
                    bool hydraRightRaycast = Physics.Raycast(hydraLeft.position, hydraLeft.forward, out RaycastHit hydraRightHit);

                    bool hydraLeftInRange = Vector3.SqrMagnitude(target - hydraLeftHit.point) <= 625f && hydraLeftHit.distance >= 80f;
                    bool hydraRightInRange = Vector3.SqrMagnitude(target - hydraRightHit.point) <= 625f && hydraRightHit.distance >= 80f;

                    if (hydraLeftRaycast && hydraRightRaycast && hydraLeftInRange && hydraRightInRange)
                    {
                        FireHydraPods();
                    }
                }
            }
        }

        private void CalculateSideStrafeDirection()
        {
            if (!FindNextTarget() && LastEnemyTarget != null && !LastEnemyTarget.IsDead)
            {
                Vector3 enemyDir = Vector3.Scale((EnemyLastSeenPosition - transform.position).normalized, new Vector3(1, 0, 1));
                float angle = Vector3.SignedAngle(enemyDir, transform.forward, Vector3.up);

                if (angle < 0)
                {
                    ChangeStrafeDirection(AH64StrafeDirection.Left);
                }
                else
                {
                    ChangeStrafeDirection(AH64StrafeDirection.Right);
                }
                ChangeMovementState(AH64MovementState.SideStrafe);
            }
        }

        private void SideStrafeState()
        {
            if (TimeSinceLastSeenEnemy >= 4f && !IsM230Firing)
            {
                EnemyLastSeenPosition = Vector3.zero;
                
                FindNextTarget();
                Vector3 randomPosInActiveField = ActiveFieldPosition + Vector3.Scale(Random.insideUnitSphere * ActiveFieldRadius, new Vector3(1, 0, 1));
                TargetDestination = randomPosInActiveField;
                ChangeMovementState(AH64MovementState.MoveToPosition);
            }

            if (TimeSinceLastSeenEnemy < 1.5f)
            {
                ChangeMovementState(AH64MovementState.Idle);
            }
        }

        public void ChangeMovementState(AH64MovementState state)
        {
            currentMovementState = state;
        }

        public void ChangeStrafeDirection(AH64StrafeDirection strafeDirection)
        {
            currentStrafeDirection = strafeDirection;
        }

        private void ResetRotation()
        {
            _pitchAngle = brownianMotion.RotationOffset.eulerAngles.x;
            _rollAngle = brownianMotion.RotationOffset.eulerAngles.z;
        }

        /// <summary>
        /// Corrects the helicopter's altitude to the target altitude.
        /// </summary>
        private void CorrectAltitude()
        {
            float yForceCorrection = _yPositionPID.GetOutput(
                rb.position.y,
                TargetAltitude + brownianMotion.PositionOffset.y,
                Time.fixedDeltaTime);

            if (yForceCorrection > 0f)
            {
                rb.AddRelativeForce(
                    Vector3.up * yForceCorrection * MAX_THROTTLE,
                    ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Corrects the helicopter's X and Z-axis rotation to stay upright.
        /// </summary>
        private void StabiliseHelicopter()
        {
            float dt = Time.fixedDeltaTime;

            float xTorqueCorrection = _xRotationAxisPID.GetOutput(
                rb.rotation.eulerAngles.x,
                brownianMotion.RotationOffset.eulerAngles.x,
                dt);

            float zTorqueCorrection = _zRotationAxisPID.GetOutput(
                rb.rotation.eulerAngles.z,
                brownianMotion.RotationOffset.eulerAngles.z,
                dt);

            Vector3 finalTorque = Vector3.right * xTorqueCorrection + Vector3.forward * zTorqueCorrection;

            rb.AddRelativeTorque(finalTorque);
        }

        public void SideStrafeInDirection(AH64StrafeDirection strafeDirection)
        {
            if (strafeDirection == AH64StrafeDirection.Left)
            {
                ClampIncrementValue(ref _rollAngle, MAX_ROLL_ANGLE * 3f * Time.fixedDeltaTime, -MAX_ROLL_ANGLE, MAX_ROLL_ANGLE);
                rb.AddRelativeForce(Vector3.left * MAX_THROTTLE * 0.35f * Mathf.Lerp(0.5f, 1f, _rollAngle / MAX_ROLL_ANGLE), ForceMode.Impulse);
            }
            else
            {
                ClampIncrementValue(ref _rollAngle, -MAX_ROLL_ANGLE * 3f * Time.fixedDeltaTime, -MAX_ROLL_ANGLE, MAX_ROLL_ANGLE);
                rb.AddRelativeForce(Vector3.right * MAX_THROTTLE * 0.35f * Mathf.Lerp(0.5f, 1f, -_rollAngle / MAX_ROLL_ANGLE), ForceMode.Impulse);
            }

            float zTorqueCorrection = _zRotationAxisPID.GetOutput(transform.rotation.eulerAngles.z, _rollAngle, Time.fixedDeltaTime);
            rb.AddRelativeTorque(Vector3.forward * zTorqueCorrection);
        }

        /// <summary>
        /// Handles the movement and rotation of the helicopter as it moves to the specified position.
        /// </summary>
        public void MoveToPosition(Vector3 position)
        {
            float dt = Time.fixedDeltaTime;
            Vector3 targetDir = Vector3.Scale(position - transform.position, new Vector3(1, 0, 1));
            float sqrMagnitudeToTarget = targetDir.sqrMagnitude;
            float inRangeSqrMagnitude = 25f;
            Vector3 rotDir = Vector3.RotateTowards(transform.forward, targetDir.normalized, 80f * Mathf.Deg2Rad, 0f);
            Quaternion targetRot = Quaternion.LookRotation(rotDir);

            float yAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, targetRot.eulerAngles.y);
            float yTorqueCorrection = _yRotationAxisPID.GetOutput(rb.rotation.eulerAngles.y, targetRot.eulerAngles.y, dt);

            float rollRateOfChange = MAX_ROLL_ANGLE * dt;
            if (yAngleError < -10f)
            {
                ClampIncrementValue(ref _rollAngle, rollRateOfChange * 3f, -MAX_ROLL_ANGLE, MAX_ROLL_ANGLE);
            }
            else if (yAngleError > 10f)
            {
                ClampIncrementValue(ref _rollAngle, -rollRateOfChange * 3f, -MAX_ROLL_ANGLE, MAX_ROLL_ANGLE);
            }
            else
            {
                var rollDeviation = brownianMotion.RotationOffset.eulerAngles.z;
                if (_rollAngle < rollDeviation)
                {
                    ClampIncrementValue(ref _rollAngle, rollRateOfChange, -MAX_ROLL_ANGLE, rollDeviation);
                }
                else if (_rollAngle > rollDeviation)
                {
                    ClampIncrementValue(ref _rollAngle, -rollRateOfChange, rollDeviation, MAX_ROLL_ANGLE);
                }
            }

            float pitchDeviation = brownianMotion.RotationOffset.eulerAngles.x;
            if (sqrMagnitudeToTarget > inRangeSqrMagnitude && _pitchAngle < MAX_PITCH_ANGLE && Mathf.Abs(yAngleError) < 20f)
            {
                ClampIncrementValue(ref _pitchAngle, 20f * dt, pitchDeviation, MAX_PITCH_ANGLE);
            }
            else if (sqrMagnitudeToTarget <= inRangeSqrMagnitude && _pitchAngle > pitchDeviation)
            {
                _pitchAngle = pitchDeviation;
                //ClampIncrementValue(ref _pitchAngle, -12f * Time.fixedDeltaTime, 5f, _maxPitchAngle);
            }

            float xTorqueCorrection = _xRotationAxisPID.GetOutput(rb.rotation.eulerAngles.x, _pitchAngle, dt);

            float zTorqueCorrection = _zRotationAxisPID.GetOutput(rb.rotation.eulerAngles.z, _rollAngle, dt);

            rb.AddRelativeTorque(Vector3.right * xTorqueCorrection + Vector3.up * yTorqueCorrection + Vector3.forward * zTorqueCorrection);

            if (sqrMagnitudeToTarget > inRangeSqrMagnitude)
            {
                rb.AddRelativeForce(Vector3.forward * MAX_THROTTLE * Mathf.Lerp(0.5f, 1f, _pitchAngle / MAX_PITCH_ANGLE), ForceMode.Impulse);
            }

            bool hasReachedPosition = sqrMagnitudeToTarget < 9f;
            if (hasReachedPosition)
            {
                _shouldResetAngles = true;
                ChangeMovementState(AH64MovementState.Idle);
                if (_isLeaving)
                {
                    ReturnToPool();
                }
            }
        }

        public bool IsEnemyInDetectionRadius(Vector3 enemyDirection)
        {
            if (enemyDirection.magnitude <= DETECTION_RADIUS && enemyDirection.magnitude > DETECTION_CLOSE_RADIUS 
                && Vector3.Dot(enemyDirection.normalized, transform.forward) > Mathf.Cos(DETECTION_ANGLE * 0.5f * Mathf.Deg2Rad))
            {
                return true;
            }

            return false;
        }

        public bool HasEnemyInSight(BotOwner enemy)
        {
            Vector3 enemyDir = enemy.Position - transform.position;
            Vector3 enemyDirFlattened = enemyDir;
            enemyDirFlattened.y = 0;

            if (!IsEnemyInDetectionRadius(enemyDirFlattened))
            {
                return false;
            }

            if (Physics.Raycast(transform.position, enemyDirFlattened.normalized, out RaycastHit hitTarget)
                && hitTarget.transform == CurrentEnemyTarget.Transform.Original)
            {
                HasSeenEnemy = true;
                EnemyLastSeenPosition = CurrentEnemyTarget.Position;
                CanFireM230 = true;
                TimeSinceLastSeenEnemy = 0f;
                return true;
            }

            HasSeenEnemy = false;
            return false;
        }

        /// <summary>
        /// Finds the next closest target in the helicopter's line of sight.
        /// </summary>
        public bool FindNextTarget()
        {
            LastEnemyTarget = CurrentEnemyTarget;
            CurrentEnemyTarget = null;
            BotOwner closestEnemy = null;
            EnemyTargets.RemoveAll(x => x == null);
            foreach (BotOwner enemy in EnemyTargets.Where(HasEnemyInSight))
            {
                if (enemy == null)
                {
                    EnemyTargets.Remove(enemy);
                    continue;
                }
                if (closestEnemy != null)
                {
                    float potentialEnemySqrMagnitude = Vector3.SqrMagnitude(enemy.Position - transform.position);
                    float closestEnemySqrMagnitude = Vector3.SqrMagnitude(closestEnemy.Position - transform.position);

                    if (potentialEnemySqrMagnitude < closestEnemySqrMagnitude)
                    {
                        closestEnemy = enemy;
                    }
                }
                else
                {
                    closestEnemy = enemy;
                }
            }

            CurrentEnemyTarget = closestEnemy;

            return CurrentEnemyTarget != null;
        }

        public void AcquireTarget()
        {
            // Rotate gun mesh to point towards target
            RotateM230Part(m230Base, Vector3.right);
            RotateM230Part(m230Gun, Vector3.up);

            Vector3 enemyDir = Vector3.Normalize(CurrentEnemyTarget.Position - m230Gun.GetChild(0).position);
            float deltaAngle = Vector3.Angle(m230Gun.forward, enemyDir);

            if (CanFireM230 && !IsM230Firing && Mathf.Abs(deltaAngle) < 0.5f)
            {
                FireM230();
            }
        }

        public void FireHydraPods()
        {
            _firingHydraPodsCoroutine = StaticManager.BeginCoroutine(FiringHydraPods());
        }

        private IEnumerator FiringHydraPods()
        {
            AreHydrasFiring = true;

            PlayVoiceover(VoiceoverType.ApacheFiringRockets);
            yield return new WaitForSecondsRealtime(0.5f);

            StaticManager.BeginCoroutine(FireSingleHydraPod(hydraLeft, ModHelper.MainPlayer));
            yield return new WaitForSecondsRealtime(0.075f);
            StaticManager.BeginCoroutine(FireSingleHydraPod(hydraRight, ModHelper.MainPlayer));

            AreHydrasFiring = false;
            HasFiredHydras = true;
        }

        private IEnumerator FireSingleHydraPod(Transform hydraPod, Player player)
        {
            ParticleSystem hydraSmoke = hydraPod.parent.GetChild(0).GetComponent<ParticleSystem>();
            Vector3 hydraRightDir = Vector3.Cross(hydraPod.forward, Vector3.up).normalized;
            Vector3 hydraUpDir = Vector3.Cross(hydraPod.forward, Vector3.left).normalized;

            float spreadValue = 0.012f;
            int burstCounter = 19;
            while (burstCounter > 0)
            {
                Vector3 hydraHorizontalSpread = hydraRightDir * Random.Range(-spreadValue, spreadValue);
                Vector3 hydraVerticalSpread = hydraUpDir * Random.Range(-spreadValue, spreadValue);
                Vector3 hydraProjectileDir = Vector3.Normalize(hydraPod.forward + hydraHorizontalSpread + hydraVerticalSpread);
                WeaponClass.FireProjectile(WeaponType.Hydra70, _hydra70Ammo, hydraPod.position, hydraProjectileDir);
                if (!hydraSmoke.isPlaying)
                {
                    hydraSmoke.Play();
                }

                float distance = Vector3.Distance(player.CameraPosition.position, hydraPod.position);
                float volume = audioController.volumeCurve.Evaluate(distance);
                float volumeDistant = 0.3f * (1f - audioController.volumeCurve.Evaluate(distance * 0.5f));
                audioController.PlayAudio(hydraPod.position, distance, AH64WeaponSoundType.Hydra70FiringClose, 1200, volume);
                audioController.PlayAudio(hydraPod.position, distance, AH64WeaponSoundType.Hydra70FiringDistant, 3200, volumeDistant);
                StaticManager.BeginCoroutine(PlayHydraRocketExplosionSound(1.5f, hydraPod.position, distance, volume, volumeDistant));
                
                burstCounter--;
                yield return new WaitForSecondsRealtime(0.075f); // 800 RPM
            }
        }

        private IEnumerator PlayHydraRocketExplosionSound(float delay, Vector3 startPos, float distance, float volume, float volumeDistant)
        {
            yield return new WaitForSecondsRealtime(delay);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.Hydra70ExplosionClose, 1200, volume);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.Hydra70ExplosionDistant, 3200, volumeDistant);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.Hydra70ExplosionTailDistant, 3200, volumeDistant);
        }

        public void FireM230()
        {
            _firingM230Coroutine = StaticManager.BeginCoroutine(FiringM230());
        }

        private IEnumerator FiringM230()
        {
            IsM230Firing = true;

            Transform m230Muzzle = m230Gun.GetChild(0);

            PlayVoiceover(VoiceoverType.ApacheFiringM230);
            yield return new WaitForSecondsRealtime(0.5f);

            ParticleSystem m230MuzzleFlash = m230Muzzle.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
            ParticleSystem m230Smoke = m230Muzzle.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
            Vector3 m230RightDir = Vector3.Cross(m230Muzzle.forward, Vector3.up).normalized;
            Vector3 m230UpDir = Vector3.Cross(m230Muzzle.forward, Vector3.left).normalized;

            float spreadValue = 0.007f;
            int burstCounter = Random.Range(4, 7);
            while (burstCounter > 0)
            {
                if (!CanFireM230)
                {
                    yield break;
                }
                Vector3 horizontalSpread = m230RightDir * Random.Range(-spreadValue, spreadValue);
                Vector3 verticalSpread = m230UpDir * Random.Range(-spreadValue, spreadValue);
                Vector3 projectileDir = Vector3.Normalize(m230Muzzle.forward + horizontalSpread + verticalSpread);
                WeaponClass.FireProjectile(WeaponType.M230, _m230Ammo, m230Muzzle.position, projectileDir);
                if (!m230MuzzleFlash.isEmitting)
                {
                    m230MuzzleFlash.Emit(1);
                }
                if (!m230Smoke.isEmitting)
                {
                    m230Smoke.Emit(3);
                }
                
                float distance = Vector3.Distance(ModHelper.MainPlayer.Position, m230Muzzle.position);
                float volume = audioController.volumeCurve.Evaluate(distance);
                float volumeDistant = 0.35f * (1f - audioController.volumeCurve.Evaluate(distance * 0.5f));
                audioController.PlayAudio(m230Muzzle.position, distance, AH64WeaponSoundType.M230FiringClose, 3200, volume);
                audioController.PlayAudio(m230Muzzle.position, distance, AH64WeaponSoundType.M230FiringDistant, 3200, volumeDistant);
                StaticManager.BeginCoroutine(PlayM230ExplosionSound(1.5f, m230Muzzle.position, distance, volume, volumeDistant));

                burstCounter--;
                yield return new WaitForSecondsRealtime(0.096f); // 625 RPM
            }

            yield return new WaitForSecondsRealtime(0.5f);
            IsM230Firing = false;
        }

        private IEnumerator PlayM230ExplosionSound(float delay, Vector3 startPos, float distance, float volume, float volumeDistant)
        {
            yield return new WaitForSecondsRealtime(delay);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.M230ExplosionClose, 3200, volume);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.M230ExplosionReflectClose, 3200, volume);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.M230ExplosionDistant, 3200, volumeDistant);
            audioController.PlayAudio(startPos, distance, AH64WeaponSoundType.M230ExplosionReflectDistant, 3200, volumeDistant);
        }

        /// <summary>
        /// Handles the rotation of the M230 gun parts
        /// </summary>
        /// <param name="rotAxis">The axis the gun part rotates on.</param>
        /// <param name="drawRay">Renders a gizmo in Unity Editor.</param>
        private void RotateM230Part(Transform gunPart, Vector3 targetPos, Vector3 rotAxis, bool drawRay = false)
        {
            Vector3 enemyDir = (CurrentEnemyTarget.Velocity * 0.75f + CurrentEnemyTarget.Position) - gunPart.position;

            Vector3 target = Vector3.RotateTowards(
                gunPart.forward,
                enemyDir.normalized,
                M230_ROTATION_SPEED * Time.deltaTime, 5f);

            Quaternion targetRot = Quaternion.Inverse(gunPart.parent.rotation) * Quaternion.LookRotation(target);
            Vector3 targetRotEuler = targetRot.eulerAngles;
            if (rotAxis == Vector3.up)
            {
                ClampAngle(ref targetRotEuler.x, -12.65f, 60f);
                targetRotEuler.y = 0f;
                targetRotEuler.z = 0f;
            }
            else if (rotAxis == Vector3.right)
            {
                targetRotEuler.x = -5.276f;
                ClampAngle(ref targetRotEuler.y, -0.5f * DETECTION_ANGLE, 0.5f * DETECTION_ANGLE);
                targetRotEuler.z = 0f;
            }

            gunPart.localRotation = Quaternion.Euler(targetRotEuler);

            if (drawRay)
                Debug.DrawRay(gunPart.position, gunPart.forward * DETECTION_RADIUS * 1.2f, Color.green);
        }

        private void StartActiveFieldTimer()
        {
            _activeFieldTimerCoroutine = StaticManager.BeginCoroutine(ActiveFieldTimer());
        }

        private IEnumerator ActiveFieldTimer()
        {
            yield return new WaitForSecondsRealtime(Plugin.ApacheActiveTime.Value);
            _isLeaving = true;
        }

        private void PlayVoiceover(VoiceoverType voiceoverType)
        {
            if (_canPlayVoiceover)
            {
                Singleton<FireSupportAudio>.Instance.PlayVoiceover(voiceoverType);
                StartVoiceoverCooldown();
            }
        }

        private void StartVoiceoverCooldown()
        {
            _voiceoverCooldownCoroutine = StaticManager.BeginCoroutine(VoiceoverCooldownTimer());
        }

        private IEnumerator VoiceoverCooldownTimer()
        {
            _canPlayVoiceover = false;
            yield return new WaitForSecondsRealtime(2f);
            _canPlayVoiceover = true;
        }

        /// <summary>
        /// Subscriber method to add bot to <see cref="EnemyTargets"/>
        /// </summary>
        private void AddBotToTargetList(IPlayer bot)
        {
            var botOwner = bot.AIData.BotOwner;
            if (botOwner == null)
            {
                return;
            }

            if (!EnemyTargets.Contains(botOwner) && botOwner.BotsGroup.Enemies.ContainsKey(ModHelper.MainPlayer))
            {
                botOwner.BotsGroup.OnEnemyAdd += CheckEnemyAdded;
                botOwner.BotsGroup.OnEnemyRemove += CheckEnemyRemoved;
                botOwner.OnIPlayerDeadOrUnspawn += StartBotRemovalFromTargetList;
                EnemyTargets.Add(botOwner);
            }
        }

        /// <summary>
        /// Subscriber method to check the IPlayer added to the bot's Enemies dictionary is the main player
        /// </summary>
        private void CheckEnemyAdded(IPlayer iPlayer, EBotEnemyCause botEnemyCause)
        {
            var player = (Player)iPlayer;
            if (player != ModHelper.MainPlayer)
            {
                return;
            }

            var enemyTargetsNotInTargetList = ModHelper.GameWorld.AllAlivePlayersList
                .Where(x => x.IsAI && !EnemyTargets.Contains(x.AIData.BotOwner))
                .Select(x => x.AIData.BotOwner);
            foreach (BotOwner botOwner in enemyTargetsNotInTargetList)
            {
                EnemyTargets.Add(botOwner);
            }
        }

        /// <summary>
        /// Subscriber method to check the IPlayer removed from the bot's Enemies dictionary is the main player
        /// </summary>
        private void CheckEnemyRemoved(IPlayer iPlayer)
        {
            var player = (Player)iPlayer;
            if (player != ModHelper.MainPlayer)
            {
                return;
            }

            var targetsNotEnemiesWithPlayer = EnemyTargets.Where(x => !x.BotsGroup.Enemies.ContainsKey(player));
            foreach (BotOwner botOwner in targetsNotEnemiesWithPlayer)
            {
                EnemyTargets.Remove(botOwner);
            }
        }

        /// <summary>
        /// Subscriber method to remove bot from <see cref="EnemyTargets"/>
        /// </summary>
        private void StartBotRemovalFromTargetList(IPlayer bot)
        {
            var botOwner = bot.AIData.BotOwner;
            if (EnemyTargets.Contains(botOwner))
            {
                StartCoroutine(RemoveBotFromTargetList(bot));
            }
#if DEBUG
            else
            {
                ConsoleScreen.LogWarning("StartBotRemovalFromTargetList: EnemyTargets does not have this bot, skipping");
            }
#endif
        }

        private IEnumerator RemoveBotFromTargetList(IPlayer bot)
        {
            bot.OnIPlayerDeadOrUnspawn -= StartBotRemovalFromTargetList;

            var fsController = FireSupportController.Instance;
            if (fsController == null)
            {
                yield break;
            }
            var botPlayer = (Player)bot;
            var botOwner = bot.AIData.BotOwner;
            botOwner.BotsGroup.OnEnemyRemove -= CheckEnemyRemoved;
            //EnemyTargets.Remove(botOwner);
            EnemyTargets.RemoveAll(x => x == null);
            if (fsController.TargetsKilledByApache.TryGetValue(botPlayer, out string killedBy) && killedBy == VehicleName)
            {
                float killConfirmationTime = Random.Range(0.5f, 1.2f);
                yield return new WaitForSecondsRealtime(killConfirmationTime);
                PlayVoiceover(VoiceoverType.ApacheConfirmKills);
                fsController.TargetsKilledByApache.Remove(botPlayer);

                if (CurrentEnemyTarget == botOwner)
                {
                    FindNextTarget();
                }
            }
        }

        //private void SetupColliders()
        //{
        //    var apacheRoot = transform.GetChild(0).GetChild(0);
        //    var backRotorCollider = apacheRoot.GetChild(0).GetComponentInChildren<Collider>();
        //    var backBladesColliders = apacheRoot.GetChild(1).GetComponentInChildren<ComplexCollider>().Colliders;
        //    var mainRotorCollider = apacheRoot.GetChild(2).GetComponentInChildren<Collider>();
        //    var mainBladesColliders = apacheRoot.GetChild(3).GetComponentInChildren<ComplexCollider>().Colliders;
        //    var bodyColliders = apacheRoot.GetChild(4).GetComponentInChildren<ComplexCollider>().Colliders;
        //    var cockpitCollider = apacheRoot.GetChild(7).GetComponentInChildren<Collider>();
        //    var m230BaseCollider = m230Base.GetChild(1).GetComponent<Collider>();
        //    var m230GunCollider = m230Gun.GetChild(1).GetComponent<Collider>();
        //    var hellfireLeftBaseCollider = apacheRoot.GetChild(9).GetComponent<Collider>();
        //    var hellfireRightBaseCollider = apacheRoot.GetChild(11).GetComponent<Collider>();
        //    var hellfireLeftMissileCollider = apacheRoot.GetChild(9).GetChild(0).GetComponent<Collider>();
        //    var hellfireRightMissileCollider = apacheRoot.GetChild(11).GetChild(0).GetComponent<Collider>();
        //    var hydraLeftCollider = apacheRoot.GetChild(10).GetChild(1).GetComponent<Collider>();
        //    var hydraRightCollider = apacheRoot.GetChild(12).GetChild(1).GetComponent<Collider>();

        //    // Body collisions
        //    foreach (var collider in bodyColliders)
        //    {
        //        Physics.IgnoreCollision(collider, backRotorCollider);
        //        foreach (var collider2 in backBladesColliders)
        //        {
        //            Physics.IgnoreCollision(collider, collider2);
        //        }

        //        Physics.IgnoreCollision(collider, mainRotorCollider);
        //        foreach (var collider3 in mainBladesColliders)
        //        {
        //            Physics.IgnoreCollision(collider, collider3);
        //        }

        //        Physics.IgnoreCollision(collider, cockpitCollider);

        //        Physics.IgnoreCollision(collider, m230BaseCollider);
        //        Physics.IgnoreCollision(collider, m230GunCollider);

        //        Physics.IgnoreCollision(collider, hellfireLeftBaseCollider);
        //        Physics.IgnoreCollision(collider, hellfireLeftMissileCollider);
        //        Physics.IgnoreCollision(collider, hellfireRightBaseCollider);
        //        Physics.IgnoreCollision(collider, hellfireRightMissileCollider);

        //        Physics.IgnoreCollision(collider, hydraLeftCollider);
        //        Physics.IgnoreCollision(collider, hydraRightCollider);
        //    }

        //    // Back rotor collisions
        //    foreach (var collider in backBladesColliders)
        //    {
        //        Physics.IgnoreCollision(collider, backRotorCollider);
        //    }

        //    // Main rotor collisions
        //    foreach (var collider in mainBladesColliders)
        //    {
        //        Physics.IgnoreCollision(collider, mainRotorCollider);
        //    }

        //    // M230 collisions
        //    Physics.IgnoreCollision(m230BaseCollider, m230GunCollider);

        //    // Hellfire missile collisions
        //    Physics.IgnoreCollision(hellfireLeftBaseCollider, hellfireLeftMissileCollider);
        //    Physics.IgnoreCollision(hellfireRightBaseCollider, hellfireRightMissileCollider);
        //}

        private void ClampAngle(ref float angle, float min, float max)
        {
            angle = (angle > 180)
                ? angle - 360
                : angle;

            angle = Mathf.Clamp(angle, min, max);
        }

        private void ClampIncrementValue(ref float value, float increment, float min, float max)
        {
            value = Mathf.Clamp(value + increment, min, max);
        }

        public void AcquireTarget(Vector3 targetPos)
        {
            RotateM230Part(m230Base, targetPos, Vector3.right);
            RotateM230Part(m230Gun, targetPos, Vector3.up);

            //Vector3 enemyDir = Vector3.Normalize(targetPos - m230Gun.GetChild(0).position);
            //float shortestAngle = Vector3.Angle(m230Gun.forward, enemyDir);

            //if (CanFireM230 && !IsM230Firing && shortestAngle < 0.5f)
            //{
            //    FireM230();
            //}
        }

        public bool IsAimingAtTarget(Vector3 targetPos)
        {
            throw new NotImplementedException();
        }

        public bool IsTargetInAimingZone(Vector3 targetPos)
        {
            throw new NotImplementedException();
        }
    }
}
