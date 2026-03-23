using UnityEngine;
using Actor;
using DG.Tweening;

public class WallRunState : ProcessState, IActorIniter
{
    private Actor.Animator _animator;
    private Transform _target;
    private WallRunInput _input;
    private BodyRotator _rotator;

    private const string _wallRunName = "WallRun";
    private const string _isRightDirectionWallRunName = "IsRightDirectionWallRun";

    private const float Speed = 5f;

    public void InitActor(ActorController actor)
    {
        _target = actor.transform;

        if (actor.TryGetInput(out _input)){};

        if (actor.TryGetSystem(out _rotator)){};
        if (actor.TryGetSystem(out _animator)){};
    }

    public override void Enter()
    {
        base.Enter();
        Run(_input.RunPath);

        _animator.SetBool(_wallRunName, true);
        _animator.SetBool(_isRightDirectionWallRunName, _input.IsRightDirection);
    }

    public override void Exit()
    {
        _animator.SetBool(_wallRunName, false);
        base.Enter();
    }

    private void Run(Transform[] runPath)
    {
        if (runPath == null || runPath.Length == 0)
        {
            FinishProcess();
            return;
        }

        var firstPoint = runPath[0].position;
        _rotator.RotateToInstant(firstPoint);

        DG.Tweening.Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < runPath.Length; i++)
        {
            Vector3 targetPos = runPath[i].position;
            Vector3 fromPos = i == 0 ? _target.position : runPath[i - 1].position;

            float distance = Vector3.Distance(fromPos, targetPos);
            float duration = distance / Speed;

            int capturedIndex = i;

            sequence.AppendCallback(() =>
            {
                _rotator.RotateToInstant(runPath[capturedIndex].position);
            });

            sequence.Append(
                DOTween.To(
                    () => _target.position,
                    x => _target.position = x,
                    targetPos,
                    duration
                ).SetEase(Ease.Linear)
            );
        }

        sequence.OnComplete(Finish);
    }

    private void Finish()
    {
        FinishProcess();
        _input.FinishWallRun();
    }
}