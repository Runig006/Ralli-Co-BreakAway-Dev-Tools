using UnityEngine;

public class SuspensionPhysicReader : RDRSReaderBase
{
    public enum SuspensionReaderParameterType
    {
        Grounded,
        SideForce,
        SideForceNormalize,
        SideForceDirection,
        Grip,
        SuspensionForce,
        SpringLength,
        SpringLengthNormalize,
        TurnAngle,
    }

    [SerializeField] private SuspensionReaderParameterType parameter;
    [SerializeField] private SuspensionPhysic suspension;

    public override object GetValue()
    {
        if (suspension == null)
        {
            return 0f;
        }

        switch (parameter)
        {
            case SuspensionReaderParameterType.Grounded:
                return suspension.GetGrounded() ? 1f : 0f;
            case SuspensionReaderParameterType.SideForce:
                return suspension.GetSideForce();
            case SuspensionReaderParameterType.SideForceNormalize:
                return suspension.GetSideForceNormalize();
            case SuspensionReaderParameterType.SideForceDirection:
                return suspension.GetSideForceDirection();
            case SuspensionReaderParameterType.Grip:
                return suspension.GetCurrentGrip();
            case SuspensionReaderParameterType.SuspensionForce:
                return suspension.GetCurrentSuspensionForce();
            case SuspensionReaderParameterType.SpringLength:
                return suspension.GetCurrentSpringLength();
            case SuspensionReaderParameterType.SpringLengthNormalize:
                return suspension.GetCurrentSpringLengthNormalize();
            case SuspensionReaderParameterType.TurnAngle:
                return suspension.GetCurrentTurnAngle();
            default:
                return 0f;
        }
    }
}
