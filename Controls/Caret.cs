using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OpenFontWPFControls.Controls
{
    internal class Caret : Control
    {
        private AnimationClock _blinkAnimationClock;


        public static readonly DependencyProperty CaretBrushProperty =
            DependencyProperty.Register(
                nameof(CaretBrush),
                typeof(Brush),
                typeof(Caret),
                new FrameworkPropertyMetadata(Brushes.Black));

        public Brush CaretBrush
        {
            get => (Brush)GetValue(CaretBrushProperty);
            set => SetValue(CaretBrushProperty, value);
        }


        static Caret()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Caret), new FrameworkPropertyMetadata(typeof(Caret)));
        }

        public Caret()
        {
            SetBlinkAnimation(Visibility == Visibility.Visible, false);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == VisibilityProperty)
            {
                SetBlinkAnimation(Visibility == Visibility.Visible, false);
            }
        }

        private void SetBlinkAnimation(bool visible, bool positionChanged)
        {
            uint blinkInterval = GetCaretBlinkTime();

            if (blinkInterval > 0)
            {
                Duration blinkDuration = new Duration(TimeSpan.FromMilliseconds(blinkInterval * 2));

                if (_blinkAnimationClock == null || _blinkAnimationClock.Timeline.Duration != blinkDuration)
                {
                    DoubleAnimationUsingKeyFrames blinkAnimation = new DoubleAnimationUsingKeyFrames();
                    blinkAnimation.BeginTime = null;
                    blinkAnimation.RepeatBehavior = RepeatBehavior.Forever;
                    blinkAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(0.0)));
                    blinkAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(0.5)));
                    blinkAnimation.Duration = blinkDuration;

                    Timeline.SetDesiredFrameRate(blinkAnimation, 10);

                    _blinkAnimationClock = blinkAnimation.CreateClock();
                    _blinkAnimationClock.Controller?.Begin();

                    ApplyAnimationClock(UIElement.OpacityProperty, _blinkAnimationClock);
                }
            }
            else if (_blinkAnimationClock != null)
            {
                ApplyAnimationClock(UIElement.OpacityProperty, null);
                _blinkAnimationClock = null;
            }

            if (_blinkAnimationClock != null)
            {
                if (visible && (!(_blinkAnimationClock.CurrentState == ClockState.Active) || positionChanged))
                {
                    _blinkAnimationClock.Controller?.Begin();
                }
                else if (!visible)
                {
                    _blinkAnimationClock.Controller?.Stop();
                }
            }
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        private static extern uint GetCaretBlinkTime();
    }
}
