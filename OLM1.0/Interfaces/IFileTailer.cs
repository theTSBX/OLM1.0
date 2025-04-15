using System;

namespace OutputLogManagerNEW.Interfaces
{
    public interface IFileTailer
    {
        void StartTailing(string filePath, Action<string> onNewLine);
        void StopTailing();
    }
}
