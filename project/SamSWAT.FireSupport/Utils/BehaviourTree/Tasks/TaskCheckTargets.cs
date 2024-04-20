using EFT;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree.Tasks
{
    public class TaskCheckTargets : Node
    {
        private readonly Transform _origin;
        private readonly float _detectionAngle;
        private readonly float _detectionRadius;
        private readonly float _detectionRadiusSqr;
        private readonly float _detectionCloseRadiusSqr;
        private readonly float _aimDistanceLeewaySqr;


        private readonly int _minJobSize = 6;
        private int _frame = 0;

        public TaskCheckTargets(
            Transform origin,
            float detectionAngle,
            float detectionRadius,
            float detectionCloseRadius,
            float aimDistanceLeeway
        )
        {
            _origin = origin;
            _detectionAngle = detectionAngle;
            _detectionRadius = detectionRadius;
            _detectionRadiusSqr = Mathf.Pow(detectionRadius, 2);
            _detectionCloseRadiusSqr = Mathf.Pow(detectionCloseRadius, 2);
            _aimDistanceLeewaySqr = Mathf.Pow(aimDistanceLeeway, 2);
         }

        public override NodeState Evaluate()
        {
            _frame++;

            Player target = GetData("target") as Player;

            if (_frame < 10 && target != null)
            {
                state = NodeState.SUCCESS;
                return state;
            }

            if (_frame >= 10)
            {
                _frame = 0;

                if (
                    (target == null || !IsTargetVisible(target))
                    && !HasAnotherValidTarget(out target)
                )
                {
                    state = NodeState.FAILURE;
                    return state;
                }

                SetData("target", target);

                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }

        private bool HasAnotherValidTarget(out Player newTarget)
        {
            List<Player> aliveBots = ModHelper.GameWorld?.AllAlivePlayersList
                .Where(player => player.IsAI)
                .ToList();

            newTarget = null;

            if (aliveBots == null || aliveBots.Count == 0)
            {
                return false;
            }

            // Raycast job
            // TODO:
            // See if this can be optimised further
            NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(
                aliveBots.Count,
                Allocator.TempJob
            );

            NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(
                aliveBots.Count,
                Allocator.TempJob
            );

            for (int i = 0; i < aliveBots.Count; i++)
            {
                Player bot = aliveBots[i];
                Vector3 direction = GetBodyDirection(bot);

                raycastCommands[i] = new RaycastCommand(
                    _origin.position,
                    direction,
                    _detectionRadius + 50f,
                    ModHelper.LayerMaskWithBot
                );
            }

            JobHandle raycastJob = RaycastCommand.ScheduleBatch(
                raycastCommands,
                raycastHits,
                _minJobSize
            );

            raycastJob.Complete();
            raycastCommands.Dispose();

            for (int i = 0; i < aliveBots.Count; i++)
            {
                Player bot = aliveBots[i];

                if (bot.HealthController.IsAlive
                    && (raycastHits[i].point - bot.Position).sqrMagnitude <= _aimDistanceLeewaySqr)
                {
                    if (newTarget == null)
                    {
                        newTarget = bot;
                        continue;
                    }

                    Vector3 bodyDir = GetBodyDirection(bot);
                    Vector3 newBodyDir = GetBodyDirection(newTarget);

                    float deltaAngle = Vector3.Angle(_origin.forward, bodyDir);
                    float newDeltaAngle = Vector3.Angle(_origin.forward, newBodyDir);

                    if (deltaAngle < newDeltaAngle)
                    {
                        newTarget = bot;
                    }
                }
            }
            
            raycastHits.Dispose();

            if (newTarget == null)
            {
                return false;
            }

            return true;
        }

        private bool IsTargetVisible(Player target)
        {
            Vector3 targetDir = Vector3.Scale(
                GetBodyDirection(target),
                ModHelper.VECTOR3_IGNORE_Y
            );

            if (!IsTargetInDetectionCone(targetDir) || !IsTargetInLineOfSight(targetDir, out RaycastHit hitTarget))
            {
                return false;
            }

            Player detectedTarget = hitTarget.collider.GetComponentInParent<Player>();
            if (
                detectedTarget is null
                || detectedTarget.ProfileId != target.ProfileId
                || !detectedTarget.HealthController.IsAlive
            )
            {
                return false;
            }

            return true;
        }

        private bool IsTargetInLineOfSight(Vector3 targetDirection, out RaycastHit hitTarget)
        {
            return Physics.Raycast(
                _origin.position,
                targetDirection.normalized,
                out hitTarget,
                _detectionRadius + 50f,
                ModHelper.LayerMaskWithBot
            );
        }

        private bool IsTargetInDetectionCone(Vector3 targetDirection)
        {
            return targetDirection.sqrMagnitude <= _detectionRadiusSqr
                && targetDirection.sqrMagnitude > _detectionCloseRadiusSqr
                && Vector3.Dot(targetDirection.normalized, _origin.forward) > Mathf.Cos(_detectionAngle * 0.5f * Mathf.Deg2Rad);
        }

        private Vector3 GetBodyDirection(Player player)
        {
            return ModHelper.GetBodyPosition(player) - _origin.position;
        }
    }
}
