using System.Collections.Generic;

namespace YouTube_Downloader_DLL.FFmpegHelpers
{
    public class FFmpegResult<T>
    {
        /// <summary>
        /// Gets the result value.
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// Gets the FFmpeg exit code.
        /// </summary>
        public int ExitCode { get; set; }
        /// <summary>
        /// Gets a list of errors from running FFmpeg. Returns null if there wasn't any errors.
        /// </summary>
        public List<string> Errors { get; private set; }

        public FFmpegResult(T result)
        {
            this.Value = result;
            this.ExitCode = 0;
            this.Errors = null;
        }

        public FFmpegResult(T value, int exitCode, IEnumerable<string> errors)
        {
            this.Value = value;
            this.ExitCode = exitCode;
            this.Errors = new List<string>(errors);
        }

        public FFmpegResult(int exitCode, IEnumerable<string> errors)
        {
            this.Value = default(T);
            this.ExitCode = exitCode;
            this.Errors = new List<string>(errors);
        }
    }
}
