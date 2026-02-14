using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class SparkTrigger : MonoBehaviour
{

    [SerializeField] ParticleSystem _particle;
    [SerializeField] SplineAnimate _anim;

    [Space(15)]
    public UnityEvent onAnimationFinish;

    public void playAnimation()
    {
        _anim.Play();
        _particle.Play();
        Invoke(nameof(callEvent), _particle.main.duration);
    }

    void callEvent()
    {
        onAnimationFinish?.Invoke();
    }

}
