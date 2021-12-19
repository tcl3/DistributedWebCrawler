import React, { Component } from "react";
import Table from "react-bootstrap/Table";
import { ComponentStats } from "./AppComponent";

export interface ComponentSummaryTableProps {
  componentStats: ComponentStats[];
}

export interface HasContentLength {
  contentLength: number | null;
}

export interface BytesDownloadedStats {
  totalDownloaded: number;
  totalDownloadedSinceLastUpdate: number;
}

const getBytesDownloaded = (
  componentStats: ComponentStats[]
): BytesDownloadedStats => {
  const result = {
    totalDownloaded: 0,
    totalDownloadedSinceLastUpdate: 0,
  };
  for (const stats of componentStats) {
    const totalBytes = stats.completedItemStats.totalBytes;
    const totalBytesSinceLastUpdate =
      stats.completedItemStats.totalBytesSinceLastUpdate;

    if (totalBytes) {
      result.totalDownloaded += totalBytes;
    }

    if (totalBytesSinceLastUpdate) {
      result.totalDownloadedSinceLastUpdate += totalBytesSinceLastUpdate;
    }
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
        Download speed:{" "}
        {getBytesString(bytesDownloaded.totalDownloadedSinceLastUpdate)}/s
      </div>
      <div>
        Total downloaded: {getBytesString(bytesDownloaded.totalDownloaded)}
      </div>
    </>
  );
};

export default ComponentSummaryTable;
