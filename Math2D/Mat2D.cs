using System;

namespace Nima.Math2D
{
	public class Mat2D
	{
		private float[] m_Buffer;

		public float this[int index]
		{
			get
			{
				return m_Buffer[index];	
			}
			set
			{
				m_Buffer[index] = value;
			}
		}

		public float[] Values
		{
			get
			{
				return m_Buffer;
			}
		}

		public Mat2D()
		{
			m_Buffer = new float[6]{1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f};
		}

		public Mat2D(Mat2D copy) : this()
		{
			m_Buffer[0] = copy.m_Buffer[0];
			m_Buffer[1] = copy.m_Buffer[1];
			m_Buffer[2] = copy.m_Buffer[2];
			m_Buffer[3] = copy.m_Buffer[3];
			m_Buffer[4] = copy.m_Buffer[4];
			m_Buffer[5] = copy.m_Buffer[5];
		}

		static public void FromRotation(Mat2D o, float rad)
		{
			float s = (float)Math.Sin(rad);
			float c = (float)Math.Cos(rad);
			o[0] = c;
			o[1] = s;
			o[2] = -s;
			o[3] = c;
			o[4] = 0;
			o[5] = 0;
		}

		static public void Scale(Mat2D o, Mat2D a, Vec2D v)
		{
			float a0 = a[0], a1 = a[1], a2 = a[2], a3 = a[3], a4 = a[4], a5 = a[5], v0 = v[0], v1 = v[1];
			o[0] = a0 * v0;
			o[1] = a1 * v0;
			o[2] = a2 * v1;
			o[3] = a3 * v1;
			o[4] = a4;
			o[5] = a5;
		}

		static public void Multiply(Mat2D o, Mat2D a, Mat2D b) 
		{
			float a0 = a[0], a1 = a[1], a2 = a[2], a3 = a[3], a4 = a[4], a5 = a[5],
					b0 = b[0], b1 = b[1], b2 = b[2], b3 = b[3], b4 = b[4], b5 = b[5];
			o[0] = a0 * b0 + a2 * b1;
			o[1] = a1 * b0 + a3 * b1;
			o[2] = a0 * b2 + a2 * b3;
			o[3] = a1 * b2 + a3 * b3;
			o[4] = a0 * b4 + a2 * b5 + a4;
			o[5] = a1 * b4 + a3 * b5 + a5;
		}

		static public void Copy(Mat2D o, Mat2D a)
		{
			o[0] = a[0];
			o[1] = a[1];
			o[2] = a[2];
			o[3] = a[3];
			o[4] = a[4];
			o[5] = a[5];
		}

		static public bool Invert(Mat2D o, Mat2D a)
		{
			float aa = a[0], ab = a[1], ac = a[2], ad = a[3], atx = a[4], aty = a[5];

			float det = aa * ad - ab * ac;
			if(det == 0.0f)
			{
				return false;
			}
			det = 1.0f / det;

			o[0] = ad * det;
			o[1] = -ab * det;
			o[2] = -ac * det;
			o[3] = aa * det;
			o[4] = (ac * aty - ad * atx) * det;
			o[5] = (ab * atx - aa * aty) * det;
			return true;
		}

	}
}