export interface ICommandHub {
    Start: () => Promise<void>;
    Pause: () => Promise<void>;
}