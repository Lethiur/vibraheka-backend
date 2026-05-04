using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Web.Entities;

/// <summary>
/// Represents a request to register a new recording and obtain a pre-signed upload URL.
/// The client must subsequently PUT the video file directly to the returned URL.
/// </summary>
public sealed class UploadRecordingRequest
{
    /// <summary>
    /// The display name of the recording.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A description of the recording content.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Specifies the tier of the recording, indicating its access level and pricing model.
    /// </summary>
    public RecordingTier Tier { get; set; }

    /// <summary>
    /// The type of recording: Meditacion, Masterclass or Taller.
    /// </summary>
    public RecordingType Type { get; set; }
}
