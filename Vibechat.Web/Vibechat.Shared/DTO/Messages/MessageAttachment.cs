namespace Vibechat.Shared.DTO.Messages
{
    public class MessageAttachment
    {
        /// <summary>
        ///     Relative url
        /// </summary>
        public string ContentUrl { get; set; }

        public string AttachmentName { get; set; }

        public AttachmentKind AttachmentKind { get; set; }

        /// <summary>
        ///     This field is only used on client-side.
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        ///     This field is only used on client-side.
        /// </summary>
        public int ImageHeight { get; set; }

        public long FileSize { get; set; }
    }
}