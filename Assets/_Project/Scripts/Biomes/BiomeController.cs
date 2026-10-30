using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the root of a biome (MarineLogic, AfricaLogic...).
// Knows WHAT belongs to the biome and in which order it moves; BiomeManager decides WHEN.
public class BiomeController : MonoBehaviour
{
    [Serializable]
    public class TransitionStep
    {
        public BiomeTransitionElement element;
        [Tooltip("Seconds after the biome starts exiting")]
        public float exitDelay;
        [Tooltip("Seconds after the biome starts entering")]
        public float enterDelay;
    }

    [Header("Elements that move in/out of the screen")]
    [SerializeField] private List<TransitionStep> transitionSteps = new List<TransitionStep>();

    [Header("Logic: off as soon as the biome exits, on when it has fully entered")]
    [SerializeField] private List<Behaviour> biomeLogic = new List<Behaviour>();

    public IEnumerator ExitRoutine()
    {
        SetLogicEnabled(false);
        yield return PlaySteps(true);
        gameObject.SetActive(false);
    }

    public IEnumerator EnterRoutine()
    {
        gameObject.SetActive(true);
        // Activating the root turns the logic on (OnEnable): keep it off until the biome is in place
        SetLogicEnabled(false);
        foreach (var step in transitionSteps)
        {
            if (step.element != null)
                step.element.HideInstant();
        }

        yield return PlaySteps(false);
        SetLogicEnabled(true);
    }

    public void ShowInstant()
    {
        gameObject.SetActive(true);
        foreach (var step in transitionSteps)
        {
            if (step.element != null)
                step.element.ShowInstant();
        }
        SetLogicEnabled(true);
    }

    public void HideInstant()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator PlaySteps(bool exit)
    {
        // All the steps run in parallel, each one waiting its own delay
        List<Coroutine> running = new List<Coroutine>();
        foreach (var step in transitionSteps)
        {
            if (step.element != null)
                running.Add(StartCoroutine(PlayStep(step, exit)));
        }

        foreach (var coroutine in running)
            yield return coroutine;
    }

    private IEnumerator PlayStep(TransitionStep step, bool exit)
    {
        float delay = exit ? step.exitDelay : step.enterDelay;
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return exit ? step.element.Exit() : step.element.Enter();
    }

    private void SetLogicEnabled(bool value)
    {
        foreach (var logic in biomeLogic)
        {
            if (logic != null)
                logic.enabled = value;
        }
    }
}
