using UnityEngine;

public class ExecuteOnTrackEvents : RDRSNode
{
    public enum LoopMoment
    {
        TrackLoaded,
        TrackStart, //Is in the exact moment the car pass the spitter point
        TrackStartDelay, //Some time after the car pass the spliter point
    }


    [SerializeField] private LoopMoment loopMoment = LoopMoment.TrackLoaded;
    [SerializeField][Tooltip("Less is more priority")] private int priority = 0;
    [SerializeField] private RDRSNode[] executorsToCall;

    private float valueToSend = 0.0f; //Some executers have readers to know if the need "What to do" this is to "fake" the value

    public override object GetValue()
    {
        return this.valueToSend;
    }

    void Start()
    {
        #if FULL_GAME
        switch (loopMoment)
        {
            case LoopMoment.TrackLoaded:
                FindFirstObjectByType<TrackManager>().RegisterToTrackLoaded(Execute,priority);
                break;

            case LoopMoment.TrackStart:
                FindFirstObjectByType<GameManager>().RegisterToTrackStart(Execute,priority);
                break;

            case LoopMoment.TrackStartDelay:
                FindFirstObjectByType<GameManager>().RegisterToTrackStartDelay(Execute,priority);
                break;
        }
        #else
            this.Execute();
        #endif
    }


    public override void Execute()
    {
        if (!this.isActiveAndEnabled)
        {
            return;
        }

        this.valueToSend = 1.0f;
        foreach (RDRSNode executor in this.executorsToCall) 
        {
            if (executor != null && executor.isActiveAndEnabled)
            {
                executor.Execute();
            }
        }
        this.valueToSend = 0.0f;
    }

}
