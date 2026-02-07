using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayerMovement))]
public class CharacterAnimatorController : MonoBehaviour
{
    #region Clip Configuration
    [Header("Animation Clips")]
    [Tooltip("The animation clip to play when the character is standing still.")]
    public AnimationClip idleClip;

    [Tooltip("The animation clip to play when the character is crouching.")]
    public AnimationClip crouchClip;
    #endregion

    // Dependencies
    [Header("References")]
    public Animator animator;
    private PlayerMovement playerMovement;

    // Playables API
    private PlayableGraph graph;
    private AnimationClip currentClip;

    private void Start()
    {
        if (animator == null)
        {
             animator = GetComponent<Animator>();
        }
        
        playerMovement = GetComponent<PlayerMovement>();

        InitializeGraph();
    }

    private void OnDestroy()
    {
        // Graphs must be destroyed manually to avoid memory leaks
        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }

    private void Update()
    {
        if (playerMovement == null) return;

        UpdateAnimationState();
    }

    #region Graph Management

    private void InitializeGraph()
    {
        // Create the graph
        graph = PlayableGraph.Create("CharacterAnimatorGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // Create an Output hooked to the Animator
        AnimationPlayableOutput.Create(graph, "Animation", animator);

        // Start the graph
        graph.Play();

        // Start default state
        PlayIdle();
    }

    private void PlayClip(AnimationClip clip)
    {
        if (clip == null || !graph.IsValid()) return;
        
        // Avoid recreating the same state
        if (currentClip == clip) return;

        currentClip = clip;

        // Create a new clip playable
        var clipPlayable = AnimationClipPlayable.Create(graph, clip);

        // Connect it to the output
        var output = graph.GetOutput(0);
        output.SetSourcePlayable(clipPlayable);
    }

    #endregion

    #region State Logic

    private void UpdateAnimationState()
    {
        // Prioritize states based on importance
        if (playerMovement.isCrouching)
        {
            PlayCrouch();
        }
        else
        {
            // Default to Idle state ("idle and crouch for now")
            PlayIdle();
        }
    }

    private void PlayIdle()
    {
        if (idleClip != null)
        {
            PlayClip(idleClip);
        }
    }

    private void PlayCrouch()
    {
        if (crouchClip != null)
        {
            PlayClip(crouchClip);
        }
    }

    #endregion
}
