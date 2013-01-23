using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System;

namespace PascalSharp
{
    public sealed class PascalGraphics
    {
        public delegate double? GraphY(double tmpX);

        private Panel canvas;
        private List<UIElement> buffer = new List<UIElement>();

        public double Width
        {
            get { return canvas.ActualWidth; }
        }
        public double Height
        {
            get { return canvas.ActualHeight; }
        }

        public Point Position = new Point(0, 0);
        public DoubleCollection LineStyle = PascalLineStyles.Solid;
        public double LineThickness = PascalLineThicknesses.NormWidth;
        public PenLineCap LineCapStyle = PenLineCap.Flat;
        public PenLineJoin LineJoinStyle = PenLineJoin.Round;

        private SolidColorBrush brush = new SolidColorBrush(PascalColors.White);
        public Color Color
        {
            get { return brush.Color; }
            set { brush = new SolidColorBrush(value); }
        }

        public PascalGraphics(Panel newCanvas)
        {
            canvas = newCanvas;
        }

        public void Render(bool clear)
        {
            if (clear) ClearCanvas();
            foreach (UIElement element in buffer) canvas.Children.Add(element);
            ClearBuffer();
        }

        public void ClearCanvas()
        {
            canvas.Children.RemoveRange(0, canvas.Children.Count);
        }

        public void ClearBuffer()
        {
            buffer.Clear();
        }

        public void PutPixel(Point at)
        {
            Rectangle tmpPixel = new Rectangle();
            Canvas.SetLeft(tmpPixel, at.X);
            Canvas.SetTop(tmpPixel, at.Y);
            tmpPixel.Width = 1;
            tmpPixel.Height = 1;
            tmpPixel.Stroke = brush;
            buffer.Add(tmpPixel);
        }

        public void MoveTo(Point to)
        {
            Position.X = to.X;
            Position.Y = to.Y;
        }

        public void MoveRelative(Point delta)
        {
            Position.X += delta.X;
            Position.Y += delta.Y;
        }

        public void Line(Point from, Point to)
        {
            Line tmpLine = new Line();
            tmpLine.X1 = from.X;
            tmpLine.Y1 = from.Y;
            tmpLine.X2 = to.X;
            tmpLine.Y2 = to.Y;
            tmpLine.Stroke = brush;
            tmpLine.StrokeThickness = LineThickness;
            tmpLine.StrokeDashArray = LineStyle;
            tmpLine.StrokeDashCap = LineCapStyle;
            tmpLine.StrokeLineJoin = LineJoinStyle;
            buffer.Add(tmpLine);
        }

        public void LineTo(Point to)
        {
            Line(Position, to);
            MoveTo(to);
        }

        public void LineRelative(Point delta)
        {
            LineTo(new Point(Position.X + delta.X, Position.Y + delta.Y));
        }

        public void Rectangle(Point topLeft, Point bottomRight)
        {
            Rectangle tmpRectangle = new Rectangle();
            tmpRectangle.Margin = new Thickness(topLeft.X, topLeft.Y, 0, 0);
            tmpRectangle.Width = bottomRight.X - topLeft.X;
            tmpRectangle.Height = bottomRight.Y - topLeft.Y;
            tmpRectangle.Stroke = brush;
            tmpRectangle.StrokeThickness = LineThickness;
            tmpRectangle.StrokeDashArray = LineStyle;
            tmpRectangle.StrokeDashCap = LineCapStyle;
            tmpRectangle.StrokeLineJoin = LineJoinStyle;
            tmpRectangle.Fill = brush;
            buffer.Add(tmpRectangle);
        }

        public KeyValuePair<Point, Point> Ellipse(Point at, double alpha, double beta, Size radiuses)
        {
            //alpha = Math.Abs(alpha) % 360;
            //beta = Math.Abs(beta) % 360;

            //if (beta - alpha >= 360d) beta = alpha + 360d;

            Point from = new Point((at.X + Math.Cos(ToRadians(alpha)) * radiuses.Width), (at.Y - Math.Sin(ToRadians(alpha)) * radiuses.Height));
            Point to = new Point((at.X + Math.Cos(ToRadians(beta)) * radiuses.Width), (at.Y - Math.Sin(ToRadians(beta)) * radiuses.Height));

            Path tmpPath = new Path();

            ArcSegment tmpArc = new ArcSegment();
            tmpArc.Point = to;
            tmpArc.Size = radiuses;
            tmpArc.SweepDirection = SweepDirection.Counterclockwise;
            tmpArc.IsLargeArc = (Math.Abs(beta - alpha) > 180d ? true : false);

            PathFigure tmpFigure = new PathFigure();
            tmpFigure.StartPoint = from;
            tmpFigure.Segments.Add(tmpArc);

            PathGeometry tmpGeometry = new PathGeometry();
            tmpGeometry.Figures.Add(tmpFigure);

            CombinedGeometry tmpCombined = new CombinedGeometry();
            tmpCombined.GeometryCombineMode = GeometryCombineMode.Exclude;
            tmpCombined.Geometry1 = tmpGeometry;

            tmpFigure = new PathFigure();
            tmpFigure.StartPoint = from;
            tmpFigure.Segments.Add(new LineSegment(at, false));
            tmpFigure.Segments.Add(new LineSegment(to, false));
            tmpFigure.IsClosed = true;
            tmpGeometry = new PathGeometry();
            tmpGeometry.Figures.Add(tmpFigure);

            tmpCombined.Geometry2 = tmpGeometry;

            tmpPath.Data = tmpCombined;
            tmpPath.Stroke = brush;
            tmpPath.StrokeThickness = LineThickness;
            tmpPath.StrokeDashArray = LineStyle;
            tmpPath.StrokeDashCap = LineCapStyle;
            tmpPath.StrokeLineJoin = LineJoinStyle;
            tmpPath.Fill = brush;
            
            buffer.Add(tmpPath);

            return new KeyValuePair<Point, Point>(from, to);
        }

        public void Graph(GraphY graphFunction, Rect bounds)
        {
            if (graphFunction != null)
            {
                Point source = new Point(), DPP = new Point(bounds.Width / Width, bounds.Height / Height);
                Point? tmpDst = null, prevDst = null;
                double tmpWidth = (int)Width, tmpHeight = (int)Height;
                double? tmpY = 0;

                for (int i = 0; i < tmpWidth; ++i)
                {
                    prevDst = tmpDst;
                    source.X = bounds.X + (i * DPP.X);
                    tmpY = graphFunction(source.X);
                    if (tmpY.HasValue)
                    {
                        source.Y = tmpY.Value;
                        tmpDst = new Point(i, (source.Y - bounds.Y) / DPP.Y);
                        if (prevDst.HasValue && Math.Abs(IsVisibleVertical(tmpDst.Value.Y, 0, tmpHeight) + IsVisibleVertical(prevDst.Value.Y, 0, tmpHeight)) < 2 && (Math.Abs(tmpDst.Value.Y) + Math.Abs(prevDst.Value.Y)) < 10000)
                            Line(new Point(prevDst.Value.X, Height - prevDst.Value.Y), new Point(tmpDst.Value.X, Height - tmpDst.Value.Y));
                    }
                    else
                        tmpDst = null;
                }
            }
        }

        public void Text(string data, Point at)
        {
            TextBlock tmpText = new TextBlock();
            tmpText.Margin = new Thickness(at.X, at.Y, 0, 0);
            tmpText.FontFamily = new FontFamily("Courier New");
            tmpText.Text = data;
            tmpText.Foreground = brush;
            buffer.Add(tmpText);
        }

        public double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public double ToRadians(double alpha)
        {
            return alpha * Math.PI / 180d;
        }

        public int IsVisibleVertical(double y, double atY, double height)
        {
            if (y > atY + height) return 1;
            if (y >= atY) return 0;
            return -1;
        }
    }

    /*public sealed class PascalGraphics
    {
        public delegate double? GraphY(double tmpX);

        private Panel canvas;
        private Storyboard storyboard;
        private double timeMultiplier;
        private List<UIElement> buffer = new List<UIElement>();
        private double currentTime = 0;

        public double Width
        {
            get { return canvas.ActualWidth; }
        }
        public double Height
        {
            get { return canvas.ActualHeight; }
        }

        public Point Position = new Point(0, 0);
        public DoubleCollection LineStyle = PascalLineStyles.Solid;
        public double LineThickness = PascalLineThicknesses.NormWidth;
        public PenLineCap LineCapStyle = PenLineCap.Flat;
        public PenLineJoin LineJoinStyle = PenLineJoin.Round;

        private SolidColorBrush brush = new SolidColorBrush(PascalColors.White);
        public Color Color
        {
            get { return brush.Color; }
            set { brush = new SolidColorBrush(value); }
        }

        public ArcSegment testArc;

        public PascalGraphics(Panel newCanvas, Storyboard newStoryboard, double slowness)
        {
            canvas = newCanvas;
            storyboard = newStoryboard;
            timeMultiplier = slowness;
        }

        public void Render(bool clear)
        {
            if (clear) ClearCanvas();
            foreach (UIElement element in buffer) canvas.Children.Add(element);
            ClearBuffer();
            storyboard.Begin();
        }

        public void ClearCanvas()
        {
            canvas.Children.RemoveRange(0, canvas.Children.Count);
        }

        public void ClearBuffer()
        {
            buffer.Clear();
            currentTime = 0;
        }

        public void PutPixel(Point at)
        {
            Rectangle tmpPixel = new Rectangle();
            Canvas.SetLeft(tmpPixel, at.X);
            Canvas.SetTop(tmpPixel, at.Y);
            tmpPixel.Width = 1;
            tmpPixel.Height = 1;
            tmpPixel.Stroke = brush;
            buffer.Add(tmpPixel);
        }

        public void MoveTo(Point to)
        {
            Position.X = to.X;
            Position.Y = to.Y;
        }

        public void MoveRelative(Point delta)
        {
            Position.X += delta.X;
            Position.Y += delta.Y;
        }

        public void Line(Point from, Point to)
        {
            Line tmpLine = new Line();
            tmpLine.X1 = from.X;
            tmpLine.Y1 = from.Y;
            tmpLine.X2 = from.X;
            tmpLine.Y2 = from.Y;
            tmpLine.Stroke = brush;
            tmpLine.StrokeThickness = LineThickness;
            tmpLine.StrokeDashArray = LineStyle;
            tmpLine.StrokeDashCap = LineCapStyle;
            tmpLine.StrokeLineJoin = LineJoinStyle;
            buffer.Add(tmpLine);

            double tmpTime = Distance(from, to) * timeMultiplier;

            DoubleAnimation tmpAnimation = new DoubleAnimation();
            Storyboard.SetTarget(tmpAnimation, tmpLine);
            Storyboard.SetTargetProperty(tmpAnimation, new PropertyPath(System.Windows.Shapes.Line.X2Property));
            tmpAnimation.To = to.X;
            tmpAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(tmpTime));
            tmpAnimation.BeginTime = TimeSpan.FromMilliseconds(currentTime);
            storyboard.Children.Add(tmpAnimation);

            tmpAnimation = new DoubleAnimation();
            Storyboard.SetTarget(tmpAnimation, tmpLine);
            Storyboard.SetTargetProperty(tmpAnimation, new PropertyPath(System.Windows.Shapes.Line.Y2Property));
            tmpAnimation.To = to.Y;
            tmpAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(tmpTime));
            tmpAnimation.BeginTime = TimeSpan.FromMilliseconds(currentTime);
            storyboard.Children.Add(tmpAnimation);

            currentTime += tmpTime;
        }

        public void LineTo(Point to)
        {
            Line(Position, to);
            MoveTo(to);
        }

        public void LineRelative(Point delta)
        {
            LineTo(new Point(Position.X + delta.X, Position.Y + delta.Y));
        }

        public void Rectangle(Point topLeft, Point bottomRight)
        {
            Rectangle tmpRectangle = new Rectangle();
            tmpRectangle.Margin = new Thickness(topLeft.X, topLeft.Y, 0, 0);
            tmpRectangle.Width = 0;
            tmpRectangle.Height = 0;
            tmpRectangle.Stroke = brush;
            tmpRectangle.StrokeThickness = LineThickness;
            tmpRectangle.StrokeDashArray = LineStyle;
            tmpRectangle.StrokeDashCap = LineCapStyle;
            tmpRectangle.StrokeLineJoin = LineJoinStyle;
            buffer.Add(tmpRectangle);

            double tmpTime = Distance(topLeft, bottomRight) * timeMultiplier;

            DoubleAnimation tmpAnimation = new DoubleAnimation();
            Storyboard.SetTarget(tmpAnimation, tmpRectangle);
            Storyboard.SetTargetProperty(tmpAnimation, new PropertyPath(System.Windows.Shapes.Rectangle.WidthProperty));
            tmpAnimation.To = bottomRight.X - topLeft.X;
            tmpAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(tmpTime));
            tmpAnimation.BeginTime = TimeSpan.FromMilliseconds(currentTime);
            storyboard.Children.Add(tmpAnimation);

            tmpAnimation = new DoubleAnimation();
            Storyboard.SetTarget(tmpAnimation, tmpRectangle);
            Storyboard.SetTargetProperty(tmpAnimation, new PropertyPath(System.Windows.Shapes.Rectangle.HeightProperty));
            tmpAnimation.To = bottomRight.Y - topLeft.Y;
            tmpAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(tmpTime));
            tmpAnimation.BeginTime = TimeSpan.FromMilliseconds(currentTime);
            storyboard.Children.Add(tmpAnimation);

            currentTime += tmpTime;
        }

        public void Ellipse(Point at, double alpha, double beta, Size radiuses)
        {
            Point from = new Point((at.X + Math.Cos(ToRadians(alpha)) * radiuses.Width), (at.Y - Math.Sin(ToRadians(alpha)) * radiuses.Height));
            Point to = new Point((at.X + Math.Cos(ToRadians(beta)) * radiuses.Width), (at.Y - Math.Sin(ToRadians(beta)) * radiuses.Height));

            Path tmpPath = new Path();

            testArc = new ArcSegment();
            testArc.Point = from;
            testArc.Size = radiuses;
            testArc.SweepDirection = SweepDirection.Counterclockwise;
            testArc.IsLargeArc = (Math.Abs(beta - alpha) > 180d ? true : false);

            PathFigure tmpFigure = new PathFigure();
            tmpFigure.StartPoint = from;
            tmpFigure.Segments.Add(testArc);

            PathGeometry tmpGeometry = new PathGeometry();
            tmpGeometry.Figures.Add(tmpFigure);

            tmpPath.Data = tmpGeometry;
            tmpPath.Stroke = brush;
            tmpPath.StrokeThickness = LineThickness;
            tmpPath.StrokeDashArray = LineStyle;
            tmpPath.StrokeDashCap = LineCapStyle;
            tmpPath.StrokeLineJoin = LineJoinStyle;
            buffer.Add(tmpPath);

            double tmpTime = Distance(from, to) * timeMultiplier;

            PointAnimation tmpAnimation = new PointAnimation();
            Storyboard.SetTarget(tmpAnimation, testArc);
            Storyboard.SetTargetProperty(tmpAnimation, new PropertyPath(System.Windows.Media.ArcSegment.PointProperty));
            tmpAnimation.To = to;
            tmpAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(tmpTime));
            tmpAnimation.BeginTime = TimeSpan.FromMilliseconds(currentTime);
            storyboard.Children.Add(tmpAnimation);

            currentTime += tmpTime;

            //tmpPath.Data = Geometry.Parse(
            //    "M " + 
            //    (at.X + Math.Cos(ToRadians(alpha)) * radiuses.X) + "," + (at.Y - Math.Sin(ToRadians(alpha)) * radiuses.Y) + 
            //    " A " + 
            //    radiuses.X + "," + radiuses.Y + 
            //    " 0 " + 
            //    (Math.Abs(alpha - beta) > 180d ? 1 : 0) + 
            //    " 0 " + 
            //MessageBox.Show("M " + (at.X + Math.Cos(ToRadians(alpha)) * radiuses.X) + "," + (at.Y - Math.Sin(ToRadians(alpha)) * radiuses.Y) + " A " + radiuses.X + "," + radiuses.Y + " 0 " + (Math.Abs(alpha - beta) > 180d ? 1 : 0) + " 0 " + (at.X + Math.Cos(ToRadians(beta)) * radiuses.X) + "," + (at.Y - Math.Sin(ToRadians(beta)) * radiuses.Y));
        }

        public void Graph(GraphY graphFunction, Rect bounds)
        {
            if (graphFunction != null)
            {
                Point source = new Point(), DPP = new Point(bounds.Width / Width, bounds.Height / Height);
                Point? tmpDst = null, prevDst = null;
                double tmpWidth = (int)Width, tmpHeight = (int)Height;
                double? tmpY = 0;

                for (int i = 0; i < tmpWidth; ++i)
                {
                    prevDst = tmpDst;
                    source.X = bounds.X + (i * DPP.X);
                    tmpY = graphFunction(source.X);
                    if (tmpY.HasValue)
                    {
                        source.Y = tmpY.Value;
                        tmpDst = new Point(i, (source.Y - bounds.Y) / DPP.Y);
                        if (prevDst.HasValue && Math.Abs(IsVisibleVertical(tmpDst.Value.Y, 0, tmpHeight) + IsVisibleVertical(prevDst.Value.Y, 0, tmpHeight)) < 2 && (Math.Abs(tmpDst.Value.Y) + Math.Abs(prevDst.Value.Y)) < 10000)
                            Line(new Point(prevDst.Value.X, Height - prevDst.Value.Y), new Point(tmpDst.Value.X, Height - tmpDst.Value.Y));
                    }
                    else
                        tmpDst = null;
                }
            }
        }

        public double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public double ToRadians(double alpha)
        {
            return alpha * Math.PI / 180d;
        }

        public int IsVisibleVertical(double y, double atY, double height)
        {
            if (y > atY + height) return 1;
            if (y >= atY) return 0;
            return -1;
        }
    }*/

    public static class PascalColors
    {
        public static Color Black = Colors.Black;
        public static Color Maroon = Colors.Maroon;
        public static Color Green = Colors.Green;
        public static Color Olive = Colors.Olive;
        public static Color Navy = Colors.Navy;
        public static Color Purple = Colors.Purple;
        public static Color Teal = Colors.Teal;
        public static Color Gray = Colors.Gray;
        public static Color Silver = Colors.Silver;
        public static Color Red = Colors.Red;
        public static Color Lime = Colors.Lime;
        public static Color Yellow = Colors.Yellow;
        public static Color Blue = Colors.Blue;
        public static Color Fuchsia = Colors.Fuchsia;
        public static Color Aqua = Colors.Aqua;
        public static Color LightGray = Colors.Silver;
        public static Color DarkGray = Colors.Gray;
        public static Color White = Colors.White;

        private static Color[] byNumber = { Colors.Black, Colors.Maroon, Colors.Green, Colors.Olive, Colors.Navy, Colors.Purple, Colors.Teal, Colors.Gray, Colors.Silver, Colors.Red, Colors.Lime, Colors.Yellow, Colors.Blue, Colors.Fuchsia, Colors.Aqua, Colors.Silver, Colors.Gray, Colors.White };

        public static Color ById(int index)
        {
            if (index >= 0 && index <= 16) return byNumber[index];
            else return Black;
        }
    }

    public static class PascalLineStyles
    {
        public static DoubleCollection Solid = null;
        public static DoubleCollection Dotted = new DoubleCollection() { 1, 1 };
        public static DoubleCollection Dashed = new DoubleCollection() { 4, 4 };
    }

    public static class PascalLineThicknesses
    {
        public static double None = 0;
        public static double NormWidth = 1;
        public static double ThickWidth = 2;
        public static double EpicMealTime = 8;
    }
}
