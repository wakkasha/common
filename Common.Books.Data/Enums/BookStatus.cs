namespace Common.Books.Data.Enums;

public enum BookStatus
{
    Requested,
    TextGenerated,
    UserApproved,
    PdfGenerated,
    ImagesGenerated,
    PendingReview,
    AdminApproved,
    Completed
}