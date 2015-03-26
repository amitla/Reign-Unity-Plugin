﻿#if UNITY_BLACKBERRY
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Reign.Plugin
{
    public class BlackBerryAdvertising_AdPlugin_BB10 : IAdPlugin
    {
    	private bool visible = true;
		public bool Visible
		{
			get {return visible;}
			set
			{
				Debug.Log("Visible Ad property does not work on native BB10 Ads.");
				/*visible = value;
				if (!value)
				{
					if (bbads_banner_set_position(banner, (uint)(UnityEngine.Screen.width-(width*2)), (uint)(UnityEngine.Screen.height-(height*2))) != 0)
					{
						Debug.LogError("FAILED: bbads_banner_set_position");
					}
				}
				else
				{
					SetGravity(gravity);
				}*/
			}
		}
		
		private static IntPtr context, window;
		private IntPtr banner;
		private uint width, height;
		//private AdGravity gravity;
		
		private const uint BANNER_TEST_ZONE_ID = 117145;
		
		private const int SCREEN_PROPERTY_TYPE = 47;
		private const int SCREEN_EVENT_CREATE = 1;
		private const int SCREEN_CHILD_WINDOW = 1;
		private const int SCREEN_EMBEDDED_WINDOW = 2;
		private const int SCREEN_APPLICATION_CONTEXT = 0;
		private const int SCREEN_FORMAT_RGBA8888 = 8;
		private const int SCREEN_USAGE_NATIVE = (1 << 3);
		private const int SCREEN_PROPERTY_VISIBLE = 51;
		private const int SCREEN_PROPERTY_FORMAT = 14;
		private const int SCREEN_PROPERTY_USAGE = 48;
		private const int SCREEN_PROPERTY_TRANSPARENCY = 46;
		private const int SCREEN_TRANSPARENCY_SOURCE_OVER = 3;
		private const int SCREEN_PROPERTY_BUFFER_SIZE = 5;
		private const int SCREEN_PROPERTY_SOURCE_SIZE = 42;
		private const int SCREEN_PROPERTY_COLOR = 93;
		
		private const uint NAVIGATOR_WINDOW_ACTIVE = 0x0a;
		private const uint NAVIGATOR_WINDOW_STATE = 0x03;
		private const uint NAVIGATOR_WINDOW_INACTIVE = 0x0b;
		
		// libscreen
		[DllImport("libscreen", EntryPoint="screen_create_context")]
		private static extern int screen_create_context(ref IntPtr context, int flags);
		
		[DllImport("libscreen", EntryPoint="screen_create_window_type")]
		private static extern int screen_create_window_type(ref IntPtr window, IntPtr context, int flags);
		
		[DllImport("libscreen", EntryPoint="screen_create_window_group")]
		private static extern int screen_create_window_group(IntPtr window, string name);
		
		[DllImport("libscreen", EntryPoint="screen_join_window_group")]
		private static extern int screen_join_window_group(IntPtr window, string name);
		
		[DllImport("libscreen", EntryPoint="screen_destroy_window")]
		private static extern int screen_destroy_window(IntPtr window);
		
		[DllImport("libscreen", EntryPoint="screen_destroy_context")]
		private static extern int screen_destroy_context(IntPtr context);
		
		[DllImport("libscreen", EntryPoint="screen_set_window_property_iv")]
		private static extern int screen_set_window_property_iv(IntPtr context, int pname, ref int param);
		
		[DllImport("libscreen", EntryPoint="screen_create_window_buffers")]
		private static extern int screen_create_window_buffers(IntPtr window, int count);
		
		[DllImport("libscreen", EntryPoint="screen_flush_context")]
		private static extern int screen_flush_context(IntPtr window, int flags);
		
		// libbbads
		[DllImport("libbbads", EntryPoint="bbads_banner_create")]
		private static extern int bbads_banner_create(ref IntPtr banner, IntPtr window, string name, uint id);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_set_size")]
		private static extern int bbads_banner_set_size(IntPtr banner, uint width, uint height);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_set_position")]
		private static extern int bbads_banner_set_position(IntPtr banner, uint x, uint y);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_load")]
		private static extern int bbads_banner_load(IntPtr banner);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_set_window_visible")]
		private static extern int bbads_banner_set_window_visible(IntPtr banner);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_display")]
		private static extern int bbads_banner_display(IntPtr banner, IntPtr context, IntPtr _event);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_is_visible")]
		private static extern int bbads_banner_is_visible(IntPtr banner, ref int visible);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_set_placeholder_url")]
		private static extern int bbads_banner_set_placeholder_url(IntPtr banner, string placeholder_url);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_set_refresh_rate")]
		private static extern int bbads_banner_set_refresh_rate(IntPtr banner, uint seconds);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_set_border_width")]
		private static extern int bbads_banner_set_border_width(IntPtr banner, uint width);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_request_events")]
		private static extern int bbads_banner_request_events(IntPtr banner);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_stop_events")]
		private static extern int bbads_banner_stop_events(IntPtr banner);
		
		[DllImport("libbbads", EntryPoint="bbads_banner_destroy")]
		private static extern int bbads_banner_destroy(IntPtr banner);
		
		// libbps
		[DllImport("libbps", EntryPoint="screen_event_get_event")]
		private static extern IntPtr screen_event_get_event(IntPtr _event);
		
		[DllImport("libbps", EntryPoint="screen_get_event_property_iv")]
		private static extern int screen_get_event_property_iv(IntPtr _screen_event, int name, ref int parm);

		public BlackBerryAdvertising_AdPlugin_BB10(AdDesc desc, AdCreatedCallbackMethod createdCallback)
		{
			try
			{
				// get root window group id
				int id = Common.getpid();
				string windowGroup = id.ToString();
				Debug.Log("getpid: " + id);
			
				// create a screen to place ads in
				if (context == IntPtr.Zero)
				{
					if (screen_create_context(ref context, SCREEN_APPLICATION_CONTEXT) != 0) throw new Exception("FAILED: screen_create_context");
					if (screen_create_window_type(ref window, context, SCREEN_EMBEDDED_WINDOW) != 0) throw new Exception("FAILED: screen_create_window_type");
					
					int usage = SCREEN_USAGE_NATIVE;
					if (screen_set_window_property_iv(window, SCREEN_PROPERTY_USAGE, ref usage) != 0) throw new Exception("FAILED: screen_set_window_property_iv SCREEN_PROPERTY_USAGE");
					if (screen_join_window_group(window, windowGroup) != 0) throw new Exception("FAILED: screen_join_window_group");
				}
				
				// create ad banner
				if (bbads_banner_create(ref banner, window, windowGroup, desc.Testing ? BANNER_TEST_ZONE_ID : uint.Parse(desc.BB10_BlackBerryAdvertising_ZoneID)) != 0) throw new Exception("FAILED: bbads_banner_create");
				string placeholderFile = "";
				switch (desc.BB10_BlackBerryAdvertising_AdSize)
				{
					case BB10_BlackBerryAdvertising_AdSize.Wide_320x53:
						width = 320;
						height = 53;
						placeholderFile = "PlaceHolder_320x53.png";
						break;
						
					case BB10_BlackBerryAdvertising_AdSize.Wide_300x50:
						width = 300;
						height = 50;
						placeholderFile = "PlaceHolder_300x50.png";
						break;
						
					case BB10_BlackBerryAdvertising_AdSize.Wide_216x36:
						width = 216;
						height = 36;
						placeholderFile = "PlaceHolder_216x36.png";
						break;
						
					case BB10_BlackBerryAdvertising_AdSize.Wide_168x28:
						width = 168;
						height = 28;
						placeholderFile = "PlaceHolder_168x28.png";
						break;
					
					case BB10_BlackBerryAdvertising_AdSize.Wide_120x20:
						width = 120;
						height = 20;
						placeholderFile = "laceHolder_120x20.png";
						break;
						
					default:
						throw new Exception("AdPlugin: Unsuported Ad size: " + desc.BB10_BlackBerryAdvertising_AdSize);
				}
				
				if (bbads_banner_set_size(banner, width, height) != 0) throw new Exception("FAILED: bbads_banner_set_size");
				SetGravity(desc.BB10_BlackBerryAdvertising_AdGravity);
				
				if (bbads_banner_set_placeholder_url(banner, "file://%s/app/native/Data/Raw/Reign/BB10/Ads/"+placeholderFile) != 0) throw new Exception("FAILED: bbads_banner_set_placeholder_url");
				if (bbads_banner_set_refresh_rate(banner, (uint)desc.BB10_BlackBerryAdvertising_RefreshRate) != 0) throw new Exception("FAILED: bbads_banner_set_refresh_rate");
				if (bbads_banner_set_border_width(banner, 0) != 0) throw new Exception("FAILED: bbads_banner_set_border_width");
				if (bbads_banner_request_events(banner) != 0) throw new Exception("FAILED: bbads_banner_request_events");
				if (bbads_banner_load(banner) != 0) throw new Exception("FAILED: bbads_banner_load");
				
				// wait for add to load
				while (true)
				{
					IntPtr _event = IntPtr.Zero;
					Common.bps_get_event(ref _event, -1);// wait here for next event
					if (_event != IntPtr.Zero)
					{
						if (Common.screen_get_domain() == Common.bps_event_get_domain(_event))
						{
							int screen_val = 0;
							
							IntPtr screenEvent = screen_event_get_event(_event);
							if (screenEvent == IntPtr.Zero) throw new Exception("FAILED: screen_event_get_event");
							if (screen_get_event_property_iv(screenEvent, SCREEN_PROPERTY_TYPE, ref screen_val) != 0) throw new Exception("FAILED: screen_get_event_property_iv");
							
							if (screen_val == SCREEN_EVENT_CREATE)
							{
								int visible = 0;
								if (bbads_banner_is_visible(banner, ref visible) != 0) throw new Exception("FAILED: bbads_banner_is_visible");
								if (visible == 0)
								{
									if (bbads_banner_set_window_visible(banner) != 0) throw new Exception("FAILED: bbads_banner_set_window_visible");
									int error = bbads_banner_display(banner, context, _event);
									//if (error != 0) throw new Exception("FAILED: bbads_banner_display with ErrorCode: " + error);
									if (error != 0) Debug.LogError("FAILED: bbads_banner_display with ErrorCode: " + error + " NOTE: Ads still work, don't know why this error happens.");
									Debug.Log("Ad created for ZoneID: " + (desc.Testing ? BANNER_TEST_ZONE_ID.ToString() : desc.BB10_BlackBerryAdvertising_ZoneID));
								}
								
								break;
							}
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
				if (createdCallback != null) createdCallback(false);
				return;
			}
			
			if (createdCallback != null) createdCallback(true);
		}

		public void Dispose()
		{
			if (banner != IntPtr.Zero)
			{
				if (bbads_banner_stop_events(banner) != 0) Debug.LogError("FAILED: bbads_banner_stop_events");
				if (bbads_banner_destroy(banner) != 0) Debug.LogError("FAILED: bbads_banner_destroy");
				banner = IntPtr.Zero;
			}
			
			//if (screen_destroy_window(window) != 0) Debug.LogError("FAILED: screen_destroy_window");
			//if (screen_destroy_context(context) != 0) Debug.LogError("FAILED: screen_destroy_context");
		}
		
		public void Refresh()
		{
			// do nothing...
		}

		public void SetGravity(AdGravity gravity)
		{
			if (banner == IntPtr.Zero) return;
			//this.gravity = gravity;
		
			uint x = 0, y = 0;
			uint screenWidth = (uint)UnityEngine.Screen.width, screenHeight = (uint)UnityEngine.Screen.height;
			switch (gravity)
			{
				case AdGravity.BottomLeft:
					x = 0;
					y = screenHeight - height;
					break;

				case AdGravity.BottomRight:
					x = screenWidth - width;
					y = screenHeight - height;
					break;

				case AdGravity.BottomCenter:
					x = (screenWidth/2) - (width/2);
					y = screenHeight - height;
					break;

				case AdGravity.TopLeft:
					x = 0;
					y = 0;
					break;

				case AdGravity.TopRight:
					x = screenWidth - width;
					y = 0;
					break;

				case AdGravity.TopCenter:
					x = (screenWidth/2) - (width/2);
					y = 0;
					break;

				case AdGravity.CenterScreen:
					x = (screenWidth/2) - (width/2);
					y = (screenHeight/2) - (height/2);
					break;

				default:
					Debug.LogError("AdPlugin: Unsuported Ad gravity");
					break;
			}
			
			if (bbads_banner_set_position(banner, x, y) != 0) Debug.LogError("FAILED: bbads_banner_set_position");
		}
		
		public void Update()
		{
			// to bad we can't get events
		}

		public void OnGUI()
		{
			// do nothing...
		}
    }
}
#endif