using MemeManager.Services.Environment.Enums;

namespace MemeManager.Services.Environment.Interfaces;

public interface IPlatformService
{
    Platform GetPlatform();
}