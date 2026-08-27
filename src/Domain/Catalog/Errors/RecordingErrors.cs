namespace VibraHeka.Domain.Catalog.Errors;

public static class RecordingErrors
{
    public const string InvalidName = "RECORDING_INVALID_NAME";
    public const string InvalidDescription = "RECORDING_INVALID_DESCRIPTION";
    public const string InvalidType = "RECORDING_INVALID_TYPE";
    public const string InvalidFile = "RECORDING_INVALID_FILE";
    public const string UploadFailed = "RECORDING_UPLOAD_FAILED";
    public const string UrlGenerationFailed = "RECORDING_URL_GENERATION_FAILED";

    // REC-NNN domain errors
    public const string NotFound = "REC-001";
    public const string InvalidRecordingId = "REC-002";
    public const string OnlyForSubscribers = "REC-003";
}
