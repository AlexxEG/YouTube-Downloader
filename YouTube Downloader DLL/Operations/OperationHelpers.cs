using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.FFmpegHelpers;

namespace YouTube_Downloader_DLL.Operations
{
    public class OperationHelpers
    {
        public static bool Combine(string audio,
                                    string video,
                                    string title,
                                    OperationLogger logger,
                                    out Exception exception,
                                    Action<int, object> reportProgress)
        {
            // Remove '_video' from video file to get a final filename.
            string error = string.Empty;
            string output = video.Replace("_video", string.Empty);
            FFmpegResult<bool> result = null;

            try
            {
                // Raise events on main thread
                reportProgress(-1, new Dictionary<string, object>()
                {
                    { "ProgressText", "Combining..." }
                });

                result = FFmpeg.Combine(video, audio, output, delegate (int percentage)
                {
                    // Combine progress
                    reportProgress(percentage, null);
                }, logger);

                // Save errors if combining failed
                if (!result.Value)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(title);
                    sb.AppendLine(string.Join(
                            Environment.NewLine,
                            result.Errors.Select(err => $" - {err}")));

                    error = sb.ToString();
                }

                // Cleanup the separate audio and video files
                Helper.DeleteFiles(audio, video);
            }
            catch (Exception ex)
            {
                exception = ex;
                Common.SaveException(ex);
                return false;
            }
            finally
            {
                // Raise events on main thread
                reportProgress(-1, new Dictionary<string, object>()
                {
                    { "ProgressText", null }
                });
            }

            exception = new Exception(error);

            return result.Value;
        }
    }
}
