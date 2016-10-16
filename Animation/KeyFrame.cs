using System;
using System.IO;
using System.Collections.Generic;

namespace Nima.Animation
{
	public abstract class KeyFrameInterpolator
	{
		abstract public bool SetNextFrame(KeyFrameWithInterpolation frame, KeyFrame nextFrame);
	}

	// TODO: CSS style curve interoplation
	public class ProgressionTimeCurveInterpolator : KeyFrameInterpolator
	{
		public override bool SetNextFrame(KeyFrameWithInterpolation frame, KeyFrame nextFrame)
		{
			return false;
		}
	}
	
	public abstract class KeyFrame
	{
		public enum InterpolationTypes
		{
			Hold = 0,
			Linear = 1,
			Mirrored = 2,
			Asymmetric = 3,
			Disconnected = 4,
			Progression = 5
		};

		protected float m_Time;

		public float Time
		{
			get
			{
				return m_Time;
			}
		}

		public static bool Read(BinaryReader reader, KeyFrame frame)
		{
			frame.m_Time = (float)reader.ReadDouble();

			return true;
		}

		public abstract void SetNext(KeyFrame frame);
		public abstract void ApplyInterpolation(ActorNode node, float time, KeyFrame toFrame, float mix);
		public abstract void Apply(ActorNode node, float mix);
	}

	public abstract class KeyFrameWithInterpolation : KeyFrame
	{
		protected InterpolationTypes m_InterpolationType;
		protected KeyFrameInterpolator m_Interpolator;

		public InterpolationTypes InterpolationType
		{
			get
			{
				return m_InterpolationType;
			}
		}
		
		public KeyFrameInterpolator Interpolator
		{
			get
			{
				return m_Interpolator;
			}
		}

		public static bool Read(BinaryReader reader, KeyFrameWithInterpolation frame)
		{
			if(!KeyFrame.Read(reader, frame))
			{
				return false;
			}
			int type = reader.ReadByte();
			if(!Enum.IsDefined(typeof(InterpolationTypes), type))
			{
				return false;
			}
			
			frame.m_InterpolationType = (InterpolationTypes)type;
			switch(frame.m_InterpolationType)
			{
				case KeyFrame.InterpolationTypes.Mirrored:
				case KeyFrame.InterpolationTypes.Asymmetric:
				case KeyFrame.InterpolationTypes.Disconnected:
				case KeyFrame.InterpolationTypes.Hold:
					frame.m_Interpolator = ValueTimeCurveInterpolator.Read(reader, frame.m_InterpolationType);
					break;

				default:
					frame.m_Interpolator = null;
					break;

			}
			return true;
		}

		public override void SetNext(KeyFrame frame)
		{
			// Null out the interpolator if the next frame doesn't validate.
			if(m_Interpolator != null && !m_Interpolator.SetNextFrame(this, frame))
			{
				m_Interpolator = null;
			}
		}
	}

	public abstract class KeyFrameNumeric : KeyFrameWithInterpolation
	{
		private float m_Value;

		public float Value
		{
			get
			{
				return m_Value;
			}
		}

		public static bool Read(BinaryReader reader, KeyFrameNumeric frame)
		{
			if(!KeyFrameWithInterpolation.Read(reader, frame))
			{
				return false;
			}
			frame.m_Value = reader.ReadSingle();
			/*if(frame.m_Interpolator != null)
			{
				// TODO: in the future, this could also be a progression curve.
				ValueTimeCurveInterpolator vtci = frame.m_Interpolator as ValueTimeCurveInterpolator;
				if(vtci != null)
				{
					vtci.SetKeyFrameValue(m_Value);
				}
			}*/
			return true;
		}

		public override void ApplyInterpolation(ActorNode node, float time, KeyFrame toFrame, float mix)
		{
			switch(m_InterpolationType)
			{
				case KeyFrame.InterpolationTypes.Mirrored:
				case KeyFrame.InterpolationTypes.Asymmetric:
				case KeyFrame.InterpolationTypes.Disconnected:
				{
					ValueTimeCurveInterpolator interpolator = m_Interpolator as ValueTimeCurveInterpolator;
					if(interpolator != null)
					{
						float v = (float)interpolator.Get((double)time);
						SetValue(node, v, mix);
					}
					break;
				}

				case KeyFrame.InterpolationTypes.Hold:
				{
					SetValue(node, m_Value, mix);
					break;
				}

				case KeyFrame.InterpolationTypes.Linear:
				{
					KeyFrameNumeric to = toFrame as KeyFrameNumeric;

					float f = (time - m_Time)/(to.m_Time-m_Time);
					SetValue(node, m_Value * (1.0f-f) + to.m_Value * f, mix);
					break;
				}
			}
		}
		
		public override void Apply(ActorNode node, float mix)
		{
			SetValue(node, m_Value, mix);
		}

		protected abstract void SetValue(ActorNode node, float value, float mix);
	}

	public class KeyFramePosX : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFramePosX frame = new KeyFramePosX();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			node.X = node.X * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFramePosY : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFramePosY frame = new KeyFramePosY();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			node.Y = node.Y * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFrameScaleX : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameScaleX frame = new KeyFrameScaleX();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			node.ScaleX = node.ScaleX * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFrameScaleY : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameScaleY frame = new KeyFrameScaleY();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			node.ScaleY = node.ScaleY * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFrameRotation : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameRotation frame = new KeyFrameRotation();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			node.Rotation = node.Rotation * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFrameOpacity : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameOpacity frame = new KeyFrameOpacity();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			node.Opacity = node.Opacity * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFrameLength : KeyFrameNumeric
	{
		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameLength frame = new KeyFrameLength();
			if(KeyFrameNumeric.Read(reader, frame))
			{
				return frame;
			}
			return null;
		}

		protected override void SetValue(ActorNode node, float value, float mix)
		{
			ActorBone bone = node as ActorBone;
			if(bone == null)
			{
				return;
			}
			bone.Length = bone.Length * (1.0f - mix) + value * mix;
		}
	}

	public class KeyFrameDrawOrder : KeyFrame
	{
		private struct DrawOrderIndex
		{
			public ushort nodeIdx;
			public ushort order;
		}
		private DrawOrderIndex[] m_OrderedNodes;

		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameDrawOrder frame = new KeyFrameDrawOrder();
			if(!KeyFrame.Read(reader, frame))
			{
				return null;
			}
			int numOrderedNodes = reader.ReadUInt16();
			frame.m_OrderedNodes = new DrawOrderIndex[numOrderedNodes];
			for(int i = 0; i < numOrderedNodes; i++)
			{
				DrawOrderIndex drawOrder = new DrawOrderIndex();
				drawOrder.nodeIdx = reader.ReadUInt16();
				drawOrder.order = reader.ReadUInt16();
				frame.m_OrderedNodes[i] = drawOrder;
			}
			return frame;
		}

		public override void SetNext(KeyFrame frame)
		{
			// Do nothing.
		}

		public override void ApplyInterpolation(ActorNode node, float time, KeyFrame toFrame, float mix)
		{
			Apply(node, mix);
		}

		public override void Apply(ActorNode node, float mix)
		{
			Actor actor = node.Actor;

			foreach(DrawOrderIndex doi in m_OrderedNodes)
			{
				ActorImage actorImage = actor[doi.nodeIdx] as ActorImage;
				if(actorImage != null)
				{
					actorImage.DrawOrder = doi.order;
				}
			}
		}
	}

	public class KeyFrameVertexDeform : KeyFrameWithInterpolation
	{
		private float[] m_Vertices;

		public float[] Vertices
		{
			get
			{
				return m_Vertices;
			}
		}

		public static KeyFrame Read(BinaryReader reader, ActorNode node)
		{
			KeyFrameVertexDeform frame = new KeyFrameVertexDeform();
			if(!KeyFrameWithInterpolation.Read(reader, frame))
			{
				return null;
			}

			ActorImage imageNode = node as ActorImage;
			frame.m_Vertices = new float[imageNode.VertexCount * 2];
			Actor.ReadFloat32Array(reader, frame.m_Vertices);
			
			imageNode.DoesAnimationVertexDeform = true;

			return frame;
		}

		public void TransformVertices(float[] wt)
		{
			int aiVertexCount = m_Vertices.Length/2;
			float[] fv = m_Vertices;

			int vidx = 0;
			for(int j = 0; j < aiVertexCount; j++)
			{
				float x = fv[vidx];
				float y = fv[vidx+1];

				fv[vidx] = wt[0] * x + wt[2] * y + wt[4];
				fv[vidx+1] = wt[1] * x + wt[3] * y + wt[5];

				vidx += 2;
			}
		}

		public override void SetNext(KeyFrame frame)
		{
			// Do nothing.
		}

		public override void ApplyInterpolation(ActorNode node, float time, KeyFrame toFrame, float mix)
		{
			ActorImage imageNode = node as ActorImage;
			float[] wr = imageNode.AnimationDeformedVertices;
			float[] to = (toFrame as KeyFrameVertexDeform).m_Vertices;
			int l = m_Vertices.Length;

			float f = (time - m_Time)/(toFrame.Time-m_Time);
			float fi = 1.0f - f;
			if(mix == 1.0f)
			{
				for(int i = 0; i < l; i++)
				{
					wr[i] = m_Vertices[i] * fi + to[i] * f;
				}
			}
			else
			{
				float mixi = 1.0f - mix;
				for(int i = 0; i < l; i++)
				{
					float v = m_Vertices[i] * fi + to[i] * f;

					wr[i] = wr[i] * mixi + v * mix;
				}
			}

			imageNode.IsVertexDeformDirty = true;
		}
		
		public override void Apply(ActorNode node, float mix)
		{
			ActorImage imageNode = node as ActorImage;
			int l = m_Vertices.Length;
			float[] wr = imageNode.AnimationDeformedVertices;
			if(mix == 1.0f)
			{
				for(int i = 0; i < l; i++)
				{
					wr[i] = m_Vertices[i];
				}
			}
			else
			{
				float mixi = 1.0f - mix;
				for(int i = 0; i < l; i++)
				{
					wr[i] = wr[i] * mixi + m_Vertices[i] * mix;
				}
			}

			imageNode.IsVertexDeformDirty = true;
		}
	}

}