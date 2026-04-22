export type DiffResult = "equal" | "a-better" | "b-better" | "different";
export type DiffMap = Record<string, DiffResult>;
