import { HubConnection } from "@microsoft/signalr";
import React from "react";
import { NodeStatusStats } from "../types/NodeStatusStats";
import { ComponentStats } from "./AppComponent";
import ComponentSummaryTable from "./ComponentSummaryTableComponent";
import CrawlerControls from "./CrawlerControlsComponent";

export interface OverviewProps {
  isRunning: boolean;
  setIsRunning: React.Dispatch<boolean>;
  connection: HubConnection;
  componentStats: ComponentStats[];
  nodeStats: NodeStatusStats[];
}

const Overview: React.FC<OverviewProps> = ({
  isRunning,
  setIsRunning,
  connection,
  componentStats,
  nodeStats
}) => {
  return (<>
    <header>
      <h3>Overview</h3>
    </header>
    <CrawlerControls
      isRunning={isRunning}
      setIsRunning={setIsRunning}
      connection={connection}
    />
    <ComponentSummaryTable
      componentStats={componentStats}
      nodeStats={nodeStats}
    />
    </>
  );
};

export default Overview;
