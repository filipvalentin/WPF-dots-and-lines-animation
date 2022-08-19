using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace collidingdots {
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

		public class line {
			public Line _line;
			public double x1, x2, y1, y2;
			public point pointingTo;
			public line(double x1, double y1, double x2, double y2, Canvas canvas) {//, point pointingTo
				this.x1 = x1; this.x2 = x2; this.y1 = y1; this.y2 = y2;
				//this.pointingTo = pointingTo;

				_line = new() {
					X1 = x1,
					Y1 = y1,//canvas.ActualHeight-
					X2 = x2,
					Y2 = y2,
					Stroke = Brushes.Black,
					StrokeThickness = 2.5
				};
				canvas.Children.Add(_line);
			}
			public void SetXYXY(double x1, double y1, double x2, double y2) {
				this.x1 = x1; _line.X1 = x1;
				this.y1 = y1; _line.Y1 = y1;
				this.x2 = x2; _line.X2 = x2;
				this.y2 = y2; _line.Y2 = y2;
			}

			public void DeleteLine(Canvas canvas) {
				canvas.Children.Remove(_line);
			}

		}

		public class point {
			public double x, y;
			public double angle;
			public Ellipse dot;
			public Canvas currentcanvas;
			public double travelDistance = 0.5;

			public int lasti, lastj;

			public List<line> lines = new();
			public List<point> pointsInProximity = new();

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
				Canvas.SetTop(dot, y);
			}

			public void SetXY(double x, double y) {
				Canvas.SetLeft(dot, x); this.x = x;
				Canvas.SetTop(dot, y); this.y = y;
			}

			public void Move() {
				double futurex = x + (travelDistance * Math.Cos(angle));
				double futurey = y + (travelDistance * Math.Sin(angle));

				if (futurex < 0 || futurex > currentcanvas.ActualWidth)
					angle = 3 * Math.PI - angle;
				if (futurey < 0 || futurey > currentcanvas.ActualHeight)
					angle = 2 * Math.PI - angle;

				SetXY(x + (travelDistance * Math.Cos(angle)), y + (travelDistance * Math.Sin(angle)));
				//Debug.WriteLine(futurey);
			}

			public void ManageLines(List<point> points, List<point>[,] pointMatrix, double radius, Canvas canvas, double maxLineLength) {
				foreach (point point in points) {
					if (IsInCircle(point.x, point.y, this.x, this.y, radius) && !pointsInProximity.Contains(point))
						pointsInProximity.Add(point);

					if (!IsInCircle(point.x, point.y, this.x, this.y, radius) && pointsInProximity.Contains(point))
						pointsInProximity.Remove(point);
				}

				foreach (line line in lines)
					line.DeleteLine(canvas);
				lines.Clear();

				if (x < maxLineLength) {
					if (y > (canvas.ActualHeight - maxLineLength) && y <= canvas.ActualHeight) {

					}
					if (y >= maxLineLength && y <= (canvas.ActualHeight - maxLineLength)) {

					}
					if (y < maxLineLength) {
						for (int i = 0; i < 2; i++)
							for (int j = 0; j < 2; j++)
								foreach (point point in pointMatrix[i, j])
									lines.Add(new(this.x, this.y, point.x, point.y, canvas));

						//for (int i = 0; i <= x / maxLineLength; i++)
						//	for (int j = 0; j <= y / maxLineLength; j++)
						//		//for (int j = (int)(canvas.ActualHeight / maxLineLength) - 1; j >= y / maxLineLength; j--)
						//		foreach (point point in pointMatrix[i, j])
						//			lines.Add(new(this.x, this.y, point.x, point.y, canvas));
					}
				}
				if (x > (canvas.ActualWidth - maxLineLength) && x <= canvas.ActualWidth) {
					if (y >= 0 && y < maxLineLength) { // y>=0 poate fi scos

					}
					if (y >= maxLineLength && y <= (canvas.ActualWidth - maxLineLength)) {

					}
					if (y > (canvas.ActualWidth - maxLineLength) && y <= canvas.ActualWidth) {

					}
				}
				if (x >= maxLineLength && x <= (canvas.ActualWidth - maxLineLength)) {
					if (y >= 0 && y < maxLineLength) {

					}
					if (y >= maxLineLength && y <= (canvas.ActualWidth - maxLineLength)) {

					}
					if (y > (canvas.ActualWidth - maxLineLength) && y <= canvas.ActualWidth) {

					}

				}
			}
		}

		public static bool IsInCircle(double x0, double y0, double x, double y, double R) {
			double dx = Math.Abs(x - x0);
			if (dx > R) return true;
			double dy = Math.Abs(y - y0);
			if (dy > R) return false;
			if (dx + dy <= R) return true;
			return dx * dx + dy * dy <= R * R;
		}

		public double speedMultiplier = 0.6;
		public List<point>? points;
		public int numberOfPoints = 200;
		public int radius = 10;
		public int maxLineLength = 50; //in px

		public List<point>[,] pointMatrix;

		private void OnLoaded(object sender, RoutedEventArgs e) {
			pointMatrix = new List<point>[(int)canvas.ActualWidth / maxLineLength, (int)(canvas.ActualHeight / maxLineLength)];
			for (int i = 0; i < pointMatrix.GetLength(0); i++)
				for (int j = 0; j < pointMatrix.GetLength(1); j++)
					pointMatrix[i, j] = new List<point>();

			InitializePoints();

			//CompositionTarget.Rendering += MovePoints2;
			CompositionTargetEx.Rendering += MovePoints2;
			//line l = new(100, 100, 200, 200, canvas);
			//l.SetXYXY(200, 330, 211, 444);
		}

		public void InitializePoints() {
			
			points = new List<point>();
			var random = new Random();
			//points.Add(new(100, 100, 0.5, 1, canvas));
			//points.Add(new(200, 200, 0.1, 1, canvas));
			for (int i = 0; i < numberOfPoints; i++) {
				double randomx = random.NextInt64(0, (long)canvas.ActualWidth);
				double randomy = random.NextInt64(0, (long)canvas.ActualHeight);
				double angle = (double)random.NextDouble() * 2 * Math.PI;
				var pointToAdd = new point(randomx, randomy, angle, random.NextDouble() * speedMultiplier + 0.2, canvas);
				points.Add(pointToAdd);//new point(randomx, randomy, angle, random.NextDouble() * speedMultiplier + 0.2, canvas)
				Manage_pointMatrix(pointToAdd);
			}
		}

		public void MovePoints2(object sender, EventArgs e) {
			foreach (point currentPoint in points) {
				currentPoint.Move();
				Manage_pointMatrix(currentPoint);

				List<point> points2 = new(points);
				points2.Remove(currentPoint);
				currentPoint.ManageLines(points2, pointMatrix, radius, canvas, maxLineLength);
			}
		}


		public void Manage_pointMatrix(point point) { //move the point in the arrays inside the grid matrix.
			int i = (int)point.x / maxLineLength;
			int j = (int)point.y / maxLineLength;
			if (!pointMatrix[i, j].Contains(point)) {
				pointMatrix[i, j].Add(point);
				pointMatrix[point.lasti, point.lastj].Remove(point);
				point.lasti = i; point.lastj = j;
			}
			//if(pointMatrix[i, j].Contains(point)){
		}
	}
}
