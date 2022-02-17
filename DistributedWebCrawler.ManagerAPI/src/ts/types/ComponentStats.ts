﻿import { ComponentInfo } from "./ComponentInfo";
import { CompletedItemStats } from "./CompletedItemStats";
import { FailedItemStats } from "./FailedItemStats";
import { ComponentStatusStats } from "./ComponentStatusStats";

// This file was generated by TypeWriter. Do not modify.
export interface ComponentStats {
    
    componentInfo: ComponentInfo;
    completed: CompletedItemStats | null;
    failed: FailedItemStats | null;
    componentStatus: ComponentStatusStats | null;
}