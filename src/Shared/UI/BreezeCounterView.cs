using System;
using Godot;

public partial class BreezeCounterView : Control
{
    private const string CountLabelPath = "MarginContainer/CountLabel";
    private const string Layer1Path = "Icon/RotationLayers/Layer1";
    private const string Layer2Path = "Icon/RotationLayers/Layer2";

    private Label _countLabel;
    private Control _layer1;
    private Control _layer2;
    private Tween _valueTween;
    private Tween _pulseTween;

    private int _displayedValue;
    private int _targetValue;

    public override void _Ready()
    {
        _countLabel = GetNodeOrNull<Label>(CountLabelPath);
        _layer1 = GetNodeOrNull<Control>(Layer1Path);
        _layer2 = GetNodeOrNull<Control>(Layer2Path);

        if (_countLabel != null && int.TryParse(_countLabel.Text, out var parsed))
        {
            _displayedValue = Math.Max(0, parsed);
            _targetValue = _displayedValue;
        }

        WriteValue(_displayedValue);
    }

    public override void _Process(double delta)
    {
        if (_layer1 != null)
        {
            _layer1.Rotation += (float)(0.95d * delta);
        }

        if (_layer2 != null)
        {
            _layer2.Rotation -= (float)(0.7d * delta);
        }
    }

    public void SetCount(int value, bool animate = true)
    {
        var clamped = Math.Max(0, value);
        if (_countLabel == null)
        {
            _displayedValue = clamped;
            _targetValue = clamped;
            return;
        }

        if (clamped == _targetValue && clamped == _displayedValue)
        {
            return;
        }

        var from = _displayedValue;
        _targetValue = clamped;
        PlayPulse(from, clamped);

        if (!animate || Math.Abs(clamped - from) <= 1)
        {
            _valueTween?.Kill();
            _displayedValue = clamped;
            WriteValue(_displayedValue);
            return;
        }

        _valueTween?.Kill();
        var duration = Math.Clamp(Math.Abs(clamped - from) * 0.04d, 0.08d, 0.35d);
        _valueTween = GetTree().CreateTween();
        _valueTween.SetEase(Tween.EaseType.Out);
        _valueTween.SetTrans(Tween.TransitionType.Cubic);
        _valueTween.TweenMethod(Callable.From<float>(OnTweenValue), from, clamped, duration);
    }

    private void OnTweenValue(float rawValue)
    {
        var rounded = (int)MathF.Round(rawValue);
        if (rounded == _displayedValue)
        {
            return;
        }

        _displayedValue = rounded;
        WriteValue(_displayedValue);
    }

    private void PlayPulse(int from, int to)
    {
        if (_countLabel == null || from == to)
        {
            return;
        }

        _pulseTween?.Kill();
        _countLabel.Scale = Vector2.One;
        _countLabel.Modulate = to > from
            ? new Color(0.72f, 0.95f, 1.0f, 1.0f)
            : new Color(1.0f, 0.77f, 0.77f, 1.0f);

        _pulseTween = GetTree().CreateTween();
        _pulseTween.SetEase(Tween.EaseType.Out);
        _pulseTween.SetTrans(Tween.TransitionType.Quad);
        _pulseTween.TweenProperty(_countLabel, "scale", new Vector2(1.14f, 1.14f), 0.09d);
        _pulseTween.TweenProperty(_countLabel, "scale", Vector2.One, 0.12d);
        _pulseTween.TweenProperty(_countLabel, "modulate", Colors.White, 0.20d);
    }

    private void WriteValue(int value)
    {
        if (_countLabel == null)
        {
            return;
        }

        _countLabel.Text = value.ToString();
    }
}
