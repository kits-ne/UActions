﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace UActions.Editor.Actions
{
    [Input("package-name", isOptional: true)]
    [Input("architectures", true, typeof(AndroidArchitecture[]), isOptional: true)]
    [Input("keystore", true, typeof(Keystore), isOptional: true)]
    [Input("increment-version-code", type: typeof(bool), isOptional: true)]
    [Input("optimized-frame-pacing", type: typeof(bool), isOptional: true)]
    [Input(KeyAppBundle, type: typeof(bool))]
    public class PlayerSettingsAndroid : IAction
    {
        private const string KeyAppBundle = "app-bundle";

        public class Registration : IRegistration
        {
            public void Register(DeserializerBuilder builder)
            {
#if UNITY_2021_2_OR_NEWER
                builder.WithTagMapping(new TagName("!architectures"), typeof(AndroidArchitecture[]));
                builder.WithTagMapping(new TagName("!keystore"), typeof(Keystore));
#else
                builder.WithTagMapping("!architectures", typeof(AndroidArchitecture[]));
                builder.WithTagMapping("!keystore", typeof(Keystore));
#endif
            }
        }

        [Serializable]
        public class Keystore
        {
            public string path;
            public string passwd;
            public string alias;
            public string aliasPasswd;
        }

        public TargetPlatform Targets => TargetPlatform.Android;

        public void Execute(IWorkflowContext context)
        {
            if (context.With.TryGetFormat("package-name", out string packageName))
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);
            }

            if (context.With.TryGetValue("architectures", out AndroidArchitecture[] architectures))
            {
                if (architectures.Contains(AndroidArchitecture.ARM64))
                {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                }

                PlayerSettings.Android.targetArchitectures = architectures
                    .Aggregate((acc, current) => acc | current);
            }

            if (context.With.TryGetValue("keystore", out Keystore keystore))
            {
                context.Log("setup keystore");
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = context.Format(keystore.path);
                PlayerSettings.Android.keystorePass = context.Format(keystore.passwd);
                PlayerSettings.Android.keyaliasName = context.Format(keystore.alias);
                PlayerSettings.Android.keyaliasPass = context.Format(keystore.aliasPasswd);
            }
            else if (context.With.TryIs("keystore", out bool value) && !value)
            {
                context.Log("clean keystore");
                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.Android.keystoreName = string.Empty;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasName = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
            }
            else
            {
                context.Log("skip keystore");
            }

            if (context.With.Is("increment-version-code"))
            {
                PlayerSettings.Android.bundleVersionCode++;
            }
            else if (context.With.TryGetFormat("version-code", out string versionCode))
            {
                PlayerSettings.Android.bundleVersionCode = Convert.ToInt32(versionCode);
                context.Log($"set version: {PlayerSettings.Android.bundleVersionCode}");
            }

#if UNITY_2019_4_OR_NEWER
            PlayerSettings.Android.optimizedFramePacing = context.With.Is("optimized-frame-pacing");
#endif
            EditorUserBuildSettings.buildAppBundle = context.With.Is(KeyAppBundle);
        }
    }
}