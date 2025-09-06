using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Scraping.Processing;

public sealed class StudentPhotoAttribute(int order = 0) : DataProcessorAttribute(StudentPhotoProcessor.Name, order);