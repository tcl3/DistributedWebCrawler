﻿import { CrawlerComponentStatus } from "./CrawlerComponentStatus";
import { NodeStatusStats } from "./NodeStatusStats";
import { ComponentStatsBase } from "./ComponentStatsBase";


// This file was generated by TypeWriter. Do not modify.
export interface ComponentStatusStats extends ComponentStatsBase {
    
    averageTasksInUse: number;
    averageQueueCount: number;
    maxTasks: number;
    currentStatus: CrawlerComponentStatus;
    nodeStatus: { [key: string]: NodeStatusStats; };
}