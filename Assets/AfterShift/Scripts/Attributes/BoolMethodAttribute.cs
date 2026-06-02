using UnityEngine;

namespace AfterShift.Runtime
{
    public sealed class ASBoolMethodAttribute : PropertyAttribute
    {
        public string TargetFieldName { get; }

        public ASBoolMethodAttribute(string targetFieldName)
        {
            TargetFieldName = targetFieldName;
        }
    }
}