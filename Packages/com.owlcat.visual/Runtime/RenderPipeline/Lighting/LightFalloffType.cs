using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1)]
	public enum LightFalloffType
	{
		InverseSquared,
		Legacy
	}
}