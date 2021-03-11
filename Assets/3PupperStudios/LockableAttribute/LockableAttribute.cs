using AttributeUsage = System.AttributeUsageAttribute;
using Targets = System.AttributeTargets;
using PropertyAttribute = UnityEngine.PropertyAttribute;

namespace ThreePupperStudios.Lockable
{
    [AttributeUsage(Targets.Field)]
    public class LockableAttribute : PropertyAttribute
    {
        public bool locked = false;

        public bool rememberSelection;

        public bool showIcon;

        /// <summary>
        /// Sets the default locked state
        /// </summary>
        /// <param name="locked">Locked by default</param>
        /// <param name="rememberSelection">Keep inspector selection persistent</param>
        public LockableAttribute(bool locked = true, bool rememberSelection = true)
        {
            this.locked = locked;
            this.rememberSelection = rememberSelection;
            showIcon = true;
        }
    }
}