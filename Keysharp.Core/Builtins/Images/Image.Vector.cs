#if WINDOWS
using NativeBrush = System.Drawing.Brush;
using NativeMatrix = System.Drawing.Drawing2D.Matrix;
#else
using NativeBrush = Eto.Drawing.Brush;
using NativeMatrix = Eto.Drawing.IMatrix;
#endif

namespace Keysharp.Builtins
{
	public partial class Ks
	{
		public partial class KeysharpImage
		{
			private const string TransformPropertyNames = "ScaleX, ScaleY, OffsetX, OffsetY, SkewX, SkewY";
			private readonly object vectorOwner = new();
			private VectorTransform drawingTransform = VectorTransform.Identity;
			private VectorClip drawingClip;

			private readonly record struct VectorTransform(
				double ScaleX, double ScaleY, double OffsetX, double OffsetY, double SkewX, double SkewY)
			{
				internal static VectorTransform Identity => new(1, 1, 0, 0, 0, 0);
				internal bool IsIdentity => this == Identity;
				internal bool IsAxisAligned => SkewX == 0 && SkewY == 0;
				internal double MaximumScale => Math.Sqrt(
					ScaleX * ScaleX + ScaleY * ScaleY + SkewX * SkewX + SkewY * SkewY);
			}

			private sealed class VectorClip
			{
				internal readonly VectorClip Previous;
				internal readonly GraphicsPath Path;
				internal readonly RectangleF Bounds;

				internal VectorClip(VectorClip previous, GraphicsPath path, RectangleF bounds)
				{
					Previous = previous;
					Path = path;
					Bounds = previous == null ? bounds : IntersectBounds(previous.Bounds, bounds);
				}

				~VectorClip()
				{
					try { Path.Dispose(); } catch { }
				}
			}

			private readonly record struct VectorDrawingState(VectorTransform Transform, VectorClip Clip);

			private sealed class VectorState : KeysharpObject
			{
				internal readonly object Owner;
				internal readonly VectorDrawingState State;

				internal VectorState(object owner, VectorDrawingState state) : base(null)
				{
					Owner = owner;
					State = state;
				}
			}

			[UserDeclaredName("Path")]
			public class KeysharpPath : KeysharpObject, IDisposable
			{
				private GraphicsPath geometry = NewPath(false);
				private bool constructed;
				private bool nonZero;
				private bool hasCurrent;
				private PointF current;
				private bool hasGeometry;
				private bool hasBounds;
				private bool boundsValid = true;
				private float left, top, right, bottom;

				public KeysharpPath(params object[] args) : base(args) { }

				public object __New(object FillRule = null)
				{
					if (constructed)
						return Errors.ValueErrorOccurred("An Image.Path's FillRule is fixed at construction.", ret: this);

					if (FillRule != null)
					{
						if (FillRule is not string value || value.Length == 0)
							return InvalidFillRule(FillRule);

						if (value.Equals("NonZero", StringComparison.OrdinalIgnoreCase))
							nonZero = true;
						else if (!value.Equals("EvenOdd", StringComparison.OrdinalIgnoreCase))
							return InvalidFillRule(FillRule);
					}

					SetFillMode(geometry, nonZero);
					constructed = true;
					return this;
				}

				private object InvalidFillRule(object value) => Errors.ValueErrorOccurred(
					$"Invalid FillRule {value}; accepted values are EvenOdd and NonZero.", value, this);

				internal GraphicsPath CloneGeometry() => ClonePath(geometry);
				internal bool HasGeometry => hasGeometry;

				internal bool TryGetBounds(out RectangleF bounds)
				{
					if (!boundsValid)
					{
						bounds = RectangleF.Empty;
						return false;
					}

					bounds = hasBounds ? new RectangleF(left, top, right - left, bottom - top) : RectangleF.Empty;
					return true;
				}

				private void Include(PointF point)
				{
					if (!boundsValid)
						return;

					if (!hasBounds)
					{
						left = right = point.X;
						top = bottom = point.Y;
						hasBounds = true;
					}
					else
					{
						left = Math.Min(left, point.X);
						top = Math.Min(top, point.Y);
						right = Math.Max(right, point.X);
						bottom = Math.Max(bottom, point.Y);
					}
				}

				private void Include(RectangleF bounds)
				{
					var rightEdge = (double)bounds.X + bounds.Width;
					var bottomEdge = (double)bounds.Y + bounds.Height;

					if (!float.IsFinite((float)rightEdge) || !float.IsFinite((float)bottomEdge))
					{
						boundsValid = false;
						return;
					}

					Include(new PointF(bounds.X, bounds.Y));
					Include(new PointF((float)rightEdge, (float)bottomEdge));
				}

				public object MoveTo(object X, object Y)
				{
					if (!TryVectorPoint(X, Y, out current))
						return this;

					geometry.StartFigure();
					hasCurrent = true;
					return this;
				}

				public object LineTo(object X, object Y)
				{
					if (!TryVectorPoint(X, Y, out var end))
						return this;

					if (!hasCurrent)
						return Errors.ValueErrorOccurred("LineTo requires an active figure; call MoveTo first.", ret: this);

					geometry.AddLine(current, end);
					Include(current);
					hasGeometry = true;
					Include(end);
					current = end;
					return this;
				}

				public object CubicTo(object ControlX1, object ControlY1, object ControlX2, object ControlY2,
					object X, object Y)
				{
					if (!TryVectorPoint(ControlX1, ControlY1, out var control1)
						|| !TryVectorPoint(ControlX2, ControlY2, out var control2)
						|| !TryVectorPoint(X, Y, out var end))
						return this;

					if (!hasCurrent)
						return Errors.ValueErrorOccurred("CubicTo requires an active figure; call MoveTo first.", ret: this);

					geometry.AddBezier(current, control1, control2, end);
					Include(current);
					hasGeometry = true;
					Include(control1);
					Include(control2);
					Include(end);
					current = end;
					return this;
				}

				public object ArcTo(object X, object Y, object Width, object Height, object StartAngle, object SweepAngle)
				{
					if (!TryVectorRect(X, Y, Width, Height, out var rect)
						|| !TryVectorNumber(StartAngle, nameof(StartAngle), out var startAngle)
						|| !TryVectorNumber(SweepAngle, nameof(SweepAngle), out var sweepAngle))
						return this;

					if (Math.Abs(sweepAngle) > 360)
						return Errors.ValueErrorOccurred("SweepAngle must be from -360 through 360.", SweepAngle, this);

					if (rect.Width <= 0 || rect.Height <= 0 || sweepAngle == 0)
						return this;

					startAngle = Math.IEEERemainder(startAngle, 360);
					var first = ArcPoint(rect, startAngle);

					if (hasCurrent)
					{
						geometry.AddLine(current, first);
						Include(current);
						Include(first);
					}
					else
						geometry.StartFigure();

					geometry.AddArc(rect.X, rect.Y, rect.Width, rect.Height, (float)startAngle, (float)sweepAngle);
					hasGeometry = true;
					Include(rect);
					current = ArcPoint(rect, startAngle + sweepAngle);
					hasCurrent = true;
					return this;
				}

				private static PointF ArcPoint(RectangleF rect, double angle)
				{
					var radians = angle * Math.PI / 180;
					return new PointF(
						(float)(rect.X + rect.Width / 2 + rect.Width / 2 * Math.Cos(radians)),
						(float)(rect.Y + rect.Height / 2 + rect.Height / 2 * Math.Sin(radians)));
				}

				public object Close()
				{
					if (hasCurrent)
					{
						geometry.CloseFigure();
						hasCurrent = false;
					}

					return this;
				}

				public object AddRect(object X, object Y, object Width, object Height)
				{
					if (!TryVectorRect(X, Y, Width, Height, out var rect))
						return this;

					if (rect.Width > 0 && rect.Height > 0)
					{
						geometry.AddRectangle(rect);
						hasGeometry = true;
						Include(rect);
						hasCurrent = false;
					}

					return this;
				}

				public object AddRoundRect(object X, object Y, object Width, object Height, object Radius)
				{
					if (!TryVectorRect(X, Y, Width, Height, out var rect)
						|| !TryVectorNumber(Radius, nameof(Radius), out var radius))
						return this;

					if (rect.Width <= 0 || rect.Height <= 0)
						return this;

					using var rounded = MakeRoundRectPath(rect,
						(float)Math.Clamp(radius, 0, Math.Min(rect.Width, rect.Height) / 2));
					geometry.AddPath(rounded, false);
					hasGeometry = true;
					Include(rect);
					hasCurrent = false;
					return this;
				}

				public object AddEllipse(object X, object Y, object Width, object Height)
				{
					if (!TryVectorRect(X, Y, Width, Height, out var rect))
						return this;

					if (rect.Width > 0 && rect.Height > 0)
					{
						geometry.AddEllipse(rect);
						hasGeometry = true;
						Include(rect);
						hasCurrent = false;
					}

					return this;
				}

				public object AddPolygon(object Points)
				{
					if (Points is not Array points)
						return Errors.TypeErrorOccurred("Points must be an Array of objects with X and Y properties.", this);

					var count = points.array?.Count ?? 0;

					if (count == 0)
						return this;

					if (count < 3)
						return Errors.ValueErrorOccurred("A nonempty polygon requires at least three points.", Points, this);

					var nativePoints = new PointF[count];

					for (var i = 0; i < count; i++)
					{
						var point = points.array[i];
						var x = point is Any any ? Script.GetPropertyValueOrNull(any, "X") : null;
						var y = point is Any anyY ? Script.GetPropertyValueOrNull(anyY, "Y") : null;

						if (!TryVectorPoint(x, y, out nativePoints[i], $"Points[{i + 1}].X", $"Points[{i + 1}].Y"))
							return this;
					}

					geometry.StartFigure();
					geometry.AddLines(nativePoints);
					geometry.CloseFigure();
					hasGeometry = true;

					foreach (var point in nativePoints)
						Include(point);

					hasCurrent = false;
					return this;
				}

				public object AddPath(object OtherPath, object Transform = null)
				{
					if (OtherPath is not KeysharpPath other)
						return Errors.TypeErrorOccurred("OtherPath must be an Image.Path.", this);

					var transform = VectorTransform.Identity;

					if (Transform != null && !TryVectorTransform(Transform, transform, out transform))
						return this;

					if (!other.hasGeometry)
						return this;

					using var copy = other.CloneGeometry();

					if (!transform.IsIdentity)
					{
						using NativeMatrix matrix = NativeVectorMatrix(transform);
						copy.Transform(matrix);
					}

					geometry.AddPath(copy, false);
					hasGeometry = true;

					if (!other.TryGetBounds(out var sourceBounds)
						|| !TryTransformBounds(sourceBounds, transform, out sourceBounds))
						boundsValid = false;
					else
						Include(sourceBounds);

					hasCurrent = false;
					return this;
				}

				public object Clear()
				{
					geometry.Dispose();
					geometry = NewPath(nonZero);
					hasCurrent = hasGeometry = hasBounds = false;
					boundsValid = true;
					return this;
				}

				public new object Clone()
				{
					var clone = new KeysharpPath
					{
						constructed = true,
						nonZero = nonZero,
						hasCurrent = hasCurrent,
						current = current,
						hasGeometry = hasGeometry,
						hasBounds = hasBounds,
						boundsValid = boundsValid,
						left = left,
						top = top,
						right = right,
						bottom = bottom
					};
					clone.geometry.Dispose();
					clone.geometry = CloneGeometry();
					return clone;
				}

				void IDisposable.Dispose()
				{
					DisposePath();
					GC.SuppressFinalize(this);
				}

				private void DisposePath()
				{
					try { geometry?.Dispose(); } catch { }
					geometry = null;
				}
			}

			[UserDeclaredName("Brush")]
			public class KeysharpBrush : KeysharpObject
			{
				internal enum BrushKind : byte { Linear, Radial }
				internal BrushKind kind;
				internal PointF first, second;
				internal int startColor, endColor;
				internal bool valid;

				public KeysharpBrush(params object[] args) : base(args) { }

				private KeysharpBrush(BrushKind kind, PointF first, PointF second,
					int startColor, int endColor) : base(null)
				{
					this.kind = kind;
					this.first = first;
					this.second = second;
					this.startColor = startColor;
					this.endColor = endColor;
					valid = true;
				}

				public override object __New(params object[] args) =>
					Errors.ErrorOccurred("Image.Brush has no instances; use LinearGradient or RadialGradient.");

				[Static]
				public static object LinearGradient(object @this, object X1, object Y1, object X2, object Y2,
					object StartColor, object EndColor)
				{
					if (!TryVectorPoint(X1, Y1, out var start)
						|| !TryVectorPoint(X2, Y2, out var end)
						|| !TryVectorColor(StartColor, nameof(StartColor), out var startColor)
						|| !TryVectorColor(EndColor, nameof(EndColor), out var endColor))
						return DefaultObject;

					if (start.X == end.X && start.Y == end.Y)
						return Errors.ValueErrorOccurred("Linear gradient endpoints must be distinct.");

					return new KeysharpBrush(BrushKind.Linear, start, end, startColor, endColor);
				}

				[Static]
				public static object RadialGradient(object @this, object CenterX, object CenterY,
					object RadiusX, object RadiusY, object CenterColor, object EdgeColor)
				{
					if (!TryVectorPoint(CenterX, CenterY, out var center)
						|| !TryVectorNumber(RadiusX, nameof(RadiusX), out var radiusX)
						|| !TryVectorNumber(RadiusY, nameof(RadiusY), out var radiusY)
						|| !TryVectorColor(CenterColor, nameof(CenterColor), out var centerColor)
						|| !TryVectorColor(EdgeColor, nameof(EdgeColor), out var edgeColor))
						return DefaultObject;

					if (radiusX <= 0 || radiusY <= 0)
						return Errors.ValueErrorOccurred("Radial gradient radii must be positive.");

					return new KeysharpBrush(BrushKind.Radial, center,
						new PointF((float)radiusX, (float)radiusY), centerColor, edgeColor);
				}
			}

			private readonly record struct VectorPaint(int Solid, KeysharpBrush Brush)
			{
				internal bool IsSolid => Brush == null;
				internal bool IsTransparent => IsSolid && ((uint)Solid >> 24) == 0;
			}

			public new object Clone() => Errors.ErrorOccurred("Use Image.Copy() to duplicate an Image.");

			public object Transform
			{
				get
				{
					ThrowIfDisposed();
					return TransformObject(drawingTransform);
				}

				set
				{
					ThrowIfDisposed();

					if (TryVectorTransform(value, drawingTransform, out var result))
						drawingTransform = result;
				}
			}

			public object Clip(object Path = null)
			{
				ThrowIfDisposed();

				if (Path == null)
				{
					drawingClip = null;
					return this;
				}

				if (Path is not KeysharpPath path)
					return Errors.TypeErrorOccurred("Clip requires an Image.Path, or no argument to reset clipping.", this);

				var snapshot = path.CloneGeometry();
				path.TryGetBounds(out var bounds);

				if (!drawingTransform.IsIdentity)
				{
					using NativeMatrix matrix = NativeVectorMatrix(drawingTransform);
					snapshot.Transform(matrix);
					_ = TryTransformBounds(bounds, drawingTransform, out bounds);
				}

				drawingClip = new VectorClip(drawingClip, snapshot, bounds);
				return this;
			}

			public object SaveState()
			{
				ThrowIfDisposed();
				return new VectorState(vectorOwner, SnapshotDrawingState());
			}

			public object RestoreState(object State)
			{
				ThrowIfDisposed();

				if (State is not VectorState state)
					return Errors.TypeErrorOccurred("State must be a snapshot returned by Image.SaveState.", this);

				if (!ReferenceEquals(state.Owner, vectorOwner))
					return Errors.ValueErrorOccurred("State must be a snapshot created by this Image.", State, this);

				var drawingState = state.State;
				drawingTransform = drawingState.Transform;
				drawingClip = drawingState.Clip;
				return this;
			}

			public object DrawPath(object Path, object Color = null, object Thickness = null)
			{
				ThrowIfDisposed();

				if (Path is not KeysharpPath path)
					return Errors.TypeErrorOccurred("Path must be an Image.Path.", this);

				var thickness = 1.0;

				if (!TryVectorPaint(Color, unchecked((int)0xFF000000u), out var paint)
					|| (Thickness != null && !TryVectorNumber(Thickness, nameof(Thickness), out thickness)))
					return this;

				if (!path.HasGeometry || thickness <= 0 || paint.IsTransparent)
					return this;

				var snapshot = path.CloneGeometry();
				var boundsKnown = path.TryGetBounds(out var bounds);
				var coverage = ExpandBounds(bounds, thickness * 5 + 1);
				var state = SnapshotDrawingState();

				if (!eagerDraw)
					pendingResources.Add(snapshot);

				try
				{
					QueueDraw(bitmap =>
					{
						using var lease = DrawG(bitmap, state);
						var graphics = lease.Graphics;

						if (paint.IsSolid)
						{
							using var pen = new Pen(ImageHelper.ArgbToColor(paint.Solid), (float)thickness);
							ConfigureVectorPen(pen);
							graphics.DrawPath(pen, snapshot);
						}
						else
						{
							using var brush = CreateVectorBrush(paint.Brush, coverage);
							using var pen = new Pen(brush, (float)thickness);
							ConfigureVectorPen(pen);
							graphics.DrawPath(pen, snapshot);
						}

						return bitmap;
					});
				}
				finally
				{
					if (eagerDraw)
						snapshot.Dispose();
				}

				if (boundsKnown)
					DamageVector(bounds, thickness * 5, state);
				else
					DamageAll();

				return this;
			}

			public object FillPath(object Path, object Color = null)
			{
				ThrowIfDisposed();

				if (Path is not KeysharpPath path)
					return Errors.TypeErrorOccurred("Path must be an Image.Path.", this);

				if (!TryVectorPaint(Color, unchecked((int)0xFF000000u), out var paint))
					return this;

				if (!path.HasGeometry || paint.IsTransparent)
					return this;

				var snapshot = path.CloneGeometry();
				var boundsKnown = path.TryGetBounds(out var bounds);
				var coverage = ExpandBounds(bounds, 1);
				var state = SnapshotDrawingState();

				if (!eagerDraw)
					pendingResources.Add(snapshot);

				try
				{
					QueueDraw(bitmap =>
					{
						using var lease = DrawG(bitmap, state);

						if (paint.IsSolid)
							lease.Graphics.FillPath(Brush(paint.Solid), snapshot);
						else
						{
							using var brush = CreateVectorBrush(paint.Brush, coverage);
							lease.Graphics.FillPath(brush, snapshot);
						}

						return bitmap;
					});
				}
				finally
				{
					if (eagerDraw)
						snapshot.Dispose();
				}

				if (boundsKnown)
					DamageVector(bounds, 1, state);
				else
					DamageAll();

				return this;
			}

			private VectorDrawingState SnapshotDrawingState() => new(drawingTransform, drawingClip);

			private static KeysharpObject TransformObject(VectorTransform transform)
			{
				var result = new KeysharpObject();
				result.DefinePropInternal("ScaleX", new OwnPropsDesc(result, transform.ScaleX));
				result.DefinePropInternal("ScaleY", new OwnPropsDesc(result, transform.ScaleY));
				result.DefinePropInternal("OffsetX", new OwnPropsDesc(result, transform.OffsetX));
				result.DefinePropInternal("OffsetY", new OwnPropsDesc(result, transform.OffsetY));
				result.DefinePropInternal("SkewX", new OwnPropsDesc(result, transform.SkewX));
				result.DefinePropInternal("SkewY", new OwnPropsDesc(result, transform.SkewY));
				return result;
			}

			private static bool TryVectorTransform(object descriptor, VectorTransform baseline,
				out VectorTransform result)
			{
				result = baseline;

				if (descriptor?.GetType() != typeof(KeysharpObject))
				{
					_ = Errors.TypeErrorOccurred("Transform must be an ordinary object with named numeric properties.");
					return false;
				}

				var values = new[]
				{
					baseline.ScaleX, baseline.ScaleY, baseline.OffsetX,
					baseline.OffsetY, baseline.SkewX, baseline.SkewY
				};

				var updates = (KeysharpObject)descriptor;

				if (updates.op != null)
				{
					foreach (var (name, property) in updates.op)
					{
						if (property.Value == null || property.Get != null || property.Set != null || property.Call != null)
						{
							_ = Errors.TypeErrorOccurred($"Transform property {name} must be a data property.");
							return false;
						}

						var index = name.ToLowerInvariant() switch
						{
							"scalex" => 0,
							"scaley" => 1,
							"offsetx" => 2,
							"offsety" => 3,
							"skewx" => 4,
							"skewy" => 5,
							_ => -1
						};

						if (index < 0)
						{
							_ = Errors.ValueErrorOccurred(
								$"Unknown Transform property {name}; accepted properties are {TransformPropertyNames}.", name);
							return false;
						}

						if (!TryVectorNumber(property.Value, name, out values[index]))
							return false;
					}
				}

				var candidate = new VectorTransform(values[0], values[1], values[2], values[3], values[4], values[5]);
				var determinant = candidate.ScaleX * candidate.ScaleY - candidate.SkewX * candidate.SkewY;

				if (!double.IsFinite(determinant) || (float)determinant == 0)
				{
					_ = Errors.ValueErrorOccurred("Transform must be finite, representable, and nonsingular.");
					return false;
				}

				result = candidate;
				return true;
			}

			private static bool TryVectorNumber(object value, string name, out double result)
			{
				if (!value.TryParseDouble(out result))
				{
					_ = Errors.TypeErrorOccurred($"{name} must be numeric.");
					return false;
				}

				if (!double.IsFinite(result) || !float.IsFinite((float)result))
				{
					_ = Errors.ValueErrorOccurred($"{name} must be finite and representable.", value);
					return false;
				}

				return true;
			}

			private static bool TryVectorPoint(object x, object y, out PointF point,
				string xName = "X", string yName = "Y")
			{
				point = default;

				if (!TryVectorNumber(x, xName, out var px) || !TryVectorNumber(y, yName, out var py))
					return false;

				point = new PointF((float)px, (float)py);
				return true;
			}

			private static bool TryVectorRect(object X, object Y, object Width, object Height, out RectangleF rect)
			{
				rect = RectangleF.Empty;

				if (!TryVectorPoint(X, Y, out var origin)
					|| !TryVectorNumber(Width, nameof(Width), out var width)
					|| !TryVectorNumber(Height, nameof(Height), out var height))
					return false;

				rect = new RectangleF(origin.X, origin.Y, (float)width, (float)height);
				return true;
			}

			private static bool TryVectorColor(object value, string name, out int argb)
			{
				argb = 0;

				if (value is long or int or double)
				{
					if (value is double number && !double.IsFinite(number))
					{
						_ = Errors.ValueErrorOccurred($"{name} must be a valid color.", value);
						return false;
					}

					var raw = (uint)value.Al();
					argb = unchecked((int)(raw > 0xFFFFFFu ? raw : 0xFF000000u | (raw & 0xFFFFFFu)));
					return true;
				}

				if (value is string text && text.Length > 0)
				{
					if (Conversions.TryParseColor(text, out var color))
					{
						argb = color.ToArgb();
						return true;
					}

					if (text.ParseLong().HasValue)
					{
						argb = ParseColorArg(text);
						return true;
					}
				}

				_ = Errors.ValueErrorOccurred($"{name} must be a valid color.", value);
				return false;
			}

			private static bool TryVectorPaint(object value, int defaultColor, out VectorPaint paint)
			{
				if (value is KeysharpBrush brush)
				{
					if (!brush.valid)
					{
						paint = default;
						_ = Errors.ValueErrorOccurred(
							"Brush must be created by Image.Brush.LinearGradient or Image.Brush.RadialGradient.", value);
						return false;
					}

					paint = new VectorPaint(0, brush);
					return true;
				}

				paint = new VectorPaint(ParseColorArg(value, defaultColor, allowTransparentEmpty: false), null);
				return true;
			}

			private static GraphicsPath NewPath(bool nonZero)
			{
				var path = new GraphicsPath();
				SetFillMode(path, nonZero);
				return path;
			}

			private static void SetFillMode(GraphicsPath path, bool nonZero)
			{
#if WINDOWS
				path.FillMode = nonZero ? System.Drawing.Drawing2D.FillMode.Winding : System.Drawing.Drawing2D.FillMode.Alternate;
#else
				path.FillMode = nonZero ? Eto.Drawing.FillMode.Winding : Eto.Drawing.FillMode.Alternate;
#endif
			}

			private static GraphicsPath ClonePath(GraphicsPath path) => (GraphicsPath)path.Clone();

			private static NativeBrush CreateVectorBrush(KeysharpBrush description, RectangleF coverage)
			{
#if WINDOWS
				return description.kind == KeysharpBrush.BrushKind.Linear
					? WindowsLinearGradient(description, coverage)
					: WindowsRadialGradient(description, coverage);
#else
				if (description.kind == KeysharpBrush.BrushKind.Linear)
					return new LinearGradientBrush(ImageHelper.ArgbToColor(description.startColor),
						ImageHelper.ArgbToColor(description.endColor), description.first, description.second)
					{ Wrap = GradientWrapMode.Pad };

				return new RadialGradientBrush(ImageHelper.ArgbToColor(description.startColor),
					ImageHelper.ArgbToColor(description.endColor), description.first, description.first,
					new SizeF(description.second.X, description.second.Y)) { Wrap = GradientWrapMode.Pad };
#endif
			}

#if WINDOWS
			private static NativeBrush WindowsLinearGradient(KeysharpBrush description, RectangleF coverage)
			{
				var start = description.first;
				var end = description.second;
				var dx = (double)end.X - start.X;
				var dy = (double)end.Y - start.Y;
				var length = dx * dx + dy * dy;
				var minimum = 0.0;
				var maximum = 1.0;

				void Include(double x, double y)
				{
					var position = ((x - start.X) * dx + (y - start.Y) * dy) / length;
					minimum = Math.Min(minimum, position);
					maximum = Math.Max(maximum, position);
				}

				Include(coverage.Left, coverage.Top);
				Include(coverage.Right, coverage.Top);
				Include(coverage.Left, coverage.Bottom);
				Include(coverage.Right, coverage.Bottom);

				var nativeStart = new PointF((float)(start.X + minimum * dx), (float)(start.Y + minimum * dy));
				var nativeEnd = new PointF((float)(start.X + maximum * dx), (float)(start.Y + maximum * dy));
				var startColor = ImageHelper.ArgbToColor(description.startColor);
				var endColor = ImageHelper.ArgbToColor(description.endColor);
				var brush = new LinearGradientBrush(nativeStart, nativeEnd, startColor, endColor);

				if (minimum < 0 || maximum > 1)
				{
					var range = maximum - minimum;
					var firstStop = (float)(-minimum / range);
					var lastStop = (float)((1 - minimum) / range);

					if (minimum < 0 && maximum > 1)
						brush.InterpolationColors = new ColorBlend
						{
							Colors = new[] { startColor, startColor, endColor, endColor },
							Positions = new[] { 0, firstStop, lastStop, 1 }
						};
					else if (minimum < 0)
						brush.InterpolationColors = new ColorBlend
						{
							Colors = new[] { startColor, startColor, endColor },
							Positions = new[] { 0, firstStop, 1 }
						};
					else
						brush.InterpolationColors = new ColorBlend
						{
							Colors = new[] { startColor, endColor, endColor },
							Positions = new[] { 0, lastStop, 1 }
						};
				}

				return brush;
			}

			private static NativeBrush WindowsRadialGradient(KeysharpBrush description, RectangleF coverage)
			{
				var center = description.first;
				var radius = description.second;
				var factor = 1.0;

				void Include(double x, double y)
				{
					var dx = (x - center.X) / radius.X;
					var dy = (y - center.Y) / radius.Y;
					factor = Math.Max(factor, Math.Sqrt(dx * dx + dy * dy));
				}

				Include(coverage.Left, coverage.Top);
				Include(coverage.Right, coverage.Top);
				Include(coverage.Left, coverage.Bottom);
				Include(coverage.Right, coverage.Bottom);

				var rx = (float)(radius.X * factor);
				var ry = (float)(radius.Y * factor);

				using var ellipse = new GraphicsPath();
				ellipse.AddEllipse(center.X - rx, center.Y - ry, rx * 2, ry * 2);
				var centerColor = ImageHelper.ArgbToColor(description.startColor);
				var edgeColor = ImageHelper.ArgbToColor(description.endColor);
				var brush = new PathGradientBrush(ellipse)
				{
					CenterColor = centerColor,
					CenterPoint = center,
					SurroundColors = new[] { edgeColor }
				};

				if (factor > 1)
				{
					brush.InterpolationColors = new ColorBlend
					{
						Colors = new[] { edgeColor, edgeColor, centerColor },
						Positions = new[] { 0, (float)(1 - 1 / factor), 1 }
					};
				}

				return brush;
			}
#endif

			private static void ConfigureVectorPen(Pen pen)
			{
#if WINDOWS
				pen.StartCap = LineCap.Flat;
				pen.EndCap = LineCap.Flat;
				pen.LineJoin = LineJoin.Miter;
				pen.MiterLimit = 10;
#else
				pen.LineCap = PenLineCap.Butt;
				pen.LineJoin = PenLineJoin.Miter;
				pen.MiterLimit = 10;
#endif
			}

			private void ApplyDrawingState(Graphics graphics, VectorDrawingState state)
			{
				graphics.ResetClip();

				if (drawScaleX != 1 || drawScaleY != 1)
					graphics.ScaleTransform((float)drawScaleX, (float)drawScaleY);

				ApplyClipChain(graphics, state.Clip);

				if (!state.Transform.IsIdentity)
				{
					using NativeMatrix matrix = NativeVectorMatrix(state.Transform);
					graphics.MultiplyTransform(matrix);
				}
			}

			private static void ApplyClipChain(Graphics graphics, VectorClip clip)
			{
				if (clip == null)
					return;

				ApplyClipChain(graphics, clip.Previous);
#if WINDOWS
				graphics.SetClip(clip.Path, CombineMode.Intersect);
#else
				graphics.IntersectClip(clip.Path);
#endif
			}

			private static NativeMatrix NativeVectorMatrix(VectorTransform transform)
			{
#if WINDOWS
				return new NativeMatrix((float)transform.ScaleX, (float)transform.SkewY,
					(float)transform.SkewX, (float)transform.ScaleY,
					(float)transform.OffsetX, (float)transform.OffsetY);
#else
				return Eto.Drawing.Matrix.Create((float)transform.ScaleX, (float)transform.SkewY,
					(float)transform.SkewX, (float)transform.ScaleY,
					(float)transform.OffsetX, (float)transform.OffsetY);
#endif
			}

			private void DamageVector(RectangleF bounds, double logicalPad, VectorDrawingState state)
			{
				if (damage == null)
					return;

				if (!TryTransformBounds(bounds, state.Transform, out var transformed))
				{
					DamageAll();
					return;
				}

				transformed = ExpandBounds(transformed, logicalPad * state.Transform.MaximumScale);

				if (state.Clip != null)
					transformed = IntersectBounds(transformed, state.Clip.Bounds);

				if (transformed.Width > 0 && transformed.Height > 0)
					Damage(transformed, 1);
			}

			private static bool TryTransformBounds(RectangleF bounds, VectorTransform transform, out RectangleF result)
			{
				var halfWidth = bounds.Width / 2.0;
				var halfHeight = bounds.Height / 2.0;
				var centerX = bounds.X + halfWidth;
				var centerY = bounds.Y + halfHeight;
				var transformedX = transform.ScaleX * centerX + transform.SkewX * centerY + transform.OffsetX;
				var transformedY = transform.SkewY * centerX + transform.ScaleY * centerY + transform.OffsetY;
				var extentX = Math.Abs(transform.ScaleX) * halfWidth + Math.Abs(transform.SkewX) * halfHeight;
				var extentY = Math.Abs(transform.SkewY) * halfWidth + Math.Abs(transform.ScaleY) * halfHeight;
				var left = transformedX - extentX;
				var top = transformedY - extentY;
				var right = transformedX + extentX;
				var bottom = transformedY + extentY;

				if (!float.IsFinite((float)left) || !float.IsFinite((float)top)
					|| !float.IsFinite((float)right) || !float.IsFinite((float)bottom))
				{
					result = RectangleF.Empty;
					return false;
				}

				result = new RectangleF((float)left, (float)top, (float)(right - left), (float)(bottom - top));
				return true;
			}

			private static RectangleF ExpandBounds(RectangleF bounds, double amount) => new(
				(float)(bounds.X - amount), (float)(bounds.Y - amount),
				(float)(bounds.Width + amount * 2), (float)(bounds.Height + amount * 2));

			private static RectangleF IntersectBounds(RectangleF first, RectangleF second)
			{
				var left = Math.Max(first.Left, second.Left);
				var top = Math.Max(first.Top, second.Top);
				var right = Math.Min(first.Right, second.Right);
				var bottom = Math.Min(first.Bottom, second.Bottom);
				return right <= left || bottom <= top
					? new RectangleF(left, top, 0, 0)
					: new RectangleF(left, top, right - left, bottom - top);
			}

		}
	}
}
