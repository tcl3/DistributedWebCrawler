import { HubConnection } from "@microsoft/signalr";
import React from "react";
import { ComponentStats } from "./AppComponent";
import ComponentSummaryTable from "./ComponentSummaryTableComponent";
import CrawlerControls from "./CrawlerControlsComponent";

export interface OverviewProps {
  isRunning: boolean;
  setIsRunning: React.Dispatch<boolean>;
  connection: HubConnection;
  componentStats: ComponentStats[]
}

const Overview: React.FC<OverviewProps> = ({
  isRunning,
  setIsRunning,
  connection,
  componentStats
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
    <ComponentSummaryTable componentStats={componentStats} />
    </>
  );
};

export default Overview;
