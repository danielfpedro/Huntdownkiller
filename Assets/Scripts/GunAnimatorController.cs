using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections;

public class GunAnimatorController : MonoBehaviour
{
    [Header("Clips")]
    public AnimationClip idleClip;
    public AnimationClip shotClip;
    public AnimationClip reloadClip;

    private GunController gunController;
    private Animator animator;
    private PlayableGraph graph;

    void Start()
    {
        gunController = GetComponent<GunController>();
        
        // Ensure Animator component exists
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }

        // Create the PlayableGraph
        graph = PlayableGraph.Create("GunAnimatorGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // Create an Output hooked to the Animator
        AnimationPlayableOutput.Create(graph, "Animation", animator);

        // Start the graph
        graph.Play();

        // Start Idle Loop
        PlayIdle();

        // Subscribe to events
        if (gunController != null)
        {
            gunController.onShot.AddListener(OnShot);
            gunController.onReloadStart.AddListener(OnReload);
        }
    }

    void OnDestroy()
    {
        // Graphs must be destroyed manually
        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }

    void PlayClip(AnimationClip clip, bool loop, bool returnToIdle = false)
    {
        if (clip == null || !graph.IsValid()) return;

        // Create a new clip playable
        var clipPlayable = AnimationClipPlayable.Create(graph, clip);

        // Connect it to the output (Source 0)
        var output = graph.GetOutput(0);
        output.SetSourcePlayable(clipPlayable);

        // If specific logic is needed (like returning to idle)
        StopAllCoroutines();
        if (returnToIdle)
        {
            StartCoroutine(ReturnToIdleRoutine(clip.length));
        }
    }

    void PlayIdle()
    {
        if (idleClip != null)
        {
            PlayClip(idleClip, true);
        }
    }

    void OnShot()
    {
        if (shotClip != null)
        {
            PlayClip(shotClip, false, true);
        }
    }

    void OnReload()
    {
        if (reloadClip != null)
        {
            PlayClip(reloadClip, false, true);
        }
    }

    IEnumerator ReturnToIdleRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayIdle();
    }
}
