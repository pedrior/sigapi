using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Scraping.Processing;

public sealed class EnrollmentDataAttribute(int order = 0) 
    : DataProcessorAttribute(EnrollmentDataProcessor.Name, order);