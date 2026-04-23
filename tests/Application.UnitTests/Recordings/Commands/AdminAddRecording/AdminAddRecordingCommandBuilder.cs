using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Domain.Recordings.Enums;

namespace VibraHeka.Application.UnitTests.Recordings.Commands.AdminAddRecording;

/// <summary>
/// Provides pre-built <see cref="AdminAddRecordingCommand"/> instances for use in tests.
/// </summary>
public static class AdminAddRecordingCommandBuilder
{
    public static AdminAddRecordingCommand BuildValid() =>
        new(
            Name: "Sesion de meditacion",
            Description: "Descripcion de la sesion de meditacion guiada",
            Type: RecordingType.Meditacion);
}
