﻿using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ColorModule : ParticleSystemModule
	{
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gradient.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(GradientName, Gradient.ExportYAML(container));
			return node;
		}

		public const string GradientName = "gradient";

		public MinMaxGradient Gradient;
	}
}
