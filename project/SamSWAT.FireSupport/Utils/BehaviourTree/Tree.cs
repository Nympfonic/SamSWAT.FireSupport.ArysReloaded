using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree
{
    public abstract class Tree : MonoBehaviour, IBatchUpdate
    {
        private Node _root = null;

        public void BatchUpdate()
        {
            _root?.Evaluate();
        }

        protected virtual void Start()
        {
            _root = SetupTree();
        }        

        protected abstract Node SetupTree();
    }
}
