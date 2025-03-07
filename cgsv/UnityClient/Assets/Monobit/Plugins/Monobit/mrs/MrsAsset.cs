#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace mrs {
    public class Asset : AssetPostprocessor {
        public struct PluginPlatform {
            public string Path;
            public string[] PlatformNames;
            
            public PluginPlatform( string path, string[] platform_names ){
                Path = path;
                PlatformNames = platform_names;
            }
        }
        
        private static List< PluginPlatform > s_PluginPlatforms;
        
        static Asset(){
            s_PluginPlatforms = new List< PluginPlatform >( 50 );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/x86/mrs.dll", new string[]{ "windows32" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/x86_64/mrs.dll", new string[]{ "windows64" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/Monobit/mrs/mrs.bundle", new string[]{ "osx" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/Android/libs/x86/libmrs.so", new string[]{ "android32" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/Android/libs/armeabi-v7a/libmrs.so", new string[]{ "androidV7" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/Android/libs/arm64-v8a/libmrs.so", new string[]{ "android64" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/iOS/Monobit/mrs/library/libenet.a", new string[]{ "ios" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/iOS/Monobit/mrs/library/libuv.a", new string[]{ "ios" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/iOS/Monobit/mrs/library/libcrypto.a", new string[]{ "ios" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/iOS/Monobit/mrs/library/libssl.a", new string[]{ "ios" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/iOS/Monobit/mrs/library/libdecrepit.a", new string[]{ "ios" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/iOS/Monobit/mrs/library/libmrs.a", new string[]{ "ios" } ) );
            s_PluginPlatforms.Add( new PluginPlatform( "Assets/Monobit/Plugins/Monobit/mrs/mrs.jslib", new string[]{ "webgl" } ) );
            Update();
        }
        
        public static void AddPluginPlatform( PluginPlatform plugin_platform ){
            s_PluginPlatforms.Add( plugin_platform );
        }
        
        public static void Update(){
#if UNITY_WSA
            PlayerSettings.WSA.SetCapability( PlayerSettings.WSACapability.InternetClient, true );
            PlayerSettings.WSA.SetCapability( PlayerSettings.WSACapability.Microphone, true );
#endif
            if ( 0 == s_PluginPlatforms.Count ) return;
            
            foreach ( PluginPlatform pluginPlatform in s_PluginPlatforms ){
                AssetImporter asset = AssetImporter.GetAtPath( pluginPlatform.Path );
                if ( null == asset ) continue;
                
                PluginImporter plugin = (PluginImporter)asset;
                ResetPlatformAll( plugin );
                
                foreach ( string platform_name in pluginPlatform.PlatformNames ){
                    SetPlatform( plugin, platform_name, true );
                }
            }
        }
        
        public static void SetPlatform( PluginImporter plugin, string platform_name, bool enable ){
            if ( "windows32" == platform_name ){
                SetPlatform( plugin, BuildTarget.StandaloneWindows, enable );
                SetPlatform( plugin, BuildTarget.WSAPlayer, enable );
                if ( enable ){
                    SetPlatformCPU( plugin, BuildTarget.WSAPlayer, "X86" );
                }
            }else if ( "windows64" == platform_name ){
                SetPlatform( plugin, BuildTarget.StandaloneWindows64, enable );
                SetPlatform( plugin, BuildTarget.WSAPlayer, enable );
                if ( enable ){
                    SetPlatformCPU( plugin, BuildTarget.WSAPlayer, "X64" );
                }
            }else if ( "osx" == platform_name ){
#if UNITY_2017_3_OR_NEWER
                SetPlatform( plugin, BuildTarget.StandaloneOSX, enable );
#else
                SetPlatform( plugin, BuildTarget.StandaloneOSXIntel, enable );
                SetPlatform( plugin, BuildTarget.StandaloneOSXIntel64, enable );
                SetPlatform( plugin, BuildTarget.StandaloneOSXUniversal, enable );
#endif
            }else if ( "android32" == platform_name ){
                SetPlatform( plugin, BuildTarget.Android, enable );
                SetPlatformCPU( plugin, BuildTarget.Android, "x86" );
            }else if ( "androidV7" == platform_name ){
                SetPlatform( plugin, BuildTarget.Android, enable );
                SetPlatformCPU( plugin, BuildTarget.Android, "ARMv7" );
            }else if ( "android64" == platform_name ){
                SetPlatform( plugin, BuildTarget.Android, enable );
                SetPlatformCPU( plugin, BuildTarget.Android, "ARM64" );
            }else if ( "ios" == platform_name ){
                SetPlatform( plugin, BuildTarget.iOS, enable );
            }else if ( "webgl" == platform_name ){
                SetPlatform( plugin, BuildTarget.WebGL, enable );
            }else{
                UnityEngine.Debug.LogError( "Invalid platform_name: "+ platform_name +" (at "+ plugin.assetPath +")" );
            }
        }
        
        public static void SetPlatform( PluginImporter plugin, BuildTarget platform, bool enable ){
            plugin.SetCompatibleWithPlatform( platform, enable );
            
            if ( enable ){
                switch ( platform ){
                case BuildTarget.StandaloneWindows:{
                    SetPlatformEditor( plugin, "Windows", "X86" );
                }break;
                
                case BuildTarget.StandaloneWindows64:{
                    SetPlatformEditor( plugin, "Windows", "X86_64" );
                }break;
                
#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:{
                    SetPlatformCPU( plugin, platform, "AnyCPU" );
                    SetPlatformEditor( plugin, "OSX", "AnyCPU" );
                }break;
#else
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:{
                    SetPlatformCPU( plugin, platform, "AnyCPU" );
                }break;
                
                case BuildTarget.StandaloneOSXUniversal:{
                    SetPlatformCPU( plugin, platform, "None" );
                    SetPlatformEditor( plugin, "OSX", "AnyCPU" );
                }break;
#endif
                }
            }
        }
        
        public static void SetPlatformCPU( PluginImporter plugin, BuildTarget platform, string cpu ){
            plugin.SetPlatformData( platform, "CPU", cpu );
        }
        
        public static void SetPlatformEditor( PluginImporter plugin, string os, string cpu ){
            SetPlatformEditor( plugin, true );
            plugin.SetPlatformData( "Editor", "OS", os );
            plugin.SetPlatformData( "Editor", "CPU", cpu );
        }
        
        public static void SetPlatformEditor( PluginImporter plugin, bool enable ){
            plugin.SetCompatibleWithEditor( enable );
        }
        
        public static void SetPlatformAny( PluginImporter plugin, bool enable ){
            plugin.SetCompatibleWithAnyPlatform( enable );
        }
        
        public static void ResetPlatformAll( PluginImporter plugin ){
            SetPlatform( plugin, BuildTarget.Android, false );
            
            SetPlatform( plugin, BuildTarget.iOS, false );
            
            SetPlatform( plugin, BuildTarget.StandaloneLinux, false );
            SetPlatform( plugin, BuildTarget.StandaloneLinux64, false );
            SetPlatform( plugin, BuildTarget.StandaloneLinuxUniversal, false );
            
            SetPlatform( plugin, "osx", false );
            
            SetPlatform( plugin, BuildTarget.StandaloneWindows, false );
            SetPlatform( plugin, BuildTarget.StandaloneWindows64, false );
            SetPlatform( plugin, BuildTarget.WSAPlayer, false );
            
            SetPlatformEditor( plugin, false );
            
            SetPlatformAny( plugin, false );
        }
        
        private static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths ){
            Update();
        }
    }
}
#endif
