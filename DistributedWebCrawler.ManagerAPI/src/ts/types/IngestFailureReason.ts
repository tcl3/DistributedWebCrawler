
export const enum IngestFailureReason {
    None = 0,
    UnknownError = 1,
    MaxDepthReached = 2,
    Http4xxError = 3,
    NetworkConnectivityError = 4,
    UriFormatError = 5,
    RequestTimeout = 6,
    ContentTooLarge = 7,
    MediaTypeNotPermitted = 8,
    MaxRedirectsReached = 9
}