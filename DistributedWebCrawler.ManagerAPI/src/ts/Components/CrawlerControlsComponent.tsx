import { HubConnection } from "@microsoft/signalr";
import React from "react";
import Switch from "react-bootstrap/Switch";


export interface CrawlerControlsProps {
  isRunning: boolean;
  setIsRunning: React.Dispatch<boolean>;
  connection: HubConnection;
}

const CrawlerControls: React.FC<CrawlerControlsProps> = ({isRunning, setIsRunning, connection}) => {

  return (
    <>
      <div className="block">
        <Switch defaultChecked={isRunning}
          onChange={(e) => {
            setIsRunning(e.currentTarget.checked);
            const hubMethod = e.currentTarget.checked ? "Resume" : "Pause";
            connection.send(hubMethod);
          }}
        />
        Crawler is currently {isRunning ? "running" : "not running"}
      </div>
    </>
  );
};

export default CrawlerControls;
