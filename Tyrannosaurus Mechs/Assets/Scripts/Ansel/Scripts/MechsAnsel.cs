using System;
using System.Runtime.InteropServices;
using TMechs;
using TMechs.Player;
using TMechs.UI;
using UnityEngine;
using UnityEngine.Rendering;
using static NVIDIA.AnselBindings;

// ReSharper disable once CheckNamespace consistency with original Ansel script
namespace NVIDIA
{
    public class MechsAnsel : MonoBehaviour
    {
        public static bool CanUseAnsel
        {
            get => canUseAnsel && Player.Instance && Player.Instance.Camera && Time.timeScale > Mathf.Epsilon;
            set => canUseAnsel = value;
        }
        private static bool canUseAnsel = true;

        private static MechsAnsel instance;
        
        // The speed at which camera moves in the world
        public float translationalSpeedInWorldUnitsPerSecond = 5.0f;
        // The speed at which camera rotates 
        public float rotationalSpeedInDegreesPerSecond = 45.0f;
        // How many frames it takes for camera update to be reflected in a rendered frame
        public uint captureLatency = 0;
        // How many frames we must wait for a new frame to settle - i.e. temporal AA and similar
        // effects to stabilize after the camera has been adjusted
        public uint captureSettleLatency = 10;
        // Game scale, the size of a world unit measured in meters
        public float metersInWorldUnit = 1.0f;
        
        public static bool IsSessionActive => sessionActive;
        public static bool IsCaptureActive => captureActive;
        public static bool IsAvailable => anselIsAvailable();

        private Camera toyCamera;
        
        private CommandBuffer[] hintBufferPreBindCBs;
        private CommandBuffer[] hintBufferPostBindCBs;
        private static bool sessionActive = false;
        private static bool captureActive = false;

        private bool initialized;
        private ConfigData anselConfig;
        private CameraData anselCamera;
        private SessionData anselSession;

        private bool cursorVisible;
        private CursorLockMode cursorLocked;
        
        private void Awake()
        {
            Debug.Log("[ANSEL] Ansel has awoken");
            
            if (instance)
            {
                Debug.Log("[ANSEL] Duplicate instance is detected, destroying...");
                Destroy(this);
                return;
            }
            
            instance = this;
            initialized = false;
            DontDestroyOnLoad(gameObject);
            
            hintBufferPreBindCBs = new CommandBuffer[(int) HintBufferType.kBufferTypeCount];
            hintBufferPostBindCBs = new CommandBuffer[(int) HintBufferType.kBufferTypeCount];
            for (int i = 0; i < (int) HintBufferType.kBufferTypeCount; i++)
            {
                hintBufferPreBindCBs[i] = new CommandBuffer();
                hintBufferPreBindCBs[i].IssuePluginEvent(GetMarkBufferPreBindRenderEventFunc(), i);
                hintBufferPostBindCBs[i] = new CommandBuffer();
                hintBufferPostBindCBs[i].IssuePluginEvent(GetMarkBufferPostBindRenderEventFunc(), i);
            }
        }

        private void Start()
        {
            Initialize();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus && !initialized)
                Initialize();
        }    

        private void Initialize()
        {
            if (initialized)
                return;
            
            if (!Application.isFocused || !IsAvailable)
            {
                string reason = !Application.isFocused ? "Application window not in focus" : !IsAvailable ? "Ansel is unavailable" : "Unknown reason";
                Debug.Log($"[ANSEL] Not initializing - {reason}");
                return;
            }
            Debug.Log("[ANSEL] Initializing...");

            toyCamera = GetComponentInChildren<Camera>(true);

            anselConfig = new ConfigData
            {
                right = Vector3.right.Split(),
                up = Vector3.up.Split(),
                forward = Vector3.forward.Split(),
                
                translationalSpeedInWorldUnitsPerSecond = translationalSpeedInWorldUnitsPerSecond,
                rotationalSpeedInDegreesPerSecond = rotationalSpeedInDegreesPerSecond,
                captureLatency = captureLatency,
                captureSettleLatency = captureSettleLatency,
                metersInWorldUnit = metersInWorldUnit,
                
                // These should always be true unless there is some special scenario
                isCameraOffcenteredProjectionSupported = true,
                isCameraRotationSupported = true,
                isCameraTranslationSupported = true,
                isCameraFovSupported = true,
            };
            
            Debug.Log("[ANSEL] Calling anselInit");
            anselInit(ref anselConfig);
            
            anselCamera = new CameraData();
            
            anselSession = new SessionData
            {
                    isAnselAllowed = CanUseAnsel,
                    isFovChangeAllowed = true,
                    isHighresAllowed = true,
                    isPauseAllowed = true,
                    isRotationAllowed = true,
                    isTranslationAllowed = true,
                    is360StereoAllowed = true,
                    is360MonoAllowed = true
            };
            
            Debug.Log("[ANSEL] Configuring ansel session");
            anselConfigureSession(ref anselSession);

            initialized = true;
        }

        public void UpdateSession()
        {
            if (anselIsSessionOn())
                return;

            anselSession.isAnselAllowed = CanUseAnsel;
            anselConfigureSession(ref anselSession);
        }

        private void Update()
        {
            if(anselSession.isAnselAllowed != CanUseAnsel)
                UpdateSession();

            if (anselIsSessionOn())
            {
                if (!sessionActive)
                    SetSessionActive(true);

                captureActive = anselIsCaptureOn();
                Transform cam = toyCamera.transform;

                anselCamera.fov = toyCamera.fieldOfView;
                anselCamera.projectionOffset = new float[] { 0, 0 };
                anselCamera.position = cam.position.Split();
                anselCamera.rotation = cam.rotation.Split();
                anselUpdateCamera(ref anselCamera);
                
                toyCamera.ResetProjectionMatrix();

                // Collision detection for camera
                Vector3 targetPosition = new Vector3(anselCamera.position[0], anselCamera.position[1], anselCamera.position[2]);

                if (!IsCaptureActive)
                {
                    Vector3 heading = targetPosition - cam.position;
                    float distance = heading.magnitude;
                    Vector3 direction = heading / distance;

                    if (Physics.BoxCast(cam.position, Vector3.one, direction, cam.rotation, distance, ~LayerMask.GetMask("Ignore Raycast", "ARC")))
                        targetPosition = cam.position;
                }
                
                cam.position = targetPosition;
                cam.rotation = new Quaternion(anselCamera.rotation[0], anselCamera.rotation[1], anselCamera.rotation[2], anselCamera.rotation[3]);
                toyCamera.fieldOfView = anselCamera.fov;
                if (anselCamera.projectionOffset[0] > Mathf.Epsilon || anselCamera.projectionOffset[1] > Mathf.Epsilon)
                {
                    // Hi-res screen shots require projection matrix adjustment
                    Matrix4x4 projectionMatrix = toyCamera.projectionMatrix;
                    float l = -1.0f + anselCamera.projectionOffset[0];
                    float r = l + 2.0f;
                    float b = -1.0f + anselCamera.projectionOffset[1];
                    float t = b + 2.0f;
                    projectionMatrix[0, 2] = (l + r) / (r - l);
                    projectionMatrix[1, 2] = (t + b) / (t - b);
                    toyCamera.projectionMatrix = projectionMatrix;
                }
            }
            else if(sessionActive)
                SetSessionActive(false);
        }

        private void SetSessionActive(bool active)
        {
            sessionActive = active;
            if (!active)
                captureActive = false;
            
            MenuActions.SetPause(active, false);

            Transform playerCam = Player.Instance.Camera.transform;
            toyCamera.transform.position = playerCam.position;
            toyCamera.transform.rotation = playerCam.rotation;
            
            toyCamera.gameObject.SetActive(active);
            
            if (active)
            {
                cursorVisible = Cursor.visible;
                cursorLocked = Cursor.lockState;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = cursorVisible;
                Cursor.lockState = cursorLocked;
            }
        }
    }

    public static class AnselBindings
    {
#if (UNITY_64 || UNITY_EDITOR_64 || PLATFORM_ARCH_64)
        private const string PLUGIN_DLL = "AnselPlugin64";
#else
        private const string PLUGIN_DLL = "AnselPlugin32";
#endif

        [StructLayout(LayoutKind.Sequential)]
        public struct ConfigData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] forward;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] up;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] right;

            // The speed at which camera moves in the world
            public float translationalSpeedInWorldUnitsPerSecond;
            // The speed at which camera rotates 
            public float rotationalSpeedInDegreesPerSecond;
            // How many frames it takes for camera update to be reflected in a rendered frame
            public uint captureLatency;
            // How many frames we must wait for a new frame to settle - i.e. temporal AA and similar
            // effects to stabilize after the camera has been adjusted
            public uint captureSettleLatency;
            // Game scale, the size of a world unit measured in meters
            public float metersInWorldUnit;
            // Integration will support Camera::screenOriginXOffset/screenOriginYOffset
            [MarshalAs(UnmanagedType.I1)]
            public bool isCameraOffcenteredProjectionSupported;
            // Integration will support Camera::position
            [MarshalAs(UnmanagedType.I1)]
            public bool isCameraTranslationSupported;
            // Integration will support Camera::rotation
            [MarshalAs(UnmanagedType.I1)]
            public bool isCameraRotationSupported;
            // Integration will support Camera::horizontalFov
            [MarshalAs(UnmanagedType.I1)]
            public bool isCameraFovSupported;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CameraData
        {
            public float fov; // degrees
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public float[] projectionOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] position;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] rotation;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct SessionData
        {
            [MarshalAs(UnmanagedType.I1)]
            public bool isAnselAllowed; // if set to false none of the below parameters is relevant
            [MarshalAs(UnmanagedType.I1)]
            public bool is360MonoAllowed;
            [MarshalAs(UnmanagedType.I1)]
            public bool is360StereoAllowed;
            [MarshalAs(UnmanagedType.I1)]
            public bool isFovChangeAllowed;
            [MarshalAs(UnmanagedType.I1)]
            public bool isHighresAllowed;
            [MarshalAs(UnmanagedType.I1)]
            public bool isPauseAllowed;
            [MarshalAs(UnmanagedType.I1)]
            public bool isRotationAllowed;
            [MarshalAs(UnmanagedType.I1)]
            public bool isTranslationAllowed;
        };

        // Buffer hints for Ansel
        public enum HintBufferType
        {
            kBufferTypeHDR = 0,
            kBufferTypeDepth,
            kBufferTypeHUDless,
            kBufferTypeCount
        };

        // User control status
        public enum UserControlStatus
        {
            kUserControlOk = 0,
            kUserControlIdAlreadyExists,
            kUserControlInvalidId,
            kUserControlInvalidType,
            kUserControlInvalidLabel,
            kUserControlNameTooLong,
            kUserControlInvalidValue,
            kUserControlInvalidLocale,
            kUserControlInvalidCallback
        };

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void anselInit(ref ConfigData conf);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void anselUpdateConfiguration(ref ConfigData conf);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void anselUpdateCamera(ref CameraData cam);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void anselConfigureSession(ref SessionData ses);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool anselIsSessionOn();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool anselIsCaptureOn();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool anselIsAvailable();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void anselStartSession();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void anselStopSession();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern System.IntPtr GetMarkBufferPreBindRenderEventFunc();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern System.IntPtr GetMarkBufferPostBindRenderEventFunc();

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern UserControlStatus anselAddUserControlSlider(uint userControlId, string labelUtf8, float value);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern UserControlStatus anselSetUserControlSliderValue(uint userControlId, float value);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern float anselGetUserControlSliderValue(uint userControlId);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern UserControlStatus anselAddUserControlBoolean(uint userControlId, string labelUtf8, bool value);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern UserControlStatus anselSetUserControlBooleanValue(uint userControlId, bool value);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool anselGetUserControlBooleanValue(uint userControlId);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern UserControlStatus anselSetUserControlLabelLocalization(uint userControlId, string lang, string labelUtf8);

        [DllImport(PLUGIN_DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern UserControlStatus anselRemoveUserControl(uint userControlId);
    }
}