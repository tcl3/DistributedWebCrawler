﻿import { ComponentStats } from "./ComponentStats";
import { NodeStatusStats } from "./NodeStatusStats";

// This file was generated by TypeWriter. Do not modify.
export interface ComponentStatsCollection {
    
    componentStats: ComponentStats[];
    nodeStatus: { [key: string]: NodeStatusStats; };
}