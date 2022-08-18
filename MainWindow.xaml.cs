using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace collidingdots {	
	
	public class point {
		public double x, y;
		public double angle;
		public Ellipse dot;
		public Canvas currentcanvas;
		public double travelDistance = 0.5;

		public List<line> lines;

		public point(double x, double y, double angle, double travelDistance, Canvas canvas) {
			this.x = x;
			this.y = y;
			this.angle = angle;
			this.travelDistance = travelDistance;

			dot = new();
			dot.Stroke = dot.Fill = Brushes.Black;
			dot.Width = dot.Height = 4;

			currentcanvas = canvas;
			canvas.Children.Add(dot);
			Canvas.SetLeft(dot, x);
			Canvas.SetBottom(dot, y);
		}

		public void SetXY(double x, double y) {
			Canvas.SetLeft(dot, x); this.x = x;
			Canvas.SetBottom(dot, y); this.y = y;
		}

		public void Move() {
			double futurex = x + (travelDistance * Math.Cos(angle));
			double futurey = y + (travelDistance * Math.Sin(angle));

			if (futurex < 0 || futurex > currentcanvas.ActualWidth)
				angle = 3 * Math.PI - angle;
			if (futurey < 0 || futurey > currentcanvas.ActualHeight)
				angle = 2 * Math.PI - angle;

			SetXY(x + (travelDistance * Math.Cos(angle)), y+(travelDistance*Math.Sin(angle)));
		}
	}

	public class line {
		public Line _line;
		public double x1, x2, y1, y2;
		public line(double x1, double y1, double x2, double y2, Canvas canvas) {
			this.x1 = x1;
			this.x2 = x2;
			this.y1 = y1;
			this.y2 = y2;

			_line = new();
			_line.X1= x1;
			_line.Y1= y1;
			_line.X2= x2;
			_line.Y2= y2;
			_line.Stroke = Brushes.Black;
			_line.StrokeThickness = 3;
			canvas.Children.Add(_line);
		}
		public void SetXYXY(double x1, double y1, double x2, double y2) {
			this.x1 = x1; _line.X1 = x1;
			this.y1 = y1; _line.Y1 = y1;
			this.x2 = x2; _line.X2 = x2;
			this.y2 = y2; _line.Y2 = y2;
		}

	}

	public static class CompositionTargetEx {
		private static TimeSpan _last = TimeSpan.Zero;
		private static event EventHandler<RenderingEventArgs> _FrameUpdating;
		public static event EventHandler<RenderingEventArgs> Rendering {
			add {
				if (_FrameUpdating == null)
					CompositionTarget.Rendering += CompositionTarget_Rendering;
				_FrameUpdating += value;
			}
			remove {
				_FrameUpdating -= value;
				if (_FrameUpdating == null)
					CompositionTarget.Rendering -= CompositionTarget_Rendering;
			}
		}
		static void CompositionTarget_Rendering(object sender, EventArgs e) {
			RenderingEventArgs args = (RenderingEventArgs)e;
			if (args.RenderingTime == _last)
				return;
			_last = args.RenderingTime; _FrameUpdating(sender, args);
		}
	}

	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			//InitializePoints();
			//System.Windows.Media.CompositionTarget.Rendering += MovePoints2;
		}
				

		public double speedMultiplier = 0.6;
		public List<point>? points;
		public int numberOfPoints =200;
		public int radius = 10;

		private void OnLoaded(object sender, RoutedEventArgs e) {
			InitializePoints();
			//CompositionTarget.Rendering += MovePoints2;
			CompositionTargetEx.Rendering += MovePoints2;
			//line l = new(100, 200, 122, 323, canvas);
			//l.SetXYXY(200, 330, 211, 444);
		}

		public void InitializePoints() {
			points = new List<point>();
			var random = new Random();

			for (int i = 0; i < numberOfPoints; i++) {
				double randomx = random.NextInt64(0, (long)canvas.ActualWidth);
				double randomy = random.NextInt64(0, (long)canvas.ActualHeight);
				double angle = (double)random.NextDouble() * 2 * Math.PI;
				points.Add(new point(randomx, randomy, angle, random.NextDouble() * speedMultiplier + 0.2, canvas));
			}
		}

		public void MovePoints2(object sender, EventArgs e) {			
			foreach (point currentPoint in points)
				currentPoint.Move();
		}
	}
}
