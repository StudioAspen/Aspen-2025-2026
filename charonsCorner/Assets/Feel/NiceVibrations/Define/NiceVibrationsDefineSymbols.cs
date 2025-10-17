using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Profile;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	#if UNITY_EDITOR
	/// <summary>
	/// This class lets you specify (in code, by editing it) symbols that will be added to the build settings' define symbols list automatically
	/// </summary>
	[InitializeOnLoad]
	public class NiceVibrationsDefineSymbols
	{
		/// <summary>
		/// A list of all the symbols you want added to the build settings
		/// </summary>
		public static readonly string[] Symbols = new string[]
		{
			"MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED"
		};

		/// <summary>
		/// As soon as this class has finished compiling, adds the specified define symbols to the build settings
		/// </summary>
		static NiceVibrationsDefineSymbols()
		{
			BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();

			if (activeProfile != null)
			{
				var symbols = PlayerSettings.GetScriptingDefineSymbols(
					UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings
						.selectedBuildTargetGroup));

				if (!symbols.Contains("MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED"))
				{
					string[] currentDefines = activeProfile.scriptingDefines;
					if (!Array.Exists(currentDefines, define => define == Symbols[0]))
					{
						var updatedDefines = new List<string>(currentDefines);
						updatedDefines.Add(Symbols[0]);
						activeProfile.scriptingDefines = updatedDefines.ToArray();
					}
				}
			}
			else
			{
				string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
				List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
				scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
				PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), string.Join(";", scriptingDefinesStringList.ToArray()));
			}
		}
	}
	#endif
}