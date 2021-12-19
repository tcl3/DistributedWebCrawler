import React, { useEffect, useState } from "react";
import { Routes, Route } from "react-router-dom";
import
{
  setupSignalRConnection,
  ComponentMessageHandler,
  addSignalRHandler,
  removeSignalRHandler
} from "../SignalR";
import { HubConnection } from "@microsoft/signalr";
import Navbar from "react-bootstrap/Navbar";

import ComponentStatus from "./ComponentStatusComponent";
import Sidebar from "./SidebarComponent";
import CrawlerControls from "./CrawlerControlsComponent";

import { CompletedItemStats } from '../types/CompletedItemStats';
import { FailedItemStats } from '../types/FailedItemStats';
import { ComponentStatusStats } from '../types/ComponentStatusStats';
import Overview from "./OverviewComponent";

// TODO: replace this with a SignalR call
export interface ComponentDescription {
    name: string,
    friendlyName: string
}

const componentDescriptions: ComponentDescription[] = [
  {
    name: "scheduler",
    friendlyName: "Scheduler"
  },
  {
    name: "ingester",
    friendlyName: "Ingester"
  },
  {
    name: "robotsdownloader",
    friendlyName: "Robots Downloader"
  },
  {
    name: "parser",
    friendlyName: "Parser"
  }
];

export interface ComponentStats {
  name: string,
  friendlyName: string,
  completedItemStats: CompletedItemStats,
  failedItemStats: FailedItemStats,
  componentStatusStats: ComponentStatusStats
}

const getComponentStats = (component: ComponentDescription): ComponentStats => {
  const [completedItemStats, setCompletedItemStats] = useState<CompletedItemStats>({} as CompletedItemStats);
  const [failedItemStats, setFailedItemStats] = useState<FailedItemStats>({} as FailedItemStats);
  const [componentStatusStats, setComponentStatusStats] = useState<ComponentStatusStats>({} as ComponentStatusStats);

  useEffect(() => {
      const handler: ComponentMessageHandler = {
          OnCompleted(data: CompletedItemStats) {
              setCompletedItemStats(data);
          },
          OnFailed(data: FailedItemStats) {
              setFailedItemStats(data);
          },
          OnComponentUpdate(data: ComponentStatusStats) {
              setComponentStatusStats(data);
          }
      };
      addSignalRHandler(component.name, handler);

      return () => removeSignalRHandler(component.name);
  }, []);

  return {
    name: component.name,
    friendlyName: component.friendlyName,
    completedItemStats: completedItemStats,
    failedItemStats: failedItemStats,
    componentStatusStats: componentStatusStats
  }
}

const App: React.FC = () => {
  const [connection, setConnection] = useState<HubConnection>(null);
  const [isRunning, setIsRunning] = useState<boolean>(false);

  useEffect(() => {
    if (connection == null) {
      const hubName = "/crawlerHub";
      const newConnection = setupSignalRConnection(hubName);
      setConnection(newConnection);
    }
  }, [connection]);

  const componentStats = componentDescriptions.map((component) => getComponentStats(component));

  const overviewElement = (
    <Overview
      isRunning={isRunning}
      setIsRunning={setIsRunning}
      connection={connection}
      componentStats={componentStats}
    />
  );

  return (
    <>
      <Navbar bg="dark" variant="dark">
          <Navbar.Brand href="/">Distributed Web Crawler</Navbar.Brand>
      </Navbar>
      <div className="app">
        <Sidebar componentDescriptions={componentDescriptions} />
        <main>
          <Routes>
            <Route path="/" element={overviewElement} />
            {componentStats.map((stats, i) => (
              <Route
                path={`/component/${stats.name.toLowerCase()}`}
                key={i.toString()}
                element={<ComponentStatus componentStats={stats} />}
              />
            ))}
          </Routes>
        </main>
      </div>
    </>
  );
};

export default App;
