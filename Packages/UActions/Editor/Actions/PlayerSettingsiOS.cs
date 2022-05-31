﻿using System;
using System.Collections.Generic;
using UnityEditor;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace UActions.Editor.Actions
{
    [Action("player-settings-ios")]
    public class PlayerSettingsiOS : IAction
    {
        public class Registration : IRegistration
        {
            public void Register(DeserializerBuilder builder)
            {
                builder.WithTagMapping(new TagName("!iossdkversion"), typeof(iOSSdkVersion));
            }
        }

        public TargetPlatform Targets => TargetPlatform.iOS;
        private readonly Dictionary<string, object> _with;


        [Input("identifier", type: typeof(string), isOptional: true)]
        [Input("increment-version-code", type: typeof(bool), isOptional: true)]
        [Input("target-sdk", "!iossdkversion", typeof(iOSSdkVersion), isOptional: true)]
        [Input("ios-version", type: typeof(string), isOptional: true)]
        public PlayerSettingsiOS(Dictionary<string, object> with)
        {
            _with = with;
        }

        public void Execute(WorkflowContext context)
        {
            if (_with.TryGetIsValue("identifier", out string identifier))
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, identifier);
            }

            if (_with.Is("increment-version-code"))
            {
                var buildNumber = Convert.ToUInt32(PlayerSettings.iOS.buildNumber);
                PlayerSettings.iOS.buildNumber = (buildNumber + 1).ToString();
            }

            PlayerSettings.iOS.sdkVersion = _with.GetIsValue("target-sdk", iOSSdkVersion.DeviceSDK);

            if (_with.TryGetIsValue("ios-version", out string version))
            {
                PlayerSettings.iOS.targetOSVersionString = version;
            }
        }
    }
}