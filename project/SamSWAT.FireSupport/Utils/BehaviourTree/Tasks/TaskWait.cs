using EFT;
using System.Collections;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree.Tasks
{
    public class TaskWait : Node
    {
        private readonly float _minTime; // Don't use zero as a value for either of these
        private readonly float _maxTime;
        
        private bool _timeUp = false;
        private float _randomTime = 0f;

        public TaskWait(float minTime, float maxTime)
        {
            _minTime = minTime;
            _maxTime = maxTime;
        }

        public override NodeState Evaluate()
        {
            if (_randomTime == 0f)
            {
                _randomTime = Random.Range(_minTime, _maxTime);

                StaticManager.BeginCoroutine(WaitTimer(_randomTime));
            }

            if (!_timeUp)
            {
                state = NodeState.RUNNING;
                return state;
            }

            // Reset variables on success
            _timeUp = false;
            _randomTime = 0f;

            state = NodeState.SUCCESS;
            return state;
        }

        private IEnumerator WaitTimer(float time)
        {
            yield return new WaitForSecondsRealtime(time);

            _timeUp = true;
        }
    }
}
