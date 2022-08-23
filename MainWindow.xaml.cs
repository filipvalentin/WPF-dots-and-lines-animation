using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;

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
				GC.Collect();
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

			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce; //does smth
		}

		public class line {
			public Line _line;
			public double x1, y1, x2, y2;
			public point pointingFrom, pointingTo;
			public static Canvas canvas;
			public static double lineThickness;
			//public line(double x1, double y1, double x2, double y2, point pointingTo) {//
			//	this.x1 = x1; this.x2 = x2; this.y1 = y1; this.y2 = y2;
			//	this.pointingTo = pointingTo;

			//	_line = new() {
			//		X1 = x1,
			//		Y1 = y1,
			//		X2 = x2,
			//		Y2 = y2,
			//		Stroke = Brushes.DimGray,
			//		StrokeThickness = lineThickness
			//	};
			//	canvas.Children.Add(_line);
			//}
			public line(point pointingFrom, point pointingTo) {//
				this.x1 = pointingFrom.x; this.y1 = pointingFrom.y;
				this.x2 = pointingTo.x; this.y2 = pointingTo.y;
				this.pointingFrom = pointingFrom; this.pointingTo = pointingTo;

				_line = new() {
					X1 = x1,
					Y1 = y1,
					X2 = x2,
					Y2 = y2,
					Stroke = Brushes.DimGray,
					StrokeThickness = 2.3
				};
				canvas.Children.Add(_line);
			}
			public void SetXYXY(double x1, double y1, double x2, double y2) {
				this.x1 = x1; _line.X1 = x1;
				this.y1 = y1; _line.Y1 = y1;
				this.x2 = x2; _line.X2 = x2;
				this.y2 = y2; _line.Y2 = y2;
			}
			public void SetXYXY(point origin, point destination) {
				this.x1 = origin.x; _line.X1 = origin.x;
				this.y1 = origin.y; _line.Y1 = origin.y;
				this.x2 = destination.x; _line.X2 = destination.x;
				this.y2 = destination.y; _line.Y2 = destination.y;
				this.pointingFrom = origin;
				this.pointingTo = destination;
			}

			public void DeleteLine() {
				canvas.Children.Remove(_line);
			}

		}

		public class point {
			public double x, y;
			public double angle;
			public Ellipse dot;
			public static Canvas currentcanvas;
			public double travelDistance = 0.5;
			public static double radius;
			public static double dotRadius;
			public int lasti, lastj;

			public List<line> lines = new();
			public List<point> pointsInProximity = new();

			//public static Canvas canvas;

			public point(double x, double y, double angle, double travelDistance) {
				dot = new();
				dot.Stroke = dot.Fill = Brushes.Black;
				dot.Width = dot.Height = dotRadius * 2;

				this.x = x - dotRadius;
				this.y = y - dotRadius;
				this.angle = angle;
				this.travelDistance = travelDistance;

				currentcanvas.Children.Add(dot);
				Canvas.SetLeft(dot, x);
				Canvas.SetTop(dot, y);
			}


			public void SetXY(double x, double y) {
				Canvas.SetLeft(dot, x - dotRadius); this.x = x;
				Canvas.SetTop(dot, y - dotRadius); this.y = y;
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

			//ATM I am aware that if there are two lines
			private void ForeachManageLines(int i, int j, ref List<point>[,] pointMatrix) { ///massive memory improvement since deleting every frame and recreating the objects in memory makes a pretty hard time for the GC
				foreach (point point in pointMatrix[i, j]) {
					{/*	foreach(line line in lines) {
					//		if(line.pointingTo == point) {
					//			if (IsInCircle(this.x, this.y, point.x, point.y, radius)) {
					//				line.SetXYXY(this, point);
					//			}
					//			else {
					//				line.DeleteLine();
					//				lines.Remove(line);
					//			}
					//			break;
					//		}
					//	}
					*/
					}

					var currentLine = lines.Find(x => x.pointingTo == point); //check if there is a line to this point
																			  //foreach (line currentLine in lines)
					if (IsInCircle(this.x, this.y, point.x, point.y, radius)) { //if the point is inside the circle

						if (currentLine != null) //if there's a line ideed, I will modify it's coordinates to not delete and create another object [hence this has been a very big drawback]
							currentLine.SetXYXY(this, point);
						else //else I will create one
							if (point.lines.Find(x => x.pointingFrom == this) == null)//but only if the other point doesn't have another line assigned already
							lines.Add(new(this, point));
					}
					else if (currentLine != null) { //if there's a line assigned to a point which is not inside the point's circle, I will delete it.
						currentLine.DeleteLine();
						lines.Remove(currentLine);
					}


				}

				foreach (line line in lines) //now I check if there are lines of which end point went outside the point's circle
					if (!IsInCircle(this.x, this.y, line.pointingTo.x, line.pointingTo.y, radius))
						line.DeleteLine();

				lines.RemoveAll(x => !IsInCircle(this.x, this.y, x.pointingTo.x, x.pointingTo.y, radius)); //and delete them since I can't call Remove inside foreach [altering the List at runtime]

			}


			public void ManageLines(List<point>[,] pointMatrix, double radius, double maxLineLength) {

				if (x <= maxLineLength) {
					if (y <= maxLineLength) {///top-left corner
						for (int i = 0; i < 2; i++)
							for (int j = 0; j < 2; j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
					if (y > maxLineLength && y < (currentcanvas.ActualHeight - maxLineLength)) {///left edge
						for (int i = 0; i < 2; i++)
							for (int j = (int)(this.y / maxLineLength) - 1; j <= (int)(this.y / maxLineLength) + 1; j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
					if (y >= (currentcanvas.ActualHeight - maxLineLength)) { ///bottom-left corner
						for (int i = 0; i < 2; i++)
							for (int j = (int)(this.y / maxLineLength) - 1; j <= (int)(this.y / maxLineLength); j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}

				}
				if (x > maxLineLength && x < (currentcanvas.ActualWidth - maxLineLength)) {
					if (y <= maxLineLength) { ///bottom edge
						for (int i = (int)(this.x / maxLineLength) - 1; i < (int)(this.x / maxLineLength) + 2; i++)
							for (int j = 0; j <= 1; j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
					if (y > maxLineLength && y < (currentcanvas.ActualHeight - maxLineLength)) { ///mid-section
						for (int i = (int)(this.x / maxLineLength) - 1; i < (int)(this.x / maxLineLength) + 2; i++)
							for (int j = (int)(this.y / maxLineLength) - 1; j < (int)(this.y / maxLineLength) + 2; j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
					if (y >= (currentcanvas.ActualHeight - maxLineLength)) { ///top edge
						for (int i = (int)(this.x / maxLineLength) - 1; i < (int)(this.x / maxLineLength) + 2; i++)
							for (int j = (int)(this.y / maxLineLength) - 1; j < (int)(currentcanvas.ActualHeight / maxLineLength); j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}

				}
				if (x >= (currentcanvas.ActualWidth - maxLineLength)) {
					if (y <= maxLineLength) {///top-right corner
						for (int i = (int)(this.x / maxLineLength) - 1; i < currentcanvas.ActualWidth / maxLineLength; i++)
							for (int j = 0; j < (int)(this.y / maxLineLength) + 1; j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
					if (y > maxLineLength && y < (currentcanvas.ActualHeight - maxLineLength)) { ///right edge
						for (int i = (int)(this.x / maxLineLength) - 1; i < currentcanvas.ActualWidth / maxLineLength; i++)
							for (int j = (int)(this.y / maxLineLength) - 1; j < (int)(this.y / maxLineLength) + 2; j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
					if (y >= (currentcanvas.ActualHeight - maxLineLength)) {///bottom-right corner
						for (int i = (int)(this.x / maxLineLength) - 1; i < currentcanvas.ActualWidth / maxLineLength; i++)
							for (int j = (int)(this.y / maxLineLength) - 1; j < (int)(currentcanvas.ActualHeight / maxLineLength); j++)
								ForeachManageLines(i, j, ref pointMatrix);
					}
				}
			}
		}
		public static bool IsInCircle2(double x0, double y0, double x, double y, double R) {
			return Math.Pow(x - x0, 2) + Math.Pow(y - y0, 2) < Math.Pow(R, 2);
		}
		public static bool IsInCircle(double x0, double y0, double x, double y, double R) { ///it is said this metod is optimal
			double dx = x - x0;
			if (dx > R) return false;
			double dy = y - y0;
			if (dy > R) return false;
			//if (dx + dy <= R) return false;
			return dx * dx + dy * dy <= R * R;
		}

		public double speedMultiplier;//0.6
		public List<point>? points;
		public int numberOfPoints; //250
		public int maxLineLength; //in px

		public List<point>[,] pointMatrix;

		private void OnLoaded(object sender, RoutedEventArgs e) {

			InitializeVariables();

			pointMatrix = new List<point>[(int)canvas.ActualWidth / maxLineLength, (int)(canvas.ActualHeight / maxLineLength)];
			for (int i = 0; i < pointMatrix.GetLength(0); i++)
				for (int j = 0; j < pointMatrix.GetLength(1); j++)
					pointMatrix[i, j] = new List<point>(); //every cell has to be initialized

			InitializePoints();

			CompositionTargetEx.Rendering += MovePoints; //the heart which makes this program work
		}

		public void InitializePoints() {
			point.currentcanvas = line.canvas = canvas;
			point.radius = maxLineLength;

			points = new List<point>();
			var random = new Random();

			for (int i = 0; i < numberOfPoints; i++) {
				double randomx = random.NextInt64(0, (long)canvas.ActualWidth);
				double randomy = random.NextInt64(0, (long)canvas.ActualHeight);
				double angle = (double)random.NextDouble() * 2 * Math.PI + 0.1;
				var pointToAdd = new point(randomx, randomy, angle, random.NextDouble() * speedMultiplier + 0.2);
				points.Add(pointToAdd);//new point(randomx, randomy, angle, random.NextDouble() * speedMultiplier + 0.2, canvas)
				Manage_pointMatrix(pointToAdd);
			}
		}

		public void MovePoints(object sender, EventArgs e) {
			foreach (point currentPoint in points) {
				currentPoint.Move();

				Manage_pointMatrix(currentPoint);

				currentPoint.ManageLines(pointMatrix, point.radius, maxLineLength);
			}
		}

		/*
		private void Manage_pointMatrix_SeparateEvent(object sender, EventArgs e) {// !!!nope

			List<line>? toDelete = null;
			foreach (point currentPoint in points) {

				foreach (line line in currentPoint.lines) {
					if (!IsInCircle(currentPoint.x, currentPoint.y, line.pointingTo.x, line.pointingTo.y, point.radius)) {
						line.DeleteLine();
						if (toDelete == null)
							toDelete = new List<line>();
						toDelete.Add(line);
						//currentPoint.lines.Remove(line);
					}
				}
			}
			if (toDelete != null)
				toDelete.Clear();
		}
		*/

		public void Manage_pointMatrix(point point) { //move the point in the arrays inside the grid matrix.
			int i = (int)point.x / maxLineLength;
			int j = (int)point.y / maxLineLength;
			if (!pointMatrix[i, j].Contains(point)) {
				pointMatrix[i, j].Add(point);
				pointMatrix[point.lasti, point.lastj].Remove(point);
				point.lasti = i; point.lastj = j;
			}
		}

		[Serializable]
		public class Variables {
			public double speedMultiplier;//0.6
			public int numberOfPoints;
			public int maxLineLength; //in px

			public double dotRadius;
			public double lineThickness;
		}

		public void InitializeVariables() {
			string filepath = new(Directory.GetCurrentDirectory() + @"\variables.xml");
			if (File.Exists(filepath)) {
				var readVariables = ReadFromXmlFile<Variables>(filepath);
				if(readVariables != null) {
					speedMultiplier = readVariables.speedMultiplier;
					numberOfPoints = readVariables.numberOfPoints;
					maxLineLength = readVariables.maxLineLength;
					point.dotRadius = readVariables.dotRadius;
					line.lineThickness = readVariables.lineThickness;
				}
				else Close();
			}
			else {
				var toSave = new Variables();
				toSave.speedMultiplier = speedMultiplier = 2;
				toSave.numberOfPoints = numberOfPoints = 200;
				toSave.maxLineLength = maxLineLength = 50;
				toSave.dotRadius = point.dotRadius = 2;
				toSave.lineThickness = line.lineThickness = 2;

				WriteToXmlFile<Variables>(filepath, toSave);
			}
		}

		public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new() {
			TextWriter writer = null;
			try {
				var serializer = new XmlSerializer(typeof(T));
				writer = new StreamWriter(filePath, append);
				using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true })) {
					serializer.Serialize(xmlWriter, objectToWrite);
				}
				//serializer.Serialize(writer, objectToWrite);
			}
			finally {
				if (writer != null)
					writer.Close();
			}
		}
		public static T ReadFromXmlFile<T>(string filePath) where T : new() {
			TextReader reader = null;
			try {
				var serializer = new XmlSerializer(typeof(T));
				reader = new StreamReader(filePath);
				return (T)serializer.Deserialize(reader);
			}
			finally {
				if (reader != null)
					reader.Close();
			}
		}
	}


}
