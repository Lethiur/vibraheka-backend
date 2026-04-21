using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Web.Entities;

/// <summary>
/// Represents a request to upload a new recording.
/// The file content must be provided as a Base64-encoded string.
/// </summary>
public class UploadRecordingRequest
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
    /// The type of recording: Meditacion, Masterclass or Taller.
    /// </summary>
    public RecordingType Type { get; set; }

    /// <summary>
    /// The Base64-encoded content of the recording file.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// The original file name including its extension (e.g. "meditacion.mp4").
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

