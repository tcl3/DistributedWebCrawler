import React from "react";
import Table from "react-bootstrap/Table";
import { NodeStatusStats } from "../types/NodeStatusStats";
import { ComponentStats } from "./AppComponent";

export interface ComponentSummaryTableProps {
  componentStats: ComponentStats[];
}

export interface HasContentLength {
  contentLength: number | null;
}

export interface BytesDownloadedStats {
  totalIngested: number,
  totalIngestedSinceLastUpdate: number,
  totalDownloaded: number,
  totalUploaded: number,
  totalDownloadedSinceLastUpdate: number,
  totalUploadedSinceLastUpdate: number,
}

const getBytesDownloaded = (
  componentStats: ComponentStats[]
): BytesDownloadedStats => {
  const result = {
    totalIngested: 0,
    totalIngestedSinceLastUpdate: 0,
    totalDownloaded: 0,
    totalUploaded: 0,
    totalDownloadedSinceLastUpdate: 0,
    totalUploadedSinceLastUpdate: 0,
  };
  const nodeStatuses: {[key: string]: NodeStatusStats } = {};
  for (const stats of componentStats) {
    const totalBytesIngested = stats.completedItemStats.totalBytesIngested;
    const totalBytesIngestedSinceLastUpdate =
      stats.completedItemStats.totalBytesIngestedSinceLastUpdate;

    if (stats.componentStatusStats && stats.componentStatusStats.nodeStatus) {
      Object.entries(stats.componentStatusStats.nodeStatus).forEach(entry => {
        const [key, value] = entry;
        if (!nodeStatuses[key]) {
          nodeStatuses[key] = value;
        }
      });
  }

    if (totalBytesIngested) {
      result.totalIngested += totalBytesIngested;
    }

    if (totalBytesIngestedSinceLastUpdate) {
      result.totalIngestedSinceLastUpdate += totalBytesIngestedSinceLastUpdate;
    }
  }

  let totalBytesDownloaded = 0;
  let totalBytesUploaded = 0;
  let totalBytesDownloadedSinceLastUpdate = 0;
  let totalBytesUploadedSinceLastUpdate = 0;

  Object.values(nodeStatuses).forEach(nodeStatus => {
    totalBytesDownloaded += nodeStatus.totalBytesDownloaded;
    totalBytesUploaded += nodeStatus.totalBytesUploaded;
    totalBytesDownloadedSinceLastUpdate += nodeStatus.totalBytesDownloadedSinceLastUpdate;
    totalBytesUploadedSinceLastUpdate += nodeStatus.totalBytesUploadedSinceLastUpdate;
  });

  if (totalBytesDownloaded) {
    result.totalDownloaded += totalBytesDownloaded;
  }

  if (totalBytesUploaded) {
    result.totalUploaded += totalBytesUploaded;
  }

  if (totalBytesDownloadedSinceLastUpdate) {
    result.totalDownloadedSinceLastUpdate += totalBytesDownloadedSinceLastUpdate;
  }

  if (totalBytesUploadedSinceLastUpdate) {
    result.totalUploadedSinceLastUpdate += totalBytesUploadedSinceLastUpdate;
  }

  return result;
};

const getBytesString = (bytes: number): string => {
  const bytesPerGigabyte = 1073741824;
  const bytesPerMegabyte = 1048576;
  const bytesPerKilobyte = 1024;
  if (bytes > bytesPerGigabyte) {
    return `${(bytes / bytesPerGigabyte).toFixed(2)} GB`;
  }

  if (bytes > bytesPerMegabyte) {
    return `${(bytes / bytesPerMegabyte).toFixed(1)} MB`;
  }

  if (bytes > bytesPerKilobyte) {
    return `${(bytes / bytesPerKilobyte).toFixed(0)} KB`;
  }

  return `${bytes} bytes`;
};

const ComponentSummaryTable: React.FC<ComponentSummaryTableProps> = ({
  componentStats,
}): JSX.Element => {
  const renderTableRow = (
    componentStats: ComponentStats,
    key: number
  ): JSX.Element => {
    const statusStats = componentStats.componentStatusStats;
    let statsInfo: JSX.Element;
    if (statusStats.total) {
      statsInfo = (
        <>
          <td>
            {statusStats.averageTasksInUse} / {statusStats.maxTasks}
          </td>
          <td>{statusStats.averageQueueCount}</td>
          <td>{statusStats.sinceLastUpdate}</td>
          <td>{statusStats.total}</td>
        </>
      );
    } else {
      statsInfo = (
        <td colSpan={4} align="center">
          Component not active
        </td>
      );
    }
    return (
      <tr key={key}>
        <td>
          <strong>{componentStats.friendlyName}</strong>
        </td>
        {statsInfo}
      </tr>
    );
  };

  const bytesDownloaded = getBytesDownloaded(componentStats);

  return (
    <>
      <Table bordered striped hover size="sm">
        <thead>
          <tr>
            <th></th>
            <th>Average tasks in use</th>
            <th>Average queue size</th>
            <th>Items per second</th>
            <th>Total items processed</th>
          </tr>
        </thead>
        <tbody>
          {componentStats.map((component, i) => renderTableRow(component, i))}
        </tbody>
      </Table>
      <div>
        Ingest speed:{" "}
        {getBytesString(bytesDownloaded.totalIngestedSinceLastUpdate)}/s
      </div>
      <div></div>
      <div>
        Download speed:{" "}
        {getBytesString(bytesDownloaded.totalDownloadedSinceLastUpdate)}/s
      </div>
      <div>
        Upload speed:{" "}
        {getBytesString(bytesDownloaded.totalUploadedSinceLastUpdate)}/s
      </div>
      <div></div>
      <div>
        Total ingested: {getBytesString(bytesDownloaded.totalIngested)}
      </div>
      <div>
        Total downloaded: {getBytesString(bytesDownloaded.totalDownloaded)}
      </div>
    </>
  );
};

export default ComponentSummaryTable;
