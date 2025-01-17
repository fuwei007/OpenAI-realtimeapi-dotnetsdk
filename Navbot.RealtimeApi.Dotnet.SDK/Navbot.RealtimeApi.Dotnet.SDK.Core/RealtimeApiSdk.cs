using NAudio.Wave;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using log4net;
using System.Reflection;
using log4net.Config;
using System.Security.Cryptography;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Common;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core;

public partial class RealtimeApiSdk
{
    private NetworkProtocolBase networkProtocol;
    private Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> functionRegistries = new Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>>();
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public SessionConfiguration SessionConfiguration { get; }

    public event EventHandler<EventArgs> SpeechStarted;
    public event EventHandler<AudioEventArgs> SpeechDataAvailable;
    public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
    public event EventHandler<AudioEventArgs> SpeechEnded;

    //TODO
    public event EventHandler<EventArgs> PlaybackStarted;
    public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
    public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
    public event EventHandler<EventArgs> PlaybackEnded;

    public RealtimeApiSdk() : this("")
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
    }

    public RealtimeApiSdk(string apiKey)
    {
        OpenAiConfig = new OpenAiConfig(apiKey);
        this.SessionConfiguration = new SessionConfiguration();
        this.NetworkProtocolType = NetworkProtocolType.WebSocket;
    }

    #region property
    public bool IsRunning { get; private set; }

    public NetworkProtocolType NetworkProtocolType { get; set; }

    public bool IsMuted
    {
        get { return networkProtocol.IsMuted; }
        set { networkProtocol.IsMuted = value; }
    }

    private OpenAiConfig OpenAiConfig;
    public string OpenApiUrl
    {
        get { return OpenAiConfig.OpenApiUrl; }
        set { OpenAiConfig.OpenApiUrl = value; }
    }
    public string OpenApiRtcUrl
    {
        get { return OpenAiConfig.OpenApiRtcUrl; }
        set { OpenAiConfig.OpenApiRtcUrl = value; }
    }
    public string ApiKey
    {
        get { return OpenAiConfig.ApiKey; }
        set { OpenAiConfig.ApiKey = value; }
    }
    public string Model
    {
        get { return OpenAiConfig.Model; }
        set { OpenAiConfig.Model = value; }
    }
    public string Voice
    {
        get { return OpenAiConfig.Voice; }
        set { OpenAiConfig.Voice = value; }
    }
    public Dictionary<string, string> RequestHeaderOptions
    {
        get { return OpenAiConfig.RequestHeaderOptions; }
        set { OpenAiConfig.RequestHeaderOptions = value; }
    }
    #endregion

    #region Event
    protected virtual void OnSpeechStarted(EventArgs e)
    {
        SpeechStarted?.Invoke(this, e);
    }
    protected virtual void OnSpeechEnded(AudioEventArgs e)
    {
        SpeechEnded?.Invoke(this, e);
    }
    protected virtual void OnPlaybackDataAvailable(AudioEventArgs e)
    {
        PlaybackDataAvailable?.Invoke(this, e);
    }
    protected virtual void OnSpeechDataAvailable(AudioEventArgs e)
    {
        SpeechDataAvailable?.Invoke(this, e);
    }
    protected virtual void OnSpeechActivity(bool isActive, AudioEventArgs? audioArgs = null)
    {
        if (isActive)
        {
            SpeechStarted?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            SpeechEnded?.Invoke(this, audioArgs ?? new AudioEventArgs(new byte[0]));
        }
    }
    protected virtual void OnPlaybackStarted(EventArgs e)
    {
        PlaybackStarted?.Invoke(this, e);
    }
    protected virtual void OnPlaybackEnded(EventArgs e)
    {
        PlaybackEnded?.Invoke(this, e);
    }
    #endregion


    public async void StartSpeechRecognitionAsync()
    {
        if (!IsRunning)
        {
            IsRunning = true;

            networkProtocol = GetNetworkProtocol();
            await networkProtocol.ConnectAsync(SessionConfiguration);
        }
    }

    public async void StopSpeechRecognitionAsync()
    {
        if (IsRunning)
        {
            await networkProtocol.DisconnectAsync();
            IsRunning = false;
        }
    }


    public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FuncationCallArgument, JObject> functionCallback)
    {
        functionRegistries.Add(functionCallSetting, functionCallback);
    }


    private NetworkProtocolBase GetNetworkProtocol()
    {
        NetworkProtocolBase rtn = null;

        switch (NetworkProtocolType)
        {
            case NetworkProtocolType.WebSocket:
                rtn = new NetworkProtocolWebSocket(OpenAiConfig, log);
                break;
            case NetworkProtocolType.WebRTC:
                rtn = new NetworkProtocolWebRTC(OpenAiConfig, log);
                ((NetworkProtocolWebRTC)rtn).RtcPlaybackDataAvailable += RealtimeApiSdk_RtcPlaybackDataAvailable;
                break;
            default:
                break;
        }

        rtn.SpeechStarted += (s, e) => { OnSpeechStarted(e); };
        rtn.SpeechEnded += (s, e) => { OnSpeechEnded(e); };
        rtn.SpeechDataAvailable += (s, e) => { OnSpeechDataAvailable(e); };
        rtn.SpeechTextAvailable += (s, e) => { OnSpeechTextAvailable(e); };

        rtn.PlaybackStarted += (s, e) => { OnPlaybackStarted(e); };
        rtn.PlaybackEnded += (s, e) => { OnPlaybackEnded(e); };
        rtn.PlaybackDataAvailable += (s, e) => { OnPlaybackDataAvailable(e); };
        rtn.PlaybackTextAvailable += (s, e) => { OnPlaybackTextAvailable(e); };
        return rtn;
    }

    private void RealtimeApiSdk_RtcPlaybackDataAvailable(object? sender, AudioEventArgs e)
    {
        OnPlaybackDataAvailable(e);
    }


}
