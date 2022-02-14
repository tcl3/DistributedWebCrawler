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

export interface ComponentMessageHandler {
    OnCompleted: (data: CompletedItemStats) => void;
    OnFailed: (data: FailedItemStats) => void;
    OnComponentUpdate: (data: ComponentStatusStats) => void;
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

const actionEventMap: { [key: string]: ComponentMessageHandler } = {};

export const addSignalRHandler = (componentName: string, handler: ComponentMessageHandler) => {
    actionEventMap[componentName] = handler;
}
export const removeSignalRHandler = (componentName: string) => {
    delete actionEventMap[componentName];
}

// Set up a SignalR connection to the specified hub URL, and actionEventMap.
// actionEventMap should be an object mapping event names, to eventHandlers that will
// be dispatched with the message body.
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

    const handleCompleted = (componentName: string, data: CompletedItemStats) => {
        componentName = componentName.toLowerCase();
        const componentMessageHandler = actionEventMap[componentName] ?? actionEventMap['default'];
        const eventHandler = componentMessageHandler && componentMessageHandler.OnCompleted;
        eventHandler && eventHandler(data);
    };

    const handleFailed = (componentName: string, data: FailedItemStats) => {
        componentName = componentName.toLowerCase();
        const componentMessageHandler = actionEventMap[componentName] ?? actionEventMap['default'];
        const eventHandler = componentMessageHandler && componentMessageHandler.OnFailed;
        eventHandler && eventHandler(data);
    };

    const handleComponentUpdate = (componentName: string, data: ComponentStatusStats) => {
        componentName = componentName.toLowerCase();
        const componentMessageHandler = actionEventMap[componentName] ?? actionEventMap['default'];
        const eventHandler = componentMessageHandler && componentMessageHandler.OnComponentUpdate;
        eventHandler && eventHandler(data);
    };

    connection.on('OnCompleted', handleCompleted);
    connection.on('OnFailed', handleFailed);
    connection.on('OnComponentUpdate', handleComponentUpdate);

    return connection;
};