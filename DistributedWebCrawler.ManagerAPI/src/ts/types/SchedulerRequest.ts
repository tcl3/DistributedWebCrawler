﻿import { RequestBase } from "./RequestBase";


// This file was generated by TypeWriter. Do not modify.
export interface SchedulerRequest extends RequestBase {
    
    uri: string;
    currentCrawlDepth: number;
    paths: string[];
}