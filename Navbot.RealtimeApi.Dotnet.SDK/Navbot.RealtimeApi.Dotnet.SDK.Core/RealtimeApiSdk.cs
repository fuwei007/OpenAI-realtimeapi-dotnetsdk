using log4net;
using log4net.Config;
using Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core;

public partial class RealtimeApiSdk
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private NetworkProtocolBase networkProtocol;
    private Dictionary<FunctionCallSetting, Func<FunctionCallArgument, JObject>> functionRegistries;
    
    public event EventHandler<EventArgs> SpeechStarted;
    public event EventHandler<AudioEventArgs> SpeechDataAvailable;
    public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
    public event EventHandler<EventArgs> SpeechEnded;

    public event EventHandler<EventArgs> PlaybackStarted;
    public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
    public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
    public event EventHandler<EventArgs> PlaybackEnded;

    public RealtimeApiSdk() : this("")
    {
        // TODO test if log4net.config deleted in sample project, if error.
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
    }

    public RealtimeApiSdk(string apiKey)
    {
        OpenAiConfig = new OpenAiConfig(apiKey);
        this.SessionConfiguration = new SessionConfiguration();
        this.NetworkProtocolType = NetworkProtocolType.WebSocket;

        this. functionRegistries = new Dictionary<FunctionCallSetting, Func<FunctionCallArgument, JObject>>();
    }

    #region property
    public NetworkProtocolType NetworkProtocolType { get; set; }

    public SessionConfiguration SessionConfiguration { get; }

    public bool IsRunning { get; private set; }

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
    protected virtual void OnSpeechEnded(EventArgs e)
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

    //protected virtual void OnSpeechActivity(bool isActive, AudioEventArgs? audioArgs = null)
    //{
    //    if (isActive)
    //    {
    //        SpeechStarted?.Invoke(this, EventArgs.Empty);
    //    }
    //    else
    //    {
    //        SpeechEnded?.Invoke(this, audioArgs ?? new AudioEventArgs(new byte[0]));
    //    }
    //}
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

            networkProtocol = CreateNetworkProtocol();
            await networkProtocol.ConnectAsync(SessionConfiguration, functionRegistries);
        }
    }

    public async void StopSpeechRecognitionAsync()
    {
        if (IsRunning)
        {
            CloseNetworkProtocol();
            await networkProtocol.DisconnectAsync();

            IsRunning = false;
        }
    }


    public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FunctionCallArgument, JObject> functionCallback)
    {
        functionRegistries.Add(functionCallSetting, functionCallback);
    }


    private NetworkProtocolBase CreateNetworkProtocol()
    {
        switch (NetworkProtocolType)
        {
            case NetworkProtocolType.WebSocket:
                networkProtocol = new NetworkProtocolWebSocket(OpenAiConfig, log);
                break;
            case NetworkProtocolType.WebRTC:
                networkProtocol = new NetworkProtocolWebRTC(OpenAiConfig, log);
                ((NetworkProtocolWebRTC)networkProtocol).RtcPlaybackDataAvailable += (s,e) => OnPlaybackDataAvailable(e);
                break;
            default:
                break;
        }
        
        networkProtocol.SpeechStarted += (s, e) => { OnSpeechStarted(e); };
        networkProtocol.SpeechEnded += (s, e) => { OnSpeechEnded(e); };
        networkProtocol.SpeechDataAvailable += (s, e) => { OnSpeechDataAvailable(e); };
        networkProtocol.SpeechTextAvailable += (s, e) => { OnSpeechTextAvailable(e); };

        networkProtocol.PlaybackStarted += (s, e) => { OnPlaybackStarted(e); };
        networkProtocol.PlaybackEnded += (s, e) => { OnPlaybackEnded(e); };
        networkProtocol.PlaybackDataAvailable += (s, e) => { OnPlaybackDataAvailable(e); };
        networkProtocol.PlaybackTextAvailable += (s, e) => { OnPlaybackTextAvailable(e); };

        return networkProtocol;
    }

    private void CloseNetworkProtocol() 
    {
        if (networkProtocol != null) 
        {
            //TODO this may not work??? need test.
            networkProtocol.SpeechStarted -= (s, e) => { OnSpeechStarted(e); };
            networkProtocol.SpeechEnded -= (s, e) => { OnSpeechEnded(e); };
            networkProtocol.SpeechDataAvailable -= (s, e) => { OnSpeechDataAvailable(e); };
            networkProtocol.SpeechTextAvailable -= (s, e) => { OnSpeechTextAvailable(e); };

            networkProtocol.PlaybackStarted -= (s, e) => { OnPlaybackStarted(e); };
            networkProtocol.PlaybackEnded -= (s, e) => { OnPlaybackEnded(e); };
            networkProtocol.PlaybackDataAvailable -= (s, e) => { OnPlaybackDataAvailable(e); };
            networkProtocol.PlaybackTextAvailable -= (s, e) => { OnPlaybackTextAvailable(e); };

        }
    }
     
}
