namespace YouTube_Downloader.Operations
{
    public interface IOperation
    {
        /// <summary>
        /// Gets the input file or download url.
        /// </summary>
        string Input { get; }
        /// <summary>
        /// Gets the output file.
        /// </summary>
        string Output { get; }
        /// <summary>
        /// Gets the operation status.
        /// </summary>
        OperationStatus Status { get; }

        /// <summary>
        /// Occurs when the operation is complete.
        /// </summary>
        event OperationEventHandler OperationComplete;

        /// <summary>
        /// Opens the output file.
        /// </summary>
        /// <returns></returns>
        bool Open();
        /// <summary>
        /// Opens the containing folder of the output file(s).
        /// </summary>
        bool OpenContainingFolder();
        /// <summary>
        /// Pauses the operation if supported &amp; available.
        /// </summary>
        void Pause();
        /// <summary>
        /// Resumes the operation if supported &amp; available.
        /// </summary>
        void Resume();
        /// <summary>
        /// Stops the operation if supported &amp; available.
        /// </summary>
        /// <param name="remove">Remove operation from it's ListView if set to true.</param>
        /// <param name="deleteUnfinishedFiles">Delete unfinished files if set to true.</param>
        bool Stop(bool remove, bool deleteUnfinishedFiles);

        /// <summary>
        /// Returns whether 'Open' method is supported and available at the moment.
        /// </summary>
        bool CanOpen();
        /// <summary>
        /// Returns whether 'Pause' method is supported and available at the moment.
        /// </summary>
        bool CanPause();
        /// <summary>
        /// Returns whether 'Resume' method is supported and available at the moment.
        /// </summary>
        bool CanResume();
        /// <summary>
        /// Returns whether 'Stop' method is supported and available at the moment.
        /// </summary>
        bool CanStop();
    }
}
