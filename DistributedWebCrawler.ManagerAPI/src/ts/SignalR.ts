// Adapted from https://stackoverflow.com/a/62162742
import {
    JsonHubProtocol,
    HubConnectionState,
    HubConnectionBuilder,
    LogLevel,
    IHttpConnectionOptions,
    HubConnection
} from '@microsoft/signalr/';

import { CompletedItemStats } from './types/CompletedItemStats';
import { FailedItemStats } from './types/FailedItemStats';
import { ComponentStatusStats } from './types/ComponentStatusStats';
import { ComponentStatsCollection } from './types/ComponentStatsCollection';
import { NodeStatusStats } from './types/NodeStatusStats';

export interface ComponentUpdateMessageHandler {
    OnCompleted: (data: CompletedItemStats) => void;
    OnFailed: (data: FailedItemStats) => void;
    OnComponentUpdate: (data: ComponentStatusStats) => void;
}
export interface NodeStatsUpdateMessageHandler {
    OnNodeStatsUpdate: (data: NodeStatusStats[]) => void;
}

const isDev = process.env.NODE_ENV === 'development';

const startSignalRConnection = async (
    connection: HubConnection,
    onConnectedCallback?: (connection: HubConnection) => void) => {
    try {
        await connection.start().then(() => onConnectedCallback(connection));
        console.assert(connection.state === HubConnectionState.Connected);
        console.log('SignalR connection established');
    } catch (err) {
        console.assert(connection.state === HubConnectionState.Disconnected);
        console.error('SignalR Connection Error: ', err);
        setTimeout(() => startSignalRConnection(connection, onConnectedCallback), 5000);
    }
};

const componentUpdateEventMap: { [key: string]: ComponentUpdateMessageHandler } = {};

export const addComponentUpdateHandler = (componentName: string, handler: ComponentUpdateMessageHandler) => {
    componentUpdateEventMap[componentName] = handler;
}

export const removeComponentUpdateHandler = (componentName: string) => {
    delete componentUpdateEventMap[componentName];
}

const nodeStatsUpdateEvents: Set<NodeStatsUpdateMessageHandler> = new Set<NodeStatsUpdateMessageHandler>();

export const addNodeStatsUpdateHandler = (handler: NodeStatsUpdateMessageHandler) => {
    nodeStatsUpdateEvents.add(handler);
}

export const removeNodeStatsUpdateHandler = (handler: NodeStatsUpdateMessageHandler) => {
    nodeStatsUpdateEvents.delete(handler);
}

const handleComponentUpdate = (componentStatsCollection: ComponentStatsCollection) => {
    for (const entry of componentStatsCollection.componentStats) {
        if (!entry.componentInfo || !entry.componentInfo.componentName) {
            continue;
        }
        const componentName = entry.componentInfo.componentName.toLowerCase();
        const componentMessageHandler = componentUpdateEventMap[componentName];
        if (componentMessageHandler) {
            executeEventHandler(componentMessageHandler.OnCompleted, entry.completed);
            executeEventHandler(componentMessageHandler.OnFailed, entry.failed);
            executeEventHandler(componentMessageHandler.OnComponentUpdate, entry.componentStatus);
        }
    }

    if (componentStatsCollection.nodeStatus) {
        for (const handler of nodeStatsUpdateEvents) {
            const nodeStatusList = Object.values(componentStatsCollection.nodeStatus);
            handler.OnNodeStatsUpdate(nodeStatusList);
        }
    }
};

const executeEventHandler = <TData>(handler: (data: TData) => void | null, data: TData | null) => {
    if (handler && data) {
        handler(data);
    }
}

// Set up a SignalR connection to the specified hub URL
export const setupSignalRConnection = (
    connectionHub: string,
    onConnectedCallback?: (connection: HubConnection) => void) => {

    const options: IHttpConnectionOptions = {
        logMessageContent: isDev,
        logger: isDev ? LogLevel.Warning : LogLevel.Error,
    };
    // create the connection instance
    // withAutomaticReconnect will automatically try to reconnect
    // and generate a new socket connection if needed
    const connection = new HubConnectionBuilder()
        .withUrl(connectionHub, options)
        .withAutomaticReconnect()
        .withHubProtocol(new JsonHubProtocol())
        .configureLogging(LogLevel.Information)
        .build();

    // Note: to keep the connection open the serverTimeout should be
    // larger than the KeepAlive value that is set on the server
    // keepAliveIntervalInMilliseconds default is 15000 and we are using default
    // serverTimeoutInMilliseconds default is 30000 and we are using 60000 set below
    connection.serverTimeoutInMilliseconds = 60000;

    // re-establish the connection if connection dropped
    connection.onclose(error => {
        console.assert(connection.state === HubConnectionState.Disconnected);
        console.log('Connection closed due to error. Try refreshing this page to restart the connection', error);
    });

    connection.onreconnecting(error => {
        console.assert(connection.state === HubConnectionState.Reconnecting);
        console.log('Connection lost due to error. Reconnecting.', error);
    });

    connection.onreconnected(connectionId => {
        console.assert(connection.state === HubConnectionState.Connected);
        console.log('Connection reestablished. Connected with connectionId', connectionId);
    });

    startSignalRConnection(connection, onConnectedCallback);

    connection.on('OnComponentUpdate',  handleComponentUpdate);

    return connection;
};