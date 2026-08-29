namespace VibraHeka.Application.Catalog.Models;

/// <summary>
/// Result returned after creating a recording entry.
/// Contains the new recording identifier and a pre-signed PUT URL
/// the client uses to upload the video directly to S3.
/// </summary>
public sealed record AddRecordingResult(string RecordingId, string UploadUrl);

