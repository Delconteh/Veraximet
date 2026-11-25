using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Veraximet.Wpf.Features.Verification.Views
{
    public sealed class MeasurementPlot : Control
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(MeasurementPlotData), typeof(MeasurementPlot),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public MeasurementPlotData? Data { get => (MeasurementPlotData?)GetValue(DataProperty); set => SetValue(DataProperty, value); }

        public MeasurementPlot()
        {
            ClipToBounds = true;
            SetResourceReference(ForegroundProperty, "PrimaryTextBrush");
            SetResourceReference(BackgroundProperty, "Workspace.InputBrush");
            SetResourceReference(BorderBrushProperty, "StrokeBrush");
            SetResourceReference(FontFamilyProperty, "Font.Primary");
        }

        protected override Size MeasureOverride(Size availableSize) =>
            new(double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,Data is null ? 78 : 82 + Data.Readings.Count * 25);

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);

            dc.DrawRoundedRectangle(Background ?? Brushes.Transparent, new Pen(BorderBrush ?? Brushes.Gray, 1), bounds, 8, 8);

            if (ActualWidth < 200) 
                return;

            Brush accent = ResourceBrush("AccentBrush", Brushes.SeaGreen);
            Brush danger = ResourceBrush("DangerBrush", Brushes.IndianRed);
            Brush muted = ResourceBrush("MutedTextBrush", Brushes.Gray);
            Brush warning = ResourceBrush("WarningBrush", Brushes.Goldenrod);

            if (Data is not { } data)
            {
                Text(dc, "Complete the point to plot its readings.", 15, 22, muted);
                return;
            }

            double left = 48, right = ActualWidth - 24;
            double top = 25, bottom = top + data.Readings.Count * 25;
            double mpe = (double)data.Mpe;
            double extent = Math.Max(mpe, (double)data.Result.MaximumAbsoluteError);

            extent = extent > 0 ? extent * 1.18 : 1;

            double X(double error) => left + (error / extent + 1) * (right - left) / 2;
            double zero = X(0), low = X(-mpe), high = X(mpe);

            dc.PushOpacity(0.14);
            dc.DrawRectangle(accent, null, new Rect(low, top, high - low, bottom - top));
            dc.Pop();

            var limitPen = new Pen(accent, 1);

            dc.DrawLine(limitPen, new Point(low, top), new Point(low, bottom));
            dc.DrawLine(limitPen, new Point(high, top), new Point(high, bottom));

            dc.DrawLine(new Pen(muted, 1.5), new Point(zero, top), new Point(zero, bottom));

            var meanPen = new Pen(warning, 1.5) { DashStyle = DashStyles.Dash };
            double meanX = X((double)data.Result.MeanError);

            dc.DrawLine(meanPen, new Point(meanX, top), new Point(meanX, bottom));

            for (int i = 0; i < data.Readings.Count; i++)
            {
                decimal error = data.Readings[i] - data.Reference;
                double x = X((double)error), y = top + i * 25 + 12.5;
                Brush brush = Math.Abs(error) <= data.Mpe ? accent : danger;
                Text(dc, $"M{i + 1}", 10, y - 8, muted);
                dc.DrawLine(new Pen(brush, 1.5), new Point(zero, y), new Point(x, y));
                dc.DrawEllipse(brush, null, new Point(x, y), 4.5, 4.5);
                if (Math.Abs(error) > data.Mpe)
                    dc.DrawEllipse(null, new Pen(brush, 1), new Point(x, y), 7, 7);
            }
            dc.DrawLine(new Pen(muted, 1), new Point(left, bottom), new Point(right, bottom));
            CenterText(dc, "0", zero, bottom + 4, muted);
            if (mpe > 0 && high - zero > 45)
            {
                CenterText(dc, (-data.Mpe).ToString("G6", CultureInfo.CurrentCulture), low, bottom + 4, muted);
                CenterText(dc, "+" + data.Mpe.ToString("G6", CultureInfo.CurrentCulture), high, bottom + 4, muted);
            }
            Text(dc, "Error relative to reference · dashed: mean", 10, 5, muted);
            Text(dc, $"MPE ±{data.Mpe:G6}  |  axis ±{extent:G4}", 10, bottom + 25, muted);
        }



        private Brush ResourceBrush(string key, Brush fallback) => 
            TryFindResource(key) as Brush ?? fallback;


        private FormattedText Format(string value, Brush brush) => new(value, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            11, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        private void Text(DrawingContext dc, string value, double x, double y, Brush brush) =>
            dc.DrawText(Format(value, brush), new Point(x, y));
        private void CenterText(DrawingContext dc, string value, double x, double y, Brush brush)
        {
            var text = Format(value, brush);
            dc.DrawText(text, new Point(Math.Clamp(x - text.Width / 2, 0, Math.Max(0, ActualWidth - text.Width)), y));
        }
    }
}
