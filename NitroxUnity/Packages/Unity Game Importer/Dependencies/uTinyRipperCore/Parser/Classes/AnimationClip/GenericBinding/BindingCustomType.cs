﻿namespace uTinyRipper.Classes.AnimationClips
{
	public enum BindingCustomType : byte
	{
		None				= 0,
		Transform			= 4,
		AnimatorMuscle		= 8,

		BlendShape			= 20,
		Renderer			= 21,
		RendererMaterial	= 22,
		SpriteRenderer		= 23,
		MonoBehaviour		= 24,
		Light				= 25,
		RendererShadows		= 26,
		ParticleSystem		= 27,
		RectTransform		= 28,
		LineRenderer		= 29,
		TrailRenderer		= 30,
		PositionConstraint	= 31,
		RotationConstraint	= 32,
		ScaleConstraint		= 33,
		AimConstraint		= 34,
		ParentConstraint	= 35,
		LookAtConstraint	= 36,
		Camera				= 37,
	}
}
