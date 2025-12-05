using System;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace MoreMountains.FeedbacksForThirdParty
{
	[Serializable]
	public class NVHapticData
	{
		public int SampleCount;
		public HapticClip Clip;
		public List<NVAmplitudePoint> AmplitudePoints;
		public List<NVFrequencyPoint> FrequencyPoints;
		public GamepadRumble RumbleData;
	}

	[Serializable]
	public class NVHapticFile
	{
		public NVVersion version;
		public NVMetadata metadata;
		public NVSignals signals;
	}

	[Serializable]
	public class NVVersion
	{
		public int major = 1;
		public int minor = 0;
		public int patch = 0;
	}

	[Serializable]
	public class NVMetadata
	{
		public string editor;
		public string author;
		public string source;
		public string project;
		public List<string> tags;
		public string description;
	}

	[Serializable]
	public class NVSignals
	{
		public NVContinuous continuous;
	}

	[Serializable]
	public class NVContinuous
	{
		public NVEnvelopes envelopes;
	}

	[Serializable]
	public class NVEnvelopes
	{
		public List<NVAmplitudePoint> amplitude;
		public List<NVFrequencyPoint> frequency;
	}

	[Serializable]
	public class NVAmplitudePoint
	{
		public float time;
		public float amplitude;
		public NVEmphasis emphasis;
	}

	[Serializable]
	public class NVFrequencyPoint
	{
		public float time;
		public float frequency;
	}

	[Serializable]
	public class NVEmphasis
	{
		public float amplitude;
		public float frequency;
	}
}