﻿import { HttpStatusCode } from "./HttpStatusCode";
import { RedirectResult } from "./RedirectResult";
// This file was generated by TypeWriter. Do not modify.
export interface IngestSuccess {
    
    httpStatusCode: HttpStatusCode | null;
    uri: string;
    requestStartTime: Date;
    timeTaken: string;
    contentId: string | null;
    contentLength: number;
    mediaType: string;
    redirects: RedirectResult[];
}