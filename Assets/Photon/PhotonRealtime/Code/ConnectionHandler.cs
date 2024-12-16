// ----------------------------------------------------------------------------
// <copyright file="ConnectionHandler.cs" company="Exit Games GmbH">
// Photon Realtime API - Copyright (C) 2022 Exit Games GmbH
// </copyright>
// <summary>
// Utility class to keep a clients connection, even if not used otherwise.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------


#if UNITY_2017_4_OR_NEWER
#define SUPPORTED_UNITY
#endif


namespace Photon.Realtime
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Diagnostics;
    using Photon.Client;

    #if SUPPORTED_UNITY
    using UnityEngine;
    #endif


    /// <summary>Handler class to use threading to keep a RealtimeClient connection alive (calling SendAcksOnly if it was not called in time).</summary>
    #if SUPPORTED_UNITY
    public class ConnectionHandler : MonoBehaviour
    #else
    public class ConnectionHandler
    #endif
    {
        /// <summary>
        /// Photon client to log information and statistics from.
        /// </summary>
        public RealtimeClient Client { get; set; }
        
        /// <summary>Optional string identifier for the instance (for debugging).</summary>
        public string Id;

        /// <summary>Option to let the fallback thread call Disconnect after the KeepAliveInBackground time. Default: false.</summary>
        /// <remarks>
        /// If set to true, the thread will disconnect the client regularly, should the client not call SendOutgoingCommands / Service.
        /// This may happen due to an app being in background (and not getting a lot of CPU time) or when loading assets.
        ///
        /// If false, a regular timeout time will have to pass (on top) to time out the client.
        /// </remarks>
        public bool DisconnectAfterKeepAlive = false;

        /// <summary>Defines for how long the Fallback Thread should keep the connection, before it may time out as usual.</summary>
        /// <remarks>We want to the Client to keep it's connection when an app is in the background (and doesn't call Update / Service Clients should not keep their connection indefinitely in the background, so after some milliseconds, the Fallback Thread should stop keeping it up.</remarks>
        public int KeepAliveInBackground = 60000;

        /// <summary>Counts how often the Fallback Thread called SendAcksOnly, which is purely of interest to monitor if the game logic called SendOutgoingCommands as intended.</summary>
        public int CountSendAcksOnly { get; private set; }

        /// <summary>True if a fallback thread is running. Will call the client's SendAcksOnly() method to keep the connection up.</summary>
        public bool FallbackThreadRunning { get; private set; }


        /// <summary>Indicates that the app is closing. Set in OnApplicationQuit().</summary>
        [NonSerialized]
        public static bool AppQuits;

        /// <summary>Indicates that the (Unity) app is Paused. This means the main thread is not running.</summary>
        [NonSerialized]
        public static bool AppPause;
        
        /// <summary>Indicates that the app was paused within the last 5 seconds.</summary>
        [NonSerialized]
        public static bool AppPauseRecent;
        
        /// <summary>Indicates that the app is not in focus.</summary>
        [NonSerialized]
        public static bool AppOutOfFocus;
        
        /// <summary>Indicates that the app was out of focus within the last 5 seconds.</summary>
        [NonSerialized]
        public static bool AppOutOfFocusRecent;


        private bool didSendAcks;
        private readonly Stopwatch backgroundStopwatch = new Stopwatch();

        private Timer stateTimer;

        /// <summary>
        /// Creates an instance of the ConnectionHandler, assigns the given client. In Unity this uses a single GameObject to store all components on and applies DontDestroyOnLoad.
        /// </summary>
        /// <param name="client">The client to handle.</param>
        /// <param name="id">Optional ID for this handle (could be based / related to the client instance).</param>
        /// <returns></returns>
        public static ConnectionHandler BuildInstance(RealtimeClient client, string id = null)
        {
            ConnectionHandler result;

            #if SUPPORTED_UNITY
            if (go == null)
            {
                go = new GameObject(nameof(ConnectionHandler));
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(go);
                }
            }
            result = go.AddComponent<ConnectionHandler>();
            #else
            result = new ConnectionHandler();
            #endif
            result.Id = id;
            result.Client = client;
            return result;
        }


        public void RemoveInstance()
        {
            #if SUPPORTED_UNITY
            Destroy(this);
            #else
            this.StopFallbackSendAckThread();
            #endif
        }


        #if SUPPORTED_UNITY
        private static GameObject go;


        #if UNITY_2019_4_OR_NEWER

        /// <summary>
        /// Resets statics for Domain Reload
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticReset()
        {
            go = null;
            AppQuits = false;
            AppPause = false;
            AppPauseRecent = false;
            AppOutOfFocus = false;
            AppOutOfFocusRecent = false;
        }

        #endif


        protected virtual void Start()
        {
            if (Client == null)
            {
                UnityEngine.Debug.LogError("A ConnectionHandler should not be put into a scene. It is created by RealtimeClient.ConnectUsingSettings().", this);
            }
        }

        protected virtual void OnEnable()
        {
            this.StartFallbackSendAckThread();
        }


        protected virtual void OnDisable()
        {
            this.StopFallbackSendAckThread();
        }

        /// <summary>Called by Unity when the application gets closed. The UnityEngine will also call OnDisable.</summary>
        protected void OnApplicationQuit()
        {
            AppQuits = true;

            if (this.Client != null && this.Client.IsConnected)
            {
                this.Client.Disconnect(DisconnectCause.ApplicationQuit);
            }
        }

        /// <summary>Called by Unity when the application gets paused or resumed.</summary>
        public void OnApplicationPause(bool pause)
        {
            AppPause = pause;

            if (pause)
            {
                AppPauseRecent = true;
                this.CancelInvoke(nameof(this.ResetAppPauseRecent));
            }
            else
            {
                Invoke(nameof(this.ResetAppPauseRecent), 5f);
            }
        }

        private void ResetAppPauseRecent()
        {
            AppPauseRecent = false;
        }

        /// <summary>Called by Unity when the application changes focus.</summary>
        public void OnApplicationFocus(bool focus)
        {
            AppOutOfFocus = !focus;
            if (!focus)
            {
                AppOutOfFocusRecent = true;
                this.CancelInvoke(nameof(this.ResetAppOutOfFocusRecent));
            }
            else
            {
                this.Invoke(nameof(this.ResetAppOutOfFocusRecent), 5f);
            }
        }

        private void ResetAppOutOfFocusRecent()
        {
            AppOutOfFocusRecent = false;
        }


        #endif


        /// <summary>
        /// When run in Unity, this returns Application.internetReachability != NetworkReachability.NotReachable.
        /// </summary>
        /// <returns>Application.internetReachability != NetworkReachability.NotReachable</returns>
        public static bool IsNetworkReachableUnity()
        {
            #if SUPPORTED_UNITY
            return Application.internetReachability != NetworkReachability.NotReachable;
            #else
            return true;
            #endif
        }

        /// <summary>Starts a thread to run RealtimeFallbackThread.</summary>
        public void StartFallbackSendAckThread()
        {
            #if UNITY_WEBGL
            if (!this.FallbackThreadRunning) this.InvokeRepeating(nameof(this.RealtimeFallbackInvoke), 0.05f, 0.05f);
            #else
            if (this.stateTimer != null)
            {
                return;
            }

            stateTimer = new Timer(this.RealtimeFallback, null, 50, 50);
            #endif

            this.FallbackThreadRunning = true;
        }


        /// <summary>Stops the thread which runs RealtimeFallbackThread.</summary>
        public void StopFallbackSendAckThread()
        {
            #if UNITY_WEBGL
            if (this.FallbackThreadRunning) this.CancelInvoke(nameof(this.RealtimeFallbackInvoke));
            #else
            if (this.stateTimer != null)
            {
                this.stateTimer.Dispose();
                this.stateTimer = null;
            }
            #endif

            this.FallbackThreadRunning = false;
        }


        public void RealtimeFallbackInvoke()
        {
            this.RealtimeFallback();
        }



        /// <summary>A thread which runs independent from the Update() calls. Keeps connections online while loading or in background. See <see cref="KeepAliveInBackground"/>.</summary>
        public void RealtimeFallback(object state = null)
        {
            if (this.Client == null)
            {
                return;
            }


            //Log.Warn($"PeerId {this.Client.RealtimePeer.PeerID} RealtimeFallback {this.Client.IsConnected}. {this.Client.RealtimePeer.ConnectionTime} - {this.Client.RealtimePeer.Stats.LastSendOutgoingTimestamp} = {this.Client.RealtimePeer.ConnectionTime - this.Client.RealtimePeer.Stats.LastSendOutgoingTimestamp}  backgroundStopwatch.ElapsedMilliseconds: {this.backgroundStopwatch.ElapsedMilliseconds.ToString("N0")}");
            if (this.Client.IsConnected && this.Client.RealtimePeer.ConnectionTime - this.Client.RealtimePeer.Stats.LastSendOutgoingTimestamp > 100)
            {
                if (!this.didSendAcks)
                {
                    this.backgroundStopwatch.Restart();
                }

                // check if the client should disconnect after some seconds in background
                if (this.backgroundStopwatch.ElapsedMilliseconds > this.KeepAliveInBackground)
                {
                    if (this.DisconnectAfterKeepAlive)
                    {
                        this.Client.Disconnect();
                    }
                    return;
                }


                this.didSendAcks = true;
                this.CountSendAcksOnly++;

                this.Client.RealtimePeer.SendAcksOnly();
            }
            else
            {
                // not connected or the LastSendOutgoingTimestamp was below the threshold
                if (this.backgroundStopwatch.IsRunning)
                {
                    this.backgroundStopwatch.Reset();
                }
                this.didSendAcks = false;
            }
        }
    }


    /// <summary>
    /// The SystemConnectionSummary (SBS) is useful to analyze low level connection issues in Unity. This requires a ConnectionHandler in the scene.
    /// </summary>
    /// <remarks>
    /// A LoadBalancingClient automatically creates a SystemConnectionSummary on these disconnect causes:
    /// DisconnectCause.ExceptionOnConnect, DisconnectCause.Exception, DisconnectCause.ServerTimeout and DisconnectCause.ClientTimeout.
    ///
    /// The SBS can then be turned into an integer (ToInt()) or string to debug the situation or use in analytics.
    /// Both, ToString and ToInt summarize the network-relevant conditions of the client at and before the connection fail, including the PhotonPeer.SocketErrorCode.
    ///
    /// Important: To correctly create the SBS instance, a ConnectionHandler component must be present and enabled in the
    /// Unity scene hierarchy. In best case, keep the ConnectionHandler on a GameObject which is flagged as
    /// DontDestroyOnLoad.
    /// </remarks>
    public class SystemConnectionSummary
    {
        // SystemConditionSummary v0  has 32 bits:
        // Version bits (4 bits)
        // UDP, TCP, WS, WSS (WebRTC potentially) (3 bits)
        // 1 bit empty
        //
        // AppQuits
        // AppPause
        // AppPauseRecent
        // AppOutOfFocus
        //
        // AppOutOfFocusRecent
        // NetworkReachability (Unity value)
        // ErrorCodeFits (ErrorCode > short.Max would be a problem)
        // WinSock (true) or BSD (false) Socket Error Codes
        //
        // Time since receive?
        // Times of send?!
        //
        // System/Platform -> should be in other analytic values (not this)

        public readonly byte Version = 0;

        public byte UsedProtocol;

        public bool AppQuits;
        public bool AppPause;
        public bool AppPauseRecent;
        public bool AppOutOfFocus;

        public bool AppOutOfFocusRecent;
        public bool NetworkReachable;
        public bool ErrorCodeFits;
        public bool ErrorCodeWinSock;

        public int SocketErrorCode;

        private static readonly string[] ProtocolIdToName = { "UDP", "TCP", "2(N/A)", "3(N/A)", "WS", "WSS", "6(N/A)", "7WebRTC" };

        private class SCSBitPos
        {
            /// <summary>28 and up. 4 bits.</summary>
            public const int Version = 28;
            /// <summary>25 and up. 3 bits.</summary>
            public const int UsedProtocol = 25;
            public const int EmptyBit = 24;

            public const int AppQuits = 23;
            public const int AppPause = 22;
            public const int AppPauseRecent = 21;
            public const int AppOutOfFocus = 20;

            public const int AppOutOfFocusRecent = 19;
            public const int NetworkReachable = 18;
            public const int ErrorCodeFits = 17;
            public const int ErrorCodeWinSock = 16;
        }


        /// <summary>
        /// Creates a SystemConnectionSummary for an incident of a local LoadBalancingClient. This gets used automatically by the LoadBalancingClient!
        /// </summary>
        /// <remarks>
        /// If the LoadBalancingClient.SystemConnectionSummary is non-null after a connection-loss, you can call .ToInt() and send this to analytics or log it.
        ///
        /// </remarks>
        /// <param name="client"></param>
        public SystemConnectionSummary(RealtimeClient client)
        {
            if (client != null)
            {
                // protocol = 3 bits! potentially adding WebRTC.
                this.UsedProtocol = (byte)((int)client.RealtimePeer.UsedProtocol & 7);
                this.SocketErrorCode = (int)client.RealtimePeer.SocketErrorCode;
            }

            this.AppQuits = ConnectionHandler.AppQuits;
            this.AppPause = ConnectionHandler.AppPause;
            this.AppPauseRecent = ConnectionHandler.AppPauseRecent;
            this.AppOutOfFocus = ConnectionHandler.AppOutOfFocus;

            this.AppOutOfFocusRecent = ConnectionHandler.AppOutOfFocusRecent;
            this.NetworkReachable = ConnectionHandler.IsNetworkReachableUnity();

            this.ErrorCodeFits = this.SocketErrorCode <= short.MaxValue; // socket error code <= short.Max (everything else is a problem)
            this.ErrorCodeWinSock = true;
        }

        /// <summary>
        /// Creates a SystemConnectionSummary instance from an int (reversing ToInt()). This can then be turned into a string again.
        /// </summary>
        /// <param name="summary">An int, as provided by ToInt(). No error checks yet.</param>
        public SystemConnectionSummary(int summary)
        {
            this.Version = GetBits(ref summary, SCSBitPos.Version, 0xF);
            this.UsedProtocol = GetBits(ref summary, SCSBitPos.UsedProtocol, 0x7);
            // 1 empty bit

            this.AppQuits = GetBit(ref summary, SCSBitPos.AppQuits);
            this.AppPause = GetBit(ref summary, SCSBitPos.AppPause);
            this.AppPauseRecent = GetBit(ref summary, SCSBitPos.AppPauseRecent);
            this.AppOutOfFocus = GetBit(ref summary, SCSBitPos.AppOutOfFocus);

            this.AppOutOfFocusRecent = GetBit(ref summary, SCSBitPos.AppOutOfFocusRecent);
            this.NetworkReachable = GetBit(ref summary, SCSBitPos.NetworkReachable);
            this.ErrorCodeFits = GetBit(ref summary, SCSBitPos.ErrorCodeFits);
            this.ErrorCodeWinSock = GetBit(ref summary, SCSBitPos.ErrorCodeWinSock);

            this.SocketErrorCode = summary & 0xFFFF;
        }

        /// <summary>
        /// Turns the SystemConnectionSummary into an integer, which can be be used for analytics purposes. It contains a lot of info and can be used to instantiate a new SystemConnectionSummary.
        /// </summary>
        /// <returns>Compact representation of the context for a disconnect issue.</returns>
        public int ToInt()
        {
            int result = 0;
            SetBits(ref result, this.Version, SCSBitPos.Version);
            SetBits(ref result, this.UsedProtocol, SCSBitPos.UsedProtocol);
            // 1 empty bit

            SetBit(ref result, this.AppQuits, SCSBitPos.AppQuits);
            SetBit(ref result, this.AppPause, SCSBitPos.AppPause);
            SetBit(ref result, this.AppPauseRecent, SCSBitPos.AppPauseRecent);
            SetBit(ref result, this.AppOutOfFocus, SCSBitPos.AppOutOfFocus);

            SetBit(ref result, this.AppOutOfFocusRecent, SCSBitPos.AppOutOfFocusRecent);
            SetBit(ref result, this.NetworkReachable, SCSBitPos.NetworkReachable);
            SetBit(ref result, this.ErrorCodeFits, SCSBitPos.ErrorCodeFits);
            SetBit(ref result, this.ErrorCodeWinSock, SCSBitPos.ErrorCodeWinSock);


            // insert socket error code as lower 2 bytes
            int socketErrorCode = this.SocketErrorCode & 0xFFFF;
            result |= socketErrorCode;

            return result;
        }

        /// <summary>
        /// A readable debug log string of the context for network problems.
        /// </summary>
        /// <returns>SystemConnectionSummary as readable string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string transportProtocol = ProtocolIdToName[this.UsedProtocol];

            sb.Append($"SCS v{this.Version} {transportProtocol} SocketErrorCode: {this.SocketErrorCode} ");

            if (this.AppQuits) sb.Append("AppQuits ");
            if (this.AppPause) sb.Append("AppPause ");
            if (!this.AppPause && this.AppPauseRecent) sb.Append("AppPauseRecent ");
            if (this.AppOutOfFocus) sb.Append("AppOutOfFocus ");
            if (!this.AppOutOfFocus && this.AppOutOfFocusRecent) sb.Append("AppOutOfFocusRecent ");
            if (!this.NetworkReachable) sb.Append("NetworkUnreachable ");
            if (!this.ErrorCodeFits) sb.Append("ErrorCodeRangeExceeded ");

            if (this.ErrorCodeWinSock) sb.Append("WinSock");
            else sb.Append("BSDSock");

            string result = sb.ToString();
            return result;
        }


        public static bool GetBit(ref int value, int bitpos)
        {
            int result = (value >> bitpos) & 1;
            return result != 0;
        }

        public static byte GetBits(ref int value, int bitpos, byte mask)
        {
            int result = (value >> bitpos) & mask;
            return (byte)result;
        }

        /// <summary>Applies bitval to bitpos (no matter value's initial bit value).</summary>
        public static void SetBit(ref int value, bool bitval, int bitpos)
        {
            if (bitval)
            {
                value |= 1 << bitpos;
            }
            else
            {
                value &= ~(1 << bitpos);
            }
        }

        /// <summary>Applies bitvals via OR operation (expects bits in value to be 0 initially).</summary>
        public static void SetBits(ref int value, byte bitvals, int bitpos)
        {
            value |= bitvals << bitpos;
        }
    }
}