using System;

namespace WCell.Util.Graphics
{
	public class RectInt32
	{
	  internal int x;
        internal int y;
        internal int width;
        internal int height;
        
        private static readonly RectInt32 s_empty;

        public static bool operator ==(RectInt32 rect1, RectInt32 rect2)
        {
            return ((((rect1.X == rect2.X) && (rect1.Y == rect2.Y)) && (rect1.Width == rect2.Width)) && (rect1.Height == rect2.Height));
        }

        public static bool operator !=(RectInt32 rect1, RectInt32 rect2)
        {
            return !(rect1 == rect2);
        }

        public static bool Equals(RectInt32 rect1, RectInt32 rect2)
        {
            if (rect1.IsEmpty)
            {
                return rect2.IsEmpty;
            }
            return (((rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y)) && rect1.Width.Equals(rect2.Width)) && rect1.Height.Equals(rect2.Height));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is RectInt32))
            {
                return false;
            }
            var rect = (RectInt32)o;
            return Equals(this, rect);
        }

        public bool Equals(RectInt32 value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }
            return (((X.GetHashCode() ^ Y.GetHashCode()) ^ Width.GetHashCode()) ^ Height.GetHashCode());
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return "Empty";
            }
            return string.Format("X: {0}, Y: {1}, Width: {2}, Height: {3}", x, y, width, height);
        }

        public RectInt32(int x, int y, int width, int height)
        {
            if ((width < 0.0) || (height < 0.0))
            {
                throw new ArgumentException("Width and Height cannot be negative");
            }
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

		private RectInt32()
		{
		}

        public static RectInt32 Empty
        {
            get
            {
                return s_empty;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (width < 0.0f);
            }
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                }
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                }
                y = value;
            }
        }
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                }
                if (value < 0.0)
                {
                    throw new ArgumentException("Width cannot be negative.");
                }
                width = value;
            }
        }
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty RectInt32.");
                }
                if (value < 0.0)
                {
                    throw new ArgumentException("Height cannot be negative");
                }
                height = value;
            }
        }
        public int Left
        {
            get
            {
                return x;
            }
        }
        public int Top
        {
            get
            {
                return y;
            }
        }
        public int Right
        {
            get
            {
                return (x + width);
            }
        }
        public int Bottom
        {
            get
            {
                return (y + height);
            }
        }
        public Point TopLeft
        {
            get
            {
                return new Point(Left, Top);
            }
        }
        public Point TopRight
        {
            get
            {
                return new Point(Right, Top);
            }
        }
        public Point BottomLeft
        {
            get
            {
                return new Point(Left, Bottom);
            }
        }
        public Point BottomRight
        {
            get
            {
                return new Point(Right, Bottom);
            }
        }

        /// <summary>
        /// Whether the rectangle contains the given Point(x, y). 
        /// Points laying on the rectangles border are considered to be contained.
        /// </summary>
        public bool Contains(int xPos, int yPos)
        {
            if (IsEmpty)
            {
                return false;
            }
            return ContainsInternal(xPos, yPos);
        }

        public bool Contains(RectInt32 rect)
        {
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((x <= rect.x) && (y <= rect.y)) &&
                     ((x + width) >= (rect.x + rect.width))) && ((y + height) >= (rect.y + rect.height)));
        }

        public bool IntersectsWith(RectInt32 rect)
        {
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((rect.Left <= Right) && (rect.Right >= Left)) && (rect.Top <= Bottom)) && (rect.Bottom >= Top));
        }

        public bool IntersectsWith(Ray2D ray)
        {
            var time = IntersectWith(ray);
            return time.HasValue;
        }

        public float? IntersectWith(Ray2D ray)
        {
            var time = 0.0f;
            float maxValue = int.MaxValue;
            
            if (Math.Abs(ray.Direction.X) < 1E-06f)
            {
                if ((ray.Position.X < X) || (ray.Position.X > Right))
                {
                    return null;
                }
            }
            else
            {
                var invDenom = 1f / ray.Direction.X;
                var time0 = (X - ray.Position.X) * invDenom;
                var time1 = (Right - ray.Position.X) * invDenom;
                if (time0 > time1)
                {
                    var temp = time0;
                    time0 = time1;
                    time1 = temp;
                }
                time = MathHelper.Max(time0, time);
                maxValue = MathHelper.Min(time1, maxValue);
                if (time > maxValue)
                {
                    return null;
                }
            }

            if (Math.Abs(ray.Direction.Y) < 1E-06f)
            {
                if ((ray.Position.Y < Y) || (ray.Position.Y > Bottom))
                {
                    return null;
                }
            }
            else
            {
                var invDenom = 1f / ray.Direction.Y;
                var time0 = (Y - ray.Position.Y) * invDenom;
                var time1 = (Bottom - ray.Position.Y) * invDenom;
                if (time0 > time1)
                {
                    var temp = time0;
                    time0 = time1;
                    time1 = temp;
                }
                time = MathHelper.Max(time0, time);
                maxValue = MathHelper.Min(time1, maxValue);
                if (time > maxValue)
                {
                    return null;
                }
            }

            return time;
        }

        public bool IntersectsWith(Point p1, Point p2)
        {
            // Find min and max X for the segment

            var minX = p1.X;
            var maxX = p2.X;

            if (p1.X > p2.X)
            {
                minX = p2.X;
                maxX = p1.X;
            }

            // Find the intersection of the segment's and rectangle's x-projections

            if (maxX > Right) maxX = Right;
            if (minX < Left) minX = Left;

            if (minX > maxX) // If their projections do not intersect return false
            {
                return false;
            }

            // Find corresponding min and max Y for min and max X we found before
            var minY = p1.Y;
            var maxY = p2.Y;

            var dx = p2.X - p1.X;

            if (Math.Abs(dx) > 0.000001)
            {
                var a = (p2.Y - p1.Y)/dx;
                var b = p1.Y - a*p1.X;
                minY = a*minX + b;
                maxY = a*maxX + b;
            }

            if (minY > maxY)
            {
                var tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            // Find the intersection of the segment's and rectangle's y-projections

            if (maxY > Bottom) maxY = Bottom;
            if (minY < Top) minY = Top;

            return !(minY > maxY);
        }

		public RectInt32 Intersect(RectInt32 rect)
        {
            if (!IntersectsWith(rect))
            {
                return Empty;
            }
            else
            {
                int num = Math.Max(Left, rect.Left);
                int num2 = Math.Max(Top, rect.Top);
				return new RectInt32() {
                width = Math.Max(Math.Min(Right, rect.Right) - num, 0),
                height = Math.Max(Math.Min(Bottom, rect.Bottom) - num2, 0),
                x = num,
                y = num2
				};
            }
        }

        public static RectInt32 Intersect(RectInt32 rect1, RectInt32 rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        public void Offset(int offsetX, int offsetY)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Cannot offset empty RectInt32.");
            }
            x += offsetX;
            y += offsetY;
        }

        public static RectInt32 Offset(RectInt32 rect, int offsetX, int offsetY)
        {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public void Inflate(int w, int h)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Cannot inflate empty RectInt32.");
            }
            x -= w;
            y -= h;
            width += w;
            width += w;
            height += h;
            height += h;
        }

        public void Scale(int scaleX, int scaleY)
        {
            if (!IsEmpty)
            {
                x *= scaleX;
                y *= scaleY;
                width *= scaleX;
                height *= scaleY;
                if (scaleX < 0.0)
                {
                    x += width;
                    width *= -1;
                }
                if (scaleY < 0.0)
                {
                    y += height;
                    height *= -1;
                }
            }
        }

        private bool ContainsInternal(int xPos, int yPos)
        {
            return ((((xPos >= x) && ((xPos - width) <= x)) && (yPos >= y)) && ((yPos - height) <= y));
        }

        static RectInt32()
        {
            s_empty = new RectInt32
            {
                x = 0,
                y = 0,
                width = 0,
                height = 0
            };
        }
	}
}
