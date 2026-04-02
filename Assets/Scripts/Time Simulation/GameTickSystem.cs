using System;

public class GameTickSystem
{
    private float currentTime;
    private int currentTick;

    private readonly float tickDuration;

    private float accumulator;

    public float TimeScale { get; set; } = 1f;

    public float CurrentTime => currentTime;
    public int CurrentTick => currentTick;
    public float TickDuration => tickDuration;

    public GameTickSystem(float tickDuration)
    {
        this.tickDuration = tickDuration;
    }



}