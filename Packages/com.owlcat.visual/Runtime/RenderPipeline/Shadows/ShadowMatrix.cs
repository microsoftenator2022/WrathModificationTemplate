using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows
{
	[GenerateHLSL(PackingRules.Exact, false, false, false, 1, false, false, false, -1)]
	public struct ShadowMatrix
	{
		public Matrix4x4 worldToShadow;

		public Vector3 spherePosition;

		public float sphereRadius;

		public float sphereRadiusSq;

		public Vector3 lightDirection;

		public float normalBias;

		public float depthBias;

		public Vector2 unused;
	}
}
