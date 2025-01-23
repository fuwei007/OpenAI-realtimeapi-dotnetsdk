using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response
{
    public class BaseResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        public static BaseResponse Parse(JObject json)
        {
            BaseResponse baseResponse = null;
            var type = json["type"]?.ToString();

            var typeMapping = new Dictionary<string, Func<JObject, BaseResponse>>
            {
                { "error", j => j.ToObject<ResponseError>() },
                { "session.created", j => j.ToObject<SessionCreated>() },
                { "session.updated", j => j.ToObject<SessionUpdate>() },
                { "input_audio_buffer.speech_started", j => j.ToObject<SpeechStarted>() },
                { "input_audio_buffer.speech_stopped", j => j.ToObject<SpeechStopped>() },
                { "conversation.item.input_audio_transcription.completed", j => j.ToObject<TranscriptionCompleted>() },
                { "response.audio_transcript.done", j => j.ToObject<ResponseAudioTranscriptDone>() },
                { "conversation.item.created", j => j.ToObject<ConversationItemCreated>() },
                { "input_audio_buffer.committed", j => j.ToObject<BufferCommitted>() },
                { "conversation.created", j => j.ToObject<ConversationCreated>() },
                { "conversation.item.input_audio_transcription.failed", j => j.ToObject<TranscriptionFailed>() },
                { "conversation.item.truncated", j => j.ToObject<ConversationItemTruncate>() },
                { "conversation.item.deleted", j => j.ToObject<ConversationItemDeleted>() },
                { "input_audio_buffer.cleared", j => j.ToObject<BufferClear>() },
                { "response.created", j => j.ToObject<ResponseCreated>() },
                { "response.done", j => j.ToObject<ResponseDone>() },
                { "response.output_item.added", j => j.ToObject<ResponseOutputItemAdded>() },
                { "response.output_item.done", j=> j.ToObject<ResponseOutputItemDone>() },
                { "response.content_part.added", j=> j.ToObject<ResponseContentPartAdded>() },
                { "response.content_part.done", j=> j.ToObject<ResponseContentPartDone>() },
                { "response.text.done", j=> j.ToObject<ResponseTextDone>() },
                { "response.function_call_arguments.delta", j=> j.ToObject<ResponseFunctionCallArgumentsDelta>() },
                { "response.function_call_arguments.done", j=> j.ToObject<FunctionCallArgument>() },
                { "rate_limits.updated", j=> j.ToObject<RateLimitsUpdated>() }
            };

            if (typeMapping.ContainsKey(type))
            {
                baseResponse = typeMapping[type](json);
            }
            else if (type == "response.audio_transcript.delta" || type == "response.audio.delta" || type == "response.audio.done"||type== "response.text.delta")
            {
                var responseDelta = json.ToObject<ResponseDelta>();

                if (type == "response.audio_transcript.delta")
                    responseDelta.ResponseDeltaType = ResponseDeltaType.AudioTranscriptDelta;
                else if (type == "response.audio.delta")
                    responseDelta.ResponseDeltaType = ResponseDeltaType.AudioDelta;
                else if (type == "response.audio.done")
                    responseDelta.ResponseDeltaType = ResponseDeltaType.AudioDone;
                else if (type == "response.text.delta")
                    responseDelta.ResponseDeltaType = ResponseDeltaType.TextDelta;

                baseResponse = responseDelta;
            }

            return baseResponse;
        }
    }
}
