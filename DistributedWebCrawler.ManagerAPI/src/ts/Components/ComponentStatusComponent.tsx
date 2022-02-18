import React from "react";
import { ComponentModel } from "./AppComponent";

export interface ComponentStatusProps {
  componentStats: ComponentModel;
}

const ComponentStatus: React.FC<ComponentStatusProps> = ({
  componentStats,
}): JSX.Element => {
  const completedItemStats = componentStats.completedItemStats;
  const failedItemStats = componentStats.failedItemStats;
  const componentStatusStats = componentStats.componentStatusStats;

  return (
    <div>
      <header>
        <h3>{componentStats.friendlyName}</h3>
      </header>
      <div>Successfully completed items:</div>
      {completedItemStats &&
        completedItemStats.recentItems &&
        completedItemStats.recentItems.map((item, i) => (
          <div key={i.toString()}>{JSON.stringify(item)}</div>
        ))}
      <div>Total: {completedItemStats.total}</div>

      <div>Failed items:</div>
      <div>Error counts:</div>
      {failedItemStats &&
        failedItemStats.errorCounts &&
        Object.entries(failedItemStats.errorCounts).map(([k, v], i) => (
          <div key={i.toString()}>
            Error: {k}, Occurrences: {v}
          </div>
        ))}
      <div>Total: {failedItemStats.total}</div>

      <div>Component Status:</div>
      <div>
        Tasks in use: {componentStatusStats.averageTasksInUse} of{" "}
        {componentStatusStats.maxTasks}
      </div>
    </div>
  );
};

export default ComponentStatus;
